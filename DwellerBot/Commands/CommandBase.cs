﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    abstract class CommandBase : ICommand
    {
        protected TelegramBotClient  Bot { get; private set; }

        public CommandBase(TelegramBotClient bot)
        {
            Bot = bot;
        }

        public virtual Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
        {
            throw new NotImplementedException();
        }
    }
}
