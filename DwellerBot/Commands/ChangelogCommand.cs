using DwellerBot.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DwellerBot.Commands
{
    class ChangelogCommand : CommandBase
    {
        public const string CommandName = "/changelog";
        private readonly List<AppVersion> _versions;
        private const string PreviousVersion = "Previous";
        private const string NextVersion = "Next";
        private const string PreviousVersionToken = "p";
        private const string NextVersionToken = "n";

        public ChangelogCommand(TelegramBotClient bot) : base(bot)
        {
            var resourceName = @"Resources/changelog.json";

            using (FileStream stream = new FileStream(resourceName, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(stream))
            {
                _versions = JsonConvert.DeserializeObject<List<AppVersion>>(reader.ReadToEnd());
            }
        }

        public override async Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
        {
            var version = _versions.Last();
            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: GetTextMessage(version),
                parseMode: ParseMode.Markdown,
                replyToMessageId: message.MessageId,
                replyMarkup: GetKeyboardMarkup(version.VersionNumber));
        }

        public override async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            var versionString = callbackQuery.Data.Split(';')[1];
            var direction = callbackQuery.Data.Split(';')[2];
            AppVersion version = null;
            switch (direction) {
                case PreviousVersionToken:
                    var i = _versions.FindIndex(v => v.VersionNumber.Equals(versionString));
                    version = i > 0 ? _versions[i-1] : _versions.First();
                    break;
                case NextVersionToken:
                    i = _versions.FindIndex(v => v.VersionNumber.Equals(versionString));
                    version = i < _versions.Count - 1 ? _versions[i+1] : _versions.Last();
                    break;
            }

            if (versionString.Equals(version.VersionNumber)) {
                var response = direction == NextVersionToken ?
                    "Reached the latest version" :
                    "Reached the frist version";
                await Bot.AnswerCallbackQueryAsync(
                    callbackQueryId: callbackQuery.Id,
                    text: response
                );
                return;
            }

            await Bot.EditMessageTextAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                text: GetTextMessage(version),
                parseMode: ParseMode.Markdown,
                replyMarkup: GetKeyboardMarkup(version.VersionNumber));
        }

        private string GetTextMessage(AppVersion version)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Ver. *{version.VersionNumber}* :: {version.ReleaseDate}");
            if (version.Description?.Length > 0)
                sb.AppendLine($"*{version.Description}*");
            sb.AppendLine();
            foreach (var change in version.Changes)
                sb.AppendLine(change);

            return sb.ToString();
        }

        private InlineKeyboardMarkup GetKeyboardMarkup(string payload) {
            return new InlineKeyboardMarkup(new[]
                {
                new [] {
                    InlineKeyboardButton.WithCallbackData(PreviousVersion, $"{CommandName};{payload};{PreviousVersionToken}"),
                    InlineKeyboardButton.WithCallbackData(NextVersion, $"{CommandName};{payload};{NextVersionToken}"),
                }
            });
        }
    }
}
