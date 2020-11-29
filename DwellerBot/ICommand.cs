using System.Collections.Generic;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DwellerBot
{
    public interface ICommand
    {
        Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage);
    }
}
