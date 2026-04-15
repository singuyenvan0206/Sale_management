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
    }
}

