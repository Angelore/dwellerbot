using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Telegram.Bot.Types;

namespace DwellerBot.Utility
{
    public enum MessageSeverity { Info, Error, Warning }

    public interface ILogger
    {
        void Log(string message, MessageSeverity severity);

        void Log(string[] messages, MessageSeverity severity);
    }
}
