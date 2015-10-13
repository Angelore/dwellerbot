using System.IO;
using System.Xml.Serialization;
using DwellerBot.Utility;

namespace DwellerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Logger.AddLogger(new ConsoleLogger());

            var resourceName = @"Resources\Settings.xml";
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
