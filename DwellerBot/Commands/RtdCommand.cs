using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class RtdCommand : CommandBase
    {
        private Random _rng;

        public RtdCommand(TelegramBotClient bot):base(bot)
        {
            _rng = new Random();
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            var rtdRegex = new Regex(@"^(\d*)d(4|6|20|100)");

            if (parsedMessage.ContainsKey("message"))
            {
                if (rtdRegex.IsMatch(parsedMessage["message"]))
                {
                    var rtdMatch = rtdRegex.Match(parsedMessage["message"]);
                    var numberOfDice = rtdMatch.Groups[1].Value == "" ? 1 : int.Parse(rtdMatch.Groups[1].Value);
                    var diceEdges = int.Parse(rtdMatch.Groups[2].Value);
                    if ((numberOfDice > 0 && numberOfDice <= 6) &&
                        (diceEdges == 4 || diceEdges == 6 || diceEdges == 20 || diceEdges == 100))
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine(update.Message.From.FirstName + " rolled the dice.");
                        for (var index = 0; index < numberOfDice; index++)
                        {
                            sb.AppendLine("Dice " + (index + 1) + ": " + (_rng.Next(diceEdges) + 1));
                        }
                        await Bot.SendTextMessageAsync(update.Message.Chat.Id, sb.ToString(), Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, update.Message.MessageId);
                        return;
                    }
                }
            }

            await Bot.SendTextMessageAsync(update.Message.Chat.Id, "Format: [number(?1-6)]d[number(4|6|20|100)].", Telegram.Bot.Types.Enums.ParseMode.Markdown, false, false, update.Message.MessageId);
        }
    }
}
