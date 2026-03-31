using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using FashionStore.Core;
using FashionStore.Models;

namespace FashionStore.Services
{
    public static class PromotionService
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static List<Promotion> GetAllPromotions()
        {
            var list = new List<Promotion>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "SELECT * FROM Promotions ORDER BY Id";
            using var cmd = new MySqlCommand(sql, connection);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(MapPromotion(r));
            }
            return list;
        }

        public static List<Promotion> GetActivePromotions()
        {
            var list = new List<Promotion>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = @"SELECT * FROM Promotions 
                           WHERE IsActive = 1 
                           AND StartDate <= NOW() 
                           AND EndDate >= NOW()";
            using var cmd = new MySqlCommand(sql, connection);
            using var r = cmd.ExecuteReader();
            while (r.Read())
            {
                list.Add(MapPromotion(r));
            }
            return list;
        }

        public static bool AddPromotion(Promotion promo)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = @"INSERT INTO Promotions 
                (Name, Type, StartDate, EndDate, DiscountPercent, DiscountAmount, RequiredProductId, RequiredQuantity, RewardProductId, RewardQuantity, TargetCategoryId, IsActive) 
                VALUES 
                (@Name, @Type, @StartDate, @EndDate, @DiscountPercent, @DiscountAmount, @RequiredProductId, @RequiredQuantity, @RewardProductId, @RewardQuantity, @TargetCategoryId, @IsActive)";

            using var cmd = new MySqlCommand(sql, connection);
            AddParams(cmd, promo);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool UpdatePromotion(Promotion promo)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = @"UPDATE Promotions SET 
                Name=@Name, Type=@Type, StartDate=@StartDate, EndDate=@EndDate, 
                DiscountPercent=@DiscountPercent, DiscountAmount=@DiscountAmount, 
                RequiredProductId=@RequiredProductId, RequiredQuantity=@RequiredQuantity, 
                RewardProductId=@RewardProductId, RewardQuantity=@RewardQuantity, 
                TargetCategoryId=@TargetCategoryId, IsActive=@IsActive 
                WHERE Id=@Id";

            using var cmd = new MySqlCommand(sql, connection);
            AddParams(cmd, promo);
            cmd.Parameters.AddWithValue("@Id", promo.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DeletePromotion(int id)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "DELETE FROM Promotions WHERE Id=@Id";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        private static void AddParams(MySqlCommand cmd, Promotion promo)
        {
            cmd.Parameters.AddWithValue("@Name", promo.Name);
            cmd.Parameters.AddWithValue("@Type", promo.Type);
            cmd.Parameters.AddWithValue("@StartDate", promo.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", promo.EndDate);
            cmd.Parameters.AddWithValue("@DiscountPercent", promo.DiscountPercent);
            cmd.Parameters.AddWithValue("@DiscountAmount", promo.DiscountAmount);
            cmd.Parameters.AddWithValue("@RequiredProductId", promo.RequiredProductId.HasValue ? promo.RequiredProductId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@RequiredQuantity", promo.RequiredQuantity);
            cmd.Parameters.AddWithValue("@RewardProductId", promo.RewardProductId.HasValue ? promo.RewardProductId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@RewardQuantity", promo.RewardQuantity);
            cmd.Parameters.AddWithValue("@TargetCategoryId", promo.TargetCategoryId.HasValue ? promo.TargetCategoryId.Value : DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", promo.IsActive);
        }

        private static Promotion MapPromotion(MySqlDataReader r)
        {
            return new Promotion
            {
                Id = r.GetInt32("Id"),
                Name = r.GetString("Name"),
                Type = r.GetString("Type"),
                StartDate = r.GetDateTime("StartDate"),
                EndDate = r.GetDateTime("EndDate"),
                DiscountPercent = r.IsDBNull(r.GetOrdinal("DiscountPercent")) ? 0 : r.GetDecimal("DiscountPercent"),
                DiscountAmount = r.IsDBNull(r.GetOrdinal("DiscountAmount")) ? 0 : r.GetDecimal("DiscountAmount"),
                RequiredProductId = r.IsDBNull(r.GetOrdinal("RequiredProductId")) ? (int?)null : r.GetInt32("RequiredProductId"),
                RequiredQuantity = r.IsDBNull(r.GetOrdinal("RequiredQuantity")) ? 0 : r.GetInt32("RequiredQuantity"),
                RewardProductId = r.IsDBNull(r.GetOrdinal("RewardProductId")) ? (int?)null : r.GetInt32("RewardProductId"),
                RewardQuantity = r.IsDBNull(r.GetOrdinal("RewardQuantity")) ? 0 : r.GetInt32("RewardQuantity"),
                TargetCategoryId = r.IsDBNull(r.GetOrdinal("TargetCategoryId")) ? (int?)null : r.GetInt32("TargetCategoryId"),
                IsActive = r.GetBoolean("IsActive"),
                CreatedDate = r.IsDBNull(r.GetOrdinal("CreatedDate")) ? DateTime.Now : r.GetDateTime("CreatedDate")
            };
        }
    }
}
