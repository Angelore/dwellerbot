using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class SaveStateCommand : CommandBase
    {
        private DwellerBot _dwellerBot;

        public SaveStateCommand(TelegramBotClient bot, DwellerBot dwellerBot):base(bot)
        {
            _dwellerBot = dwellerBot;
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (!DwellerBot.IsUserOwner(update.Message.From))
                return;

            _dwellerBot.CommandService.SaveCommandStates();

            await Bot.SendTextMessageAsync(update.Message.Chat.Id, "State saved.", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, update.Message.MessageId);
        }
    }
}
