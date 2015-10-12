using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class DebugCommand : ICommand
    {
        private readonly Api _bot;
        private readonly DwellerBot _dwellerBot;

        public DebugCommand(Api bot, DwellerBot dwellerBot)
        {
            _bot = bot;
            _dwellerBot = dwellerBot;
        }

        public void Execute(Update update)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Launch time: " + _dwellerBot.LaunchTime.ToUniversalTime());
            sb.AppendLine("Commands processed: " + _dwellerBot.CommandsProcessed);
            sb.AppendLine("Errors: " + _dwellerBot.ErrorCount);
            await _bot.SendTextMessage(update.Message.Chat.Id, sb.ToString(), false, update.Message.MessageId);
        }
    }
}
