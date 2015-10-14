using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class FeatureRequestCommand: CommandBase
    {
        private Dictionary<int, DateTime> _nextRequestAllowedForUser;
        private string _featureRequestsFilePath;

        public FeatureRequestCommand(Api bot, string featureRequestsFilePath):base(bot)
        {
            _featureRequestsFilePath = featureRequestsFilePath;
            _nextRequestAllowedForUser = new Dictionary<int, DateTime>();
        }

        public override async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            if (parsedMessage.ContainsKey("message"))
            {
                if (!IsUserBanned(update.Message.From.Id))
                {

                }
            }

            await _bot.SendTextMessage(update.Message.Chat.Id, "You have to describe your request :)", false, update.Message.MessageId);
        }

        private bool IsUserBanned(int id)
        {
            return false; // In case this will be needed.
        }
    }
}
