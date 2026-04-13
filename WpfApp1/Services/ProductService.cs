using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FashionStore.Models;
using FashionStore.Data.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Product>> GetAllProductsWithCategoriesAsync()
        {
            return await _productRepository.GetAllWithCategoriesAsync();
        }

        public async Task<Product?> GetProductByCodeAsync(string code)
        {
            return await _productRepository.GetByCodeAsync(code);
        }

        public async Task<bool> AddProductAsync(Product product)
        {
            return await _productRepository.AddAsync(product);
        }

        public async Task<bool> UpdateProductAsync(Product product)
        {
            return await _productRepository.UpdateAsync(product);
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            return await _productRepository.DeleteAsync(id);
        }

        public async Task<int> GetProductStockQuantityAsync(int productId)
        {
            return await _productRepository.GetStockQuantityAsync(productId);
        }

        public async Task<IEnumerable<(string ProductName, int StockQuantity, string CategoryName)>> GetLowStockProductsAsync(int threshold = 10)
        {
            return await _productRepository.GetLowStockProductsAsync(threshold);
        }

        public async Task<int> GetTotalProductsAsync()
        {
            return await _productRepository.GetTotalProductsAsync();
        }

        public async Task<bool> DeleteAllProductsAsync()
        {
            return await _productRepository.DeleteAllProductsAsync();
        }

        // Bridge for legacy static calls - SHOULD BE DEPRECATED
        private static IProductService GetService() => App.ServiceProvider?.GetRequiredService<IProductService>() ?? throw new InvalidOperationException("DI not initialized");

        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();

        public static List<(int Id, string Name, string Code, int CategoryId, string CategoryName, decimal SalePrice, decimal PromoDiscountPercent, DateTime? PromoStartDate, DateTime? PromoEndDate, decimal PurchasePrice, string PurchaseUnit, int ImportQuantity, int StockQuantity, string Description, decimal CategoryTaxPercent, int SupplierId, string SupplierName)> GetAllProductsWithCategories()
        {
            var products = RunSync(() => GetService().GetAllProductsWithCategoriesAsync());
            return products.Select(p => (
                p.Id, p.Name, p.Code, p.CategoryId, p.CategoryName, p.SalePrice, p.PromoDiscountPercent, 
                p.PromoStartDate, p.PromoEndDate, p.PurchasePrice, p.PurchaseUnit, p.ImportQuantity, 
                p.StockQuantity, p.Description, p.CategoryTaxPercent, p.SupplierId, p.SupplierName
            )).ToList();
        }

        public static bool AddProduct(string name, string code, int categoryId, decimal salePrice, decimal purchasePrice, string purchaseUnit, int importQuantity, int stockQuantity, string description = "", decimal promoDiscountPercent = 0m, DateTime? promoStartDate = null, DateTime? promoEndDate = null, int supplierId = 0)
            => RunSync(() => GetService().AddProductAsync(new Product
            {
                Name = name, Code = code, CategoryId = categoryId, SalePrice = salePrice, 
                PurchasePrice = purchasePrice, PurchaseUnit = purchaseUnit, ImportQuantity = importQuantity, 
                StockQuantity = stockQuantity, Description = description, PromoDiscountPercent = promoDiscountPercent, 
                PromoStartDate = promoStartDate, PromoEndDate = promoEndDate, SupplierId = supplierId
            }));

        public static bool UpdateProduct(int id, string name, string code, int categoryId, decimal salePrice, decimal purchasePrice, string purchaseUnit, int importQuantity, int stockQuantity, string description = "", decimal promoDiscountPercent = 0m, DateTime? promoStartDate = null, DateTime? promoEndDate = null, int supplierId = 0)
            => RunSync(() => GetService().UpdateProductAsync(new Product
            {
                Id = id, Name = name, Code = code, CategoryId = categoryId, SalePrice = salePrice, 
                PurchasePrice = purchasePrice, PurchaseUnit = purchaseUnit, ImportQuantity = importQuantity, 
                StockQuantity = stockQuantity, Description = description, PromoDiscountPercent = promoDiscountPercent, 
                PromoStartDate = promoStartDate, PromoEndDate = promoEndDate, SupplierId = supplierId
            }));

        public static int GetProductStockQuantity(int productId) => RunSync(() => GetService().GetProductStockQuantityAsync(productId));
        public static bool DeleteProduct(int id) => RunSync(() => GetService().DeleteProductAsync(id));
        public static bool DeleteAllProducts() => RunSync(() => GetService().DeleteAllProductsAsync());
        public static int GetTotalProducts() => RunSync(() => GetService().GetTotalProductsAsync());
        public static List<(string ProductName, int StockQuantity, string CategoryName)> GetLowStockProducts(int threshold = 10) => RunSync(() => GetService().GetLowStockProductsAsync(threshold)).ToList();

        public async Task<IEnumerable<(string ProductName, int Quantity, decimal Revenue)>> GetTopProductsAsync(int topN = 10) => await _productRepository.GetTopProductsAsync(topN);
        public static List<(string ProductName, int Quantity, decimal Revenue)> GetTopProducts(int topN = 10) => RunSync(() => GetService().GetTopProductsAsync(topN)).ToList();

        public async Task<bool> DoesProductIdExistAsync(int productId) => await _productRepository.DoesProductIdExistAsync(productId);
        public static bool DoesProductIdExist(int productId) => RunSync(() => GetService().DoesProductIdExistAsync(productId));

        public async Task<int> FindProductIdByNameAsync(string productName) => await _productRepository.FindProductIdByNameAsync(productName);
        public static int FindProductIdByName(string productName) => RunSync(() => GetService().FindProductIdByNameAsync(productName));

        public static Product? GetProductByCode(string code) => RunSync(() => GetService().GetProductByCodeAsync(code));

        // Note: CSV Import/Export methods left as-is for now (static) or moved to another service if needed.
        // To keep this refactoring manageable, I'm focusing on the DB interaction layer.

        public static int ImportProductsFromCsv(string filePath)
        {
            // Placeholder/Legacy - should be refactored to instance service
            return -1; 
        }

        public static bool ExportProductsToCsv(string filePath)
        {
            // Placeholder/Legacy - should be refactored to instance service
            return false;
        }
    }
}
