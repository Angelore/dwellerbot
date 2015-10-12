using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;

namespace DwellerBot
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var dwellerBot = new DwellerBot())
            {
                dwellerBot.Run().Wait();
            }
        }
    }
}
