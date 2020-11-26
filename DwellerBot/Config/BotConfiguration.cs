namespace DwellerBot.Config
{
    public class BotConfiguration
    {
        public string BotName { get; set; }
        public string BotKey { get; set; }
        public BotOwner Owner { get; set; }
        public ServiceConfiguration ServiceConfig { get; set; }
        public ReactionCommandConfiguration ReactionCommandConfig { get; set; }
        public BaseCommandConfiguration FeatureRequestConfig { get; set; }
        public BaseCommandConfiguration AskMeCommandConfig { get; set; }
    }

    public class BotOwner
    {
        public string OwnerName { get; set; }
        public int OwnerId { get; set; }
    }

    public class ServiceConfiguration
    {
        public string OpenWeatherKey { get; set; }
    }

    public class ReactionCommandConfiguration: BaseCommandConfiguration
    {
        public string[] FolderPaths { get; set; }
    }

    public class BaseCommandConfiguration
    {
        public string ConfigPath { get; set; }
    }
}
