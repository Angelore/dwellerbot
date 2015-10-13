using System.IO;
using System.Reflection;
using System.Xml.Serialization;
using Serilog;

namespace DwellerBot
{
    class Program
    {
        private static void Main(string[] args)
        {
	        var baseDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	        var logFileTempalate = Path.Combine(baseDirectory, "Log-{{Date}}.txt");

	        Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.ColoredConsole()
				.WriteTo.RollingFile(logFileTempalate)
                .CreateLogger();

            var resourceName = @"Resources\Settings.xml";
            Settings settings;

            using (FileStream stream = new FileStream(resourceName, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(stream))
            {
                var deserializer = new XmlSerializer(typeof (Settings));
                settings = deserializer.Deserialize(reader) as Settings;
            }

            var dwellerBot = new DwellerBot(settings);
            dwellerBot.Run().Wait();
        }
    }
}
