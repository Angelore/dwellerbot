using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DwellerBot.Services
{
    class CommandService
    {
        private readonly string _botName;

        public Dictionary<string, ICommand> RegisteredCommands { get; private set; }

        public CommandService(string botName)
        {
            _botName = botName;
            RegisteredCommands = new Dictionary<string, ICommand>();
        }

        public void RegisterCommand(string commandAlias, ICommand commandInstance)
        {
            RegisteredCommands[commandAlias] = commandInstance;
        }

        public void RegisterCommands(Dictionary<string, ICommand> commands)
        {
            foreach (var c in commands)
            {
                RegisteredCommands.Add(c.Key, c.Value);
            }
        }

        public void LoadCommandStates()
        {
            // Load states of commands that support states
            foreach (var command in RegisteredCommands.Values)
            {
                if (command is ISaveable)
                {
                    ((ISaveable)command).LoadState();
                }
            }
        }

        public void SaveCommandStates()
        {
            foreach (var command in RegisteredCommands.Values)
            {
                if (command is ISaveable)
                {
                    ((ISaveable)command).SaveState();
                }
            }
        }

        public async Task HandleUpdate(Update update)
        {
            if (update.Message != null && update.Message.Text != null)
            {
                Log.Logger.Debug("A message in chat {0} from user {1}: {2}", update.Message.Chat.Id, update.Message.From.Username, update.Message.Text);

                Dictionary<string, string> parsedMessage = new Dictionary<string, string>();
                try
                {
                    parsedMessage = ParseCommand(update.Message.Text);
                    parsedMessage.Add("interpretedCommand", InterpretCommand(parsedMessage["command"]));

                }
                catch (Exception ex)
                {
                    Log.Logger.Error("An error has occured during message parsing. Error message: {0}", ex.Message);
                    //ErrorCount++;
                }
                if (RegisteredCommands.ContainsKey(parsedMessage["command"]))
                {
                    try
                    {
                        await RegisteredCommands[parsedMessage["command"]].ExecuteAsync(update, parsedMessage);
                        //CommandsProcessed++;
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("An error has occured during {0} command. Error message: {1}", parsedMessage["command"], ex.Message);
                        //ErrorCount++;
                    }
                }
                else if (RegisteredCommands.ContainsKey(parsedMessage["interpretedCommand"]))// Check if the command mas typed in a russian layout
                {
                    try
                    {
                        await RegisteredCommands[parsedMessage["interpretedCommand"]].ExecuteAsync(update, parsedMessage);
                        //CommandsProcessed++;
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("An error has occured during {0} command. Error message: {1}", parsedMessage["interpretedCommand"], ex.Message);
                        //ErrorCount++;
                    }
                }
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
                if (!fullCommandRegexIsMatch || (fullCommandRegexMatch = _fullCommandRegex.Match(input)).Value == _botName)
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

        public static string InterpretCommand(string inputCommand)
        {
            var rusCharSet = @"абвгдеёжзийклмнопрстуфхцчшщъьыэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЫЭЮЯ\""№;:?/.,";
            var engCharSet = @"f,dult`;pbqrkvyjghcnea[wxio]ms'.zF<DULT~:PBQRKVYJGHCNEA{WXIO}MS'>Z\@#$^&|/?";

            var a = rusCharSet.Length;
            var b = engCharSet.Length;

            inputCommand = inputCommand.ToLower();
            //List<char> outputSymbols = new List<char>();
            string result = "/";

            for (var i = 1; i < inputCommand.Length; i++)
            {
                //outputSymbols.Add(rusCharSet[rusCharSet.IndexOf(inputCommand[i])]);
                var ind = rusCharSet.IndexOf(inputCommand[i]);
                if (ind < 0)
                    return "";

                result += engCharSet[ind];
            }

            return result;
        }
    }
}
