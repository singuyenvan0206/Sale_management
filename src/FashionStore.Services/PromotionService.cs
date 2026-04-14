using Dapper;
using FashionStore.Core.Models;
using FashionStore.Core.Settings;
using MySql.Data.MySqlClient;

namespace FashionStore.Services
{
    public static class PromotionService
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static List<Promotion> GetAllPromotions()
        {
            using var connection = new MySqlConnection(ConnectionString);
            return connection.Query<Promotion>("SELECT * FROM Promotions ORDER BY Id").ToList();
        }

        public static List<Promotion> GetActivePromotions()
        {
            using var connection = new MySqlConnection(ConnectionString);
            string sql = @"SELECT * FROM Promotions 
                           WHERE IsActive = 1 
                           AND StartDate <= NOW() 
                           AND EndDate >= NOW()";
            return connection.Query<Promotion>(sql).ToList();
        }

        public static bool AddPromotion(Promotion promo)
        {
            using var connection = new MySqlConnection(ConnectionString);
            string sql = @"INSERT INTO Promotions 
                (Name, Type, StartDate, EndDate, DiscountPercent, DiscountAmount, RequiredProductId, RequiredQuantity, RewardProductId, RewardQuantity, TargetCategoryId, IsActive) 
                VALUES 
                (@Name, @Type, @StartDate, @EndDate, @DiscountPercent, @DiscountAmount, @RequiredProductId, @RequiredQuantity, @RewardProductId, @RewardQuantity, @TargetCategoryId, @IsActive)";
            return connection.Execute(sql, promo) > 0;
        }

        public static bool UpdatePromotion(Promotion promo)
        {
            using var connection = new MySqlConnection(ConnectionString);
            string sql = @"UPDATE Promotions SET 
                Name=@Name, Type=@Type, StartDate=@StartDate, EndDate=@EndDate, 
                DiscountPercent=@DiscountPercent, DiscountAmount=@DiscountAmount, 
                RequiredProductId=@RequiredProductId, RequiredQuantity=@RequiredQuantity, 
                RewardProductId=@RewardProductId, RewardQuantity=@RewardQuantity, 
                TargetCategoryId=@TargetCategoryId, IsActive=@IsActive 
                WHERE Id=@Id";
            return connection.Execute(sql, promo) > 0;
        }

        public static bool DeletePromotion(int id)
        {
            using var connection = new MySqlConnection(ConnectionString);
            string sql = "DELETE FROM Promotions WHERE Id=@Id";
            return connection.Execute(sql, new { Id = id }) > 0;
        }
    }
}
