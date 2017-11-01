using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Web;

namespace DwellerBot.Commands
{
    class BashimCommand:CommandBase
    {
        private readonly string QuoteRequestUrl = "http://bash.im/forweb/?u";
        private readonly string QuoteTagId = "b_q_t";
        private readonly string RatingTagId = "b_q_h";
        private readonly string QuoteLineBreak = "<br>";

        public BashimCommand(TelegramBotClient bot):base(bot)
        { }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            var response = await GetHtml();
            string responseString;
            using (var tr = new StreamReader(response))
            {
                responseString = tr.ReadToEnd();
            }

            var startIndex = responseString.IndexOf("+=") + 2;
            var endIndex = responseString.IndexOf(";\ndocument");
            var tempStr = responseString.Substring(startIndex, endIndex - startIndex);
            var partsList = tempStr.Split('+').Select(x => x.Trim()).ToList();
            tempStr = "";
            // remove ' symbols
            partsList.ForEach(x => tempStr += x.Substring(1, x.Length - 2));

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(tempStr);
            var quote = document.GetElementbyId(QuoteTagId);
            var rating = document.GetElementbyId(RatingTagId);
            
            var result = rating.InnerText + Environment.NewLine + quote.InnerHtml.Replace(QuoteLineBreak, Environment.NewLine);
            
            await Bot.SendTextMessageAsync(update.Message.Chat.Id, HttpUtility.HtmlDecode(result), Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, update.Message.MessageId);
        }

        public async Task<Stream> GetHtml ()
        {
            var hc = new HttpClient();
            return await hc.GetStreamAsync(QuoteRequestUrl);
        }
    }
}
