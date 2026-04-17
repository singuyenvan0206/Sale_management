using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FashionStore.Core.Models;

namespace FashionStore.Core.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsWithCategoriesAsync();
        Task<PaginatedList<Product>> GetPagedProductsAsync(int pageIndex, int pageSize);
        Task<Product?> GetProductByCodeAsync(string code);
        Task<bool> AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<int> GetProductStockQuantityAsync(int productId);
        Task<IEnumerable<(string ProductName, int StockQuantity, string CategoryName)>> GetLowStockProductsAsync(int threshold = 10);
        Task<bool> UpdateProductStockAsync(int productId, int newQuantity);
        Task<Product?> GetProductByIdAsync(int id);
        Task<int> GetTotalProductsAsync();
        Task<bool> DeleteAllProductsAsync();
        Task<IEnumerable<(string ProductName, int Quantity, decimal Revenue)>> GetTopProductsAsync(int topN = 10);
        Task<bool> DoesProductIdExistAsync(int productId);
        Task<int> FindProductIdByNameAsync(string productName);

        // Variant Support
        Task<IEnumerable<ProductVariant>> GetProductVariantsAsync(int productId);
        Task<ProductVariant?> GetVariantByBarcodeAsync(string barcode);
        Task<ProductVariant?> GetVariantByIdAsync(int variantId);
        Task<bool> AddVariantAsync(ProductVariant variant);
        Task<bool> UpdateVariantAsync(ProductVariant variant);
        Task<bool> DeleteVariantAsync(int variantId);
        Task<bool> UpdateVariantStockAsync(int variantId, int newQuantity);

        // CSV Support
        Task<bool> ExportProductsToCsvAsync(string filePath);
        Task<int> ImportProductsFromCsvAsync(string filePath);
    }
}

