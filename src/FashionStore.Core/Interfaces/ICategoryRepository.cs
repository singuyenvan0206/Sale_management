using FashionStore.Core.Models;

namespace FashionStore.Core.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<bool> HasProductsAsync(int categoryId);
    }
}
