using Dapper;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using MySql.Data.MySqlClient;

namespace FashionStore.Data.Repositories
{
    public class StockMovementRepository : MySqlRepositoryBase, IStockMovementRepository
    {
        public async Task<IEnumerable<StockMovement>> GetAllAsync()
        {
            using var connection = GetConnection();
            return await connection.QueryAsync<StockMovement>("SELECT * FROM StockMovements ORDER BY CreatedDate DESC");
        }

        public async Task<StockMovement?> GetByIdAsync(int id)
        {
            using var connection = GetConnection();
            return await connection.QueryFirstOrDefaultAsync<StockMovement>("SELECT * FROM StockMovements WHERE Id = @Id", new { Id = id });
        }

        public async Task<bool> AddAsync(StockMovement entity)
        {
            using var connection = GetConnection();
            string sql = @"INSERT INTO StockMovements 
                (ProductId, MovementType, Quantity, PreviousStock, NewStock, ReferenceType, ReferenceId, Notes, EmployeeId, CreatedDate) 
                VALUES 
                (@ProductId, @MovementType, @Quantity, @PreviousStock, @NewStock, @ReferenceType, @ReferenceId, @Notes, @EmployeeId, @CreatedDate)";
            return await connection.ExecuteAsync(sql, entity) > 0;
        }

        public async Task<bool> UpdateAsync(StockMovement entity)
        {
            using var connection = GetConnection();
            string sql = "UPDATE StockMovements SET Notes=@Notes WHERE Id=@Id";
            return await connection.ExecuteAsync(sql, entity) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            return await connection.ExecuteAsync("DELETE FROM StockMovements WHERE Id = @Id", new { Id = id }) > 0;
        }

        public async Task<IEnumerable<StockMovement>> GetByProductIdAsync(int productId)
        {
            using var connection = GetConnection();
            return await connection.QueryAsync<StockMovement>("SELECT * FROM StockMovements WHERE ProductId = @ProductId ORDER BY CreatedDate DESC", new { ProductId = productId });
        }

        public async Task<IEnumerable<StockMovement>> GetLatestMovementsAsync(int count)
        {
            using var connection = GetConnection();
            return await connection.QueryAsync<StockMovement>("SELECT * FROM StockMovements ORDER BY CreatedDate DESC LIMIT @Count", new { Count = count });
        }
    }
}
