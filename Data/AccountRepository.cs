using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using ProfanityShock.Config;
using System.Diagnostics;

namespace ProfanityShock.Data
{
    static class AccountRepository
    {
        private static bool _hasBeenInitialized = false;

        private static async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @" 
                CREATE TABLE IF NOT EXISTS Account (
                Token TEXT PRIMARY KEY,
                Password TEXT,
                Email TEXT,
                Backend TEXT NOT NULL
                );";
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                throw;
            }

            _hasBeenInitialized = true;
        }

        public static async Task<AppConfig?> LoadAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT 1 FROM Account";

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AppConfig
                {
                    Token = reader.GetString(0),
                    Password = reader.GetString(1),
                    Email = reader.GetString(2),
                    Backend = new Uri(reader.GetString(3))
                };
            }
            else
            {
                return null;
            }

        }

        public static async Task<int> SaveItemAsync(AppConfig item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            saveCmd.CommandText = @"
            INSERT OR REPLACE INTO Account (Token, Password, Email, Backend)
            VALUES (@Token, @Password, @Email, @Backend);";

            saveCmd.Parameters.AddWithValue("@Token", item.Token);
            saveCmd.Parameters.AddWithValue("@Password", item.Password);
            saveCmd.Parameters.AddWithValue("@Email", item.Email);
            saveCmd.Parameters.AddWithValue("@Backend", item.Backend?.ToString());

            return await saveCmd.ExecuteNonQueryAsync();
        }

        public static async Task<int> DeleteItemAsync(string item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Account WHERE Token = @Token";
            deleteCmd.Parameters.AddWithValue("@Token", item);

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        public static async Task DropTableAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var dropTableCmd = connection.CreateCommand();
            dropTableCmd.CommandText = "DROP TABLE IF EXISTS Account";

            await dropTableCmd.ExecuteNonQueryAsync();
            _hasBeenInitialized = false;
        }
    }
}
