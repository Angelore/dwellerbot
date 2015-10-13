using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class SaveStateCommand : ICommand
    {
        private Api _bot;
        private DwellerBot _dwellerBot;

        public SaveStateCommand(Api bot, DwellerBot dwellerBot)
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
            if (DwellerBot.OwnerUsername != update.Message.From.Username)
                return;

            foreach (var command in _dwellerBot.Commands.Values)
            {
                if (command is ISaveable)
                {
                    ((ISaveable)command).SaveState();
                }
            }

            await _bot.SendTextMessage(update.Message.Chat.Id, "State saved.", false, update.Message.MessageId);
        }
    }
}
