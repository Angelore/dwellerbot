using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class RateCommand : ICommand
    {
        private const string CurrencyQueryUrl = @"https://query.yahooapis.com/v1/public/yql?q=select+*+from+yahoo.finance.xchange+where+pair+=+%22USDBYR,EURBYR,RUBBYR%22&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback=";
        private readonly Api _bot;

        public RateCommand(Api bot)
        {
            _bot = bot;
        }

        public void Execute(Update update)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            var responseStream = new StreamReader(await GetCurrencyRates());
            var currencyContainer = JsonConvert.DeserializeObject<CurrencyContainer>(responseStream.ReadToEnd());
            var sb = new StringBuilder();
            sb.Append("Курсы валют на ");
            sb.AppendLine(currencyContainer.query.created.ToShortDateString());
            sb.AppendLine();
            foreach (var currency in currencyContainer.query.results.rate)
            {
                sb.AppendLine(currency.Name.Substring(0, 3) + ": " + currency.Rate.Substring(0, currency.Rate.Length - 2));
            }
            await _bot.SendTextMessage(update.Message.Chat.Id, sb.ToString(), false, update.Message.MessageId);
        }

        public async Task<Stream> GetCurrencyRates()
        {
            var hc = new HttpClient();
            return await hc.GetStreamAsync(CurrencyQueryUrl);
        }
    }
}
