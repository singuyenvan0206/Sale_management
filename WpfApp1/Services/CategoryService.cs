using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FashionStore.Services
{
    public static class CategoryService
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static List<(int Id, string Name, decimal TaxPercent)> GetAllCategories()
        {
            var categories = new List<(int, string, decimal)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string selectCmd = "SELECT Id, Name, TaxPercent FROM Categories ORDER BY Name;";
            using var cmd = new MySqlCommand(selectCmd, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                categories.Add((reader.GetInt32(0), reader.GetString(1), reader.GetDecimal(2)));
            }
            return categories;
        }

        public static bool AddCategory(string name, decimal taxPercent)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string insertCmd = "INSERT INTO Categories (Name, TaxPercent) VALUES (@name, @tax);";
            using var cmd = new MySqlCommand(insertCmd, connection);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@tax", taxPercent);
            try { return cmd.ExecuteNonQuery() > 0; }
            catch { return false; }
        }

        public static bool UpdateCategory(int id, string name, decimal taxPercent)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string updateCmd = "UPDATE Categories SET Name=@name, TaxPercent=@tax WHERE Id=@id;";
            using var cmd = new MySqlCommand(updateCmd, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@tax", taxPercent);
            try { return cmd.ExecuteNonQuery() > 0; }
            catch { return false; }
        }

        public static bool DeleteCategory(int id)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string checkCmd = "SELECT COUNT(*) FROM Products WHERE CategoryId=@id;";
            using var check = new MySqlCommand(checkCmd, connection);
            check.Parameters.AddWithValue("@id", id);
            long count = (long)check.ExecuteScalar();

            if (count > 0) return false;

            string deleteCmd = "DELETE FROM Categories WHERE Id=@id;";
            using var cmd = new MySqlCommand(deleteCmd, connection);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DeleteAllCategories()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();
            try
            {
                try
                {
                    string checkCmd = "SELECT COUNT(*) FROM Products WHERE CategoryId > 0;";
                    using var check = new MySqlCommand(checkCmd, connection, tx);
                    long count = (long)check.ExecuteScalar();
                    if (count > 0) return false;
                }
                catch { }

                using var disableFK = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", connection, tx);
                disableFK.ExecuteNonQuery();
                using var truncateCmd = new MySqlCommand("TRUNCATE TABLE Categories;", connection, tx);
                truncateCmd.ExecuteNonQuery();
                using var enableFK = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", connection, tx);
                enableFK.ExecuteNonQuery();
                tx.Commit();
                return true;
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                return false;
            }
        }

        public static int EnsureCategory(string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name)) return 0;
                name = name.Trim();
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();
                using (var getCmd = new MySqlCommand("SELECT Id FROM Categories WHERE Name=@n;", connection))
                {
                    getCmd.Parameters.AddWithValue("@n", name);
                    var idObj = getCmd.ExecuteScalar();
                    if (idObj != null) return Convert.ToInt32(idObj);
                }

                using (var insCmd = new MySqlCommand("INSERT INTO Categories (Name) VALUES (@n);", connection))
                {
                    insCmd.Parameters.AddWithValue("@n", name);
                    if (insCmd.ExecuteNonQuery() > 0)
                    {
                        using var lastIdCmd = new MySqlCommand("SELECT LAST_INSERT_ID();", connection);
                        return Convert.ToInt32(lastIdCmd.ExecuteScalar());
                    }
                }
                return 0;
            }
            catch { return 0; }
        }
    }
}

