using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using Microsoft.Extensions.DependencyInjection;

namespace FashionStore.Services
{
    public class StockMovementService
    {
        private readonly IStockMovementRepository _repository;

        public StockMovementService(IStockMovementRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<StockMovement>> GetLatestAsync(int count = 100)
        {
            return await _repository.GetLatestMovementsAsync(count);
        }

        // Bridge for legacy static calls (WPF)
        private static StockMovementService GetService() => ServiceLocator.ServiceProvider?.GetRequiredService<StockMovementService>() ?? throw new InvalidOperationException("DI not initialized");
        private static T RunSync<T>(Func<Task<T>> func) => Task.Run(func).GetAwaiter().GetResult();

        public static List<StockMovement> GetLatestMovements(int count = 100) => RunSync(() => GetService().GetLatestAsync(count)).ToList();
    }
}
