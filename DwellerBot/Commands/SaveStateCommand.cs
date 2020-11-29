using System.Collections.Generic;
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

        public override async Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
        {
            if (!DwellerBot.IsUserOwner(message.From))
                return;

            _dwellerBot.CommandService.SaveCommandStates();

            await Bot.SendTextMessageAsync(message.Chat.Id, "State saved.", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, message.MessageId);
        }
    }
}
