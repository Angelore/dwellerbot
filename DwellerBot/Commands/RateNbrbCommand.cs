using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DwellerBot.Models;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Serilog;
using Telegram.Bot.Types.Enums;
using Newtonsoft.Json.Linq;

namespace DwellerBot.Commands
{
    class RateNbrbCommand: CommandBase
    {
        private const string CurrencyListUrl = @"http://www.nbrb.by/API/ExRates/Currencies";
        private const string CurrencyRatesApi = @"http://www.nbrb.by/API/ExRates/Rates?onDate={0}&Periodicity=0";
        private const string CurrencyRateApi = @"http://www.nbrb.by/API/ExRates/Rates/{0}?onDate={1}";
        private readonly List<string> _defaultCurrenciesList  = new List<string> {"usd","eur","rub"};
        private const string BaseCurrency = "byn";

        private List<Currency> _currencies = new List<Currency>();
        private List<Rate> _previousRates;

        public RateNbrbCommand(TelegramBotClient bot):base(bot)
        {
            ;
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            // initialize currencies
            if (!_currencies.Any())
            {
                try
                {
                    _currencies = await GetCurrencyList();
                }
                catch(Exception ex)
                {
                    Log.Logger.Error("Unable to get currencies list. Error message: {0}", ex.Message);
                    await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Сервис НБРБ не отвечает на запрос списка валют.", false, false, update.Message.MessageId, null, ParseMode.Markdown);
                    return;
                }
            }
            
            List<string> currenciesList = new List<string>();
            List<string> message = new List<string>();

            if (parsedMessage.ContainsKey("message"))
            {
                message = parsedMessage["message"].Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToList();
                if (message.Contains("to"))
                {
                    if (message.Count != 4)
                    {
                        await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Неверное количество параметров. Формат использования: \"/rate 300 usd to rub\"", false, false, update.Message.MessageId, null, ParseMode.Markdown);
                        return;
                    }

                    int quantity;
                    bool parseResult = int.TryParse(message[0], out quantity);
                    if (!parseResult)
                    {
                        await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Неверно введено количество валюты.", false, false, update.Message.MessageId, null, ParseMode.Markdown);
                        return;
                    }

                    currenciesList.Add(message[1].ToLower());
                    currenciesList.Add(message[3].ToLower());
                    await ConverterRatesCommand(update, currenciesList, quantity);
                }
                else
                {
                    currenciesList = message.Select(s => s.ToLower()).ToList();
                    await RegularRatesCommand(update, currenciesList);
                }
            }
            else
            {
                currenciesList = _defaultCurrenciesList;
                await RegularRatesCommand(update, currenciesList);
            }
        }

        private async Task RegularRatesCommand(Update update, List<string> currenciesList)
        {
            List<Rate> rates = null;
            try
            {
                rates = await GetCurrencyRatesFromApi(DateTime.Today.AddDays(1), currenciesList);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Unable to get currencies. Error message: {0}", ex.Message);
            }

            if (rates == null)
            {
                await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Сервис НБРБ не вернул данные, либо введенной валюты не существует.", false, false, update.Message.MessageId, null, ParseMode.Markdown);
                return;
            }

            // if the array is empty, try getting rates for today instead of tomorrow
            if (!rates.Any())
            {
                try
                {
                    rates = await GetCurrencyRatesFromApi(DateTime.Today, currenciesList);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Unable to get currencies. Error message: {0}", ex.Message);
                }
            }

            // Get data for previous date for comparison
            //if (_previousRates == null ||
            //    !_previousRates.Any() ||
            //    _previousRates.First().Date.AddDays(1) != rates.First().Date)
            {
                var ondate = rates.First().Date.AddDays(-1);
                // Rates do not update on weekend (at least here, duh)
                if (ondate.DayOfWeek == DayOfWeek.Sunday)
                {
                    ondate = ondate.AddDays(-2);
                }
                _previousRates = await GetCurrencyRatesFromApi(ondate, currenciesList);
            }

            var isComparisonPossible = _previousRates != null && _previousRates.Any();

            var sb = new StringBuilder();
            sb.Append("Курсы валют на ");
            sb.AppendLine(rates.First().Date.ToShortDateString());
            if (isComparisonPossible)
            {
                sb.Append("По отношению к ");
                sb.AppendLine(_previousRates.First().Date.ToShortDateString());
                sb.AppendLine();
            }

            foreach (var currency in rates.Where(r => currenciesList.Contains(r.Cur_Abbreviation.ToLower())))
            {
                sb.Append(currency.Cur_Abbreviation + ": " + currency.Cur_OfficialRate + $" `[{currency.Cur_Scale}]`");
                if (isComparisonPossible)
                {
                    var diff = currency.Cur_OfficialRate -
                               _previousRates.First(x => x.Cur_Abbreviation == currency.Cur_Abbreviation).Cur_OfficialRate;
                    sb.Append(" _(");
                    sb.Append(diff > 0 ? "+" : "");
                    sb.Append(diff);
                    sb.Append(")_");
                }
                sb.AppendLine();
            }

            await Bot.SendTextMessageAsync(update.Message.Chat.Id, sb.ToString(), false, false, update.Message.MessageId, null, ParseMode.Markdown);
        }

