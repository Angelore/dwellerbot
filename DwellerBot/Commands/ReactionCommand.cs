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

namespace DwellerBot.Commands
{
    class ReactionCommand : CommandBase, ISaveable
    {
        private readonly List<FileInfo> _files;
        private readonly Random _rng;
        private readonly string _cacheFilePath;
        private Dictionary<string, string> _sentFiles;

        public ReactionCommand(Api bot, List<string> folderNames, string cacheFilePath):base(bot)
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
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (_files.Count == 0)
            {
                await
                    Bot.SendTextMessage(update.Message.Chat.Id, "No files available.", false, update.Message.MessageId);
                return;
            }

            var ind = _rng.Next(0, _files.Count);
            if (_sentFiles.ContainsKey(_files[ind].FullName))
            {
                try
                {
                    // It is recommended by telegram team that the chataction should be send if the operation is expected to take some time,
                    // which is not the case if you use an image from telegram servers, so this better stay deactivated.
                    // await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.UploadPhoto);
                    await Bot.SendPhoto(update.Message.Chat.Id, _sentFiles[_files[ind].FullName], "", update.Message.MessageId);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "An error has occured during file resending!");
                }
                return;
            }

            using (var fs = new FileStream(_files[ind].FullName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    await Bot.SendChatAction(update.Message.Chat.Id, ChatAction.UploadPhoto);
                    var message = await Bot.SendPhoto(update.Message.Chat.Id, new FileToSend(_files[ind].Name, fs ), "", update.Message.MessageId);
                    _sentFiles.Add(_files[ind].FullName, message.Photo.Last().FileId);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "An error has occured during file sending!");
                }
            }
        }

        public void SaveState()
        {
            if (_sentFiles.Count == 0)
                return;

            using (var sw = new StreamWriter(new FileStream(_cacheFilePath, FileMode.Create, FileAccess.Write)))
            {
                var config = JsonConvert.SerializeObject(_sentFiles, Formatting.Indented);
                sw.WriteLine(config);
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
                    Dictionary<string, string> config = null;
                    try
                    {
                        config = JsonConvert.DeserializeObject<Dictionary<string, string>>(str);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error(ex, "An error as occured during parsing of {0} file.", _cacheFilePath);
                    }
                    if (config != null)
                        _sentFiles = config;
                }
                else
                {
                    Log.Logger.Warning("The file {0} was expected to be populated with data, but was empty.", _cacheFilePath);
                }
            }
        }
    }
}
