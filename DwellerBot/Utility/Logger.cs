using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DwellerBot.Utility
{
    public static class Logger
    {
        private static readonly List<ILogger> _loggers = new List<ILogger>();

        public static void AddLogger(ILogger loggerImplementation)
        {
            _loggers.Add(loggerImplementation);
        }

        public static void AddLoggers(IEnumerable<ILogger> loggerImplementations)
        {
            _loggers.AddRange(loggerImplementations);
        }

        // Single message
        public static void Log(string message, MessageSeverity severity)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(message, severity);
            }
        }

        public static void Info(string message)
        {
            Logger.Log(message, MessageSeverity.Info);
        }

        public static void Error(string message)
        {
            Logger.Log(message, MessageSeverity.Error);
        }

        public static void Warning(string message)
        {
            Logger.Log(message, MessageSeverity.Warning);
        }

        // Message array
        public static void Log(string[] messages, MessageSeverity severity)
        {
            foreach (var logger in _loggers)
            {
                logger.Log(messages, severity);
            }
        }

        public static void Info(string[] messages)
        {
            Logger.Log(messages, MessageSeverity.Info);
        }

        public static void Error(string[] messages)
        {
            Logger.Log(messages, MessageSeverity.Error);
        }

        public static void Warning(string[] messages)
        {
            Logger.Log(messages, MessageSeverity.Warning);
        }
    }
}
