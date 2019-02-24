using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using Serilog;
using Telegram.Bot.Types.Enums;
using System.Text;
using Telegram.Bot.Types.InputFiles;

namespace DwellerBot.Commands
{
    class ReactionCommand : CommandBase, ISaveable
    {
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly Random _rng;
        private readonly string _cacheFilePath;
        private readonly List<String> _folderNames;

        private List<FileInfo> _files;
        private Dictionary<string, string> _sentFiles;
        private List<string> _ignoredFiles;
        private string _lastUsedFile;

        public ReactionCommand(TelegramBotClient bot, List<string> folderNames, string cacheFilePath) : base(bot)
        {
            _rng = new Random();
            _folderNames = folderNames;
            _files = new List<FileInfo>();
            foreach (var folderName in folderNames)
            {
                var dir = new DirectoryInfo(folderName);
                if (dir.Exists)
                {
                    _files.AddRange(dir.EnumerateFiles().Where(f => AllowedExtensions.Contains(f.Extension)));
                }
            }
            _cacheFilePath = cacheFilePath;

            // Since Telegram allows you to "send" files by using their id (if they are on the server already),
            // I use this to create a simple cache by sending an id of a file if it was already sent once.
            _sentFiles = new Dictionary<string, string>();
            _ignoredFiles = new List<string>();
            _lastUsedFile = null;
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (_files.Count == 0)
            {
                await
                    Bot.SendTextMessageAsync(update.Message.Chat.Id, "No files available.", ParseMode.Markdown, false, false, update.Message.MessageId);
                return;
            }

            if (parsedMessage.ContainsKey("message") && !string.IsNullOrEmpty(parsedMessage["message"]))
            {
                var message = parsedMessage["message"].Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToList();

                if (message[0] == "ignorelast")
                {
                    await IgnoreLastSubCommand(update);
                }
                else if (message[0] == "set")
                {
                    var folderName = message.Count >= 2 ? message[1] : "";
                    await SelectReactionFromSetSubCommand(update, folderName);
                }
                // add more handles here if needed
                else
                {
                    await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Unrecognized arguments", ParseMode.Markdown, false, false, update.Message.MessageId);
                }
            }
            else
            {
                await DefaultReactionSubCommand(update, _files);
            }
        }

        async Task DefaultReactionSubCommand(Update update, List<FileInfo> files)
        {
            if (files.Count == 0)
            {
                Log.Logger.Warning("The reaction command was called on a folder which contains 0 valid files.");
                await Bot.SendTextMessageAsync(update.Message.Chat.Id, "No files available.", ParseMode.Markdown, false, false, update.Message.MessageId);
                return;
            }

            int ind = _rng.Next(0, files.Count);

            var previousUsedFile = _lastUsedFile;
            _lastUsedFile = files[ind].FullName;
            if (_sentFiles.ContainsKey(files[ind].FullName))
            {
                try
                {
                    // It is recommended by telegram team that the chataction should be send if the operation is expected to take some time,
                    // which is not the case if you use an image from telegram servers, so this better stay deactivated.
                    // await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.UploadPhoto);
                    await Bot.SendPhotoAsync(update.Message.Chat.Id, new InputOnlineFile(_sentFiles[files[ind].FullName]), null, ParseMode.Default, false, update.Message.MessageId);
                    return;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("An error has occured during file resending! Error message: {0}", ex.Message);
                    // remove the erroneous entry and try again
                    _sentFiles.Remove(files[ind].FullName);
                }
            }

            using (var fs = new FileStream(files[ind].FullName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    await Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);
                    var message = await Bot.SendPhotoAsync(update.Message.Chat.Id, new InputOnlineFile(fs, files[ind].Name), null, ParseMode.Default, false, update.Message.MessageId);
                    lock (_sentFiles)
                    {
                        _sentFiles.Add(files[ind].FullName, message.Photo.Last().FileId);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "An error has occured during file sending! Error message: {0} File name: {1}", ex.Message, files[ind].FullName);
                    _lastUsedFile = previousUsedFile;
                }
            }
        }

