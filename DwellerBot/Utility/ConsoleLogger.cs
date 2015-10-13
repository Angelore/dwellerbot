using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwellerBot.Utility
{
    public class ConsoleLogger: ILogger
    {
        public ConsoleLogger()
        { }

        public void Log(string message, MessageSeverity severity)
        {
            this.Log(new [] { message }, severity);
        }

        public void Log(string[] messages, MessageSeverity severity)
        {
            string prefix = "";
            switch (severity)
            {
                case MessageSeverity.Info:
                    prefix = "> ";
                    break;
                case MessageSeverity.Warning:
                    prefix = "!> ";
                    break;
                case MessageSeverity.Error:
                    prefix = "!!> ";
                    break;
            }

            foreach (var message in messages)
            {
                Console.WriteLine(prefix + message);
            }
        }
    }
}
