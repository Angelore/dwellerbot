using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DwellerBot.Utility;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class ReactionCommand : ICommand
    {
        private readonly Api _bot;
        private readonly List<FileInfo> _files;
        private readonly Random _rng;
        private readonly Dictionary<string, string> _sentFiles;
        private readonly string _cacheFilePath;

        public ReactionCommand(Api bot, List<string> folderNames, string cacheFilePath)
        {
            _bot = bot;
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
            _sentFiles = new Dictionary<string, string>();
        }

        public void Execute(Update update)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (_files.Count == 0)
            {
                await
                    _bot.SendTextMessage(update.Message.Chat.Id, "No files available.", false, update.Message.MessageId);
                return;
            }

            var ind = _rng.Next(0, _files.Count);
            if (_sentFiles.ContainsKey(_files[ind].Name))
            {
                try
                {
                    await _bot.SendPhoto(update.Message.Chat.Id, _sentFiles[_files[ind].Name], "", update.Message.MessageId);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("An error has occured during file resending! Message: {0}", ex.Message));
                }
                return;
            }

            using (var fs = new FileStream(_files[ind].FullName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    var message = await _bot.SendPhoto(update.Message.Chat.Id, new FileToSend(_files[ind].Name, fs ), "", update.Message.MessageId);
                    _sentFiles.Add(_files[ind].Name, message.Photo.Last().FileId);
                }
                catch (Exception ex)
                {
                    Logger.Error(string.Format("An error has occured during file sending! Message: {0}", ex.Message));
                }
            }
        }
    }
}