        async Task IgnoreLastSubCommand(Update update)
        {
            if (DwellerBot.IsUserOwner(update.Message.From))
            {
                if (!string.IsNullOrEmpty(_lastUsedFile))
                {
                    if (_sentFiles.ContainsKey(_lastUsedFile))
                        _sentFiles.Remove(_lastUsedFile);
                    else
                        Log.Logger.Debug("_lastUsedFile is not null, but is absent from _sentFiles!");

                    if (!_ignoredFiles.Contains(_lastUsedFile))
                        _ignoredFiles.Add(_lastUsedFile);
                    else
                        Log.Logger.Debug("Last file was already in the _ignoredFiles.");

                    if (_files.Any(f => f.FullName == _lastUsedFile))
                    {
                        var file = _files.First(f => f.FullName == _lastUsedFile);
                        _files.Remove(file);
                    }
                    else
                        Log.Logger.Debug("Last file was not present in _files.");

                    _lastUsedFile = null;
                    await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Last sent file has been added to ignored files.", ParseMode.Markdown, false, false, update.Message.MessageId);
                    return;
                }
            }
            else
            {
                await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Only the bot owner can use this command.", ParseMode.Markdown, false, false, update.Message.MessageId);
            }
        }

        async Task SelectReactionFromSetSubCommand(Update update, string folderName)
        {
            if (string.IsNullOrEmpty(folderName))
            {
                var sb = new StringBuilder();
                // TODO: Prettify this, maybe display folder structure?
                sb.AppendLine("Current folder list is:");
                foreach (var f in _folderNames)
                {
                    var dir = new DirectoryInfo(f);
                    if (dir.Exists)
                    {
                        sb.AppendLine($"`{dir.Name}`");
                    }
                }
                await Bot.SendTextMessageAsync(update.Message.Chat.Id, sb.ToString(), ParseMode.Markdown, false, false, update.Message.MessageId);
            }
            else
            {
                await DefaultReactionSubCommand(update, _files.Where(f => f.Directory.Name == folderName).ToList());
            }
        }

        #region ISaveable
        public void SaveState()
        {
            if (_sentFiles.Count == 0)
                return;

            using (var sw = new StreamWriter(new FileStream(_cacheFilePath, FileMode.Create, FileAccess.Write)))
            {
                var config = new ReactionImageCache() { ValidPaths = _sentFiles, IgnoredPaths = _ignoredFiles };
                var contents = JsonConvert.SerializeObject(config, Formatting.Indented);
                sw.WriteLine(contents);
            }

            Log.Logger.Debug("ReactionCommand state was successfully saved.");
        }

        public void LoadState()
        {
            using (var sr = new StreamReader(new FileStream(_cacheFilePath, FileMode.OpenOrCreate)))
            {
                var str = sr.ReadToEnd();
                if (str.Length > 0)
                {
                    ReactionImageCache config = null;
                    try
                    {
                        config = JsonConvert.DeserializeObject<ReactionImageCache>(str);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error("An error as occured during parsing of {0} file. Error message: {1}", _cacheFilePath, ex.Message);
                    }
                    if (config != null)
                    {
                        _sentFiles = config.ValidPaths;
                        _ignoredFiles = config.IgnoredPaths;
                    }
                }
                else
                {
                    Log.Logger.Warning("The file {0} was expected to be populated with data, but was empty.", _cacheFilePath);
                }
            }

            // Remove ignored files from the initialised files list
            _files = _files.Where(f => !_ignoredFiles.Contains(f.FullName)).ToList();
        }
    }
    #endregion

    class ReactionImageCache
    {
        public Dictionary<string, string> ValidPaths { get; set; }
        public List<string> IgnoredPaths { get; set; }
    }
}
