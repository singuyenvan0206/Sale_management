using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using FashionStore.Core;

namespace FashionStore.Services
{
    public static class CustomerService
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static List<(int Id, string Name, string Phone, string Email, string Address, string CustomerType)> GetAllCustomers()
        {
            var customers = new List<(int, string, string, string, string, string)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string selectCmd = "SELECT Id, Name, Phone, Email, Address, CustomerType FROM Customers ORDER BY Id LIMIT 10000;";
            using var cmd = new MySqlCommand(selectCmd, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                customers.Add((
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.IsDBNull(3) ? "" : reader.GetString(3),
                    reader.IsDBNull(4) ? "" : reader.GetString(4),
                    reader.IsDBNull(5) ? "Regular" : reader.GetString(5)
                ));
            }
            return customers;
        }

        public static (string Tier, int Points) GetCustomerLoyalty(int customerId)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "SELECT IFNULL(CustomerType,'Regular'), IFNULL(Points,0) FROM Customers WHERE Id=@id;";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@id", customerId);
            using var r = cmd.ExecuteReader();
            if (r.Read())
            {
                return (r.IsDBNull(0) ? "Regular" : r.GetString(0), r.IsDBNull(1) ? 0 : r.GetInt32(1));
            }
            return ("Regular", 0);
        }

        public static bool UpdateCustomerLoyalty(int customerId, int newPoints, string newTier)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "UPDATE Customers SET Points=@p, CustomerType=@tier WHERE Id=@id;";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@p", newPoints);
            cmd.Parameters.AddWithValue("@tier", newTier);
            cmd.Parameters.AddWithValue("@id", customerId);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static int GetTotalCustomers()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Customers", connection);
            var val = cmd.ExecuteScalar();
            return Convert.ToInt32(val ?? 0);
        }

        public static bool AddCustomer(string name, string phone, string email, string customerType, string address)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string insertCmd = "INSERT INTO Customers (Name, Phone, Email, CustomerType, Address) VALUES (@name, @phone, @email, @customerType, @address);";
            using var cmd = new MySqlCommand(insertCmd, connection);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@phone", phone);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@customerType", customerType);
            cmd.Parameters.AddWithValue("@address", address);
            try { return cmd.ExecuteNonQuery() > 0; }
            catch { return false; }
        }

        public static bool UpdateCustomer(int id, string name, string phone, string email, string customerType, string address)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string updateCmd = "UPDATE Customers SET Name=@name, Phone=@phone, Email=@email, CustomerType=@customerType, Address=@address WHERE Id=@id;";
            using var cmd = new MySqlCommand(updateCmd, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@phone", phone);
            cmd.Parameters.AddWithValue("@email", email);
            cmd.Parameters.AddWithValue("@customerType", customerType);
            cmd.Parameters.AddWithValue("@address", address);
            try { return cmd.ExecuteNonQuery() > 0; }
            catch { return false; }
        }

        public static bool DeleteCustomer(int id)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            try
            {
                string checkCmd = "SELECT COUNT(*) FROM Invoices WHERE CustomerId=@id;";
                using var check = new MySqlCommand(checkCmd, connection);
                check.Parameters.AddWithValue("@id", id);
                long count = (long)check.ExecuteScalar();
                if (count > 0) return false;
            }
            catch { }

            string deleteCmd = "DELETE FROM Customers WHERE Id=@id;";
            using var cmd = new MySqlCommand(deleteCmd, connection);
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DeleteAllCustomers()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();
            try
            {
                try
                {
                    string checkCmd = "SELECT COUNT(*) FROM Invoices;";
                    using var check = new MySqlCommand(checkCmd, connection, tx);
                    long count = (long)check.ExecuteScalar();
                    if (count > 0) return false;
                }
                catch { }

                using var disableFK = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", connection, tx);
                disableFK.ExecuteNonQuery();
                using var truncateCmd = new MySqlCommand("TRUNCATE TABLE Customers;", connection, tx);
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

        public static int GetOrCreateCustomerId(string name, string phone, string email, string address)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            try
            {
                // Try to find existing customer by phone first
                if (!string.IsNullOrWhiteSpace(phone))
                {
                    string findSql = "SELECT Id FROM Customers WHERE Phone = @phone LIMIT 1";
                    using var findCmd = new MySqlCommand(findSql, connection);
                    findCmd.Parameters.AddWithValue("@phone", phone);
                    var result = findCmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value) return Convert.ToInt32(result);
                }

                // If not found, create new
                string insertSql = @"INSERT INTO Customers (Name, Phone, Email, Address, CustomerType, Points) 
                                     VALUES (@Name, @Phone, @Email, @Address, 'Regular', 0);
                                     SELECT LAST_INSERT_ID();";
                using var insertCmd = new MySqlCommand(insertSql, connection);
                insertCmd.Parameters.AddWithValue("@Name", name ?? "Unknown");
                insertCmd.Parameters.AddWithValue("@Phone", phone ?? "");
                insertCmd.Parameters.AddWithValue("@Email", email ?? "");
                insertCmd.Parameters.AddWithValue("@Address", address ?? "");

                return Convert.ToInt32(insertCmd.ExecuteScalar());
            }
            catch
            {
                return 1; // Default to ID 1 if failure occurs (ensure ID 1 exists or handle accordingly)
            }
        }

        public static List<(int InvoiceId, DateTime CreatedAt, int ItemCount, decimal Total)> GetCustomerPurchaseHistory(int customerId)
        {
            var list = new List<(int, DateTime, int, decimal)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = @"
                SELECT i.Id, IFNULL(i.CreatedDate, NOW()) AS CreatedAt,
                       (SELECT COUNT(*) FROM InvoiceItems ii WHERE ii.InvoiceId = i.Id) AS ItemCount,
                       i.Total
                FROM Invoices i
                WHERE i.CustomerId = @cid
                ORDER BY i.CreatedDate DESC, i.Id DESC;";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@cid", customerId);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add((
                    r.GetInt32(0),
                    r.IsDBNull(1) ? DateTime.Now : r.GetDateTime(1),
                    r.IsDBNull(2) ? 0 : Convert.ToInt32(r.GetValue(2)),
                    r.IsDBNull(3) ? 0m : r.GetDecimal(3)
                ));
            }
            return list;
        }

        public static List<(string CustomerName, decimal TotalSpent)> GetTopCustomers(int topN = 10)
        {
            var list = new List<(string, decimal)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = @"SELECT c.Name, SUM(i.Total) as TotalSpent
                           FROM Invoices i
                           LEFT JOIN Customers c ON c.Id = i.CustomerId
                           WHERE c.Name IS NOT NULL
                           GROUP BY c.Name
                           ORDER BY TotalSpent DESC
                           LIMIT @top";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@top", topN);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add((r.IsDBNull(0) ? "Unknown" : r.GetString(0), r.IsDBNull(1) ? 0m : r.GetDecimal(1)));
            }
            return list;
        }

        public static int ImportCustomersFromCsv(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath)) return -1;
                var lines = System.IO.File.ReadAllLines(filePath);
                if (lines.Length == 0) return 0;

                string[] header = lines[0].Split(',');
                int idxName = Array.FindIndex(header, h => string.Equals(h.Trim(), "Name", StringComparison.OrdinalIgnoreCase));
                int idxPhone = Array.FindIndex(header, h => string.Equals(h.Trim(), "Phone", StringComparison.OrdinalIgnoreCase));
                int idxEmail = Array.FindIndex(header, h => string.Equals(h.Trim(), "Email", StringComparison.OrdinalIgnoreCase));
                int idxType = Array.FindIndex(header, h => string.Equals(h.Trim(), "CustomerType", StringComparison.OrdinalIgnoreCase));
                int idxAddress = Array.FindIndex(header, h => string.Equals(h.Trim(), "Address", StringComparison.OrdinalIgnoreCase));
                int idxTier = Array.FindIndex(header, h => string.Equals(h.Trim(), "Tier", StringComparison.OrdinalIgnoreCase));
                int idxPoints = Array.FindIndex(header, h => string.Equals(h.Trim(), "Points", StringComparison.OrdinalIgnoreCase));

                if (idxName < 0) return -1;

                int success = 0;
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();
                for (int i = 1; i < lines.Length; i++)
                {
                    var raw = lines[i];
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    var cols = SplitCsvLine(raw);
                    string name = SafeGet(cols, idxName);
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    string phone = SafeGet(cols, idxPhone);
                    string email = SafeGet(cols, idxEmail);
                    string type = SafeGet(cols, idxType);
                    if (string.IsNullOrWhiteSpace(type)) type = "Regular";
                    string address = SafeGet(cols, idxAddress);
                    string tier = SafeGet(cols, idxTier);
                    if (string.IsNullOrWhiteSpace(tier)) tier = "Regular";
                    int.TryParse(SafeGet(cols, idxPoints), out int points);

                    try
                    {
                        using var up = new MySqlCommand(@"INSERT INTO Customers (Name, Phone, Email, CustomerType, Address)
                                                          VALUES (@n, @ph, @em, @t, @ad)
                                                          ON DUPLICATE KEY UPDATE Email=VALUES(Email), CustomerType=VALUES(CustomerType), Address=VALUES(Address);", connection);
                        up.Parameters.AddWithValue("@n", name);
                        up.Parameters.AddWithValue("@ph", string.IsNullOrWhiteSpace(phone) ? DBNull.Value : phone);
                        up.Parameters.AddWithValue("@em", string.IsNullOrWhiteSpace(email) ? DBNull.Value : email);
                        up.Parameters.AddWithValue("@t", type);
                        up.Parameters.AddWithValue("@ad", string.IsNullOrWhiteSpace(address) ? DBNull.Value : address);
                        up.ExecuteNonQuery();

                        int id;
                        using (var getId = new MySqlCommand("SELECT Id FROM Customers WHERE Name=@n AND (Phone <=> @ph);", connection))
                        {
                            getId.Parameters.AddWithValue("@n", name);
                            getId.Parameters.AddWithValue("@ph", string.IsNullOrWhiteSpace(phone) ? (object)DBNull.Value : phone);
                            id = Convert.ToInt32(getId.ExecuteScalar());
                        }

                        UpdateCustomerLoyalty(id, points, tier);
                        success++;
                    }
                    catch { }
                }
                return success;
            }
            catch { return -1; }
        }

        public static bool ExportCustomersToCsv(string filePath)
        {
            try
            {
                var customers = GetAllCustomers();
                using var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                writer.WriteLine("Id,Name,Phone,Email,CustomerType,Address,Tier,Points");
                foreach (var c in customers)
                {
                    var (tier, pts) = GetCustomerLoyalty(c.Id);
                    writer.WriteLine(string.Join(",", new[]
                    {
                        c.Id.ToString(),
                        EscapeCsvField(c.Name),
                        EscapeCsvField(c.Phone),
                        EscapeCsvField(c.Email),
                        EscapeCsvField(c.CustomerType),
                        EscapeCsvField(c.Address),
                        EscapeCsvField(tier),
                        pts.ToString()
                    }));
                }
                return true;
            }
            catch { return false; }
        }

        private static string SafeGet(string[] cols, int idx)
        {
            if (idx < 0) return string.Empty;
            return idx < cols.Length ? cols[idx].Trim() : string.Empty;
        }

        private static string EscapeCsvField(string? s)
        {
            s ??= string.Empty;
            if (s.Contains('"')) s = s.Replace("\"", "\"\"");
            if (s.Contains(',') || s.Contains('\n') || s.Contains('\r') || s.Contains('"')) s = "\"" + s + "\"";
            return s;
        }

        private static string[] SplitCsvLine(string line)
        {
            var result = new List<string>();
            bool inQuotes = false;
            var current = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { current.Append('"'); i++; }
                        else inQuotes = false;
                    }
                    else current.Append(c);
                }
                else
                {
                    if (c == ',') { result.Add(current.ToString()); current.Clear(); }
                    else if (c == '"') inQuotes = true;
                    else current.Append(c);
                }
            }
            result.Add(current.ToString());
            return result.ToArray();
        }
    }
}

