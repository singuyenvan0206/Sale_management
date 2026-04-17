using Dapper;
using FashionStore.Core.Interfaces;
using FashionStore.Core.Models;
using MySql.Data.MySqlClient;

namespace FashionStore.Data.Repositories
{
    public class PromotionRepository : MySqlRepositoryBase, IPromotionRepository
    {
        public async Task<IEnumerable<Promotion>> GetAllAsync()
        {
            using var connection = GetConnection();
            return await connection.QueryAsync<Promotion>("SELECT * FROM Promotions ORDER BY Id DESC");
        }

        public async Task<Promotion?> GetByIdAsync(int id)
        {
            using var connection = GetConnection();
            return await connection.QueryFirstOrDefaultAsync<Promotion>("SELECT * FROM Promotions WHERE Id = @Id", new { Id = id });
        }

        public async Task<bool> AddAsync(Promotion entity)
        {
            using var connection = GetConnection();
            string sql = @"INSERT INTO Promotions 
                (Name, Type, StartDate, EndDate, DiscountPercent, DiscountAmount, RequiredProductId, RequiredQuantity, RewardProductId, RewardQuantity, TargetCategoryId, IsActive) 
                VALUES 
                (@Name, @Type, @StartDate, @EndDate, @DiscountPercent, @DiscountAmount, @RequiredProductId, @RequiredQuantity, @RewardProductId, @RewardQuantity, @TargetCategoryId, @IsActive)";
            return await connection.ExecuteAsync(sql, entity) > 0;
        }

        public async Task<bool> UpdateAsync(Promotion entity)
        {
            using var connection = GetConnection();
            string sql = @"UPDATE Promotions SET 
                Name=@Name, Type=@Type, StartDate=@StartDate, EndDate=@EndDate, 
                DiscountPercent=@DiscountPercent, DiscountAmount=@DiscountAmount, 
                RequiredProductId=@RequiredProductId, RequiredQuantity=@RequiredQuantity, 
                RewardProductId=@RewardProductId, RewardQuantity=@RewardQuantity, 
                TargetCategoryId=@TargetCategoryId, IsActive=@IsActive 
                WHERE Id=@Id";
            return await connection.ExecuteAsync(sql, entity) > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            return await connection.ExecuteAsync("DELETE FROM Promotions WHERE Id = @Id", new { Id = id }) > 0;
        }

        public async Task<IEnumerable<Promotion>> GetActivePromotionsAsync()
        {
            using var connection = GetConnection();
            string sql = @"SELECT * FROM Promotions 
                           WHERE IsActive = 1 
                           AND StartDate <= NOW() 
                           AND EndDate >= NOW()";
            return await connection.QueryAsync<Promotion>(sql);
        }

        public async Task<bool> IsProductInActivePromotionAsync(int productId)
        {
            using var connection = GetConnection();
            string sql = @"SELECT COUNT(*) FROM Promotions 
                           WHERE IsActive = 1 
                           AND StartDate <= NOW() 
                           AND EndDate >= NOW()
                           AND (RequiredProductId = @ProductId OR RewardProductId = @ProductId 
                                OR TargetCategoryId IN (SELECT CategoryId FROM Products WHERE Id = @ProductId))";
            return await connection.ExecuteScalarAsync<int>(sql, new { ProductId = productId }) > 0;
        }
    }
}
