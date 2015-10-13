﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DwellerBot.Commands;
using log4net.Repository.Hierarchy;
using Serilog;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot
{
    public class DwellerBot
    {
        private const string BotName = @"@DwellerBot";

        private readonly Api _bot;

        internal int Offset;
        internal DateTime LaunchTime;
        internal int CommandsProcessed;
        internal int ErrorCount;

        private Dictionary<string, ICommand> Commands { get; set; }

        public DwellerBot(Settings settings)
        {
            _bot = new Api(settings.keys.First(x => x.name == "dwellerBotKey").value);

            Offset = 0;
            CommandsProcessed = 0;
            ErrorCount = 0;
            LaunchTime = DateTime.Now.AddHours(3);

            Commands = new Dictionary<string, ICommand>
            {
                {@"/debug", new DebugCommand(_bot, this)},
                {@"/rate", new RateNbrbCommand(_bot)},
                //{@"/stason", new StasonCommand()},
                {@"/askstason", new AskStasonCommand(_bot)},
                {@"/weather", new WeatherCommand(_bot, settings.keys.First(x => x.name == "openWeatherKey").value)},
                {@"/reaction", new ReactionCommand(_bot)},
                {@"/rtd", new RtdCommand(_bot)}
            };
        }

        public async Task Run()
        {
            System.Threading.Thread.Sleep(500);
            var me = await _bot.GetMe();

            Log.Logger.Debug(string.Format("{0} is online and fully functional." + Environment.NewLine, me.Username));

            while (true)
            {
                Update[] updates = new Update[0];
                try
                {
                    updates = await _bot.GetUpdates(Offset);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "An error has occured while receiving updates");
                    ErrorCount++;
                }

                foreach (var update in updates)
                {
                    if (update.Message.Text != null)
                    {
                        Log.Logger.Debug(string.Format("A message in chat {0} from user {1}: {2}", update.Message.Chat.Id, update.Message.From.Username, update.Message.Text));

                        Dictionary<string, string> parsedMessage = new Dictionary<string, string>();
                        try
                        {
                            parsedMessage = ParseCommand(update.Message.Text);
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Error(ex, "An error has occured during message parsing.");
                            ErrorCount++;
                        }
                        if (Commands.ContainsKey(parsedMessage["command"]))
                        {
                            try
                            {
                                await Commands[parsedMessage["command"]].ExecuteAsync(update, parsedMessage);
                                CommandsProcessed++;
                            }
                            catch (Exception ex)
                            {
                                Log.Logger.Error(ex, string.Format("An error has occured during {0} command." + Environment.NewLine, parsedMessage["command"]));
                                ErrorCount++;
                            }
                        }
                    }

                    Offset = update.Id + 1;
                }

                await Task.Delay(1000);
            }
        }

        private readonly Regex _fullCommandRegex = new Regex(@"(?<=^/\w+)@\w+"); // Returns bot name from command (/com@botname => @botname)
        private readonly Regex _commandRegex = new Regex(@"^/\w+"); // Returns command (/com => /com)

        internal Dictionary<string, string> ParseCommand(string input)
        {
            var result = new Dictionary<string, string>();

            if (_commandRegex.IsMatch(input))
            {
                var fullCommandRegexIsMatch = _fullCommandRegex.IsMatch(input);
                Match fullCommandRegexMatch = null;
                if (!fullCommandRegexIsMatch || (fullCommandRegexMatch = _fullCommandRegex.Match(input)).Value == BotName)
                {
                    var commmandMatch = _commandRegex.Match(input);
                    result.Add("command", commmandMatch.Value);
                    if (!fullCommandRegexIsMatch)
                    {
                        var startIndex = commmandMatch.Index + commmandMatch.Length + 1;
                        if (input.Length > startIndex)
                            result.Add("message", input.Substring(startIndex));
                    }
                    else
                    {
                        var startIndex = fullCommandRegexMatch.Index + fullCommandRegexMatch.Length + 1;
                        if (input.Length > startIndex)
                            result.Add("message", input.Substring(startIndex));
                    }
                }
            }

            if (!result.ContainsKey("command"))
                result.Add("command", string.Empty);

            return result;
        }
    }
}
