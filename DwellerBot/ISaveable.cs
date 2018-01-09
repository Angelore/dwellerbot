namespace DwellerBot
{
    public interface ISaveable
    {
        void SaveState();

        void LoadState();
    }
}