        private async Task ConverterRatesCommand(Update update, List<string> currenciesList, int quantity)
        {
            DateTime date = DateTime.Today;
            // TODO: fix this shit (lists)
            List<Rate> rate1 = null;
            List<Rate> rate2 = null;
            try
            {
                date = DateTime.Today.AddDays(1);
                rate1 = await GetCurrencyRatesFromApi(DateTime.Today.AddDays(1), new List<string>() { currenciesList.First() });
                rate2 = await GetCurrencyRatesFromApi(DateTime.Today.AddDays(1), new List<string>() { currenciesList.Last() });
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Unable to get currencies. Error message: {0}", ex.Message);
            }
            
            // if the array is empty, try getting rates for today instead of tomorrow
            if ((rate1 == null || rate2 == null) || (!rate1.Any() || !rate2.Any()))
            {
                try
                {
                    date = DateTime.Today;
                    rate1 = await GetCurrencyRatesFromApi(DateTime.Today, new List<string>() { currenciesList.First() });
                    rate2 = await GetCurrencyRatesFromApi(DateTime.Today, new List<string>() { currenciesList.Last() });
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("Unable to get currencies. Error message: {0}", ex.Message);
                }
            }

            if (rate1 == null || rate2 == null)
            {
                if (!currenciesList.Contains(BaseCurrency))
                {
                    await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Сервис НБРБ не вернул данные, либо введенной валюты не существует.", false, false, update.Message.MessageId, null, ParseMode.Markdown);
                    return;
                }
                if (rate1 == null)
                    rate1 = new List<Rate> { new Rate { Cur_Abbreviation = BaseCurrency, Cur_Scale = 1, Cur_OfficialRate = 1 } };
                else
                    rate2 = new List<Rate> { new Rate { Cur_Abbreviation = BaseCurrency, Cur_Scale = 1, Cur_OfficialRate = 1 } };
            }
            
            Rate firstCur;
            Rate secondCur;
            try
            {
                firstCur = rate1.First(c => c.Cur_Abbreviation.ToLower() == currenciesList.First().ToLower());
                secondCur = rate2.First(c => c.Cur_Abbreviation.ToLower() == currenciesList.Last().ToLower());
            }
            catch(Exception ex)
            {
                await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Произошла ошибка при обработке результатов запроса. Скорее всего, неверно введена валюта.", false, false, update.Message.MessageId, null, ParseMode.Markdown);
                return;
            }

            decimal resultingQuantity;
            resultingQuantity = quantity * (firstCur.Cur_OfficialRate.Value / firstCur.Cur_Scale) / (secondCur.Cur_OfficialRate.Value / secondCur.Cur_Scale);

            var sb = new StringBuilder();
            sb.Append("Курсы валют на ");
            sb.AppendLine(date.ToShortDateString());
            sb.AppendLine();
            
            sb.Append(string.Format("{0} {1} = {2} {3}",
                quantity,
                currenciesList.First().ToUpper(),
                Math.Round(resultingQuantity, 4),
                currenciesList.Last().ToUpper()));

            await Bot.SendTextMessageAsync(update.Message.Chat.Id, sb.ToString(), false, false, update.Message.MessageId, null, ParseMode.Markdown);
        }

        public async Task<List<Currency>> GetCurrencyList()
        {
            var hc = new HttpClient();
            var currencyStream = new StreamReader(await hc.GetStreamAsync(CurrencyListUrl));
            var streamAsString = currencyStream.ReadToEnd();
            var currencies = JsonConvert.DeserializeObject<List<Currency>>(streamAsString);

            return currencies;
        }

        public async Task<List<Rate>> GetCurrencyRatesFromApi(DateTime date, List<string> currenciesList)
        {
            var hc = new HttpClient();
            string queryString;
            if (currenciesList.Count == 1)
            {
                var currency = _currencies.Where(c => c.Cur_Abbreviation.ToLower().Equals(currenciesList.First().ToLower()))
                                          .OrderByDescending(x => x.Cur_ID) // a case for outdated currencies that are still returned by the bank, i.e. RUB is returned as 190 and 298
                                          .FirstOrDefault();
                if (currency == null)
                    return null;

                queryString = string.Format(CurrencyRateApi, currency.Cur_ID, date.ToString("yyyy-MM-dd"));
            }
            else
            {
                queryString = string.Format(CurrencyRatesApi, date.ToString("yyyy-MM-dd"));
            }


            var stream = await hc.GetStreamAsync(queryString);
            var currencyRates = new StreamReader(stream).ReadToEnd();
            var rates = JsonConvert.DeserializeObject<List<Rate>>(EnsureJsonArray(currencyRates));

            return rates;
        }

        // HACK: find out how to do this using converter itself
        private string EnsureJsonArray(string json)
        {
            if (json.First() != '[' || json.Last() != ']')
                return "[" + json + "]";
            return json;
        }
    }
}
