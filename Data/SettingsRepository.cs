using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using ProfanityShock.Config;
using System.Diagnostics;


namespace ProfanityShock.Data
{
    internal class SettingsRepository
    {
        private bool _hasBeenInitialized = false;
        
        private async Task Init()
        {
            if (_hasBeenInitialized)
                return;

            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            try
            {
                var createTableCmd = connection.CreateCommand();
                createTableCmd.CommandText = @" 
                CREATE TABLE IF NOT EXISTS Shocker (
                ID TEXT PRIMARY KEY,
                Name TEXT NOT NULL,
                Intensity INTEGER NOT NULL,
                Duration INTEGER NOT NULL,
                Delay INTEGER NOT NULL,
                Warning INTEGER NOT NULL,
                Controltype INTEGER NOT NULL
            );"; // warning 1 = vibrate, warning 2 = sound
                await createTableCmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                throw;
            }

            _hasBeenInitialized = true;
        }

        public async Task<List<Shocker>> ListAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT * FROM Shocker";
            var categories = new List<Shocker>();

            await using var reader = await selectCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                categories.Add(new Shocker
                {
                    ID = reader.GetString(0),
                    Name = reader.GetString(1),
                    Intensity = reader.GetInt32(2),
                    Duration = reader.GetInt32(3),
                    Delay = reader.GetInt32(4),
                    Warning = reader.GetInt32(5),
                    Controltype = (ControlType)reader.GetInt32(6)
                });
            }

            return categories;

        }

        public async Task<int> SaveItemAsync(Shocker item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var saveCmd = connection.CreateCommand();
            saveCmd.CommandText = @"
            INSERT OR REPLACE INTO Shocker (ID, Name, Intensity, Duration, Delay, Warning, Controltype)
            VALUES (@ID, @Name, @Intensity, @Duration, @Delay, @Warning, @Controltype);";

            saveCmd.Parameters.AddWithValue("@ID", item.ID);
            saveCmd.Parameters.AddWithValue("@Name", item.Name);
            saveCmd.Parameters.AddWithValue("@Intensity", item.Intensity);
            saveCmd.Parameters.AddWithValue("@Duration", item.Duration);
            saveCmd.Parameters.AddWithValue("@Delay", item.Delay);
            saveCmd.Parameters.AddWithValue("@Warning", item.Warning);
            saveCmd.Parameters.AddWithValue("@Controltype", item.Controltype);

            return await saveCmd.ExecuteNonQueryAsync();
        }

        public async Task<int> DeleteItemAsync(Shocker item)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Shocker WHERE ID = @ID";
            deleteCmd.Parameters.AddWithValue("@ID", item.ID);

            return await deleteCmd.ExecuteNonQueryAsync();
        }

        public async Task<int> SyncItemsAsync(List<Shocker> items)
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = "DELETE FROM Shocker WHERE ID NOT IN (@ids)";
            var ids = string.Join(",", items.Select(i => i.ID).Select(id => $"'{id}'"));
            deleteCmd.Parameters.AddWithValue("@ids", ids);

            await deleteCmd.ExecuteNonQueryAsync();

            var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = @"
            INSERT OR IGNORE INTO Shocker (ID, Name, Intensity, Duration, Delay, Warning, Controltype)
            VALUES (@ID, @Name, @Intensity, @Duration, @Delay, @Warning, @Controltype);";

            foreach (var item in items)
            {
                insertCmd.Parameters.Clear();
                insertCmd.Parameters.AddWithValue("@ID", item.ID);
                insertCmd.Parameters.AddWithValue("@Name", item.Name);
                insertCmd.Parameters.AddWithValue("@Intensity", item.Intensity);
                insertCmd.Parameters.AddWithValue("@Duration", item.Duration);
                insertCmd.Parameters.AddWithValue("@Delay", item.Delay);
                insertCmd.Parameters.AddWithValue("@Warning", item.Warning);
                insertCmd.Parameters.AddWithValue("@Controltype", item.Controltype);

                await insertCmd.ExecuteNonQueryAsync();
            }

            return 1;
        }

        public async Task DropTableAsync()
        {
            await Init();
            await using var connection = new SqliteConnection(Constants.DatabasePath);
            await connection.OpenAsync();

            var dropTableCmd = connection.CreateCommand();
            dropTableCmd.CommandText = "DROP TABLE IF EXISTS Shocker";

            await dropTableCmd.ExecuteNonQueryAsync();
            _hasBeenInitialized = false;
        }
    }
}
