using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FashionStore.Repositories
{
    public static class ProductRepository
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static bool AddProduct(
            string name,
            string code,
            int categoryId,
            decimal salePrice,
            decimal purchasePrice,
            string purchaseUnit,
            int importQuantity,
            int stockQuantity,
            string description = "",
            decimal promoDiscountPercent = 0m,
            DateTime? promoStartDate = null,
            DateTime? promoEndDate = null,
            int supplierId = 0)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();

            string cmdText = "INSERT INTO Products (Name, Code, CategoryId, SalePrice, PromoDiscountPercent, PromoStartDate, PromoEndDate, PurchasePrice, PurchaseUnit, ImportQuantity, StockQuantity, Description, SupplierId) " +
                             "VALUES (@name, @code, @categoryId, @salePrice, @promoDiscountPercent, @promoStartDate, @promoEndDate, @purchasePrice, @purchaseUnit, @importQuantity, @stockQuantity, @description, @supplierId);";
            using var cmd = new MySqlCommand(cmdText, connection);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@code", code);
            cmd.Parameters.AddWithValue("@categoryId", categoryId);
            cmd.Parameters.AddWithValue("@salePrice", salePrice);
            cmd.Parameters.AddWithValue("@promoDiscountPercent", promoDiscountPercent);
            cmd.Parameters.AddWithValue("@promoStartDate", (object?)promoStartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@promoEndDate", (object?)promoEndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@purchasePrice", purchasePrice);
            cmd.Parameters.AddWithValue("@purchaseUnit", purchaseUnit);
            cmd.Parameters.AddWithValue("@importQuantity", importQuantity);
            cmd.Parameters.AddWithValue("@stockQuantity", stockQuantity);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@supplierId", supplierId);
            try { return cmd.ExecuteNonQuery() > 0; }
            catch { return false; }
        }

        public static List<(int Id, string Name, string Code, int CategoryId, string CategoryName, decimal SalePrice, decimal PromoDiscountPercent, DateTime? PromoStartDate, DateTime? PromoEndDate, decimal PurchasePrice, string PurchaseUnit, int ImportQuantity, int StockQuantity, string Description, decimal CategoryTaxPercent, int SupplierId, string SupplierName)> GetAllProductsWithCategories()
        {
            var products = new List<(int, string, string, int, string, decimal, decimal, DateTime?, DateTime?, decimal, string, int, int, string, decimal, int, string)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string cmdText = @"SELECT p.Id, p.Name, p.Code, p.CategoryId, c.Name as CategoryName, p.SalePrice,
                                      IFNULL(p.PromoDiscountPercent, 0) AS PromoDiscountPercent,
                                      p.PromoStartDate,
                                      p.PromoEndDate,
                                      p.PurchasePrice, p.PurchaseUnit, p.ImportQuantity, p.StockQuantity, p.Description,
                                      IFNULL(c.TaxPercent, 0) AS CategoryTaxPercent,
                                      IFNULL(p.SupplierId, 0) AS SupplierId,
                                      IFNULL(s.Name, '') AS SupplierName
                               FROM Products p 
                               LEFT JOIN Categories c ON p.CategoryId = c.Id 
                               LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                               ORDER BY p.Name
                               LIMIT 10000;";
            using var cmd = new MySqlCommand(cmdText, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                products.Add((
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.IsDBNull(2) ? "" : reader.GetString(2),
                    reader.IsDBNull(3) ? 0 : reader.GetInt32(3),
                    reader.IsDBNull(4) ? "Uncategorized" : reader.GetString(4),
                    reader.GetDecimal(5),
                    reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                    reader.IsDBNull(7) ? (DateTime?)null : reader.GetDateTime(7),
                    reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                    reader.IsDBNull(9) ? 0m : reader.GetDecimal(9),
                    reader.IsDBNull(10) ? "" : reader.GetString(10),
                    reader.IsDBNull(11) ? 0 : reader.GetInt32(11),
                    reader.GetInt32(12),
                    reader.IsDBNull(13) ? "" : reader.GetString(13),
                    reader.IsDBNull(14) ? 0m : reader.GetDecimal(14),
                    reader.IsDBNull(15) ? 0 : reader.GetInt32(15),
                    reader.IsDBNull(16) ? "" : reader.GetString(16)
                ));
            }
            return products;
        }

        public static bool UpdateProduct(
            int id,
            string name,
            string code,
            int categoryId,
            decimal salePrice,
            decimal purchasePrice,
            string purchaseUnit,
            int importQuantity,
            int stockQuantity,
            string description = "",
            decimal promoDiscountPercent = 0m,
            DateTime? promoStartDate = null,
            DateTime? promoEndDate = null,
            int supplierId = 0)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string cmdText = "UPDATE Products SET Name=@name, Code=@code, CategoryId=@categoryId, SalePrice=@salePrice, PromoDiscountPercent=@promoDiscountPercent, PromoStartDate=@promoStartDate, PromoEndDate=@promoEndDate, PurchasePrice=@purchasePrice, PurchaseUnit=@purchaseUnit, ImportQuantity=@importQuantity, StockQuantity=@stockQuantity, Description=@description, SupplierId=@supplierId WHERE Id=@id;";
            using var cmd = new MySqlCommand(cmdText, connection);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@code", code);
            cmd.Parameters.AddWithValue("@categoryId", categoryId);
            cmd.Parameters.AddWithValue("@salePrice", salePrice);
            cmd.Parameters.AddWithValue("@promoDiscountPercent", promoDiscountPercent);
            cmd.Parameters.AddWithValue("@promoStartDate", (object?)promoStartDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@promoEndDate", (object?)promoEndDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@purchasePrice", purchasePrice);
            cmd.Parameters.AddWithValue("@purchaseUnit", purchaseUnit);
            cmd.Parameters.AddWithValue("@importQuantity", importQuantity);
            cmd.Parameters.AddWithValue("@stockQuantity", stockQuantity);
            cmd.Parameters.AddWithValue("@description", description);
            cmd.Parameters.AddWithValue("@supplierId", supplierId);
            try { return cmd.ExecuteNonQuery() > 0; }
            catch { return false; }
        }

        public static int GetProductStockQuantity(int productId)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string cmd = "SELECT StockQuantity FROM Products WHERE Id = @id;";
            using var check = new MySqlCommand(cmd, connection);
            check.Parameters.AddWithValue("@id", productId);
            var result = check.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public static bool DeleteProduct(int id)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            try
            {
                string checkCmd = "SELECT COUNT(*) FROM InvoiceItems WHERE ProductId=@id;";
                using var check = new MySqlCommand(checkCmd, connection);
                check.Parameters.AddWithValue("@id", id);
                long count = (long)check.ExecuteScalar();
                if (count > 0) return false;
            }
            catch { }

            try
            {
                string cmdText = "DELETE FROM Products WHERE Id=@id;";
                using var cmd = new MySqlCommand(cmdText, connection);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch
            {
                try
                {
                    using var disableFK = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", connection);
                    disableFK.ExecuteNonQuery();
                    string cmdText = "DELETE FROM Products WHERE Id=@id;";
                    using var cmd = new MySqlCommand(cmdText, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    int result = cmd.ExecuteNonQuery();
                    using var enableFK = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 1;", connection);
                    enableFK.ExecuteNonQuery();
                    return result > 0;
                }
                catch { return false; }
            }
        }

        public static bool DeleteAllProducts()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();
            try
            {
                try
                {
                    string checkCmd = "SELECT COUNT(*) FROM InvoiceItems;";
                    using var check = new MySqlCommand(checkCmd, connection, tx);
                    long count = (long)check.ExecuteScalar();
                    if (count > 0) return false;
                }
                catch { }

                using var disableFK = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", connection, tx);
                disableFK.ExecuteNonQuery();
                using var truncateCmd = new MySqlCommand("TRUNCATE TABLE Products;", connection, tx);
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

        public static List<(string ProductName, int StockQuantity, string CategoryName)> GetLowStockProducts(int threshold = 10)
        {
            var list = new List<(string, int, string)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = @"SELECT p.Name, p.StockQuantity, IFNULL(c.Name, 'Uncategorized') as CategoryName
                           FROM Products p
                           LEFT JOIN Categories c ON c.Id = p.CategoryId
                           WHERE p.StockQuantity <= @threshold
                           ORDER BY p.StockQuantity ASC
                           LIMIT 100";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@threshold", threshold);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add((
                    r.IsDBNull(0) ? "Unknown" : r.GetString(0),
                    r.IsDBNull(1) ? 0 : r.GetInt32(1),
                    r.IsDBNull(2) ? "Uncategorized" : r.GetString(2)
                ));
            }
            return list;
        }

        public static int GetTotalProducts()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Products", connection);
            var val = cmd.ExecuteScalar();
            return Convert.ToInt32(val ?? 0);
        }

        public static int FindProductIdByName(string productName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(productName)) return 0;
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();
                using var cmd = new MySqlCommand("SELECT Id FROM Products WHERE Name=@name LIMIT 1;", connection);
                cmd.Parameters.AddWithValue("@name", productName);
                var val = cmd.ExecuteScalar();
                return val == null ? 0 : Convert.ToInt32(val);
            }
            catch { return 0; }
        }

        public static bool DoesProductIdExist(int productId)
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();
                using var cmd = new MySqlCommand("SELECT 1 FROM Products WHERE Id=@id LIMIT 1;", connection);
                cmd.Parameters.AddWithValue("@id", productId);
                var exists = cmd.ExecuteScalar();
                return exists != null;
            }
            catch { return false; }
        }

        public static List<(string ProductName, int Quantity, decimal Revenue)> GetTopProducts(int topN = 10)
        {
            var list = new List<(string, int, decimal)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = @"SELECT p.Name, SUM(ii.Quantity) as qty, SUM(ii.LineTotal) as rev
                           FROM InvoiceItems ii
                           JOIN Products p ON p.Id = ii.ProductId
                           GROUP BY p.Name
                           ORDER BY qty DESC
                           LIMIT @top";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@top", topN);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add((
                    r.IsDBNull(0) ? "(Unknown)" : r.GetString(0), 
                    r.IsDBNull(1) ? 0 : r.GetInt32(1),
                    r.IsDBNull(2) ? 0m : r.GetDecimal(2)
                ));
            }
            return list;
        }

        public static int ImportProductsFromCsv(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath)) return -1;
                var lines = System.IO.File.ReadAllLines(filePath);
                if (lines.Length == 0) return 0;

                string[] header = lines[0].Split(',');
                int idxName = Array.FindIndex(header, h => string.Equals(h.Trim(), "Name", StringComparison.OrdinalIgnoreCase));
                int idxCode = Array.FindIndex(header, h => string.Equals(h.Trim(), "Code", StringComparison.OrdinalIgnoreCase));
                int idxCategoryId = Array.FindIndex(header, h => string.Equals(h.Trim(), "CategoryId", StringComparison.OrdinalIgnoreCase));
                int idxCategoryName = Array.FindIndex(header, h => string.Equals(h.Trim(), "CategoryName", StringComparison.OrdinalIgnoreCase));
                int idxPrice = Array.FindIndex(header, h => string.Equals(h.Trim(), "Price", StringComparison.OrdinalIgnoreCase) || string.Equals(h.Trim(), "SalePrice", StringComparison.OrdinalIgnoreCase));
                int idxPurchasePrice = Array.FindIndex(header, h => string.Equals(h.Trim(), "PurchasePrice", StringComparison.OrdinalIgnoreCase));
                int idxPurchaseUnit = Array.FindIndex(header, h => string.Equals(h.Trim(), "PurchaseUnit", StringComparison.OrdinalIgnoreCase));
                int idxImportQuantity = Array.FindIndex(header, h => string.Equals(h.Trim(), "ImportQuantity", StringComparison.OrdinalIgnoreCase));
                int idxStock = Array.FindIndex(header, h => string.Equals(h.Trim(), "StockQuantity", StringComparison.OrdinalIgnoreCase));
                int idxDesc = Array.FindIndex(header, h => string.Equals(h.Trim(), "Description", StringComparison.OrdinalIgnoreCase));
                int idxPromoDiscount = Array.FindIndex(header, h => string.Equals(h.Trim(), "PromoDiscountPercent", StringComparison.OrdinalIgnoreCase));
                int idxPromoStart = Array.FindIndex(header, h => string.Equals(h.Trim(), "PromoStartDate", StringComparison.OrdinalIgnoreCase));
                int idxPromoEnd = Array.FindIndex(header, h => string.Equals(h.Trim(), "PromoEndDate", StringComparison.OrdinalIgnoreCase));

                if (idxName < 0 || idxPrice < 0) return -1;

                int successCount = 0;
                for (int i = 1; i < lines.Length; i++)
                {
                    var raw = lines[i];
                    if (string.IsNullOrWhiteSpace(raw)) continue;
                    var cols = SplitCsvLine(raw);
                    string name = SafeGet(cols, idxName);
                    if (string.IsNullOrWhiteSpace(name)) continue;
                    
                    string code = SafeGet(cols, idxCode);
                    string catName = SafeGet(cols, idxCategoryName);
                    string priceStr = SafeGet(cols, idxPrice);
                    string purchasePriceStr = SafeGet(cols, idxPurchasePrice);
                    string purchaseUnit = SafeGet(cols, idxPurchaseUnit);
                    string importQuantityStr = SafeGet(cols, idxImportQuantity);
                    string stockStr = SafeGet(cols, idxStock);
                    string desc = SafeGet(cols, idxDesc);
                    string promoDiscountStr = SafeGet(cols, idxPromoDiscount);
                    string promoStartStr = SafeGet(cols, idxPromoStart);
                    string promoEndStr = SafeGet(cols, idxPromoEnd);

                    if (!decimal.TryParse(priceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal price)) continue;

                    decimal purchasePrice = 0;
                    if (!decimal.TryParse(purchasePriceStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out purchasePrice))
                    {
                        purchasePrice = price * 0.8m;
                    }
                    
                    int.TryParse(stockStr, out int stock);
                    int.TryParse(importQuantityStr, out int importQuantity);
                    decimal.TryParse(promoDiscountStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal promoDiscountPercent);
                    
                    DateTime? promoStartDate = null;
                    if (DateTime.TryParse(promoStartStr, out var ps)) promoStartDate = ps;
                    DateTime? promoEndDate = null;
                    if (DateTime.TryParse(promoEndStr, out var pe)) promoEndDate = pe;

                    int categoryId = 0;
                    if (!string.IsNullOrWhiteSpace(catName)) categoryId = EnsureCategory(catName);
                    else int.TryParse(SafeGet(cols, idxCategoryId), out categoryId);

                    if (AddProduct(name, code ?? "", categoryId, price, purchasePrice, purchaseUnit ?? "", importQuantity, stock, desc ?? "", promoDiscountPercent, promoStartDate, promoEndDate))
                    {
                        successCount++;
                    }
                }
                return successCount;
            }
            catch { return -1; }
        }

        public static bool ExportProductsToCsv(string filePath)
        {
            try
            {
                var products = GetAllProductsWithCategories();
                using var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                writer.WriteLine("Id,Name,Code,CategoryId,CategoryName,SalePrice,PromoDiscountPercent,PromoStartDate,PromoEndDate,PurchasePrice,PurchaseUnit,ImportQuantity,StockQuantity,Description");
                foreach (var p in products)
                {
                    writer.WriteLine(string.Join(",", new string[]
                    {
                        p.Id.ToString(),
                        EscapeCsvField(p.Name),
                        EscapeCsvField(p.Code),
                        p.CategoryId.ToString(),
                        EscapeCsvField(p.CategoryName),
                        p.SalePrice.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        p.PromoDiscountPercent.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        EscapeCsvField(p.PromoStartDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""),
                        EscapeCsvField(p.PromoEndDate?.ToString("yyyy-MM-dd HH:mm:ss") ?? ""),
                        p.PurchasePrice.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        EscapeCsvField(p.PurchaseUnit),
                        p.ImportQuantity.ToString(),
                        p.StockQuantity.ToString(),
                        EscapeCsvField(p.Description)
                    }));
                }
                return true;
            }
            catch { return false; }
        }

        private static int EnsureCategory(string name)
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
                using (var insCmd = new MySqlCommand("INSERT INTO Categories (Name) VALUES (@n); SELECT LAST_INSERT_ID();", connection))
                {
                    insCmd.Parameters.AddWithValue("@n", name);
                    return Convert.ToInt32(insCmd.ExecuteScalar());
                }
            }
            catch { return 0; }
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
