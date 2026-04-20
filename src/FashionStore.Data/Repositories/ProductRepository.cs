using Dapper;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;

namespace FashionStore.Data.Repositories
{
    public class ProductRepository : MySqlRepositoryBase, IProductRepository
    {
        public async Task<IEnumerable<Product>> GetAllWithCategoriesAsync()
        {
            using var connection = GetConnection();
            string sql = @"SELECT p.*, c.Name as CategoryName, c.TaxPercent as TaxPercent, s.Name as SupplierName,
                                   v.Id AS VariantId, v.Size, v.Color, v.Barcode as VariantBarcode, v.StockQuantity AS VariantStock
                            FROM Products p 
                            LEFT JOIN Categories c ON p.CategoryId = c.Id 
                            LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                            LEFT JOIN ProductVariants v ON v.ProductId = p.Id
                            ORDER BY p.Id ASC;";

            var result = await connection.QueryAsync<dynamic>(sql);
            return MapProductsWithVariants(result);
        }

        public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedWithCategoriesAsync(int pageIndex, int pageSize, string? search = null, int? categoryId = null, string? sortBy = null, bool isDescending = false)
        {
            using var connection = GetConnection();
            int offset = (pageIndex - 1) * pageSize;

            var parameters = new DynamicParameters();
            parameters.Add("PageSize", pageSize);
            parameters.Add("Offset", offset);

            string whereClause = " WHERE 1=1 ";
            if (!string.IsNullOrWhiteSpace(search))
            {
                whereClause += " AND (p.Name LIKE @Search OR p.Code LIKE @Search) ";
                parameters.Add("Search", $"%{search}%");
            }
            if (categoryId.HasValue)
            {
                whereClause += " AND p.CategoryId = @CategoryId ";
                parameters.Add("CategoryId", categoryId.Value);
            }

            string sortField = "p.Id";
            switch (sortBy?.ToLower())
            {
                case "name": sortField = "p.Name"; break;
                case "price": sortField = "p.SalePrice"; break;
                case "stock": sortField = "p.StockQuantity"; break;
                case "code": sortField = "p.Code"; break;
            }
            string direction = isDescending ? "DESC" : "ASC";
            string orderClause = $" ORDER BY {sortField} {direction}";

            // 1. Get total count
            string countSql = $"SELECT COUNT(*) FROM Products p {whereClause};";
            int totalCount = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            // 2. Get paged product ids first
            string idSql = $@"SELECT p.Id FROM Products p 
                            {whereClause} 
                            {orderClause} 
                            LIMIT @PageSize OFFSET @Offset;";
            var productIds = await connection.QueryAsync<int>(idSql, parameters);

            if (!productIds.Any()) return (Enumerable.Empty<Product>(), totalCount);

            // 3. Get full details for these specific ids
            string sql = $@"SELECT p.*, c.Name as CategoryName, c.TaxPercent as TaxPercent, s.Name as SupplierName,
                                   v.Id AS VariantId, v.Size, v.Color, v.Barcode as VariantBarcode, v.StockQuantity AS VariantStock
                            FROM Products p 
                            LEFT JOIN Categories c ON p.CategoryId = c.Id 
                            LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                            LEFT JOIN ProductVariants v ON v.ProductId = p.Id
                            WHERE p.Id IN @Ids
                            {orderClause}, v.Id ASC;";

            var result = await connection.QueryAsync<dynamic>(sql, new { Ids = productIds });
            return (MapProductsWithVariants(result), totalCount);
        }

        private IEnumerable<Product> MapProductsWithVariants(IEnumerable<dynamic> result)
        {
            var productDict = new Dictionary<int, Product>();
            foreach (var row in result)
            {
                int id = (int)row.Id;
                if (!productDict.TryGetValue(id, out var p))
                {
                    p = new Product
                    {
                        Id = id,
                        Name = row.Name,
                        Code = row.Code,
                        CategoryId = (int)row.CategoryId,
                        CategoryName = row.CategoryName ?? "",
                        ImageUrl = row.ImageUrl ?? "",
                        SalePrice = (decimal)row.SalePrice,
                        PurchasePrice = (decimal)row.PurchasePrice,
                        StockQuantity = (int)row.StockQuantity,
                        CategoryTaxPercent = (decimal)(row.TaxPercent ?? 0m),
                        Variants = new List<ProductVariant>()
                    };
                    productDict.Add(id, p);
                }

                if (row.VariantId != null)
                {
                    p.Variants.Add(new ProductVariant
                    {
                        Id = (int)row.VariantId,
                        Size = row.Size ?? "",
                        Color = row.Color ?? "",
                        Barcode = row.VariantBarcode ?? "",
                        StockQuantity = (int)row.VariantStock
                    });
                }
            }
            return productDict.Values;
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            using var connection = GetConnection();
            string sql = @"SELECT p.*, c.Name as CategoryName, c.TaxPercent as CategoryTaxPercent, s.Name as SupplierName
                            FROM Products p 
                            LEFT JOIN Categories c ON p.CategoryId = c.Id 
                            LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                            WHERE p.Id = @Id LIMIT 1;";
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
        }

