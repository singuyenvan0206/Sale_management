using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FashionStore.Data.Interfaces;
using FashionStore.Models;

namespace FashionStore.Data.Repositories
{
    public class ProductRepository : MySqlRepositoryBase, IProductRepository
    {
        public async Task<IEnumerable<Product>> GetAllWithCategoriesAsync()
        {
            using var connection = GetConnection();
            string sql = @"SELECT p.Id, p.Name, p.Code, p.CategoryId, c.Name as CategoryName, p.SalePrice,
                                  IFNULL(p.PromoDiscountPercent, 0) AS PromoDiscountPercent,
                                  p.PromoStartDate, p.PromoEndDate,
                                  p.PurchasePrice, p.PurchaseUnit, p.ImportQuantity, p.StockQuantity, p.Description,
                                  IFNULL(c.TaxPercent, 0) AS CategoryTaxPercent,
                                  IFNULL(p.SupplierId, 0) AS SupplierId,
                                  IFNULL(s.Name, '') AS SupplierName
                           FROM Products p 
                           LEFT JOIN Categories c ON p.CategoryId = c.Id 
                           LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                           ORDER BY p.Id ASC
                           LIMIT 10000;";
            return await connection.QueryAsync<Product>(sql);
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            using var connection = GetConnection();
            string sql = @"SELECT p.Id, p.Name, p.Code, p.CategoryId, c.Name as CategoryName, p.SalePrice,
                                  IFNULL(p.PromoDiscountPercent, 0) AS PromoDiscountPercent,
                                  p.PromoStartDate, p.PromoEndDate,
                                  p.PurchasePrice, p.PurchaseUnit, p.ImportQuantity, p.StockQuantity, p.Description,
                                  IFNULL(c.TaxPercent, 0) AS CategoryTaxPercent,
                                  IFNULL(p.SupplierId, 0) AS SupplierId,
                                  IFNULL(s.Name, '') AS SupplierName
                           FROM Products p
                           LEFT JOIN Categories c ON p.CategoryId = c.Id
                           LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                           WHERE p.Code = @Code LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Code = code });
        }

        public async Task<bool> AddAsync(Product p)
        {
            using var connection = GetConnection();
            string sql = @"INSERT INTO Products (Name, Code, CategoryId, SalePrice, PromoDiscountPercent, 
                                                PromoStartDate, PromoEndDate, PurchasePrice, PurchaseUnit, 
                                                ImportQuantity, StockQuantity, Description, SupplierId) 
                           VALUES (@Name, @Code, @CategoryId, @SalePrice, @PromoDiscountPercent, 
                                   @PromoStartDate, @PromoEndDate, @PurchasePrice, @PurchaseUnit, 
                                   @ImportQuantity, @StockQuantity, @Description, @SupplierId);";
            return await connection.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> UpdateAsync(Product p)
        {
            using var connection = GetConnection();
            string sql = @"UPDATE Products SET Name=@Name, Code=@Code, CategoryId=@CategoryId, SalePrice=@SalePrice, 
                           PromoDiscountPercent=@PromoDiscountPercent, PromoStartDate=@PromoStartDate, PromoEndDate=@PromoEndDate, 
                           PurchasePrice=@PurchasePrice, PurchaseUnit=@PurchaseUnit, ImportQuantity=@ImportQuantity, 
                           StockQuantity=@StockQuantity, Description=@Description, SupplierId=@SupplierId 
                           WHERE Id=@Id;";
            return await connection.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            
            // Check for dependencies (simplified)
            string checkSql = "SELECT COUNT(*) FROM InvoiceItems WHERE ProductId=@Id;";
            long count = await connection.ExecuteScalarAsync<long>(checkSql, new { Id = id });
            if (count > 0) return false;

            string sql = "DELETE FROM Products WHERE Id=@Id;";
            return await connection.ExecuteAsync(sql, new { Id = id }) > 0;
        }

        public async Task<int> GetStockQuantityAsync(int productId)
        {
            using var connection = GetConnection();
            string sql = "SELECT StockQuantity FROM Products WHERE Id = @Id;";
            return await connection.ExecuteScalarAsync<int>(sql, new { Id = productId });
        }

        public async Task<IEnumerable<(string ProductName, int StockQuantity, string CategoryName)>> GetLowStockProductsAsync(int threshold)
        {
            using var connection = GetConnection();
            string sql = @"SELECT p.Name, p.StockQuantity, IFNULL(c.Name, 'Uncategorized') as CategoryName
                           FROM Products p
                           LEFT JOIN Categories c ON c.Id = p.CategoryId
                           WHERE p.StockQuantity <= @Threshold
                           ORDER BY p.StockQuantity ASC
                           LIMIT 100";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Threshold = threshold });
            return result.Select(r => ((string)(r.Name ?? "Unknown"), (int)r.StockQuantity, (string)(r.CategoryName ?? "Uncategorized")));
        }

        public async Task<int> GetTotalProductsAsync()
        {
            using var connection = GetConnection();
            return await connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Products");
        }

        public async Task<bool> DeleteAllProductsAsync()
        {
            using var connection = GetConnection();
            using var tx = connection.BeginTransaction();
            try
            {
                await connection.ExecuteAsync("SET FOREIGN_KEY_CHECKS = 0;", transaction: tx);
                await connection.ExecuteAsync("TRUNCATE TABLE Products;", transaction: tx);
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

        public async Task<IEnumerable<(string ProductName, int Quantity, decimal Revenue)>> GetTopProductsAsync(int topN)
        {
            using var connection = GetConnection();
            string sql = @"SELECT p.Name, SUM(ii.Quantity) as qty, SUM(ii.LineTotal) as rev
                           FROM InvoiceItems ii
                           JOIN Products p ON p.Id = ii.ProductId
                           GROUP BY p.Name
                           ORDER BY qty DESC
                           LIMIT @Top";
            
            var result = await connection.QueryAsync<dynamic>(sql, new { Top = topN });
            var list = new List<(string ProductName, int Quantity, decimal Revenue)>();
            foreach (var r in result) list.Add(((string)(r.Name ?? "(Không rõ)"), Convert.ToInt32(r.qty), Convert.ToDecimal(r.rev)));
            return list;
        }

        public async Task<bool> DoesProductIdExistAsync(int productId)
        {
            using var connection = GetConnection();
            var exists = await connection.ExecuteScalarAsync<int?>("SELECT 1 FROM Products WHERE Id=@Id LIMIT 1;", new { Id = productId });
            return exists != null;
        }

        public async Task<int> FindProductIdByNameAsync(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName)) return 0;
            using var connection = GetConnection();
            return await connection.ExecuteScalarAsync<int>("SELECT Id FROM Products WHERE Name=@Name LIMIT 1;", new { Name = productName });
        }
    }
}
