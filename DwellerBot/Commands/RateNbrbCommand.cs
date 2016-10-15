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

namespace DwellerBot.Commands
{
    class RateNbrbCommand: CommandBase
    {
        private const string CurrencyListUrl = @"http://www.nbrb.by/API/ExRates/Currencies";
        private const string CurrencyRatesApi = @"http://www.nbrb.by/API/ExRates/Rates?onDate={0}&Periodicity=0";
        private const string CurrencyRateApi = @"http://www.nbrb.by/API/ExRates/Rates/{0}?onDate={1}";
        private readonly List<string> _defaultCurrenciesList  = new List<string> {"USD","EUR","RUB"};

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
                    await Bot.SendTextMessage(update.Message.Chat.Id, "Сервис НБРБ не отвечает на запрос списка валют.", false, update.Message.MessageId, null, ParseMode.Markdown);
                    return;
                }
            }
            
            List<string> currenciesList = new List<string>();
            if (parsedMessage.ContainsKey("message"))
            {
                currenciesList = new List<string>() { parsedMessage["message"] };
            }
            if (currenciesList.Count == 0)
                currenciesList = _defaultCurrenciesList;

            List<Rate> rates = null;
            try
            {
                rates = await GetCurrencyRatesFromApi(DateTime.Today.AddDays(1), currenciesList);
            }
            catch(Exception ex)
            {
                Log.Logger.Error("Unable to get currencies. Error message: {0}", ex.Message);
            }

            if (rates == null)
            {
                await Bot.SendTextMessage(update.Message.Chat.Id, "Сервис НБРБ не вернул данные, либо введенной валюты не существует.", false, update.Message.MessageId, null, ParseMode.Markdown);
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
            if (_previousRates == null ||
                !_previousRates.Any() ||
                _previousRates.First().Date.AddDays(1) != rates.First().Date)
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

            foreach (var currency in rates.Where(r => currenciesList.Contains(r.Cur_Abbreviation)))
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
                var currency = _currencies.Where(c => c.Cur_Abbreviation.ToLower().Equals(currenciesList.First().ToLower())).FirstOrDefault();
                if (currency == null)
                    return null;

                queryString = string.Format(CurrencyRateApi, currency.Cur_ID, date.ToString("yyyy-MM-dd"));
            }
            else
            {
                queryString = string.Format(CurrencyRatesApi, date.ToString("yyyy-MM-dd"));
            }

            var stream = await hc.GetStreamAsync(queryString);
            var currencyRatesStream = new StreamReader(stream);
            var rates = JsonConvert.DeserializeObject<List<Rate>>(currencyRatesStream.ReadToEnd());

            return rates;
        }
    }
}
