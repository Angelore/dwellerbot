using Serilog;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace DwellerBot.Services
{
    public class CommandService
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
            Task handler = update.Type switch
            {
                UpdateType.Message => BotOnMessageReceived(update.Message),
                UpdateType.EditedMessage => BotOnMessageReceived(update.Message),
                UpdateType.CallbackQuery => BotOnCallbackQueryReceived(update.CallbackQuery),
                _ => Task.Run(() => Log.Logger.Warning($"Unsupported update type: {update.Type}"))
            };
            await handler;
        }

        private async Task BotOnMessageReceived(Message message)
        {
            if (message?.Text != null || message?.Caption != null)
            {
                Log.Logger.Debug("A message in chat {0} from user {1}: {2}", message.Chat.Id, message.From.Username, message.Text);

                Dictionary<string, string> parsedMessage = new Dictionary<string, string>();
                try
                {
                    parsedMessage = ParseCommand(message.Text ?? message.Caption);
                    parsedMessage.Add("interpretedCommand", InterpretCommand(parsedMessage["command"]));

                }
                catch (Exception ex)
                {
                    Log.Logger.Error("An error has occured during message parsing. Error message: {0}", ex.Message);
                }
                if (RegisteredCommands.ContainsKey(parsedMessage["command"]))
                {
                    try
                    {
                        await RegisteredCommands[parsedMessage["command"]].HandleMessageAsync(message, parsedMessage);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("An error has occured during {0} command. Error message: {1}", parsedMessage["command"], ex.Message);
                    }
                }
                else if (RegisteredCommands.ContainsKey(parsedMessage["interpretedCommand"])) // Check if the command was typed in a russian layout
                {
                    if (parsedMessage.ContainsKey("message"))
                    {
                        parsedMessage["message"] = InterpretCommand(parsedMessage["message"], false);
                    }
                    try
                    {
                        await RegisteredCommands[parsedMessage["interpretedCommand"]].HandleMessageAsync(message, parsedMessage);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("An error has occured during {0} command. Error message: {1}", parsedMessage["interpretedCommand"], ex.Message);
                    }
                }
            }

        }

        private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery)
        {
            throw new NotImplementedException();
        }

        private readonly Regex _fullCommandRegex = new Regex(@"(?<=^/\w+)@\w+"); // Returns bot name from command (/com@botname => @botname)
        private readonly Regex _commandRegex = new Regex(@"^/\w+");              // Returns command (/com => /com)

        internal Dictionary<string, string> ParseCommand(string input)
        {
            var result = new Dictionary<string, string>();

            if (_commandRegex.IsMatch(input))
            {
                var botNameMatch = _fullCommandRegex.Match(input);

                // Support for the multibot chatrooms: the commands like "/command@OtherBot" will be ignored
                // However, regular "/command" commands will always be processed
                if (botNameMatch.Value == string.Empty || botNameMatch.Value == _botName)
                {
                    var commmandMatch = _commandRegex.Match(input);
                    result.Add("command", commmandMatch.Value);

                    var startIndex = botNameMatch.Value == string.Empty ?
                        commmandMatch.Index + commmandMatch.Length + 1 :
                        botNameMatch.Index + botNameMatch.Length + 1;

                    if (input.Length > startIndex)
                        result.Add("message", input.Substring(startIndex));
                }
            }

            if (!result.ContainsKey("command"))
                result.Add("command", string.Empty);

            return result;
        }

        // Since the foward slash is a service symbol for the bot, it should be ignored while parsing the command
        public static string InterpretCommand(string inputCommand, bool ignoreForwardSlash = true)
        {
            var rusCharSet = @"абвгдеёжзийклмнопрстуфхцчшщъьыэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЪЬЫЭЮЯ\""№;:?/.,";
            var engCharSet = @"f,dult`;pbqrkvyjghcnea[wxio]ms'.zF<DULT~:PBQRKVYJGHCNEA{WXIO}MS'>Z\@#$^&|/?";

            string result = "";

            for (var i = 0; i < inputCommand.Length; i++)
            {
                var ind = rusCharSet.IndexOf(inputCommand[i]);

                if (ind < 0 || (ignoreForwardSlash && inputCommand[i] == '/'))
                {
                    result += inputCommand[i];
                    continue;
                }

                result += engCharSet[ind];
            }

            return result;
        }
    }
}
