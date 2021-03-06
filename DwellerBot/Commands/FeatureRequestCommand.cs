﻿using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class FeatureRequestCommand: CommandBase, ISaveable
    {
        private readonly string _featureRequestsFilePath;
        private Dictionary<int, string> _requests;
        private int _requestIndex;

        public FeatureRequestCommand(TelegramBotClient bot, string featureRequestsFilePath):base(bot)
        {
            _featureRequestsFilePath = featureRequestsFilePath;
            _requests = new Dictionary<int, string>();
        }

        public override async Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
        {
            if (parsedMessage.ContainsKey("message"))
            {
                var args = parsedMessage["message"].Split(',');
                if (args.Length == 1)
                {
                    // List all opened or done features
                    if (args[0].Equals("list") || args[0].Equals("done") || args[0].Equals("all"))
                    {
                        bool listOpened = args[0].Equals("list") || args[0].Equals("all");
                        string result = "";
                        foreach (var pair in _requests)
                        {
                            if (listOpened && pair.Value.IndexOf("[Done]") == 0)
                                continue;
                            if (!listOpened && pair.Value.IndexOf("[Done]") != 0)
                                continue;
                            result += string.Format("{0}: {1}{2}", pair.Key, pair.Value, Environment.NewLine);
                        }

                        await Bot.SendTextMessageAsync(message.Chat.Id, result, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, message.MessageId);
                        return;
                    }
                }
                else if (args.Length == 2)
                {
                    int index;
                    if (args[0].Equals("close") && int.TryParse(args[1], out index) && _requests.ContainsKey(index) && DwellerBot.IsUserOwner(message.From))
                    {
                        _requests[index] = "[Done] " + _requests[index];
                        var result = string.Format("{0}: {1}{2}", index, _requests[index], Environment.NewLine);
                        await Bot.SendTextMessageAsync(message.Chat.Id, result, Telegram.Bot.Types.Enums.ParseMode.Default, false, false, message.MessageId);
                        return;
                    }
                }

                _requests.Add(_requestIndex, string.Format("{0} asked for: {1}", message.From.FirstName, parsedMessage["message"]));
                _requestIndex++;

                await Bot.SendTextMessageAsync(message.Chat.Id, string.Format("A request has been added under #{0}", _requestIndex - 1), Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, message.MessageId);
                return;
            }

            await Bot.SendTextMessageAsync(message.Chat.Id, "You have to describe your request :)\nlist - opened requests\ndone - completed requests", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, message.MessageId);
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
                        Log.Logger.Error("An error as occured during parsing of {0} file. Error message: {1}", _featureRequestsFilePath, ex.Message);
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
