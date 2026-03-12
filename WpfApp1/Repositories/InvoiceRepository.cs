using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FashionStore.Repositories
{
    public static class InvoiceRepository
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();
        public static int LastSavedInvoiceId { get; private set; }

        public static bool SaveInvoice(
            int customerId,
            int employeeId,
            decimal subtotal,
            decimal taxPercent,
            decimal taxAmount,
            decimal discount,
            decimal total,
            decimal paid,
            List<(int ProductId, int Quantity, decimal UnitPrice)> items,
            DateTime? createdDate = null,
            int? invoiceId = null,
            int? voucherId = null)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();
            try
            {
                DateTime invoiceDate = createdDate ?? DateTime.Now;
                int actualInvoiceId;

                if (invoiceId.HasValue)
                {
                    string insertInvoice = @"INSERT INTO Invoices (Id, CustomerId, EmployeeId, Subtotal, TaxPercent, TaxAmount, Discount, Total, Paid, CreatedDate)
                                             VALUES (@Id, @CustomerId, @EmployeeId, @Subtotal, @TaxPercent, @TaxAmount, @Discount, @Total, @Paid, @CreatedDate);";
                    using var invCmd = new MySqlCommand(insertInvoice, connection, tx);
                    invCmd.Parameters.AddWithValue("@Id", invoiceId.Value);
                    invCmd.Parameters.AddWithValue("@CustomerId", customerId);
                    invCmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                    invCmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    invCmd.Parameters.AddWithValue("@TaxPercent", taxPercent);
                    invCmd.Parameters.AddWithValue("@TaxAmount", taxAmount);
                    invCmd.Parameters.AddWithValue("@Discount", discount);
                    invCmd.Parameters.AddWithValue("@Total", total);
                    invCmd.Parameters.AddWithValue("@Paid", paid);
                    invCmd.Parameters.AddWithValue("@CreatedDate", invoiceDate);
                    invCmd.ExecuteNonQuery();
                    actualInvoiceId = invoiceId.Value;
                }
                else
                {
                    string findGapSql = @"
                        SELECT MIN(t1.Id + 1) 
                        FROM (SELECT Id FROM Invoices UNION SELECT 0 AS Id) t1
                        WHERE NOT EXISTS (SELECT 1 FROM Invoices t2 WHERE t2.Id = t1.Id + 1);";
                        
                    int nextId = 1;
                    using (var gapCmd = new MySqlCommand(findGapSql, connection, tx))
                    {
                        var result = gapCmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null) nextId = Convert.ToInt32(result);
                    }
                    
                    string insertInvoice = @"INSERT INTO Invoices (Id, CustomerId, EmployeeId, Subtotal, TaxPercent, TaxAmount, Discount, Total, Paid, CreatedDate)
                                             VALUES (@Id, @CustomerId, @EmployeeId, @Subtotal, @TaxPercent, @TaxAmount, @Discount, @Total, @Paid, @CreatedDate);";
                                             
                    using var invCmd = new MySqlCommand(insertInvoice, connection, tx);
                    invCmd.Parameters.AddWithValue("@Id", nextId);
                    invCmd.Parameters.AddWithValue("@CustomerId", customerId);
                    invCmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                    invCmd.Parameters.AddWithValue("@Subtotal", subtotal);
                    invCmd.Parameters.AddWithValue("@TaxPercent", taxPercent);
                    invCmd.Parameters.AddWithValue("@TaxAmount", taxAmount);
                    invCmd.Parameters.AddWithValue("@Discount", discount);
                    invCmd.Parameters.AddWithValue("@Total", total);
                    invCmd.Parameters.AddWithValue("@Paid", paid);
                    invCmd.Parameters.AddWithValue("@CreatedDate", invoiceDate);
                    invCmd.ExecuteNonQuery();
                    actualInvoiceId = nextId;
                }

                foreach (var (productId, quantity, unitPrice) in items)
                {
                    string updateStock = "UPDATE Products SET StockQuantity = StockQuantity - @qty WHERE Id=@pid AND StockQuantity >= @qty;";
                    using var stockCmd = new MySqlCommand(updateStock, connection, tx);
                    stockCmd.Parameters.AddWithValue("@qty", quantity);
                    stockCmd.Parameters.AddWithValue("@pid", productId);
                    
                    if (stockCmd.ExecuteNonQuery() == 0) throw new Exception($"Product ID {productId} is out of stock.");

                    decimal lineTotal = unitPrice * quantity;
                    string insertItem = @"INSERT INTO InvoiceItems (InvoiceId, ProductId, EmployeeId, UnitPrice, Quantity, LineTotal)
                                           VALUES (@InvoiceId, @ProductId, @EmployeeId, @UnitPrice, @Quantity, @LineTotal);";
                    using var itemCmd = new MySqlCommand(insertItem, connection, tx);
                    itemCmd.Parameters.AddWithValue("@InvoiceId", actualInvoiceId);
                    itemCmd.Parameters.AddWithValue("@ProductId", productId);
                    itemCmd.Parameters.AddWithValue("@EmployeeId", employeeId);
                    itemCmd.Parameters.AddWithValue("@UnitPrice", unitPrice);
                    itemCmd.Parameters.AddWithValue("@Quantity", quantity);
                    itemCmd.Parameters.AddWithValue("@LineTotal", lineTotal);
                    itemCmd.ExecuteNonQuery();
                }

                if (voucherId.HasValue) VoucherRepository.UpdateVoucherUsage(voucherId.Value, connection, tx);

                tx.Commit();
                LastSavedInvoiceId = actualInvoiceId;
                return true;
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                return false;
            }
        }

        public static decimal GetRevenueBetween(DateTime from, DateTime to)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "SELECT IFNULL(SUM(Total), 0) FROM Invoices WHERE CreatedDate BETWEEN @from AND @to";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            return Convert.ToDecimal(cmd.ExecuteScalar() ?? 0);
        }

        public static int GetInvoiceCountBetween(DateTime from, DateTime to)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "SELECT COUNT(*) FROM Invoices WHERE CreatedDate BETWEEN @from AND @to";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        public static List<(int Id, DateTime CreatedDate, string CustomerName, decimal Subtotal, decimal TaxAmount, decimal Discount, decimal Total, decimal Paid)>
            QueryInvoices(DateTime? from, DateTime? to, int? customerId, string search)
        {
            var list = new List<(int, DateTime, string, decimal, decimal, decimal, decimal, decimal)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            var sb = new System.Text.StringBuilder();
            sb.Append(@"SELECT i.Id, i.CreatedDate, c.Name, i.Subtotal, i.TaxAmount, i.DiscountAmount, i.Total, i.Paid
                       FROM Invoices i
                       LEFT JOIN Customers c ON c.Id = i.CustomerId
                       WHERE 1=1");

            if (from.HasValue) sb.Append(" AND i.CreatedDate >= @from");
            if (to.HasValue) sb.Append(" AND i.CreatedDate <= @to");
            if (customerId.HasValue) sb.Append(" AND i.CustomerId = @cust");
            if (!string.IsNullOrWhiteSpace(search)) sb.Append(" AND (c.Name LIKE @q OR i.Id LIKE @q)");
            sb.Append(" ORDER BY i.CreatedDate DESC, i.Id DESC LIMIT 10000");

            using var cmd = new MySqlCommand(sb.ToString(), connection);
            if (from.HasValue) cmd.Parameters.AddWithValue("@from", from.Value);
            if (to.HasValue) cmd.Parameters.AddWithValue("@to", to.Value);
            if (customerId.HasValue) cmd.Parameters.AddWithValue("@cust", customerId.Value);
            if (!string.IsNullOrWhiteSpace(search)) cmd.Parameters.AddWithValue("@q", "%" + search + "%");

            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add((r.GetInt32(0), r.GetDateTime(1), r.IsDBNull(2) ? "" : r.GetString(2), r.GetDecimal(3), r.GetDecimal(4), r.GetDecimal(5), r.GetDecimal(6), r.GetDecimal(7)));
            }
            return list;
        }

        public static (InvoiceHeader Header, List<InvoiceItemDetail> Items) GetInvoiceDetails(int invoiceId)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string headerSql = @"SELECT i.Id, i.CreatedDate, c.Name, i.Subtotal, i.TaxPercent, i.TaxAmount, i.DiscountAmount, i.Total, i.Paid,
                                        IFNULL(c.Phone, ''), IFNULL(c.Email, ''), IFNULL(c.Address, ''), i.EmployeeId
                                 FROM Invoices i
                                 LEFT JOIN Customers c ON c.Id = i.CustomerId
                                 WHERE i.Id = @id";
            using var hcmd = new MySqlCommand(headerSql, connection);
            hcmd.Parameters.AddWithValue("@id", invoiceId);
            using var hr = hcmd.ExecuteReader();
            InvoiceHeader header;
            if (hr.Read())
            {
                header = new InvoiceHeader { Id = hr.GetInt32(0), CreatedDate = hr.GetDateTime(1), CustomerName = hr.IsDBNull(2) ? "" : hr.GetString(2), Subtotal = hr.GetDecimal(3), TaxPercent = hr.GetDecimal(4), TaxAmount = hr.GetDecimal(5), DiscountAmount = hr.GetDecimal(6), Total = hr.GetDecimal(7), Paid = hr.GetDecimal(8), CustomerPhone = hr.IsDBNull(9) ? "" : hr.GetString(9), CustomerEmail = hr.IsDBNull(10) ? "" : hr.GetString(10), CustomerAddress = hr.IsDBNull(11) ? "" : hr.GetString(11), EmployeeId = hr.IsDBNull(12) ? 1 : hr.GetInt32(12) };
            }
            else return (new InvoiceHeader { Id = invoiceId }, new List<InvoiceItemDetail>());
            hr.Close();

            var items = new List<InvoiceItemDetail>();
            string itemsSql = @"SELECT ii.ProductId, p.Name, ii.UnitPrice, ii.Quantity, ii.LineTotal
                                 FROM InvoiceItems ii
                                 LEFT JOIN Products p ON p.Id = ii.ProductId
                                 WHERE ii.InvoiceId = @id ORDER BY ii.Id";
            using var icmd = new MySqlCommand(itemsSql, connection);
            icmd.Parameters.AddWithValue("@id", invoiceId);
            using var ir = icmd.ExecuteReader();
            while (ir.Read())
            {
                items.Add(new InvoiceItemDetail { ProductId = ir.GetInt32(0), ProductName = ir.IsDBNull(1) ? "" : ir.GetString(1), UnitPrice = ir.GetDecimal(2), Quantity = ir.GetInt32(3), LineTotal = ir.GetDecimal(4) });
            }
            return (header, items);
        }

        public static bool DeleteAllInvoices()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();
            try
            {
                using var disableFK = new MySqlCommand("SET FOREIGN_KEY_CHECKS = 0;", connection, tx);
                disableFK.ExecuteNonQuery();

                using var truncateInvoiceItems = new MySqlCommand("TRUNCATE TABLE InvoiceItems;", connection, tx);
                truncateInvoiceItems.ExecuteNonQuery();

                using var truncateInvoices = new MySqlCommand("TRUNCATE TABLE Invoices;", connection, tx);
                truncateInvoices.ExecuteNonQuery();

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

        public static bool DeleteInvoice(int id)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var tx = connection.BeginTransaction();
            try
            {
                // Note: InvoiceItems has ON DELETE CASCADE on InvoiceId
                string sql = "DELETE FROM Invoices WHERE Id = @id";
                using var cmd = new MySqlCommand(sql, connection, tx);
                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();
                tx.Commit();
                return true;
            }
            catch
            {
                try { tx.Rollback(); } catch { }
                return false;
            }
        }

        public static bool ExportInvoicesToCsv(string filePath)
        {
            try
            {
                using var writer = new System.IO.StreamWriter(filePath, false, System.Text.Encoding.UTF8);
                writer.WriteLine("InvoiceId,InvoiceDate,CustomerName,CustomerPhone,CustomerEmail,CustomerAddress,Subtotal,TaxPercent,TaxAmount,Discount,Total,Paid,EmployeeId");

                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();

                var invoiceIds = new List<int>();
                string idsSql = "SELECT Id FROM Invoices ORDER BY Id DESC LIMIT 10000";
                using var idsCmd = new MySqlCommand(idsSql, connection);
                using var idsReader = idsCmd.ExecuteReader();
                while (idsReader.Read()) invoiceIds.Add(idsReader.GetInt32(0));
                idsReader.Close();

                foreach (int invoiceId in invoiceIds)
                {
                    string sql = @"SELECT i.Id, i.CreatedDate, c.Name, IFNULL(c.Phone, ''), IFNULL(c.Email, ''), IFNULL(c.Address, ''), i.Subtotal, i.TaxPercent, i.TaxAmount, i.DiscountAmount, i.Total, i.Paid, i.EmployeeId FROM Invoices i LEFT JOIN Customers c ON c.Id = i.CustomerId WHERE i.Id = @id";
                    using var cmd = new MySqlCommand(sql, connection);
                    cmd.Parameters.AddWithValue("@id", invoiceId);
                    using var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        writer.WriteLine(string.Join(",", new string[]
                        {
                            reader.GetInt32(0).ToString(),
                            EscapeCsvField(reader.GetDateTime(1).ToString("yyyy-MM-dd HH:mm:ss")),
                            EscapeCsvField(reader.IsDBNull(2) ? "" : reader.GetString(2)),
                            EscapeCsvField(reader.IsDBNull(3) ? "" : reader.GetString(3)),
                            EscapeCsvField(reader.IsDBNull(4) ? "" : reader.GetString(4)),
                            EscapeCsvField(reader.IsDBNull(5) ? "" : reader.GetString(5)),
                            reader.GetDecimal(6).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            reader.GetDecimal(7).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            reader.GetDecimal(8).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            reader.GetDecimal(9).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            reader.GetDecimal(10).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            reader.GetDecimal(11).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            reader.GetInt32(12).ToString()
                        }));
                    }
                    reader.Close();

                    string itemsSql = @"SELECT p.Id, p.Name, ii.Quantity, ii.UnitPrice, ii.LineTotal FROM InvoiceItems ii INNER JOIN Products p ON p.Id = ii.ProductId WHERE ii.InvoiceId = @id";
                    using var itemsCmd = new MySqlCommand(itemsSql, connection);
                    itemsCmd.Parameters.AddWithValue("@id", invoiceId);
                    using var itemsReader = itemsCmd.ExecuteReader();
                    while (itemsReader.Read())
                    {
                        writer.WriteLine(string.Join(",", new string[]
                        {
                            "", "", "", "", "", "", "ITEM",
                            EscapeCsvField(itemsReader.IsDBNull(1) ? "" : itemsReader.GetString(1)),
                            itemsReader.GetInt32(0).ToString(),
                            itemsReader.GetInt32(2).ToString(),
                            itemsReader.GetDecimal(3).ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                            itemsReader.GetDecimal(4).ToString("F2", System.Globalization.CultureInfo.InvariantCulture)
                        }));
                    }
                    itemsReader.Close();
                }
                return true;
            }
            catch { return false; }
        }

        public static int ImportInvoicesFromCsv(string filePath)
        {
            try
            {
                var lines = System.IO.File.ReadAllLines(filePath, System.Text.Encoding.UTF8);
                if (lines.Length <= 1) return 0;

                int successCount = 0;
                var currentInvoice = new InvoiceHeader();
                var currentItems = new List<(int ProductId, int Quantity, decimal UnitPrice)>();
                int employeeId = ResolveEmployeeIdFromCurrentUserOrDefault();

                for (int i = 1; i < lines.Length; i++)
                {
                    var fields = SplitCsvLine(lines[i]);
                    if (fields.Length == 0 || (fields.Length == 1 && string.IsNullOrWhiteSpace(fields[0]))) continue;

                    if (fields.Length > 6 && fields[6] == "ITEM")
                    {
                        TryAppendItemFromFields(currentItems, fields);
                    }
                    else if (fields.Length >= 13 && !string.IsNullOrEmpty(fields[0]))
                    {
                        if (currentInvoice.Id > 0 && currentItems.Count > 0)
                        {
                            if (SaveInvoiceWithResolvedCustomer(currentInvoice, currentItems, employeeId)) successCount++;
                        }

                        if (int.TryParse(fields[0], out int invId) && DateTime.TryParse(fields[1], out DateTime invDate))
                        {
                            currentInvoice = new InvoiceHeader
                            {
                                Id = invId, CreatedDate = invDate, CustomerName = fields[2].Trim('"'), CustomerPhone = fields[3].Trim('"'), CustomerEmail = fields[4].Trim('"'), CustomerAddress = fields[5].Trim('"'),
                                Subtotal = decimal.Parse(fields[6]), TaxPercent = decimal.Parse(fields[7]), TaxAmount = decimal.Parse(fields[8]), DiscountAmount = decimal.Parse(fields[9]), Total = decimal.Parse(fields[10]), Paid = decimal.Parse(fields[11]),
                                EmployeeId = int.TryParse(fields[12], out int empId) ? empId : employeeId
                            };
                            currentItems.Clear();
                        }
                    }
                }

                if (currentInvoice.Id > 0 && currentItems.Count > 0)
                {
                    if (SaveInvoiceWithResolvedCustomer(currentInvoice, currentItems, employeeId)) successCount++;
                }

                if (successCount > 0) ResetInvoicesAutoIncrement();
                return successCount;
            }
            catch { return -1; }
        }

        private static void ResetInvoicesAutoIncrement()
        {
            try
            {
                using var connection = new MySqlConnection(ConnectionString);
                connection.Open();
                string getMaxIdCmd = "SELECT IFNULL(MAX(Id), 0) FROM Invoices";
                using var mCmd = new MySqlCommand(getMaxIdCmd, connection);
                var maxId = Convert.ToInt32(mCmd.ExecuteScalar());
                using var resetCmd = new MySqlCommand($"ALTER TABLE Invoices AUTO_INCREMENT = {maxId + 1}", connection);
                resetCmd.ExecuteNonQuery();
            }
            catch { }
        }

        private static int ResolveEmployeeIdFromCurrentUserOrDefault()
        {
            try
            {
                var currentUser = System.Windows.Application.Current?.Resources["CurrentUser"] as string;
                if (!string.IsNullOrEmpty(currentUser)) return UserRepository.GetEmployeeIdByUsername(currentUser);
            }
            catch { }
            return 1;
        }

        private static void TryAppendItemFromFields(List<(int ProductId, int Quantity, decimal UnitPrice)> items, string[] fields)
        {
            if (fields.Length < 12) return;
            var productName = fields[7]?.Trim('"') ?? "";
            if (!int.TryParse(fields[9], out int qty) || !decimal.TryParse(fields[10], out decimal unitPrice)) return;

            int productIdToUse = 0;
            if (int.TryParse(fields[8], out int productIdFromCsv))
            {
                if (ProductRepository.DoesProductIdExist(productIdFromCsv)) productIdToUse = productIdFromCsv;
            }
            if (productIdToUse == 0 && !string.IsNullOrWhiteSpace(productName)) productIdToUse = ProductRepository.FindProductIdByName(productName);
            
            if (productIdToUse > 0) items.Add((productIdToUse, qty, unitPrice));
        }

        private static bool SaveInvoiceWithResolvedCustomer(InvoiceHeader header, List<(int ProductId, int Quantity, decimal UnitPrice)> items, int defaultEmployeeId)
        {
            try
            {
                var customerId = CustomerRepository.GetOrCreateCustomerId(header.CustomerName, header.CustomerPhone, header.CustomerEmail, header.CustomerAddress);
                var empId = header.EmployeeId > 0 ? header.EmployeeId : defaultEmployeeId;
                return SaveInvoice(customerId, empId, header.Subtotal, header.TaxPercent, header.TaxAmount, header.DiscountAmount, header.Total, header.Paid, items, header.CreatedDate, header.Id);
            }
            catch { return false; }
        }

        private static string EscapeCsvField(string? s)
        {
            s ??= "";
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

        public static List<(DateTime Day, decimal Revenue)> GetRevenueByDay(DateTime from, DateTime to)
        {
            var list = new List<(DateTime, decimal)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = @"SELECT DATE(CreatedDate) as d, SUM(Total) as revenue FROM Invoices WHERE CreatedDate BETWEEN @from AND @to GROUP BY DATE(CreatedDate) ORDER BY d";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@from", from);
            cmd.Parameters.AddWithValue("@to", to);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add((r.GetDateTime(0), r.IsDBNull(1) ? 0m : r.GetDecimal(1)));
            return list;
        }

        public static List<(string CategoryName, decimal Revenue)> GetRevenueByCategory(DateTime from, DateTime to, int topN = 10000)
        {
            var list = new List<(string, decimal)>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = @"SELECT IFNULL(c.Name, 'Uncategorized') as CategoryName, SUM(ii.LineTotal) as Revenue FROM InvoiceItems ii JOIN Invoices i ON i.Id = ii.InvoiceId LEFT JOIN Products p ON p.Id = ii.ProductId LEFT JOIN Categories c ON c.Id = p.CategoryId WHERE i.CreatedDate BETWEEN @from AND @to GROUP BY IFNULL(c.Name, 'Uncategorized') ORDER BY Revenue DESC LIMIT @top";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@from", from); cmd.Parameters.AddWithValue("@to", to); cmd.Parameters.AddWithValue("@top", topN);
            using var r = cmd.ExecuteReader();
            while (r.Read()) list.Add((r.IsDBNull(0) ? "Uncategorized" : r.GetString(0), r.IsDBNull(1) ? 0m : r.GetDecimal(1)));
            return list;
        }

        public static int GetTotalInvoices()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM Invoices", connection);
            return Convert.ToInt32(cmd.ExecuteScalar() ?? 0);
        }

        public static decimal GetTotalRevenue()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT IFNULL(SUM(Total), 0) FROM Invoices", connection);
            return Convert.ToDecimal(cmd.ExecuteScalar() ?? 0);
        }

        public static (DateTime? oldestDate, DateTime? newestDate) GetInvoiceDateRange()
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            using var cmd = new MySqlCommand("SELECT MIN(CreatedDate), MAX(CreatedDate) FROM Invoices", connection);
            using var reader = cmd.ExecuteReader();
            if (reader.Read()) return (reader.IsDBNull(0) ? (DateTime?)null : reader.GetDateTime(0), reader.IsDBNull(1) ? (DateTime?)null : reader.GetDateTime(1));
            return (null, null);
        }

        public class InvoiceHeader
        {
            public int Id { get; set; }
            public DateTime CreatedDate { get; set; }
            public string CustomerName { get; set; } = string.Empty;
            public decimal Subtotal { get; set; }
            public decimal TaxPercent { get; set; }
            public decimal TaxAmount { get; set; }
            public decimal DiscountAmount { get; set; }
            public decimal Total { get; set; }
            public decimal Paid { get; set; }
            public string CustomerPhone { get; set; } = string.Empty;
            public string CustomerEmail { get; set; } = string.Empty;
            public string CustomerAddress { get; set; } = string.Empty;
            public int EmployeeId { get; set; }
        }

        public class InvoiceItemDetail
        {
            public int ProductId { get; set; }
            public string ProductName { get; set; } = string.Empty;
            public decimal UnitPrice { get; set; }
            public int Quantity { get; set; }
            public decimal LineTotal { get; set; }
        }
    }
}
