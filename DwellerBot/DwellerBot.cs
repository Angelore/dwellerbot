using DwellerBot.Commands;
using DwellerBot.Config;
using DwellerBot.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;

namespace DwellerBot
{
    public class DwellerBot
    {
        private static string BotName { get; set; }
        private static string OwnerUsername { get; set; }
        private static int OwnerId { get; set; }

        private readonly TelegramBotClient _bot;

        public DateTime LaunchTime { get; set; }
        public int CommandsProcessed { get; set; }
        public int ErrorCount { get; set; }

        internal CancellationTokenSource CancellationTokenSource { get; }
        internal CommandService CommandService { get; }

        public DwellerBot(BotConfiguration settings)
        {
            BotName = settings.BotName;
            OwnerUsername = settings.Owner.OwnerName;
            OwnerId = settings.Owner.OwnerId;

            CommandService = new CommandService(BotName);
            CancellationTokenSource = new CancellationTokenSource();
            _bot = new TelegramBotClient(settings.BotKey);

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
                {@"/savestate", new SaveStateCommand(_bot, this)},
                {@"/shutdown", new ShutdownCommand(_bot, this)},
                {@"/changelog", new ChangelogCommand(_bot)}
            });

            CommandService.LoadCommandStates();
        }

        public async Task Run()
        {
            var me = await _bot.GetMeAsync();
            var cts = new CancellationTokenSource();

            Log.Logger.Information("{0} has started." + Environment.NewLine, me.Username);

            _bot.StartReceiving(
                new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                CancellationTokenSource.Token
            );

            try
            {
                await Task.Delay(-1, CancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                Log.Logger.Information("Shutting down after token cancellation.");
            }
            finally
            {
                CancellationTokenSource.Dispose();
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            var handler = CommandService.HandleUpdate(update);

            try
            {
                await handler;
            }
            catch (Exception exception)
            {
                await HandleErrorAsync(botClient, exception, cancellationToken);
            }
        }

        public async Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Log.Logger.Error("An error has occured while receiving updates. Error message: {0}", errorMessage);
            ErrorCount++;
        }

        public static bool IsUserOwner(User user)
        {
            if (user.Id == OwnerId && user.Username.Equals(OwnerUsername))
                return true;

            return false;
        }
    }
}
