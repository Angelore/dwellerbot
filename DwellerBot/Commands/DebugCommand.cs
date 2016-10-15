using System;
using System.Collections.Generic;
using System.Linq;
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

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Launch time: " + _dwellerBot.LaunchTime.ToUniversalTime());
            sb.AppendLine("Commands processed: " + _dwellerBot.CommandsProcessed);
            sb.AppendLine("Errors: " + _dwellerBot.ErrorCount);
            await Bot.SendTextMessageAsync(update.Message.Chat.Id, sb.ToString(), false, false, update.Message.MessageId);
        }
    }
}
