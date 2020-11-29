﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using DwellerBot.Models;

namespace DwellerBot.Commands
{
    class ChangelogCommand : CommandBase
    {
        private List<AppVersion> _versions;

        public ChangelogCommand(TelegramBotClient bot) : base(bot)
        {
            var resourceName = @"Resources\changelog.json";

            using (FileStream stream = new FileStream(resourceName, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(stream))
            {
                _versions = JsonConvert.DeserializeObject<List<AppVersion>>(reader.ReadToEnd());
            }
        }

        public override async Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
        {
            var version = _versions.Last();
            var sb = new StringBuilder();
            sb.AppendLine($"Ver. *{version.VersionNumber}* :: {version.ReleaseDate}");
            if (version.Description?.Length > 0)
                sb.AppendLine($"*{version.Description}*");
            sb.AppendLine();
            foreach (var change in version.Changes)
                sb.AppendLine(change);

            await Bot.SendTextMessageAsync(message.Chat.Id, sb.ToString(), Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, message.MessageId);
        }

        //*bold text*
        //_italic text_
        //[text](http://www.example.com/)
        //`inline fixed-width code`
        //```text
        //pre - formatted fixed-width code block
        //```
    }
}
