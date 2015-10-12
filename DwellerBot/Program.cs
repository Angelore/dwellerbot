using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using Telegram.Bot;

namespace DwellerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = @"DwellerBot.Resources.Settings.xml";
            Settings settings;
            
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
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
