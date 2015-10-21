using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class ShutdownCommand: CommandBase
    {
        private DwellerBot _dwellerBot;

        public ShutdownCommand(Api bot, DwellerBot dwellerBot):base(bot)
        {
            _dwellerBot = dwellerBot;
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (!DwellerBot.IsUserOwner(update.Message.From))
                return;

	        ICommand command;
	        if (_dwellerBot.Commands.TryGetValue("/savestate", out command))
	        {
		        await command.ExecuteAsync(update, parsedMessage);
            }

            _dwellerBot.IsOnline = false;

            await Bot.SendTextMessage(update.Message.Chat.Id, "Shutting down.", false, update.Message.MessageId);
            await Bot.GetUpdates(update.Id + 1);
        }
    }
}
