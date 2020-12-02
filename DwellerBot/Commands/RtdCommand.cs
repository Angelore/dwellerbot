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
        public static string CommandName = "/rtd";
        private readonly Random _rng;

        public RtdCommand(TelegramBotClient bot):base(bot)
        {
            _rng = new Random();
        }

        public override async Task HandleMessageAsync(Message message, Dictionary<string, string> parsedMessage)
        {
            await Bot.SendTextMessageAsync(
                chatId: message.Chat.Id,
                text: "Choose the number of die sides 🎲",
                parseMode: ParseMode.Markdown,
                replyToMessageId: message.MessageId,
                replyMarkup: GetDiceSidesKeyboardMarkup());
        }

        public override async Task HandleCallbackQueryAsync(CallbackQuery callbackQuery)
        {
            var step = callbackQuery.Data.Split(";")[1];

            if (step.Equals("s"))
            {
                await Bot.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: "Choose the number of dice 🔄",
                    parseMode: ParseMode.Markdown,
                    replyMarkup: GetDiceNumberKeyboardMarkup(callbackQuery.Data.Split(";")[2]));
            }
            else if (step.Equals("n"))
            {
                var diceNumber = int.Parse(callbackQuery.Data.Split(";")[2]);
                var dieSides = int.Parse(callbackQuery.Data.Split(";")[3]);

                var sb = new StringBuilder();
                sb.AppendLine(callbackQuery.From.FirstName + " rolled the dice.");
                for (var index = 0; index < diceNumber; index++)
                {
                    sb.AppendLine("Dice " + (index + 1) + ": " + (_rng.Next(dieSides) + 1));
                }

                await Bot.EditMessageTextAsync(
                    chatId: callbackQuery.Message.Chat.Id,
                    messageId: callbackQuery.Message.MessageId,
                    text: sb.ToString(),
                    parseMode: ParseMode.Markdown);
            }
        }

        private InlineKeyboardMarkup GetDiceSidesKeyboardMarkup()
        {
            return new InlineKeyboardMarkup(new[]
                {
                new [] {
                    InlineKeyboardButton.WithCallbackData("4", $"{CommandName};s;4"),
                    InlineKeyboardButton.WithCallbackData("6", $"{CommandName};s;6"),
                    InlineKeyboardButton.WithCallbackData("20", $"{CommandName};s;20"),
                    InlineKeyboardButton.WithCallbackData("100", $"{CommandName};s;100"),
                }
            });
        }

        private InlineKeyboardMarkup GetDiceNumberKeyboardMarkup(string dieSides)
        {
            return new InlineKeyboardMarkup(new[]
                {
                new [] {
                    InlineKeyboardButton.WithCallbackData("1", $"{CommandName};n;1;{dieSides}"),
                    InlineKeyboardButton.WithCallbackData("2", $"{CommandName};n;2;{dieSides}"),
                    InlineKeyboardButton.WithCallbackData("3", $"{CommandName};n;3;{dieSides}"),
                    InlineKeyboardButton.WithCallbackData("4", $"{CommandName};n;4;{dieSides}"),
                    InlineKeyboardButton.WithCallbackData("5", $"{CommandName};n;5;{dieSides}"),
                    InlineKeyboardButton.WithCallbackData("6", $"{CommandName};n;6;{dieSides}"),
                }
            });
        }
    }
}
