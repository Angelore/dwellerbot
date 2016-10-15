using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class CommandBase : ICommand
    {
        protected TelegramBotClient  Bot { get; private set; }

        public CommandBase(TelegramBotClient bot)
        {
            Bot = bot;
        }

        public virtual Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            throw new NotImplementedException();
        }
    }
}