        public async Task<Product?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;
            using var connection = GetConnection();
            string sql = @"SELECT p.*, c.Name as CategoryName, c.TaxPercent as CategoryTaxPercent, s.Name as SupplierName
                            FROM Products p
                            LEFT JOIN Categories c ON p.CategoryId = c.Id
                            LEFT JOIN Suppliers s ON p.SupplierId = s.Id
                            WHERE p.Code = @Code LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Code = code });
        }

        public async Task<bool> AddAsync(Product p)
        {
            using var connection = GetConnection();
            string sql = @"INSERT INTO Products (Name, Code, CategoryId, ImageUrl, SalePrice, PromoDiscountPercent, 
                                                PromoStartDate, PromoEndDate, PurchasePrice, PurchaseUnit, 
                                                ImportQuantity, StockQuantity, Description, SupplierId) 
                           VALUES (@Name, @Code, @CategoryId, @ImageUrl, @SalePrice, @PromoDiscountPercent, 
                                   @PromoStartDate, @PromoEndDate, @PurchasePrice, @PurchaseUnit, 
                                   @ImportQuantity, @StockQuantity, @Description, @SupplierId);";
            return await connection.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> UpdateAsync(Product p)
        {
            using var connection = GetConnection();
            string sql = @"UPDATE Products SET Name=@Name, Code=@Code, CategoryId=@CategoryId, ImageUrl=@ImageUrl, SalePrice=@SalePrice, 
                           PromoDiscountPercent=@PromoDiscountPercent, PromoStartDate=@PromoStartDate, PromoEndDate=@PromoEndDate, 
                           PurchasePrice=@PurchasePrice, PurchaseUnit=@PurchaseUnit, ImportQuantity=@ImportQuantity, 
                           StockQuantity=@StockQuantity, Description=@Description, SupplierId=@SupplierId 
                           WHERE Id=@Id;";
            return await connection.ExecuteAsync(sql, p) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
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

        // Variant Support Implementation
        public async Task<IEnumerable<ProductVariant>> GetVariantsAsync(int productId)
        {
            using var connection = GetConnection();
            string sql = "SELECT * FROM ProductVariants WHERE ProductId = @ProductId";
            return await connection.QueryAsync<ProductVariant>(sql, new { ProductId = productId });
        }

        public async Task<ProductVariant?> GetVariantByBarcodeAsync(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode)) return null;
            using var connection = GetConnection();
            string sql = "SELECT * FROM ProductVariants WHERE Barcode = @Barcode LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<ProductVariant>(sql, new { Barcode = barcode });
        }

        public async Task<ProductVariant?> GetVariantByIdAsync(int variantId)
        {
            using var connection = GetConnection();
            string sql = "SELECT * FROM ProductVariants WHERE Id = @Id LIMIT 1";
            return await connection.QueryFirstOrDefaultAsync<ProductVariant>(sql, new { Id = variantId });
        }

        public async Task<bool> AddVariantAsync(ProductVariant variant)
        {
            using var connection = GetConnection();
            string sql = @"INSERT INTO ProductVariants (ProductId, Size, Color, Sku, Barcode, StockQuantity, PriceAdjustment)
                           VALUES (@ProductId, @Size, @Color, @Sku, @Barcode, @StockQuantity, @PriceAdjustment)";
            return await connection.ExecuteAsync(sql, variant) > 0;
        }

        public async Task<bool> UpdateVariantAsync(ProductVariant variant)
        {
            using var connection = GetConnection();
            string sql = @"UPDATE ProductVariants 
                           SET Size=@Size, Color=@Color, Sku=@Sku, Barcode=@Barcode, 
                               StockQuantity=@StockQuantity, PriceAdjustment=@PriceAdjustment
                           WHERE Id=@Id";
            return await connection.ExecuteAsync(sql, variant) > 0;
        }

        public async Task<bool> DeleteVariantAsync(int variantId)
        {
            using var connection = GetConnection();
            string sql = "DELETE FROM ProductVariants WHERE Id=@Id";
            return await connection.ExecuteAsync(sql, new { Id = variantId }) > 0;
        }
        
        public async Task<bool> AdjustStockAsync(int productId, int delta)
        {
            using var connection = GetConnection();
            // Use atomic SQL arithmetic to prevent race conditions
            string sql = "UPDATE Products SET StockQuantity = StockQuantity + @Delta WHERE Id = @Id";
            return await connection.ExecuteAsync(sql, new { Id = productId, Delta = delta }) > 0;
        }

        public async Task<bool> AdjustVariantStockAsync(int variantId, int delta)
        {
            using var connection = GetConnection();
            // Use atomic SQL arithmetic to prevent race conditions
            string sql = "UPDATE ProductVariants SET StockQuantity = StockQuantity + @Delta WHERE Id = @Id";
            return await connection.ExecuteAsync(sql, new { Id = variantId, Delta = delta }) > 0;
        }
    }
}
