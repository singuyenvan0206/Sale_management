using FashionStore.Core.Models;

namespace FashionStore.Services
{
    public interface IPromotionService
    {
        Task<IEnumerable<Promotion>> GetAllPromotionsAsync();
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
        Task<Promotion?> GetPromotionByIdAsync(int id);
        Task<bool> AddPromotionAsync(Promotion promotion);
        Task<bool> UpdatePromotionAsync(Promotion promotion);
        Task<bool> DeletePromotionAsync(int id);
        Task<decimal> CalculateDiscountedPriceAsync(int productId, decimal originalPrice);
    }
}
