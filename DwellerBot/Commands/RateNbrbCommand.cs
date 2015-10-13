﻿using System;
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

namespace DwellerBot.Commands
{
    class RateNbrbCommand: CommandBase
    {
        private const string CurrencyQueryUrl = @"http://www.nbrb.by/Services/XmlExRates.aspx";
        private readonly List<string> _defaultCurrenciesList  = new List<string> {"USD","EUR","RUB"}; 

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
        }

        public async Task<Stream> GetCurrencyRates()
        {
            var hc = new HttpClient();
            return await hc.GetStreamAsync(CurrencyQueryUrl);
        }
    }
}