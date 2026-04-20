using Dapper;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using System.Text;

namespace FashionStore.Data.Repositories
{
    public class CustomerRepository : MySqlRepositoryBase, ICustomerRepository
    {
        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            using var connection = GetConnection();
            string sql = @"SELECT Id, Name, Phone, Email, Address, CustomerType, Points, IFNULL(TotalSpent, 0) AS TotalSpent 
                           FROM Customers ORDER BY Id ASC LIMIT 10000;";
            return await connection.QueryAsync<Customer>(sql);
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            using var connection = GetConnection();
            string sql = "SELECT Id, Name, Phone, Email, Address, CustomerType, Points, IFNULL(TotalSpent, 0) AS TotalSpent FROM Customers WHERE Id = @Id;";
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
        }

        public async Task<Customer?> GetByPhoneAsync(string phone)
        {
            using var connection = GetConnection();
            string sql = "SELECT Id, Name, Phone, Email, Address, CustomerType, Points, IFNULL(TotalSpent, 0) AS TotalSpent FROM Customers WHERE Phone = @Phone LIMIT 1;";
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Phone = phone });
        }

        public async Task<IEnumerable<Customer>> SearchAsync(string? query = null, string? sortBy = null, bool isDescending = false)
        {
            using var connection = GetConnection();
            var sb = new StringBuilder();
            sb.Append(@"SELECT Id, Name, Phone, Email, Address, CustomerType, Points, IFNULL(TotalSpent, 0) AS TotalSpent 
                        FROM Customers WHERE 1=1");
            
            var p = new DynamicParameters();
            if (!string.IsNullOrWhiteSpace(query))
            {
                sb.Append(" AND (Name LIKE @Q OR Phone LIKE @Q OR Email LIKE @Q)");
                p.Add("Q", $"%{query}%");
            }

            string sortField = "Id";
            switch (sortBy?.ToLower())
            {
                case "name": sortField = "Name"; break;
                case "spent": sortField = "TotalSpent"; break;
                case "points": sortField = "Points"; break;
                case "type": sortField = "CustomerType"; break;
            }
            string direction = isDescending ? "DESC" : "ASC";
            sb.Append($" ORDER BY {sortField} {direction} LIMIT 1000;");

            return await connection.QueryAsync<Customer>(sb.ToString(), p);
        }

        public async Task<bool> AddAsync(Customer entity)
        {
            using var connection = GetConnection();
            string sql = @"INSERT INTO Customers (Name, Phone, Email, Address, CustomerType, Points) 
                           VALUES (@Name, @Phone, @Email, @Address, @CustomerType, @Points);";
            return await connection.ExecuteAsync(sql, entity) > 0;
        }

        public async Task<bool> UpdateAsync(Customer entity)
        {
            using var connection = GetConnection();
            string sql = @"UPDATE Customers SET Name=@Name, Phone=@Phone, Email=@Email, Address=@Address, 
                           CustomerType=@CustomerType, Points=@Points WHERE Id=@Id;";
            return await connection.ExecuteAsync(sql, entity) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            string sql = "DELETE FROM Customers WHERE Id=@Id;";
            return await connection.ExecuteAsync(sql, new { Id = id }) > 0;
        }

        public async Task<bool> UpdateLoyaltyAsync(int customerId, int points, string customerType)
        {
            using var connection = GetConnection();
            string sql = "UPDATE Customers SET Points=@Points, CustomerType=@Tier WHERE Id=@Id;";
            return await connection.ExecuteAsync(sql, new { Points = points, Tier = customerType, Id = customerId }) > 0;
        }

        public async Task<IEnumerable<(string Name, decimal TotalSpent)>> GetTopCustomersAsync(int topN)
        {
            using var connection = GetConnection();
            string sql = @"SELECT c.Name, SUM(i.Total) as TotalSpent
                           FROM Invoices i
                           LEFT JOIN Customers c ON c.Id = i.CustomerId
                           WHERE c.Name IS NOT NULL
                           GROUP BY c.Name
                           ORDER BY TotalSpent DESC
                           LIMIT @Top";
            var result = await connection.QueryAsync<dynamic>(sql, new { Top = topN });
            var list = new List<(string Name, decimal TotalSpent)>();
            foreach (var r in result) list.Add(((string)(r.Name ?? "Unknown"), Convert.ToDecimal(r.TotalSpent)));
            return list;
        }

        public async Task<bool> HasInvoicesAsync(int customerId)
        {
            using var connection = GetConnection();
            string sql = "SELECT COUNT(*) FROM Invoices WHERE CustomerId = @CustomerId;";
            var result = await connection.ExecuteScalarAsync<long>(sql, new { CustomerId = customerId });
            return result > 0;
        }

        public async Task<int> RefreshAllLoyaltyAsync(decimal spendPerPoint, int silverMin, int goldMin, int vipMin)
        {
            if (spendPerPoint <= 0) spendPerPoint = 100000;
            using var connection = GetConnection();
            string sql = @"
                UPDATE Customers 
                SET Points = FLOOR(IFNULL(TotalSpent, 0) / @Spend),
                    CustomerType = CASE
                        WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @VIP THEN 'VIP'
                        WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @Gold THEN 'Gold'
                        WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @Silver THEN 'Silver'
                        ELSE 'Regular'
                    END
                WHERE Id > 0;";
            return await connection.ExecuteAsync(sql, new { Spend = spendPerPoint, VIP = vipMin, Gold = goldMin, Silver = silverMin });
        }

        public async Task<bool> UpdateCustomerLoyaltyFromTotalSpentAsync(int customerId, decimal spendPerPoint, int silverMin, int goldMin, int vipMin)
        {
            if (spendPerPoint <= 0) spendPerPoint = 100000;
            using var connection = GetConnection();
            string sql = @"
                UPDATE Customers 
                SET Points = FLOOR(IFNULL(TotalSpent, 0) / @Spend),
                    CustomerType = CASE
                        WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @VIP THEN 'VIP'
                        WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @Gold THEN 'Gold'
                        WHEN FLOOR(IFNULL(TotalSpent, 0) / @Spend) >= @Silver THEN 'Silver'
                        ELSE 'Regular'
                    END
                WHERE Id = @Id;";
            return await connection.ExecuteAsync(sql, new { Spend = spendPerPoint, VIP = vipMin, Gold = goldMin, Silver = silverMin, Id = customerId }) > 0;
        }

        public async Task<int> BulkImportCustomersAsync(List<Customer> customers)
        {
            if (customers.Count == 0) return 0;
            using var connection = GetConnection();
            await connection.OpenAsync();
            using var tx = await connection.BeginTransactionAsync();

            try
            {
                const string sql = @"INSERT INTO Customers (Name, Phone, Email, Address, CustomerType, Points) 
                                   VALUES (@Name, @Phone, @Email, @Address, @CustomerType, @Points);";

                // Chunk into 500 records per batch for efficiency
                for (int i = 0; i < customers.Count; i += 500)
                {
                    var chunk = customers.Skip(i).Take(500);
                    await connection.ExecuteAsync(sql, chunk, transaction: tx);
                }

                await tx.CommitAsync();
                return customers.Count;
            }
            catch
            {
                await tx.RollbackAsync();
                return -1;
            }
        }

        public async Task<IEnumerable<(int InvoiceId, DateTime CreatedAt, int ItemCount, decimal Total)>> GetCustomerPurchaseHistoryAsync(int customerId)
        {
            var sql = @"
                SELECT 
                    i.Id AS InvoiceId, 
                    i.CreatedDate AS CreatedAt, 
                    CAST(COALESCE(SUM(ii.Quantity), 0) AS SIGNED) AS ItemCount, 
                    i.Total AS Total 
                FROM Invoices i
                LEFT JOIN InvoiceItems ii ON i.Id = ii.InvoiceId
                WHERE i.CustomerId = @CustomerId
                GROUP BY i.Id, i.CreatedDate, i.Total
                ORDER BY i.Id ASC;";
            
            using var connection = GetConnection();
            var result = await connection.QueryAsync<dynamic>(sql, new { CustomerId = customerId });
            
            var list = new List<(int InvoiceId, DateTime CreatedAt, int ItemCount, decimal Total)>();
            foreach (var r in result)
            {
                list.Add((
                    (int)r.InvoiceId, 
                    (DateTime)r.CreatedAt, 
                    (int)r.ItemCount, 
                    Convert.ToDecimal(r.Total)
                ));
            }
            return list;
        }
    }
}
