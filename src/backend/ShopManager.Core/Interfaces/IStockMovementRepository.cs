using ShopManager.Core.Models;

namespace ShopManager.Core.Interfaces
{
    public interface IStockMovementRepository : IRepository<StockMovement>
    {
        Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId);
        Task<IEnumerable<StockMovement>> GetLatestMovementsAsync(int count);
    }
}
