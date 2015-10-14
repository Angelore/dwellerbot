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
    class FeatureRequestCommand: CommandBase, ISaveable
    {
        private string _featureRequestsFilePath;
        private Dictionary<int, string> _requests;
        private int _requestIndex;

        public FeatureRequestCommand(Api bot, string featureRequestsFilePath):base(bot)
        {
            _featureRequestsFilePath = featureRequestsFilePath;
            _requests = new Dictionary<int, string>();
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (parsedMessage.ContainsKey("message"))
            {
                _requests.Add(_requestIndex, string.Format("{0} asked for: {1}", update.Message.From.FirstName, parsedMessage["message"]));
                _requestIndex++;

                await _bot.SendTextMessage(update.Message.Chat.Id, string.Format("A request has been added under #{0}", _requestIndex - 1), false, update.Message.MessageId);
                return;
            }

            await _bot.SendTextMessage(update.Message.Chat.Id, "You have to describe your request :)", false, update.Message.MessageId);
        }

        public void SaveState()
        {
            if (_requests.Count == 0)
                return;

            using (var sw = new StreamWriter(new FileStream(_featureRequestsFilePath, FileMode.Create, FileAccess.Write)))
            {
                var config = JsonConvert.SerializeObject(_requests, Formatting.Indented);
                sw.WriteLine(config);
            }

            Log.Logger.Debug("FeatureRequestCommand state was successfully saved.");
        }

        public void LoadState()
        {
            using (var sr = new StreamReader(new FileStream(_featureRequestsFilePath, FileMode.OpenOrCreate)))
            {
                var str = sr.ReadToEnd();
                if (str.Length > 0)
                {
                    Dictionary<int, string> config = null;
                    try
                    {
                        config = JsonConvert.DeserializeObject<Dictionary<int, string>>(str);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "An error as occured during parsing of {0} file.", _featureRequestsFilePath);
                    }
                    if (config != null)
                    {
                        _requests = config;
                        _requestIndex = _requests.Last().Key + 1;
                    }
                }
                else
                {
                    Log.Logger.Warning("The file {0} was expected to be populated with data, but was empty.", _featureRequestsFilePath);
                }
            }
        }
    }
}
