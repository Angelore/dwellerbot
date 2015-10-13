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
        protected readonly Api _bot;

        public CommandBase(Api bot)
        {
            _bot = bot;
        }

        public virtual void Execute(Update update)
        {
            throw new NotImplementedException();
        }

        public virtual Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            throw new NotImplementedException();
        }
    }
}
