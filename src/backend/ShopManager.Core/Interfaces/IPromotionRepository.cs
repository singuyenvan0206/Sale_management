using ShopManager.Core.Models;

namespace ShopManager.Core.Interfaces
{
    public interface IPromotionRepository : IRepository<Promotion>
    {
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
        Task<bool> IsProductInActivePromotionAsync(int productId);
    }
}
