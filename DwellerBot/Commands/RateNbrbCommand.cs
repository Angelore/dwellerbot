using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using DwellerBot.Models;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Serilog;

namespace DwellerBot.Commands
{
    class RateNbrbCommand: CommandBase, ISaveable
    {
        private const string CurrencyQueryUrl = @"http://www.nbrb.by/Services/XmlExRates.aspx";
        private readonly List<string> _defaultCurrenciesList  = new List<string> {"USD","EUR","RUB"};
        private const string StorageFolderName = "CurrencyRates";

        private CurrencyContainerXml _previousResult = null;

        public RateNbrbCommand(Api bot):base(bot)
        {
            ;
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            var responseStream = new StreamReader(await GetCurrencyRates());
            var xmlDeserializer = new XmlSerializer(typeof(CurrencyContainerXml.DailyExRates));
            var currencyContainer = new CurrencyContainerXml() { DailyRates = xmlDeserializer.Deserialize(responseStream) as CurrencyContainerXml.DailyExRates};
            var sb = new StringBuilder();
            sb.Append("Курсы валют на ");
            sb.AppendLine(currencyContainer.DailyRates.Date);
            sb.AppendLine();

            List<string> currenciesList = new List<string>();
            if (parsedMessage.ContainsKey("message"))
            {
                var names = parsedMessage["message"].Split(',').ToList();
                foreach (var cname in names)
                {
                    currenciesList.Add(cname.ToUpper());
                }
            }
            if (currenciesList.Count == 0)
                currenciesList = _defaultCurrenciesList;

            foreach (var currency in currencyContainer.DailyRates.Currency.Where(x => currenciesList.Contains(x.CharCode)))
            {
                sb.AppendLine(currency.CharCode + ": " + currency.Rate);
            }
            await _bot.SendTextMessage(update.Message.Chat.Id, sb.ToString(), false, update.Message.MessageId);

            _previousResult = currencyContainer;
        }

        public async Task<Stream> GetCurrencyRates()
        {
            var hc = new HttpClient();
            return await hc.GetStreamAsync(CurrencyQueryUrl);
        }

        public void SaveState()
        {
            if (_previousResult == null)
                return;

            var dirInfo = new DirectoryInfo(StorageFolderName);
            if (!dirInfo.Exists)
                dirInfo.Create();

            string newFileName = "currencyrate" + _previousResult.DailyRates.Date.Replace("/","") + ".xml";
            var fileInfo = new FileInfo(System.IO.Path.Combine(StorageFolderName, newFileName));
            if (fileInfo.Exists)
                return;

            using (var sw = new StreamWriter(new FileStream(fileInfo.FullName, FileMode.Create, FileAccess.Write)))
            {
                var xmlSerializer = new XmlSerializer(typeof(CurrencyContainerXml.DailyExRates));
                xmlSerializer.Serialize(sw, _previousResult.DailyRates);
            }

            Log.Logger.Debug("RateNbrbCommand state was successfully saved.");
        }

        public void LoadState()
        {
            LoadState(DateTime.Now.AddDays(-1).ToString("MMddyyyy"));
        }

        public void LoadState(string targetDate)
        {
            var dirInfo = new DirectoryInfo(StorageFolderName);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
                return;
            }

            string newFileName = "currencyrate" + targetDate + ".xml";
            var fileInfo = new FileInfo(System.IO.Path.Combine(StorageFolderName, newFileName));
            if (!fileInfo.Exists)
                return;

            using (var sr = new StreamReader(new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read)))
            {
                var xmlDeserializer = new XmlSerializer(typeof(CurrencyContainerXml.DailyExRates));
                _previousResult = new CurrencyContainerXml() { DailyRates = xmlDeserializer.Deserialize(sr) as CurrencyContainerXml.DailyExRates };
            }

            // Log.Logger.Debug("RateNbrbCommand state was successfully loaded.");
        }
    }
}
