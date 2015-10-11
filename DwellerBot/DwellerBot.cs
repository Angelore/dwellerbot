using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace DwellerBot
{
    public class DwellerBot
    {
        private const string _botName = @"@DwellerBot";
        private const string queryUrl = @"https://query.yahooapis.com/v1/public/yql?q=select+*+from+yahoo.finance.xchange+where+pair+=+%22USDBYR,EURBYR,RUBBYR%22&format=json&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback=";
        private FileStream _file;
        private int _offset = 0;
        private DirectoryInfo _dir;
        private List<FileInfo> _reactionImages;
        private int _reactionImagesCount;
        private Random _rng;

        public DwellerBot(string offsetFilePath = @"output.txt")
        {
            _file = new FileStream(@"offset.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _dir = new DirectoryInfo(@"D:\Pictures\Internets\Reaction_images");
            _reactionImages = _dir.EnumerateFiles().ToList();
            _reactionImagesCount = _reactionImages.Count();
            _rng = new Random();

            //if (_file.Length == 0)
            //{
            //    _offset = 0;

            //    using (var sw = new StreamWriter(_file))
            //    {
            //        sw.WriteLine(0);
            //    }
            //}
            //else
            //{
            //    using (var sr = new StreamReader(_file))
            //    {
            //        _offset = int.Parse(sr.ReadLine()) + 1;
            //    }
            //}
        }
        
        public async Task Run()
        {
            var Bot = new Api("130434822:AAEyREsiaeWIBhxPiDuKyZyheX-eHq0YGIU");

            var me = await Bot.GetMe();

            Console.WriteLine("{0} is online and fully functional. Beach." + Environment.NewLine, me.Username);

            while (true)
            {
                var updates = await Bot.GetUpdates(_offset);

                foreach (var update in updates)
                {
                    if (update.Message.Text != null)
                    {
                        Console.WriteLine("A message in chat {0} from user {1}: {2}", update.Message.Chat.Id, update.Message.From.Username, update.Message.Text);

                        if (update.Message.Text.IndexOf(@"/rate") == 0)
                        {
                            var responseStream = new StreamReader(await GetCurrencyRates());
                            var cc = JsonConvert.DeserializeObject<CurrencyContainer>(responseStream.ReadToEnd());
                            var sb = new StringBuilder();
                            sb.Append("Курсы валют на ");
                            sb.AppendLine(cc.query.created.ToShortDateString());
                            sb.AppendLine();
                            foreach (var currency in cc.query.results.rate)
                            {
                                sb.AppendLine(currency.Name.Substring(0, 3) + ": " + currency.Rate.Substring(0, currency.Rate.Length - 2));
                            }
                            await Bot.SendTextMessage(update.Message.Chat.Id, sb.ToString());
                        }
                        if (update.Message.Text.IndexOf(@"/stason") == 0)
                        {
                            var ind = _rng.Next(0, _reactionImagesCount);
                            using (var fs = new FileStream(_reactionImages[ind].FullName, FileMode.Open, FileAccess.Read))
                            {
                                try
                                {
                                    await Bot.SendPhoto(update.Message.Chat.Id, new Telegram.Bot.Types.FileToSend { Filename = _reactionImages[ind].Name, Content = fs });
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("An error has occured during file sending! Message: {0}", ex.Message);
                                }
                            }
                            
                            // await Bot.SendTextMessage(update.Message.Chat.Id, @"D:\Pictures\Internets\Reaction_images\1386450293522.jpg");
                        }
                        if (update.Message.Text.IndexOf(@"/askstason") == 0 && !update.Message.Text.Equals(@"/askstason") && !update.Message.Text.Equals(@"/askstason" + _botName))
                        {
                            var rng = new Random();
                            var ind = rng.Next(0, _responses.Length);
                            await Bot.SendTextMessage(update.Message.Chat.Id, _responses[ind], false, update.Message.MessageId);
                        }

                    }

                    _offset = update.Id + 1;
                }

                await Task.Delay(1000);
            }
        }

        public async Task<Stream> GetCurrencyRates()
        {
            var hc = new HttpClient();
            return await hc.GetStreamAsync(queryUrl);
        }

        ~DwellerBot()
        {
            _file.Seek(0, SeekOrigin.Begin);
            using (var sw = new StreamWriter(_file))
            {
                sw.WriteLine(_offset);
            }
            _file.Close();
        }

        private string[] _responses = new string[7] { "Да", "Нет", "Возможно", "Маловероятно", "Конечно", "Спросите позже", "Спроси у Пашана" };
    }
}
