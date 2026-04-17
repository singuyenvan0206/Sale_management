using FashionStore.Core.Models;

namespace FashionStore.Core.Interfaces
{
    public interface IPromotionRepository : IRepository<Promotion>
    {
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
        Task<bool> IsProductInActivePromotionAsync(int productId);
    }
}
