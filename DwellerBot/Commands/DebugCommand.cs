using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class DebugCommand : CommandBase
    {
        private readonly DwellerBot _dwellerBot;

        public DebugCommand(TelegramBotClient bot, DwellerBot dwellerBot):base(bot)
        {
            _dwellerBot = dwellerBot;
        }

        public override async Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Launch time: " + _dwellerBot.LaunchTime.ToUniversalTime());
            sb.AppendLine("Commands processed: " + _dwellerBot.CommandsProcessed);
            sb.AppendLine("Errors: " + _dwellerBot.ErrorCount);
            await Bot.SendTextMessageAsync(message.Chat.Id, sb.ToString(), Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, message.MessageId);
        }
    }
}
