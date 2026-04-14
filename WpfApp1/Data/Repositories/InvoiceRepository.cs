using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using FashionStore.Data.Interfaces;
using FashionStore.Models;

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
                    (Id, CustomerId, EmployeeId, Subtotal, TaxPercent, TaxAmount, DiscountAmount, Total, Paid, CreatedDate, VoucherId)
                    VALUES 
                    (@Id, @CustomerId, @EmployeeId, @Subtotal, @TaxPercent, @TaxAmount, @Discount, @Total, @Paid, @CreatedDate, @VoucherId);";
                
                await connection.ExecuteAsync(insertInvoice, new {
                    invoice.Id, invoice.CustomerId, invoice.EmployeeId, invoice.Subtotal, invoice.TaxPercent,
                    invoice.TaxAmount, invoice.Discount, invoice.Total, invoice.Paid, invoice.CreatedDate, VoucherId = voucherId
                }, transaction: tx);

                foreach (var item in invoice.Items)
                {
                    string updateStock = "UPDATE Products SET StockQuantity = StockQuantity - @qty WHERE Id=@pid AND StockQuantity >= @qty;";
                    if (await connection.ExecuteAsync(updateStock, new { qty = item.Quantity, pid = item.ProductId }, transaction: tx) == 0)
                        throw new Exception($"Sản phẩm ID {item.ProductId} đã hết hàng.");

                    string insertItem = @"INSERT INTO InvoiceItems (InvoiceId, ProductId, EmployeeId, UnitPrice, Quantity, LineTotal)
                                          VALUES (@InvoiceId, @ProductId, @EmployeeId, @UnitPrice, @Quantity, @LineTotal);";
                    await connection.ExecuteAsync(insertItem, new {
                        InvoiceId = invoice.Id, item.ProductId, invoice.EmployeeId, item.UnitPrice, item.Quantity, item.LineTotal
                    }, transaction: tx);
                }

                if (voucherId.HasValue)
                {
                    string updateVoucher = "UPDATE Vouchers SET UsedCount = UsedCount + 1 WHERE Id = @vid";
                    await connection.ExecuteAsync(updateVoucher, new { vid = voucherId.Value }, transaction: tx);
                }

                tx.Commit();
                return true;
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByDateRangeAsync(DateTime from, DateTime to)
        {
            using var connection = GetConnection();
            string sql = @"SELECT i.Id, i.CustomerId, i.EmployeeId, i.Subtotal, i.TaxPercent, i.TaxAmount, 
                                  IFNULL(i.DiscountAmount, 0) AS Discount, i.Total, i.Paid, i.CreatedDate, i.VoucherId,
                                  IFNULL(c.Name, '') AS CustomerName
                           FROM Invoices i
                           LEFT JOIN Customers c ON c.Id = i.CustomerId
                           WHERE i.CreatedDate BETWEEN @from AND @to 
                           ORDER BY i.Id ASC;";
            return await connection.QueryAsync<Invoice>(sql, new { from, to });
        }

        public async Task<IEnumerable<Invoice>> SearchInvoicesAsync(DateTime? from, DateTime? to, int? customerId, string search)
        {
            using var connection = GetConnection();
            var sb = new StringBuilder();
            sb.Append(@"SELECT i.Id, i.CustomerId, i.EmployeeId, i.Subtotal, i.TaxPercent, i.TaxAmount, 
                               IFNULL(i.DiscountAmount, 0) AS Discount, i.Total, i.Paid, i.CreatedDate, i.VoucherId,
                               IFNULL(c.Name, '') AS CustomerName
                        FROM Invoices i
                        LEFT JOIN Customers c ON c.Id = i.CustomerId
                        WHERE 1=1");

            var p = new DynamicParameters();
            if (from.HasValue) { sb.Append(" AND i.CreatedDate >= @from"); p.Add("@from", from.Value); }
            if (to.HasValue) { sb.Append(" AND i.CreatedDate <= @to"); p.Add("@to", to.Value); }
            if (customerId.HasValue) { sb.Append(" AND i.CustomerId = @cust"); p.Add("@cust", customerId.Value); }
            if (!string.IsNullOrWhiteSpace(search)) { sb.Append(" AND (c.Name LIKE @q OR CAST(i.Id AS CHAR) LIKE @q)"); p.Add("@q", "%" + search + "%"); }
            sb.Append(" ORDER BY i.Id ASC");

            return await connection.QueryAsync<Invoice>(sb.ToString(), p);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime from, DateTime to)
        {
            using var connection = GetConnection();
            return await connection.ExecuteScalarAsync<decimal>("SELECT IFNULL(SUM(Total), 0) FROM Invoices WHERE CreatedDate BETWEEN @from AND @to", new { from, to });
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

        public async Task<(Invoice Header, List<InvoiceItem> Items)> GetInvoiceDetailsAsync(int invoiceId)
        {
            using var connection = GetConnection();
            string headerSql = @"SELECT i.Id, i.CustomerId, i.EmployeeId, i.Subtotal, i.TaxPercent, i.TaxAmount, 
                                        IFNULL(i.DiscountAmount, 0) AS Discount, i.Total, i.Paid, i.CreatedDate, i.VoucherId,
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
            using var connection = GetConnection();
            return await connection.ExecuteAsync("DELETE FROM Invoices WHERE Id = @Id", new { Id = id }) > 0;
        }
    }
}
