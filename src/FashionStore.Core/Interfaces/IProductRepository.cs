using FashionStore.Core.Models;

namespace FashionStore.Core.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllWithCategoriesAsync();
        Task<Product?> GetByCodeAsync(string code);
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
    }
}
