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

        public ShutdownCommand(TelegramBotClient bot, DwellerBot dwellerBot):base(bot)
        {
            _dwellerBot = dwellerBot;
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (!DwellerBot.IsUserOwner(update.Message.From))
                return;

	        ICommand command;
	        if (_dwellerBot.CommandService.RegisteredCommands.TryGetValue("/savestate", out command))
	        {
		        await command.ExecuteAsync(update, parsedMessage);
            }

            _dwellerBot.IsOnline = false;

            await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Shutting down.", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, update.Message.MessageId);
            await Bot.GetUpdatesAsync(update.Id + 1);
        }
    }
}
