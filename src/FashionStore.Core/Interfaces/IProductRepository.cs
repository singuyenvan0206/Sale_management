using FashionStore.Core.Models;

namespace FashionStore.Core.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllWithCategoriesAsync();
        Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedWithCategoriesAsync(int pageIndex, int pageSize);
        Task<Product?> GetByCodeAsync(string code);
        Task<Product?> GetByIdAsync(int id);
        Task<bool> AddAsync(Product product);
        Task<bool> UpdateAsync(Product product);
        Task<bool> DeleteAsync(int id);
        Task<int> GetStockQuantityAsync(int productId);
        Task<IEnumerable<(string ProductName, int StockQuantity, string CategoryName)>> GetLowStockProductsAsync(int threshold);
        Task<int> GetTotalProductsAsync();
        Task<bool> DeleteAllProductsAsync();
        Task<IEnumerable<(string ProductName, int Quantity, decimal Revenue)>> GetTopProductsAsync(int topN);
        Task<bool> DoesProductIdExistAsync(int productId);
        Task<int> FindProductIdByNameAsync(string productName);

        // Variant Support
        Task<IEnumerable<ProductVariant>> GetVariantsAsync(int productId);
        Task<ProductVariant?> GetVariantByBarcodeAsync(string barcode);
        Task<ProductVariant?> GetVariantByIdAsync(int variantId);
        Task<bool> AddVariantAsync(ProductVariant variant);
        Task<bool> UpdateVariantAsync(ProductVariant variant);
        Task<bool> DeleteVariantAsync(int variantId);
        Task<bool> AdjustStockAsync(int productId, int delta);
        Task<bool> AdjustVariantStockAsync(int variantId, int delta);
    }
}
