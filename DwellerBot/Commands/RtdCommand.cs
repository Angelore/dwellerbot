using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace DwellerBot.Commands
{
    class RtdCommand : CommandBase
    {
        public const string CommandName = "/rtd";
        private Random _rng;

        public RtdCommand(TelegramBotClient bot):base(bot)
        {
            _rng = new Random();
        }

        public override async Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
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
                        sb.AppendLine(message.From.FirstName + " rolled the dice.");
                        for (var index = 0; index < numberOfDice; index++)
                        {
                            sb.AppendLine("Dice " + (index + 1) + ": " + (_rng.Next(diceEdges) + 1));
                        }
                        await Bot.SendTextMessageAsync(message.Chat.Id, sb.ToString(), ParseMode.Markdown, false, false, message.MessageId);
                        return;
                    }
                }
            }

            await Bot.SendTextMessageAsync(message.Chat.Id,
                "Format: Number of dice (*1-6*) + *d* + Number of sides (*4|6|20|100*)." + Environment.NewLine +
                "Example: *2d6*", ParseMode.Markdown, false, false, message.MessageId);
        }

        private InlineKeyboardMarkup GetDiceSidesKeyboardMarkup()
        {
            return new InlineKeyboardMarkup(new[]
                {
                new [] {
                    InlineKeyboardButton.WithCallbackData("4", $"{CommandName};4"),
                    InlineKeyboardButton.WithCallbackData("6", $"{CommandName};6"),
                    InlineKeyboardButton.WithCallbackData("20", $"{CommandName};20"),
                    InlineKeyboardButton.WithCallbackData("100", $"{CommandName};100"),
                }
            });
        }

        private InlineKeyboardMarkup GetDiceNumberKeyboardMarkup()
        {
            return new InlineKeyboardMarkup(new[]
                {
                new [] {
                    InlineKeyboardButton.WithCallbackData("1", $"{CommandName};1"),
                    InlineKeyboardButton.WithCallbackData("2", $"{CommandName};2"),
                    InlineKeyboardButton.WithCallbackData("3", $"{CommandName};3"),
                    InlineKeyboardButton.WithCallbackData("4", $"{CommandName};4"),
                    InlineKeyboardButton.WithCallbackData("5", $"{CommandName};5"),
                    InlineKeyboardButton.WithCallbackData("6", $"{CommandName};6"),
                }
            });
        }
    }
}
