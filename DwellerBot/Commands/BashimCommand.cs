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

namespace DwellerBot.Commands
{
    class BashimCommand:CommandBase
    {
        private readonly string QuoteRequestUrl = "http://bash.im/forweb/?u";
        private readonly string QuoteTagId = "b_q_t";
        private readonly string QuoteLineBreak = "<' + 'br>"; //hack
        private readonly string QuoteStartTag = "<' + 'div id=\"b_q_t\" style=\"padding: 1em 0;\">";
        private readonly string QuoteEndTag = "<' + '/div>";

        public BashimCommand(Api bot):base(bot)
        { }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            var response = await GetHtml();
            string responseString;
            using (var tr = new StreamReader(response))
            {
                responseString = tr.ReadToEnd();
            }

            //HtmlDocument document = new HtmlDocument();
            //document.LoadHtml(responseString);

            // temporary code, the return value is supposed to be evaluated by js, maybe will add this later

            // get rating
            var ratingRx = new Regex(@"\[\s?\d+\s?\]");
            var rating = ratingRx.Match(responseString);

            // get quote
            var quoteStart = responseString.IndexOf(QuoteStartTag) + QuoteStartTag.Length;
            var quoteEnd = responseString.IndexOf(QuoteEndTag);
            var quote = responseString.Substring(quoteStart, quoteEnd - quoteStart).Replace(QuoteLineBreak, Environment.NewLine);

            var result = rating + Environment.NewLine + quote;

            await Bot.SendTextMessage(update.Message.Chat.Id, result, false, update.Message.MessageId);
        }

        public async Task<Stream> GetHtml ()
        {
            var hc = new HttpClient();
            return await hc.GetStreamAsync(QuoteRequestUrl);
        }
    }
}
