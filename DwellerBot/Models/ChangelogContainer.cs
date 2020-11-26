namespace DwellerBot.Models
{
    public class ChangelogContainer
    {
        public AppVersion[] Versions { get; set; }
    }

    public class AppVersion
    {
        public string VersionNumber { get; set; }
        public string ReleaseDate { get; set; }
        public string Description { get; set; }
        public string[] Changes { get; set; }
    }
}