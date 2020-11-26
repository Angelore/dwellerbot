using DwellerBot.Commands;
using DwellerBot.Config;
using DwellerBot.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace DwellerBot
{
    public class DwellerBot
    {
        private static string BotName;
        private static string OwnerUsername;
        private static int OwnerId;

        private readonly TelegramBotClient _bot;

        private readonly Random _rng;

        internal int Offset;
        internal DateTime LaunchTime;
        internal int CommandsProcessed;
        internal int ErrorCount;
        internal bool IsOnline = true;

        internal CommandService CommandService { get; }

        public DwellerBot(BotConfiguration settings)
        {
            BotName = settings.BotName;
            OwnerUsername = settings.Owner.OwnerName;
            OwnerId = settings.Owner.OwnerId;

            // move to container
            CommandService = new CommandService(BotName);

            _rng = new Random();

            // Get bot api token
            _bot = new TelegramBotClient(settings.BotKey);

            Offset = 0;
            CommandsProcessed = 0;
            ErrorCount = 0;
            LaunchTime = DateTime.Now;

            CommandService.RegisterCommands(new Dictionary<string, ICommand>
            {
                {@"/debug", new DebugCommand(_bot, this)},
                {@"/rate", new RateNbrbCommand(_bot)},
                {@"/askme", new AskMeCommand(_bot, settings.AskMeCommandConfig.ConfigPath) },
                {@"/weather", new WeatherCommand(_bot, settings.ServiceConfig.OpenWeatherKey) },
                {
                    @"/reaction",
                    new ReactionCommand(
                        _bot,
                        settings.ReactionCommandConfig.FolderPaths.ToList(),
                        settings.ReactionCommandConfig.ConfigPath
                        )
                },
                {@"/rtd", new RtdCommand(_bot)},
                {@"/featurerequest", new FeatureRequestCommand(_bot, settings.FeatureRequestConfig.ConfigPath) },
                {@"/bash", new BashimCommand(_bot)},
                {@"/savestate", new SaveStateCommand(_bot, this)},
                {@"/shutdown", new ShutdownCommand(_bot, this)},
                {@"/changelog", new ChangelogCommand(_bot)}
            });

            CommandService.LoadCommandStates();
        }

        public async Task Run()
        {
            var me = await _bot.GetMeAsync();

            Log.Logger.Information("{0} has started." + Environment.NewLine, me.Username);

            while (IsOnline)
            {
                Update[] updates = new Update[0];
                try
                {
                    updates = await _bot.GetUpdatesAsync(Offset);
                }
                catch (Exception ex)
                {
                    Log.Logger.Error("An error has occured while receiving updates. Error message: {0}", ex.Message);
                    ErrorCount++;
                }

                List<Task<bool>> tasks = new List<Task<bool>>();
                try
                {
                    foreach (var update in updates)
                    {
                        var updateTask = CommandService.HandleUpdate(update);
                        tasks.Add(updateTask);

                        Offset = update.Id + 1;
                    }
                    Task.WaitAll(tasks.ToArray());
                    CommandsProcessed += tasks.Count(t => t.Result);
                }
                catch (Exception ex)
                {
                    // for debug
                    throw;
                }

                await Task.Delay(1000);
            }
        }

        public static bool IsUserOwner(User user)
        {
            if (user.Id == OwnerId && user.Username.Equals(OwnerUsername))
                return true;

            return false;
        }
    }
}
