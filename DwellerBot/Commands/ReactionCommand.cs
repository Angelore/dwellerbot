using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Newtonsoft.Json;
using Serilog;
using Telegram.Bot.Types.Enums;

namespace DwellerBot.Commands
{
    class ReactionCommand : CommandBase, ISaveable
    {
        private readonly List<FileInfo> _files;
        private readonly Random _rng;
        private readonly string _cacheFilePath;
        private Dictionary<string, string> _sentFiles;
        private List<string> _ignoredFiles;
        private string _lastUsedFile;

        public ReactionCommand(TelegramBotClient bot, List<string> folderNames, string cacheFilePath):base(bot)
        {
            _rng = new Random();
            _files = new List<FileInfo>();
            foreach (var folderName in folderNames)
            {
                var dir = new DirectoryInfo(folderName);
                if (dir.Exists)
                {
                    _files.AddRange(dir.EnumerateFiles().ToList());
                }
            }
            _cacheFilePath = cacheFilePath;
            
            // Since Telegram allows you to "send" files by using their id (if they are on the server already),
            // I use this to create a simple cache by sending id of a file if it was already sent once.
            _sentFiles = new Dictionary<string, string>();
            _ignoredFiles = new List<string>();
            _lastUsedFile = null;
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (_files.Count == 0)
            {
                await
                    Bot.SendTextMessageAsync(update.Message.Chat.Id, "No files available.", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, update.Message.MessageId);
                return;
            }

            if (parsedMessage.ContainsKey("message") && !string.IsNullOrEmpty(parsedMessage["message"]))
            {
                if (parsedMessage["message"] == "ignorelast")
                {
                    if(DwellerBot.IsUserOwner(update.Message.From))
                    {
                        if (!string.IsNullOrEmpty(_lastUsedFile))
                        {
                            if (!_sentFiles.ContainsKey(_lastUsedFile))
                                Log.Logger.Debug("_lastUsedFile is not null, but is absent from _sentFiles!");
                            else
                                _sentFiles.Remove(_lastUsedFile);

                            if (_ignoredFiles.Contains(_lastUsedFile))
                                Log.Logger.Debug("Last file was already in the _ignoredFiles.");
                            else
                                _ignoredFiles.Add(_lastUsedFile);

                            _lastUsedFile = null;
                            await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Last sent file has been added to ignored files.", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, update.Message.MessageId);
                            return;
                        }
                    }
                    else
                    {
                        await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Only bot owner can use this command.", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, update.Message.MessageId);
                        return;
                    }
                    return;
                }
                // add more handles here if needed
                else
                {
                    await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Unrecognized arguments", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, update.Message.MessageId);
                    return;
                }
            }

            int ind;
            do
            {
                ind = _rng.Next(0, _files.Count);
            }
            while (_ignoredFiles.Contains(_files[ind].FullName) || _files[ind].Name == "Thumbs.db");

            var previousUsedFile = _lastUsedFile;
            _lastUsedFile = _files[ind].FullName;
            if (_sentFiles.ContainsKey(_files[ind].FullName))
            {
                try
                {
                    // It is recommended by telegram team that the chataction should be send if the operation is expected to take some time,
                    // which is not the case if you use an image from telegram servers, so this better stay deactivated.
                    // await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.UploadPhoto);
                    await Bot.SendPhotoAsync(update.Message.Chat.Id, new FileToSend(_sentFiles[_files[ind].FullName]), "", false, update.Message.MessageId);
                    return;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("An error has occured during file resending! Error message: {0}", ex.Message);
                    // remove the erroneous entry and try again
                    _sentFiles.Remove(_files[ind].FullName);
                }
            }

            using (var fs = new FileStream(_files[ind].FullName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    await Bot.SendChatActionAsync(update.Message.Chat.Id, ChatAction.UploadPhoto);
                    var message = await Bot.SendPhotoAsync(update.Message.Chat.Id, new FileToSend(_files[ind].Name, fs), "", false, update.Message.MessageId);
                    lock (_files)
                    {
                        _sentFiles.Add(_files[ind].FullName, message.Photo.Last().FileId);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "An error has occured during file sending! Error message: {0} File name: {1}", ex.Message, _files[ind].FullName);
                    _lastUsedFile = previousUsedFile;
                }
            }
        }

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
        }
    }

    class ReactionImageCache
    {
        public Dictionary<string, string> ValidPaths { get; set; }
        public List<string> IgnoredPaths { get; set; }
    }
}
