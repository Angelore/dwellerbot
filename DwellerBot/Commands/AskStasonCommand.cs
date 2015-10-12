using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class AskStasonCommand : ICommand
    {
        private readonly Random _rng;
        private readonly Api _bot;
        private readonly string[] _responses = { "Да", "Нет", "Возможно", "Маловероятно", "Конечно", "Спросите позже", "Спроси у Пашана" };

        public AskStasonCommand(Api bot)
        {
            _bot = bot;
            _rng = new Random();
        }

        public void Execute(Update update)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (!parsedMessage.ContainsKey("message") || string.IsNullOrEmpty(parsedMessage["message"]))
                return;

            var ind = _rng.Next(0, _responses.Length);
            await _bot.SendTextMessage(update.Message.Chat.Id, _responses[ind], false, update.Message.MessageId);
        }
    }
}
