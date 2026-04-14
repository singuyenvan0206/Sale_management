using FashionStore.Core.Models;

namespace FashionStore.Services
{
    public interface IProductService
    {
        Task<IEnumerable<Product>> GetAllProductsWithCategoriesAsync();
        Task<Product?> GetProductByCodeAsync(string code);
        Task<bool> AddProductAsync(Product product);
        Task<bool> UpdateProductAsync(Product product);
        Task<bool> DeleteProductAsync(int id);
        Task<int> GetProductStockQuantityAsync(int productId);
        Task<IEnumerable<(string ProductName, int StockQuantity, string CategoryName)>> GetLowStockProductsAsync(int threshold = 10);
        Task<int> GetTotalProductsAsync();
        Task<bool> DeleteAllProductsAsync();
        Task<IEnumerable<(string ProductName, int Quantity, decimal Revenue)>> GetTopProductsAsync(int topN = 10);
        Task<bool> DoesProductIdExistAsync(int productId);
        Task<int> FindProductIdByNameAsync(string productName);
    }
}
