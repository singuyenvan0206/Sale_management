using System.Collections.Generic;
using System.Threading.Tasks;
using FashionStore.Models;

namespace FashionStore.Data.Interfaces
{
    public interface ICategoryRepository : IRepository<Category>
    {
        Task<Category?> GetByNameAsync(string name);
        Task<bool> HasProductsAsync(int categoryId);
    }
}
