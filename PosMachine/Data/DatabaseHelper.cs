using System;
using System.Data.SQLite;
using System.IO;
using POSMachine.Models;

namespace POSMachine.Data
{
    public class DatabaseHelper
    {
        private static readonly string _dbPath = "posdb.sqlite";
        private static readonly string _connectionString = $"Data Source={_dbPath};Version=3;";

        // Other methods remain the same, but ensure all connections use using statements

        // Example improved method:
        public static User AuthenticateUser(string username, string password)
        {
            using (var connection = new SQLiteConnection(_connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM Users WHERE Username = @Username AND Password = @Password;";

                using (var command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User
                            {
                                Id = Convert.ToInt32(reader["Id"]),
                                Username = reader["Username"].ToString(),
                                Password = reader["Password"].ToString(),
                                Role = (UserRole)Convert.ToInt32(reader["Role"])
                            };
                        }
                    }
                }
            }

            return null;
        }
    }
}