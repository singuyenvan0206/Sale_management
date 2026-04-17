using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public class PromotionService : IPromotionService
    {
        private readonly IPromotionRepository _promotionRepository;
        private readonly IProductRepository _productRepository;

        public PromotionService(IPromotionRepository promotionRepository, IProductRepository productRepository)
        {
            _promotionRepository = promotionRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<Promotion>> GetAllPromotionsAsync()
        {
            return await _promotionRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
        {
            return await _promotionRepository.GetActivePromotionsAsync();
        }

        public async Task<Promotion?> GetPromotionByIdAsync(int id)
        {
            return await _promotionRepository.GetByIdAsync(id);
        }

        public async Task<bool> AddPromotionAsync(Promotion promotion)
        {
            if (string.IsNullOrWhiteSpace(promotion.Name)) return false;
            return await _promotionRepository.AddAsync(promotion);
        }

        public async Task<bool> UpdatePromotionAsync(Promotion promotion)
        {
            if (promotion.Id <= 0 || string.IsNullOrWhiteSpace(promotion.Name)) return false;
            return await _promotionRepository.UpdateAsync(promotion);
        }

        public async Task<bool> DeletePromotionAsync(int id)
        {
            return await _promotionRepository.DeleteAsync(id);
        }

        public async Task<decimal> CalculateDiscountedPriceAsync(int productId, decimal originalPrice)
        {
            var activePromos = await _promotionRepository.GetActivePromotionsAsync();
            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null) return originalPrice;

            decimal bestPrice = originalPrice;

            foreach (var promo in activePromos)
            {
                if (promo.Type == Promotion.TypeFlashSale)
                {
                    // Check if promo applies to this product specifically, or its category
                    bool applies = promo.RequiredProductId == productId || promo.TargetCategoryId == product.CategoryId;
                    
                    if (applies)
                    {
                        decimal currentPrice = originalPrice;
                        if (promo.DiscountPercent > 0)
                        {
                            currentPrice = originalPrice * (1 - (promo.DiscountPercent / 100));
                        }
                        else if (promo.DiscountAmount > 0)
                        {
                            currentPrice = originalPrice - promo.DiscountAmount;
                        }

                        if (currentPrice < bestPrice) bestPrice = currentPrice;
                    }
                }
            }

            return Math.Max(0, bestPrice);
        }

        // Bridge for legacy static calls
        private static IPromotionService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<IPromotionService>() ?? throw new InvalidOperationException("DI not initialized");
        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();

        public static List<Promotion> GetAllPromotions() => RunSync(() => GetService().GetAllPromotionsAsync()).ToList();
        public static List<Promotion> GetActivePromotions() => RunSync(() => GetService().GetActivePromotionsAsync()).ToList();
        public static Promotion? GetPromotionById(int id) => RunSync(() => GetService().GetPromotionByIdAsync(id));
        public static bool AddPromotion(Promotion promotion) => RunSync(() => GetService().AddPromotionAsync(promotion));
        public static bool UpdatePromotion(Promotion promotion) => RunSync(() => GetService().UpdatePromotionAsync(promotion));
        public static bool DeletePromotion(int id) => RunSync(() => GetService().DeletePromotionAsync(id));
    }
}
