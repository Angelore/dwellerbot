﻿using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DwellerBot.Models;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using DwellerBot.Utility;

namespace DwellerBot.Commands
{
    class WeatherCommand : CommandBase
    {
        private const string WeatherQueryUrl =
            @"http://api.openweathermap.org/data/2.5/weather?q=%location%&units=metric&lang=ru&APPID=";

        private readonly string _apiKey;
        private string _location;

        public WeatherCommand(TelegramBotClient bot, string apiKey):base(bot)
        {
            _apiKey = apiKey;
        }

        public override async Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
        {
            // Restore default value
            _location = "Minsk,by";

            // If arguments are supplied, try to insert them into query
            if (parsedMessage.ContainsKey("message"))
            {
                var args = parsedMessage["message"].Split(',');
                if (args.Length >= 2)
                {
                    _location = args[0] + "," + args[1];
                }
            }

            var responseStream = new StreamReader(await GetWeather());
            var weatherContainer = JsonConvert.DeserializeObject<WeatherContainer>(responseStream.ReadToEnd());
            if (weatherContainer.cod == 404)
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, "Invalid arguments.", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, message.MessageId);
                return;
            }

            var sb = new StringBuilder();
            sb.Append("Погода в " + weatherContainer.name + ", " + weatherContainer.sys.country);
            sb.AppendLine(" на " + weatherContainer.dt.UnixTimeStampToDateTime().ToLocalTime().ToShortDateString());
            sb.AppendLine();
            sb.AppendLine("Температура: " + weatherContainer.main.temp + " *C, " + weatherContainer.weather[0].description);
            sb.AppendLine("Влажность: " + weatherContainer.main.humidity + "%");
            sb.AppendLine("Ветер: " + weatherContainer.wind.speed + " м/с");
            await Bot.SendTextMessageAsync(message.Chat.Id, sb.ToString(), Telegram.Bot.Types.Enums.ParseMode.Default, false, false, message.MessageId);
        }

        public async Task<Stream> GetWeather()
        {
            var hc = new HttpClient();
            var queryUrl = WeatherQueryUrl.Replace("%location%", _location); 
            return await hc.GetStreamAsync(queryUrl + _apiKey);
        }
    }
}
