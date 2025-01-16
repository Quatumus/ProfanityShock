using Microsoft.Data.Sqlite;
using ProfanityShock.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProfanityShock.Data
{
    internal static class SettingsRepository
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
                CREATE TABLE IF NOT EXISTS Settings (
                Language TEXT PRIMARY KEY
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

        public static async Task<SettingsConfig?> LoadAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Settings";

            await using var reader = await selectCmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new SettingsConfig
                {
                    Language = reader.GetString(0),
                };
            }
            else
            {
                return null;
            }

        }

        public static async Task<int> SaveItemAsync(SettingsConfig item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            // delete old data
            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Settings";
            await deleteCmd.ExecuteNonQueryAsync();

            // save new data
            var saveCmd = connection.CreateCommand();
            saveCmd.CommandText = @"
            INSERT OR REPLACE INTO Settings (Language)
            VALUES (@Language);";

            saveCmd.Parameters.AddWithValue("@Language", item.Language);

            return await saveCmd.ExecuteNonQueryAsync();
        }

        public static async Task<int> DeleteItemAsync(string item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Settings";

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        public static async Task DropTableAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var dropTableCmd = connection.CreateCommand();
            dropTableCmd.CommandText = "DROP TABLE IF EXISTS Settings";

            await dropTableCmd.ExecuteNonQueryAsync();
            _hasBeenInitialized = false;
        }
    }
}
