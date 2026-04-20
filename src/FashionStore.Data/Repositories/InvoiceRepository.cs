using Dapper;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using System.Text;

namespace FashionStore.Data.Repositories
{
    public class InvoiceRepository : MySqlRepositoryBase, IInvoiceRepository
    {
        public async Task<int> GetNextInvoiceIdAsync()
        {
            using var connection = GetConnection();
            string sql = @"
                SELECT MIN(t1.Id + 1) 
                FROM (SELECT Id FROM Invoices UNION SELECT 0 AS Id) t1
                WHERE NOT EXISTS (SELECT 1 FROM Invoices t2 WHERE t2.Id = t1.Id + 1);";
            var result = await connection.ExecuteScalarAsync<int?>(sql);
            return result ?? 1;
        }

        public async Task<bool> SaveInvoiceAsync(Invoice invoice, int? voucherId)
        {
            using var connection = GetConnection();
            connection.Open();
            using var tx = connection.BeginTransaction();

            try
            {
                string insertInvoice = @"INSERT INTO Invoices 
                    (Id, CustomerId, EmployeeId, Subtotal, TaxPercent, TaxAmount, DiscountAmount, Total, Paid, PaymentMethod, CreatedDate, VoucherId)
                    VALUES 
                    (@Id, @CustomerId, @EmployeeId, @Subtotal, @TaxPercent, @TaxAmount, @Discount, @Total, @Paid, @PaymentMethod, @CreatedDate, @VoucherId);";

                await connection.ExecuteAsync(insertInvoice, new
                {
                    invoice.Id,
                    invoice.CustomerId,
                    invoice.EmployeeId,
                    invoice.Subtotal,
                    invoice.TaxPercent,
                    invoice.TaxAmount,
                    invoice.Discount,
                    invoice.Total,
                    invoice.Paid,
                    invoice.PaymentMethod,
                    invoice.CreatedDate,
                    VoucherId = voucherId
                }, transaction: tx);

                foreach (var item in invoice.Items)
                {
                    string updateStock = "UPDATE Products SET StockQuantity = StockQuantity - @qty WHERE Id=@pid AND StockQuantity >= @qty;";
                    if (await connection.ExecuteAsync(updateStock, new { qty = item.Quantity, pid = item.ProductId }, transaction: tx) == 0)
                        throw new Exception($"Sản phẩm ID {item.ProductId} đã hết hàng.");

                    string insertItem = @"INSERT INTO InvoiceItems (InvoiceId, ProductId, EmployeeId, UnitPrice, Quantity, LineTotal)
                                          VALUES (@InvoiceId, @ProductId, @EmployeeId, @UnitPrice, @Quantity, @LineTotal);";
                    await connection.ExecuteAsync(insertItem, new
                    {
                        InvoiceId = invoice.Id,
                        item.ProductId,
                        invoice.EmployeeId,
                        item.UnitPrice,
                        item.Quantity,
                        item.LineTotal
                    }, transaction: tx);
                }

                if (voucherId.HasValue)
                {
                    string updateVoucher = "UPDATE Vouchers SET UsedCount = UsedCount + 1 WHERE Id = @vid";
                    await connection.ExecuteAsync(updateVoucher, new { vid = voucherId.Value }, transaction: tx);
                }

                // --- LOGIC FIX: Update Customer TotalSpent and Loyalty ---
                string updateCustSpent = "UPDATE Customers SET TotalSpent = IFNULL(TotalSpent, 0) + @Amount WHERE Id = @Id;";
                await connection.ExecuteAsync(updateCustSpent, new { Amount = invoice.Total, Id = invoice.CustomerId }, transaction: tx);

                // Recalculate Points and Tier based on NEW TotalSpent
                var settings = FashionStore.Core.Models.TierSettingsManager.Load();
                string updateCustLoyalty = @"
                    UPDATE Customers 
                    SET Points = FLOOR(IFNULL(TotalSpent, 0) / @Spend),
                        CustomerType = CASE
                            WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @VIP THEN 'VIP'
                            WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @Gold THEN 'Gold'
                            WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @Silver THEN 'Silver'
                            ELSE 'Regular'
                        END
                    WHERE Id = @Id;";
                await connection.ExecuteAsync(updateCustLoyalty, new
                {
                    Spend = settings.SpendPerPoint,
                    VIP = settings.VIPMinPoints,
                    Gold = settings.GoldMinPoints,
                    Silver = settings.SilverMinPoints,
                    Id = invoice.CustomerId
                }, transaction: tx);
                // --------------------------------------------------------

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        /// <summary>Bulk-import all invoices in a single transaction — does NOT deduct stock.</summary>
        public async Task<int> BulkImportInvoicesAsync(List<Invoice> invoices)
        {
            if (invoices.Count == 0) return 0;
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var tx = await connection.BeginTransactionAsync();

            try
            {
                // 1. Get starting ID
                int nextId = await connection.ExecuteScalarAsync<int>(
                    "SELECT IFNULL(MAX(Id), 0) + 1 FROM Invoices;", transaction: tx);

                var invoiceData = new List<object>();
                var itemData = new List<object>();

                foreach (var inv in invoices)
                {
                    inv.Id = nextId++;
                    invoiceData.Add(new
                    {
                        inv.Id,
                        inv.CustomerId,
                        inv.EmployeeId,
                        inv.Subtotal,
                        inv.TaxPercent,
                        inv.TaxAmount,
                        Discount = inv.Discount,
                        inv.Total,
                        inv.Paid,
                        inv.PaymentMethod,
                        inv.CreatedDate
                    });

                    foreach (var item in inv.Items)
                    {
                        itemData.Add(new
                        {
                            InvoiceId = inv.Id,
                            item.ProductId,
                            inv.EmployeeId,
                            item.UnitPrice,
                            item.Quantity,
                            item.LineTotal
                        });
                    }
                }

                // 2. Batch Insert Invoices (chunks of 500)
                const string insertInvoiceSql = @"INSERT INTO Invoices
                    (Id, CustomerId, EmployeeId, Subtotal, TaxPercent, TaxAmount, DiscountAmount, Total, Paid, PaymentMethod, CreatedDate)
                    VALUES
                    (@Id, @CustomerId, @EmployeeId, @Subtotal, @TaxPercent, @TaxAmount, @Discount, @Total, @Paid, @PaymentMethod, @CreatedDate);";

                // Dapper's ExecuteAsync with IEnumerable is efficient, but we chunk to be safe
                for (int i = 0; i < invoiceData.Count; i += 500)
                {
                    var chunk = invoiceData.Skip(i).Take(500);
                    await connection.ExecuteAsync(insertInvoiceSql, chunk, transaction: tx);
                }

                // 3. Batch Insert Items (chunks of 1000)
                const string insertItemSql = @"INSERT INTO InvoiceItems (InvoiceId, ProductId, EmployeeId, UnitPrice, Quantity, LineTotal)
                                              VALUES (@InvoiceId, @ProductId, @EmployeeId, @UnitPrice, @Quantity, @LineTotal);";

                for (int i = 0; i < itemData.Count; i += 1000)
                {
                    var chunk = itemData.Skip(i).Take(1000);
                    await connection.ExecuteAsync(insertItemSql, chunk, transaction: tx);
                }

                await tx.CommitAsync();

                // --- LOGIC FIX: Refresh TotalSpent and Loyalty for ALL customers after Bulk Import ---
                // We do this OUTSIDE the transaction above to avoid locking too much for too long, 
                // but still as part of the overall "Import" operation.
                try
                {
                    string refreshAll = @"
                        UPDATE Customers c
                        SET c.TotalSpent = (SELECT IFNULL(SUM(i.Total), 0) FROM Invoices i WHERE i.CustomerId = c.Id),
                            c.Points = 0, -- Will be set below
                            c.CustomerType = 'Regular'; -- Will be set below

                        -- Load settings for recalculation
                        -- Note: In a real SQL procedure, we would pass these as variables.
                        -- Here we will just run the global refresh logic we already have.
                    ";
                    await connection.ExecuteAsync(refreshAll);

                    var settings = FashionStore.Core.Models.TierSettingsManager.Load();
                    string globalSync = @"
                        UPDATE Customers 
                        SET Points = FLOOR(IFNULL(TotalSpent, 0) / @Spend),
                            CustomerType = CASE
                                WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @VIP THEN 'VIP'
                                WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @Gold THEN 'Gold'
                                WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @Silver THEN 'Silver'
                                ELSE 'Regular'
                            END
                        WHERE Id > 0;";
                    await connection.ExecuteAsync(globalSync, new
                    {
                        Spend = settings.SpendPerPoint,
                        VIP = settings.VIPMinPoints,
                        Gold = settings.GoldMinPoints,
                        Silver = settings.SilverMinPoints
                    });
                }
                catch (Exception ex)
                {
                    // If refresh fails, we still return the count of imported invoices, 
                    // but data might be stale until manual sync.
                    System.Diagnostics.Debug.WriteLine($"Loyalty refresh failed after import: {ex.Message}");
                }
                // --------------------------------------------------------------------------------------

                return invoices.Count;
            }
            catch
            {
                if (tx != null) await tx.RollbackAsync();
                return -1;
            }
        }

        /// <summary>Single JOIN query — fetches all invoices with their items. O(1) round-trips.</summary>
        public async Task<List<(Invoice Header, List<InvoiceItem> Items)>> GetAllInvoicesWithItemsAsync()
        {
            using var connection = GetConnection();
            const string sql = @"
                SELECT 
                    i.Id, i.CustomerId, i.EmployeeId, i.Subtotal, i.TaxPercent, i.TaxAmount,
                    IFNULL(i.DiscountAmount,0) AS Discount, i.Total, i.Paid, IFNULL(i.PaymentMethod, 'Cash') AS PaymentMethod, i.CreatedDate, i.VoucherId,
                    IFNULL(c.Name,'')    AS CustomerName,
                    IFNULL(c.Phone,'')   AS CustomerPhone,
                    IFNULL(c.Email,'')   AS CustomerEmail,
                    IFNULL(c.Address,'') AS CustomerAddress,
                    ii.Id AS ItemId, ii.ProductId, ii.EmployeeId AS ItemEmployeeId,
                    ii.UnitPrice, ii.Quantity, ii.LineTotal,
                    IFNULL(p.Name,'') AS ProductName
                FROM Invoices i
                LEFT JOIN Customers c  ON c.Id  = i.CustomerId
                LEFT JOIN InvoiceItems ii ON ii.InvoiceId = i.Id
                LEFT JOIN Products p   ON p.Id  = ii.ProductId
                ORDER BY i.Id ASC, ii.Id ASC;";

            // Use Dapper multi-map to split Invoice + InvoiceItem per row
            var invoiceDict = new System.Collections.Generic.Dictionary<int, (Invoice Header, List<InvoiceItem> Items)>();

            await connection.QueryAsync<Invoice, InvoiceItem, Invoice>(
                sql,
                (inv, item) =>
                {
                    if (!invoiceDict.TryGetValue(inv.Id, out var entry))
                    {
                        entry = (inv, new List<InvoiceItem>());
                        invoiceDict[inv.Id] = entry;
                    }
                    if (item?.ProductId > 0)
                        entry.Items.Add(item);
                    return inv;
                },
                splitOn: "ItemId");

            return invoiceDict.Values.ToList();
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime from, DateTime to)
        {
            using var connection = GetConnection();
            string sql = @"SELECT i.Id, i.CustomerId, i.EmployeeId, i.Subtotal, i.TaxPercent, i.TaxAmount, 
                                  IFNULL(i.DiscountAmount, 0) AS Discount, i.Total, i.Paid, IFNULL(i.PaymentMethod, 'Cash') AS PaymentMethod, i.CreatedDate, i.VoucherId,
                                  IFNULL(c.Name, '') AS CustomerName
                           FROM Invoices i
                           LEFT JOIN Customers c ON c.Id = i.CustomerId
                           WHERE i.CreatedDate BETWEEN @from AND @to 
                           ORDER BY i.Id ASC;";
            return await connection.QueryAsync<Invoice>(sql, new { from, to });
        }

        public async Task<IEnumerable<Invoice>> SearchInvoicesAsync(DateTime? from, DateTime? to, int? customerId, string search, string? status = null, string? sortBy = null, bool isDescending = true)
        {
            using var connection = GetConnection();
            var sb = new StringBuilder();
            sb.Append(@"SELECT i.Id, i.CustomerId, i.EmployeeId, i.Subtotal, i.TaxPercent, i.TaxAmount, 
                               IFNULL(i.DiscountAmount, 0) AS Discount, i.Total, i.Paid, IFNULL(i.PaymentMethod, 'Cash') AS PaymentMethod, i.CreatedDate, i.VoucherId,
                               IFNULL(c.Name, '') AS CustomerName, i.Status
                        FROM Invoices i
                        LEFT JOIN Customers c ON c.Id = i.CustomerId
                        WHERE 1=1");

            var p = new DynamicParameters();
            if (from.HasValue) { sb.Append(" AND i.CreatedDate >= @from"); p.Add("from", from.Value); }
            if (to.HasValue) { sb.Append(" AND i.CreatedDate <= @to"); p.Add("to", to.Value); }
            if (customerId.HasValue) { sb.Append(" AND i.CustomerId = @cust"); p.Add("cust", customerId.Value); }
            if (!string.IsNullOrWhiteSpace(status)) { sb.Append(" AND i.Status = @status"); p.Add("status", status); }
            if (!string.IsNullOrWhiteSpace(search)) 
            { 
                sb.Append(" AND (c.Name LIKE @q OR CAST(i.Id AS CHAR) LIKE @q OR c.Phone LIKE @q)"); 
                p.Add("q", "%" + search + "%"); 
            }
            
            string sortField = "i.Id";
            switch (sortBy?.ToLower())
            {
                case "date": sortField = "i.CreatedDate"; break;
                case "total": sortField = "i.Total"; break;
                case "customer": sortField = "c.Name"; break;
            }
            string direction = isDescending ? "DESC" : "ASC";
            sb.Append($" ORDER BY {sortField} {direction}");

            return await connection.QueryAsync<Invoice>(sb.ToString(), p);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
        {
            using var connection = GetConnection();
            return await connection.ExecuteScalarAsync<decimal>("SELECT IFNULL(SUM(Total), 0) FROM Invoices WHERE CreatedDate BETWEEN @from AND @to", new { from, to });
        }

        public async Task<decimal> GetTotalCostAsync(DateTime from, DateTime to)
        {
            using var connection = GetConnection();
            // JOIN with Products to get historic/current purchase price for costing
            string sql = @"SELECT IFNULL(SUM(ii.Quantity * p.PurchasePrice), 0)
                           FROM InvoiceItems ii
                           JOIN Invoices i ON i.Id = ii.InvoiceId
                           JOIN Products p ON p.Id = ii.ProductId
                           WHERE i.CreatedDate BETWEEN @from AND @to;";
            return await connection.ExecuteScalarAsync<decimal>(sql, new { from, to });
        }

        public async Task<decimal> GetTotalProfitAsync(DateTime from, DateTime to)
        {
            using var connection = GetConnection();
            // Profit = Aggregate(LineTotal - Cost of Goods Sold)
            string sql = @"SELECT IFNULL(SUM(ii.LineTotal - (ii.Quantity * p.PurchasePrice)), 0)
                           FROM InvoiceItems ii
                           JOIN Invoices i ON i.Id = ii.InvoiceId
                           JOIN Products p ON p.Id = ii.ProductId
                           WHERE i.CreatedDate BETWEEN @from AND @to;";
            
            // Note: We might need to subtract global invoice discounts (vouchers/tier) from the total profit
            decimal itemsProfit = await connection.ExecuteScalarAsync<decimal>(sql, new { from, to });
            
            // Subtract global discounts applied to the whole invoice
            string discountSql = "SELECT IFNULL(SUM(DiscountAmount), 0) FROM Invoices WHERE CreatedDate BETWEEN @from AND @to;";
            decimal globalDiscounts = await connection.ExecuteScalarAsync<decimal>(discountSql, new { from, to });
            
            return Math.Max(0, itemsProfit - globalDiscounts);
        }

        public async Task<int> GetInvoiceCountAsync(DateTime from, DateTime to)
        {
            using var connection = GetConnection();
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Invoices WHERE CreatedDate BETWEEN @from AND @to", new { from, to });
        }

        public async Task<IEnumerable<(DateTime Day, decimal Revenue)>> GetRevenueByDayAsync(DateTime from, DateTime to)
        {
            using var connection = GetConnection();
            string sql = @"SELECT DATE(CreatedDate) as Day, SUM(Total) as Revenue
                           FROM Invoices
                           WHERE CreatedDate BETWEEN @from AND @to
                           GROUP BY DATE(CreatedDate)
                           ORDER BY Day ASC;";
            var result = await connection.QueryAsync<dynamic>(sql, new { from, to });
            var list = new List<(DateTime Day, decimal Revenue)>();
            foreach (var r in result) list.Add(((DateTime)r.Day, Convert.ToDecimal(r.Revenue)));
            return list;
        }

        public async Task<IEnumerable<(string CategoryName, decimal Revenue)>> GetRevenueByCategoryAsync(DateTime from, DateTime to)
        {
            using var connection = GetConnection();
            string sql = @"SELECT c.Name as CategoryName, SUM(ii.LineTotal) as Revenue
                           FROM InvoiceItems ii
                           JOIN Invoices i ON i.Id = ii.InvoiceId
                           JOIN Products p ON p.Id = ii.ProductId
                           LEFT JOIN Categories c ON c.Id = p.CategoryId
                           WHERE i.CreatedDate BETWEEN @from AND @to
                           GROUP BY c.Name
                           ORDER BY Revenue DESC;";
            var result = await connection.QueryAsync<dynamic>(sql, new { from, to });
            var list = new List<(string CategoryName, decimal Revenue)>();
            foreach (var r in result) list.Add(((string)(r.CategoryName ?? "Chưa phân loại"), Convert.ToDecimal(r.Revenue)));
            return list;
        }

        public async Task<IEnumerable<(string ProductName, int Quantity)>> GetTopProductsAsync(DateTime from, DateTime to, int limit = 5)
        {
            using var connection = GetConnection();
            string sql = @"SELECT p.Name as ProductName, SUM(ii.Quantity) as Quantity
                           FROM InvoiceItems ii
                           JOIN Invoices i ON i.Id = ii.InvoiceId
                           JOIN Products p ON p.Id = ii.ProductId
                           WHERE i.CreatedDate BETWEEN @from AND @to
                           GROUP BY p.Name
                           ORDER BY Quantity DESC
                           LIMIT @limit;";
            var result = await connection.QueryAsync<dynamic>(sql, new { from, to, limit });
            var list = new List<(string ProductName, int Quantity)>();
            foreach (var r in result) list.Add(((string)r.ProductName, Convert.ToInt32(r.Quantity)));
            return list;
        }

        public async Task<(Invoice Header, List<InvoiceItem> Items)> GetInvoiceDetailsAsync(int invoiceId)
        {
            using var connection = GetConnection();
            string headerSql = @"SELECT i.Id, i.CustomerId, i.EmployeeId, i.Subtotal, i.TaxPercent, i.TaxAmount, 
                                        IFNULL(i.DiscountAmount, 0) AS Discount, i.Total, i.Paid, IFNULL(i.PaymentMethod, 'Cash') AS PaymentMethod, i.CreatedDate, i.VoucherId,
                                        IFNULL(c.Name, '') AS CustomerName,
                                        IFNULL(c.Phone, '') AS CustomerPhone,
                                        IFNULL(c.Email, '') AS CustomerEmail,
                                        IFNULL(c.Address, '') AS CustomerAddress
                                 FROM Invoices i
                                 LEFT JOIN Customers c ON c.Id = i.CustomerId
                                 WHERE i.Id = @Id;";
            var header = await connection.QueryFirstOrDefaultAsync<Invoice>(headerSql, new { Id = invoiceId });
            if (header == null) return (null!, null!);

            string itemsSql = @"SELECT ii.Id, ii.InvoiceId, ii.ProductId, ii.EmployeeId, ii.UnitPrice, ii.Quantity, ii.LineTotal,
                                       IFNULL(p.Name, '') AS ProductName
                                FROM InvoiceItems ii
                                LEFT JOIN Products p ON p.Id = ii.ProductId
                                WHERE ii.InvoiceId = @Id ORDER BY ii.Id;";
            var items = (await connection.QueryAsync<InvoiceItem>(itemsSql, new { Id = invoiceId })).ToList();
            header.Items = items;
            return (header, items);
        }

        public async Task<bool> DeleteAllInvoicesAsync()
        {
            using var connection = GetConnection();
            connection.Open();
            using var tx = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync("SET FOREIGN_KEY_CHECKS = 0;", transaction: tx);
                await connection.ExecuteAsync("TRUNCATE TABLE InvoiceItems;", transaction: tx);
                await connection.ExecuteAsync("TRUNCATE TABLE Invoices;", transaction: tx);
                await connection.ExecuteAsync("SET FOREIGN_KEY_CHECKS = 1;", transaction: tx);
                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                return false;
            }
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            return await GetInvoicesByDateRangeAsync(DateTime.Today.AddYears(-10), DateTime.Today.AddDays(1));
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            var (header, _) = await GetInvoiceDetailsAsync(id);
            return header;
        }

        public async Task<bool> AddAsync(Invoice entity)
        {
            return await SaveInvoiceAsync(entity, entity.VoucherId);
        }

        public async Task<bool> UpdateAsync(Invoice entity) => false;

        public async Task<bool> DeleteAsync(int id)
        {
            return await DeleteInvoiceAsync(id);
        }

        public async Task<bool> DeleteInvoiceAsync(int invoiceId)
        {
            // Keep stock and customer spending consistent when deleting an invoice.
            return await RefundInvoiceAsync(invoiceId);
        }

        public async Task<bool> RefundInvoiceAsync(int invoiceId)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var tx = await connection.BeginTransactionAsync();
            try
            {
                var header = await connection.QueryFirstOrDefaultAsync<(int CustomerId, decimal Total)>(
                    "SELECT CustomerId, Total FROM Invoices WHERE Id = @Id",
                    new { Id = invoiceId }, tx);
                if (header.CustomerId <= 0)
                {
                    await tx.RollbackAsync();
                    return false;
                }

                var items = (await connection.QueryAsync<(int ProductId, int Quantity)>(
                    "SELECT ProductId, Quantity FROM InvoiceItems WHERE InvoiceId = @Id",
                    new { Id = invoiceId }, tx)).ToList();

                foreach (var item in items)
                {
                    await connection.ExecuteAsync(
                        "UPDATE Products SET StockQuantity = StockQuantity + @Qty WHERE Id = @ProductId",
                        new { Qty = item.Quantity, item.ProductId }, tx);
                }

                await connection.ExecuteAsync("DELETE FROM InvoiceItems WHERE InvoiceId = @Id", new { Id = invoiceId }, tx);
                var deleted = await connection.ExecuteAsync("DELETE FROM Invoices WHERE Id = @Id", new { Id = invoiceId }, tx);
                if (deleted == 0)
                {
                    await tx.RollbackAsync();
                    return false;
                }

                await connection.ExecuteAsync(
                    "UPDATE Customers SET TotalSpent = GREATEST(0, IFNULL(TotalSpent, 0) - @Amount) WHERE Id = @CustomerId",
                    new { Amount = header.Total, header.CustomerId }, tx);

                await RecalculateCustomerLoyaltyAsync(connection, tx, header.CustomerId);
                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        private static async Task RecalculateCustomerLoyaltyAsync(System.Data.IDbConnection connection, System.Data.IDbTransaction tx, int customerId)
        {
            var settings = FashionStore.Core.Models.TierSettingsManager.Load();
            const string sql = @"
                UPDATE Customers 
                SET Points = FLOOR(IFNULL(TotalSpent, 0) / @Spend),
                    CustomerType = CASE
                        WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @VIP THEN 'VIP'
                        WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @Gold THEN 'Gold'
                        WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @Silver THEN 'Silver'
                        ELSE 'Regular'
                    END
                WHERE Id = @Id;";
            await connection.ExecuteAsync(sql, new
            {
                Spend = settings.SpendPerPoint,
                VIP = settings.VIPMinPoints,
                Gold = settings.GoldMinPoints,
                Silver = settings.SilverMinPoints,
                Id = customerId
            }, tx);
        }
    }
}
