using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DwellerBot.Commands
{
    class StasonCommand : ICommand
    {
        public void Execute(Update update)
        {
            throw new NotImplementedException();
        }

        public async Task ExecuteAsync(Update update, Dictionary<string, string> parsedMessage)
        {
            /*var ind = _rng.Next(0, _reactionImagesCount);
            using (var fs = new FileStream(_reactionImages[ind].FullName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    await _bot.SendPhoto(update.Message.Chat.Id, new Telegram.Bot.Types.FileToSend { Filename = _reactionImages[ind].Name, Content = fs });
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error has occured during file sending! Message: {0}", ex.Message);
                }
            }*/

            // await Bot.SendTextMessage(update.Message.Chat.Id, @"D:\Pictures\Internets\Reaction_images\1386450293522.jpg");
        }
    }
}
