using System.IO;
using System.Xml.Serialization;
using System.Reflection;
using Serilog;

namespace DwellerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var baseDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var logFileTempalate = System.IO.Path.Combine(baseDirectory, "Log-{{Date}}.txt");
            
            Log.Logger = new LoggerConfiguration()
                             .MinimumLevel.Debug()
                             .WriteTo.Console()
                             //.WriteTo.RollingFile(logFileTempalate)
                             .CreateLogger();

            var resourceName = @"Config\Settings.xml";
            Settings settings;
            
            using (FileStream stream = new FileStream(resourceName, FileMode.Open, FileAccess.Read))
            using (StreamReader reader = new StreamReader(stream))
            {
                var deserializer = new XmlSerializer(typeof(Settings));
                settings = deserializer.Deserialize(reader) as Settings;
            }

            var dwellerBot = new DwellerBot(settings);
            dwellerBot.Run().Wait();
        }
    }
}
