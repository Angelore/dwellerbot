using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class AskStasonCommand : CommandBase
    {
        private readonly Random _rng;
        // private readonly string[] _responses = { "Да", "Нет", "Возможно", "Маловероятно", "Конечно", "Спросите позже", "Спроси у Пашана" };
        private readonly List<string> _responses;
        private readonly List<int> _weights;
        private readonly int _maxRadnomValue;

        public AskStasonCommand(Api bot, string responsesFilePath):base(bot)
        {
            _rng = new Random();
            _responses = new List<string>();
            _weights = new List<int>();

            using (var sr = new StreamReader(new FileStream(responsesFilePath, FileMode.Open)))
            {
                var str = sr.ReadToEnd();
                if (str.Length > 0)
                {
                    Dictionary<string, int> config = null;
                    try
                    {
                        config = JsonConvert.DeserializeObject<Dictionary<string, int>>(str);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("An error as occured during parsing of {0} file. Error message: {1}", responsesFilePath, ex.Message);
                    }
                    if (config != null)
                    {
                        // { a:5, b:5, c:3, d:1 } => { 5:a, 10:b, 13:c, 14:d }
                        int accumulator = 0;
                        foreach (var item in config)
                        {
                            // Skip incorrect values
                            if (item.Value < 0)
                                continue;

                            accumulator += item.Value;
                            _responses.Add(item.Key);
                            _weights.Add(accumulator);
                        }
                        _maxRadnomValue = accumulator;
                    }
                }
                else
                {
                    Log.Logger.Warning("The file {0} was expected to be populated with data, but was empty.", responsesFilePath);
                }
            }
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (_responses.Count == 0)
                await Bot.SendTextMessage(update.Message.Chat.Id, "Зато мы делаем ракеты", false, update.Message.MessageId);

            if (!parsedMessage.ContainsKey("message") || string.IsNullOrEmpty(parsedMessage["message"]))
                return;

            var ind = _rng.Next(0, _maxRadnomValue);
            string answer = "";
            for (var i = _weights.Count - 1; i >= 0; i--)
            {
                if (ind >= _weights[i])
                {
                    answer = _responses[i];
                    break;
                }
            }
            // Border case, if the value is between 0 and first answer's weight, no value will be selected
            if (string.IsNullOrEmpty(answer))
            {
                answer = _responses[0];
            }
            await Bot.SendTextMessage(update.Message.Chat.Id, answer, false, update.Message.MessageId);
        }
    }
}
