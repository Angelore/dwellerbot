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
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;

namespace DwellerBot.Commands
{
    class ReactionCommand : CommandBase, ISaveable
    {
        private readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };
        private readonly string UploadsDirectory = "uploads";

        private readonly Random _rng;
        private readonly string _cacheFilePath;
        private readonly List<String> _folderNames;

        private List<FileInfo> _files;
        private Dictionary<string, string> _sentFiles;
        private List<string> _ignoredFiles;
        private List<string> _uploadedFiles;
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
            _uploadedFiles = new List<string>();
            _lastUsedFile = null;
        }

        public override async Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
        {
            if (_files.Count == 0)
            {
                await
                    Bot.SendTextMessageAsync(message.Chat.Id, "No files available.", ParseMode.Markdown, false, false, message.MessageId);
                return;
            }

            if (parsedMessage.ContainsKey("message") && !string.IsNullOrEmpty(parsedMessage["message"]))
            {
                var messageList = parsedMessage["message"].Split(' ').Where(s => !string.IsNullOrEmpty(s)).ToList();

                switch (messageList[0])
                {
                    case "ignorelast":
                        await IgnoreLastSubCommand(message);
                        break;
                    case "set":
                        var folderName = messageList.Count >= 2 ? messageList[1] : "";
                        await SelectReactionFromSetSubCommand(message, folderName);
                        break;
                    case "add":
                        await AddImageSubCommand(message);
                        break;
                    case "help":
                        await Bot.SendTextMessageAsync(message.Chat.Id,
                            "Reaction command without arguments returns a random image." + Environment.NewLine +
                            "Parmeters:" + Environment.NewLine +
                            "`ignorelast` - adds the latest image to the blacklist" + Environment.NewLine +
                            "`set` - returns a list of currently loaded sets (folders). `set setname` returns an image from the set" + Environment.NewLine +
                            "`add` - adds an image to the list. The command needs to be a caption on an uncompressed image",
                            ParseMode.Markdown, false, false, message.MessageId);
                        break;
                    default:
                        await Bot.SendTextMessageAsync(message.Chat.Id, "Unrecognized arguments", ParseMode.Markdown, false, false, message.MessageId);
                        break;
                }
            }
            else
            {
                await DefaultReactionSubCommand(message, _files);
            }
        }

        async Task DefaultReactionSubCommand(Message message, List<FileInfo> files)
        {
            if (files.Count == 0)
            {
                Log.Logger.Warning("The reaction command was called on a folder which contains 0 valid files.");
                await Bot.SendTextMessageAsync(message.Chat.Id, "No files available.", ParseMode.Markdown, false, false, message.MessageId);
                return;
            }

            int ind = _rng.Next(0, files.Count);

            var previousUsedFile = _lastUsedFile;
            _lastUsedFile = files[ind].FullName;
            var relativePath = GetRelativePath(files[ind].FullName);
            if (_sentFiles.ContainsKey(relativePath))
            {
                try
                {
                    // It is recommended by telegram team that the chataction should be send if the operation is expected to take some time,
                    // which is not the case if you use an image from telegram servers, so this better stay deactivated.
                    // await Bot.SendChatAction(message.Chat.Id, ChatAction.UploadPhoto);
                    await Bot.SendPhotoAsync(message.Chat.Id, new InputOnlineFile(_sentFiles[relativePath]), null, ParseMode.Default, false, message.MessageId);
                    return;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("An error has occured during file resending! Error message: {0}", ex.Message);
                    // remove the erroneous entry and try again
                    _sentFiles.Remove(relativePath);
                }
            }

            using (var fs = new FileStream(files[ind].FullName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    await Bot.SendChatActionAsync(message.Chat.Id, ChatAction.UploadPhoto);
                    var responseMessage = await Bot.SendPhotoAsync(message.Chat.Id, new InputOnlineFile(fs, files[ind].Name), null, ParseMode.Default, false, message.MessageId);
                    lock (_sentFiles)
                    {
                        _sentFiles.Add(relativePath, responseMessage.Photo.Last().FileId);
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "An error has occured during file sending! Error message: {0} File name: {1}", ex.Message, files[ind].FullName);
                    _lastUsedFile = previousUsedFile;
                }
            }
        }

        async Task IgnoreLastSubCommand(Message message)
        {
            if (DwellerBot.IsUserOwner(message.From))
            {
                if (!string.IsNullOrEmpty(_lastUsedFile))
                {
                    var relativePath = GetRelativePath(_lastUsedFile);
                    if (_sentFiles.ContainsKey(relativePath))
                        _sentFiles.Remove(relativePath);
                    else
                        Log.Logger.Debug("_lastUsedFile is not null, but is absent from _sentFiles!");

                    // TODO: Remove last used from uploaded files

                    if (!_ignoredFiles.Contains(relativePath))
                        _ignoredFiles.Add(relativePath);
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
                    await Bot.SendTextMessageAsync(message.Chat.Id, "Last sent file has been added to ignored files.", ParseMode.Markdown, false, false, message.MessageId);
                    return;
                }
            }
            else
            {
                await Bot.SendTextMessageAsync(message.Chat.Id, "Only the bot owner can use this command.", ParseMode.Markdown, false, false, message.MessageId);
            }
        }

        async Task SelectReactionFromSetSubCommand(Message message, string folderName)
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
                sb.AppendLine($"`{UploadsDirectory}`");
                await Bot.SendTextMessageAsync(message.Chat.Id, sb.ToString(), ParseMode.Markdown, false, false, message.MessageId);
            }
            else
            {
                await DefaultReactionSubCommand(message, _files.Where(f => f.Directory.Name == folderName).ToList());
            }
        }

        async Task AddImageSubCommand(Message message)
        {
            if (message.Photo != null && message.Photo.Length > 0)
            {
                var biggestImage = message.Photo.Last();
                var fakeFileName = Path.Combine(UploadsDirectory, biggestImage.FileId);
                lock (_files)
                {
                    _files.Add(new FileInfo(fakeFileName));
                    _sentFiles.Add(fakeFileName, biggestImage.FileId);
                    _uploadedFiles.Add(biggestImage.FileId);
                }
                await Bot.SendTextMessageAsync(message.Chat.Id, "The image has been added to the list.", ParseMode.Markdown, false, false, message.MessageId);
            }
            else
                await Bot.SendTextMessageAsync(message.Chat.Id, "No images were found in the message.", ParseMode.Markdown, false, false, message.MessageId);
        }

        #region ISaveable
        public void SaveState()
        {
            if (_sentFiles.Count == 0)
                return;

            using (var sw = new StreamWriter(new FileStream(_cacheFilePath, FileMode.Create, FileAccess.Write)))
            {
                var config = new ReactionImageCache() { ValidPaths = _sentFiles, IgnoredPaths = _ignoredFiles, UploadedFiles = _uploadedFiles };
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
                        _sentFiles = config.ValidPaths ?? new Dictionary<string, string>();
                        _ignoredFiles = config.IgnoredPaths ?? new List<string>();
                        _uploadedFiles = config.UploadedFiles ?? new List<string>();
                    }
                }
                else
                {
                    Log.Logger.Warning("The file {0} was expected to be populated with data, but was empty.", _cacheFilePath);
                }
            }

            // Remove ignored files from the initialised files list
            _files = _files.Where(f => !_ignoredFiles.Contains(GetRelativePath(f.FullName))).ToList();
            // Add uploaded files as fake files
            foreach(var uf in _uploadedFiles)
            {
                _files.Add(new FileInfo(Path.Combine(UploadsDirectory, uf)));
            }
        }
        #endregion

        //private void CreateUploadsDir()
        //{
        //    if (!Directory.Exists(UploadsDirectory))
        //        Directory.CreateDirectory(UploadsDirectory);
        //}

        private string GetRelativePath(string FullPath)
        {
            return Path.GetRelativePath(AppDomain.CurrentDomain.BaseDirectory, FullPath);
        }
    }

    class ReactionImageCache
    {
        public Dictionary<string, string> ValidPaths { get; set; }
        public List<string> IgnoredPaths { get; set; }
        public List<string> UploadedFiles { get; set; }
    }
}
