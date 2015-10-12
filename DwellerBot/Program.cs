using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Telegram.Bot;

namespace DwellerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            XDocument xdoc;
            Dictionary<string, string> apiKeys = new Dictionary<string, string>();
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = @"DwellerBot.Resources.keys.xml";
            
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                string text = reader.ReadToEnd();
                xdoc = XDocument.Parse(text);
                foreach (var element in xdoc.Root.Elements("key"))
                {
                    apiKeys.Add(element.Attribute("name").Value, element.Attribute("value").Value);
                }
            }

            var dwellerBot = new DwellerBot(apiKeys);
            dwellerBot.Run().Wait();
        }
    }
}
