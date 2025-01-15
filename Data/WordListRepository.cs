using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using ProfanityShock.Config;
using System.Diagnostics;

namespace ProfanityShock.Data
{
    internal class WordListRepository
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
                CREATE TABLE IF NOT EXISTS Wordlist (
                Word TEXT PRIMARY KEY
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

        public static async Task <List<string>> ListAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Wordlist";
            var words = new List<string>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                words.Add(reader.GetString(0));
            }

            return words;

        }

        public static async Task<int> UpdateListAsync(List<string> items)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var transaction = await connection.BeginTransactionAsync();
            try
            {
                var deleteCmd = connection.CreateCommand();
                deleteCmd.CommandText = "DELETE * FROM Wordlist";
                await deleteCmd.ExecuteNonQueryAsync();

                foreach (var item in items)
                {
                    var saveCmd = connection.CreateCommand();
                    saveCmd.CommandText = "INSERT INTO Wordlist (Word) VALUES (@Word)";
                    saveCmd.Parameters.AddWithValue("@Word", item);
                    await saveCmd.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return items.Count;
        }


        public static async Task<int> SaveItemAsync(string item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            saveCmd.CommandText = @"
            INSERT OR REPLACE INTO Wordlist (Word)
            VALUES (@Word);";

            saveCmd.Parameters.AddWithValue("@Word", item);

            return await saveCmd.ExecuteNonQueryAsync();
        }

        public static async Task<int> DeleteItemAsync(string item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Wordlist WHERE Word = @Word";
            deleteCmd.Parameters.AddWithValue("@Word", item);

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        public static async Task DropTableAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var dropTableCmd = connection.CreateCommand();
            dropTableCmd.CommandText = "DROP TABLE IF EXISTS Wordlist";

            await dropTableCmd.ExecuteNonQueryAsync();
            _hasBeenInitialized = false;
        }
    }
}
