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

        public SaveStateCommand(Api bot, DwellerBot dwellerBot):base(bot)
        {
            _dwellerBot = dwellerBot;
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (!DwellerBot.IsUserOwner(update.Message.From))
                return;

            foreach (var command in _dwellerBot.Commands.Values)
            {
                if (command is ISaveable)
                {
                    ((ISaveable)command).SaveState();
                }
            }

            await Bot.SendTextMessage(update.Message.Chat.Id, "State saved.", false, update.Message.MessageId);
        }
    }
}
