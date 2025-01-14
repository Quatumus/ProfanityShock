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
                ID INTEGER PRIMARY KEY,
                Email TEXT UNIQUE,
                Password TEXT,
                Token TEXT NOT NULL,
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
            selectCmd.CommandText = "SELECT * FROM Account WHERE ID = 1";

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new AppConfig
                {
                    Email = reader.GetString(1),
                    Password = reader.GetString(2),
                    Token = reader.GetString(3),
                    Backend = new Uri(reader.GetString(4))
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
            INSERT OR REPLACE INTO Account (ID, Email, Password, Token, Backend)
            VALUES (@ID, @Email, @Password, @Token, @Backend);";

            saveCmd.Parameters.AddWithValue("@ID", 1);
            saveCmd.Parameters.AddWithValue("@Email", item.Email);
            saveCmd.Parameters.AddWithValue("@Password", item.Password);
            saveCmd.Parameters.AddWithValue("@Token", item.Token);
            saveCmd.Parameters.AddWithValue("@Backend", item.Backend?.ToString());

            return await saveCmd.ExecuteNonQueryAsync();
        }

        public static async Task<int> DeleteItemAsync(string item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Account WHERE Word = @Word";
            deleteCmd.Parameters.AddWithValue("@Word", item);

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
