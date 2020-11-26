using DwellerBot.Config;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace DwellerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var logFileTempalate = Path.Combine(baseDirectory, "Log-{{Date}}.txt");
            
            Log.Logger = new LoggerConfiguration()
                             .MinimumLevel.Debug()
                             .WriteTo.Console()
                             //.WriteTo.RollingFile(logFileTempalate)
                             .CreateLogger();

            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(baseDirectory, "Config"))
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            var settings = configuration.Get<BotConfiguration>();

            var dwellerBot = new DwellerBot(settings);
            dwellerBot.Run().Wait();
        }
    }
}
