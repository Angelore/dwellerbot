using Newtonsoft.Json;
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
using Telegram.Bot;

namespace DwellerBot
{
    public class DwellerBot: IDisposable
    {
        private const string BotName = @"@DwellerBot";
        private readonly Api _bot = new Api("130434822:AAEyREsiaeWIBhxPiDuKyZyheX-eHq0YGIU");

        private FileStream _file;
        private DirectoryInfo _dir;
        private List<FileInfo> _reactionImages;
        private int _reactionImagesCount;
        private Random _rng;

        internal int Offset;
        internal DateTime LaunchTime;
        internal int CommandsProcessed;
        internal int ErrorCount;

        private Dictionary<string, ICommand> Commands { get; } 

        public DwellerBot(string offsetFilePath = @"output.txt")
        {

            _file = new FileStream(@"offset.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            //_dir = new DirectoryInfo(@"D:\Pictures\Internets\Reaction_images");
            //_reactionImages = _dir.EnumerateFiles().ToList();
            //_reactionImagesCount = _reactionImages.Count();
            _rng = new Random();
            Offset = 0;
            CommandsProcessed = 0;
            ErrorCount = 0;
            LaunchTime = DateTime.Now;

            Commands = new Dictionary<string, ICommand>
            {
                {@"/debug", new DebugCommand(_bot, this)},
                {@"/rate", new RateCommand(_bot)},
                //{@"/stason", new StasonCommand()},
                {@"/askstason", new AskStasonCommand(_bot)}
            };
        }
        
        public async Task Run()
        {
            var me = await _bot.GetMe();

            Console.WriteLine("{0} is online and fully functional." + Environment.NewLine, me.Username);

            while (true)
            {
                var updates = await _bot.GetUpdates(Offset);

                foreach (var update in updates)
                {
                    if (update.Message.Text != null)
                    {
                        Console.WriteLine("> A message in chat {0} from user {1}: {2}", update.Message.Chat.Id, update.Message.From.Username, update.Message.Text);

                        var parsedMessage = ParseCommand(update.Message.Text);
                        if (Commands.ContainsKey(parsedMessage["command"]))
                        {
                            try
                            {
                                await Commands[parsedMessage["command"]].ExecuteAsync(update, parsedMessage);
                                CommandsProcessed++;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("!> An error has occured during {0} command." + Environment.NewLine, parsedMessage["command"]);
                                Console.WriteLine("!> Error message: {0}", ex.Message);
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

        public void Dispose()
        {
            _file.Seek(0, SeekOrigin.Begin);
            using (var sw = new StreamWriter(_file))
            {
                sw.WriteLine(Offset);
            }
            _file.Close();
        }
    }
}
