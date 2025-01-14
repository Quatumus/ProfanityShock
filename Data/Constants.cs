namespace ProfanityShock.Data
{
    public static class Constants
    {
        public const string DatabaseFilename = "ProfanityShock.db3";

        public static string DatabasePath =>
            $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";
    }
}
