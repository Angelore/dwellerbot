using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class WeatherCommand : ICommand
    {
        private const string WeatherQueryUrl =
            @"http://api.openweathermap.org/data/2.5/weather?q=Minsk,by&APPID=476bdb6ccca4a148eb8bc593d612df72&units=metric&lang=ru";
        private readonly Api _bot;

        public WeatherCommand(Api bot)
        {
            _bot = bot;
        }

        public void Execute(Update update)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            var responseStream = new StreamReader(await GetWeather());
            var weatherContainer = JsonConvert.DeserializeObject<WeatherContainer>(responseStream.ReadToEnd());
            var sb = new StringBuilder();
            sb.Append("Погода в " + weatherContainer.name + ", " + weatherContainer.sys.country);
            sb.AppendLine(" на " + UnixTimeStampToDateTime(weatherContainer.dt).ToLocalTime().ToShortDateString());
            sb.AppendLine();
            sb.AppendLine("От " + weatherContainer.main.temp_min +
                          " до " + weatherContainer.main.temp_max + " *С, " +
                          weatherContainer.weather[0].description);
            sb.AppendLine("Влажность: " + weatherContainer.main.humidity + "%");
            sb.AppendLine("Ветер: " + weatherContainer.wind.speed + " м/с");
            await _bot.SendTextMessage(update.Message.Chat.Id, sb.ToString());
        }

        public async Task<Stream> GetWeather()
        {
            var hc = new HttpClient();
            return await hc.GetStreamAsync(WeatherQueryUrl);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
    }
}
