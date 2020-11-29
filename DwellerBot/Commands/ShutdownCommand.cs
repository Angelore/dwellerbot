using System.Collections.Generic;
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

        public override async Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
        {
            if (!DwellerBot.IsUserOwner(message.From))
                return;

            if (_dwellerBot.CommandService.RegisteredCommands.TryGetValue("/savestate", out ICommand command))
            {
                await command.HandleMessageAsync(message, parsedMessage);
            }

            await Bot.SendTextMessageAsync(message.Chat.Id, "Shutting down.", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, message.MessageId);

            _dwellerBot.CancellationTokenSource.CancelAfter(100);
        }
    }
}
