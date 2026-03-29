using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace FashionStore.Services
{
    public static class VoucherService
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static List<Voucher> GetAllVouchers()
        {
            var vouchers = new List<Voucher>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "SELECT * FROM Vouchers ORDER BY Id DESC";
            using var cmd = new MySqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                vouchers.Add(new Voucher
                {
                    Id = reader.GetInt32("Id"),
                    Code = reader.GetString("Code"),
                    DiscountType = reader.GetString("DiscountType"),
                    DiscountValue = reader.GetDecimal("DiscountValue"),
                    MinInvoiceAmount = reader.GetDecimal("MinInvoiceAmount"),
                    StartDate = reader.GetDateTime("StartDate"),
                    EndDate = reader.GetDateTime("EndDate"),
                    UsageLimit = reader.GetInt32("UsageLimit"),
                    UsedCount = reader.GetInt32("UsedCount"),
                    IsActive = reader.GetBoolean("IsActive")
                });
            }
            return vouchers;
        }

        public static bool AddVoucher(Voucher voucher)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "INSERT INTO Vouchers (Code, DiscountType, DiscountValue, MinInvoiceAmount, StartDate, EndDate, UsageLimit, IsActive) VALUES (@Code, @DiscountType, @DiscountValue, @MinInvoiceAmount, @StartDate, @EndDate, @UsageLimit, @IsActive)";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Code", voucher.Code);
            cmd.Parameters.AddWithValue("@DiscountType", voucher.DiscountType);
            cmd.Parameters.AddWithValue("@DiscountValue", voucher.DiscountValue);
            cmd.Parameters.AddWithValue("@MinInvoiceAmount", voucher.MinInvoiceAmount);
            cmd.Parameters.AddWithValue("@StartDate", voucher.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", voucher.EndDate);
            cmd.Parameters.AddWithValue("@UsageLimit", voucher.UsageLimit);
            cmd.Parameters.AddWithValue("@IsActive", voucher.IsActive);
            try { return cmd.ExecuteNonQuery() > 0; } catch { return false; }
        }

        public static bool UpdateVoucher(Voucher voucher)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "UPDATE Vouchers SET Code=@Code, DiscountType=@DiscountType, DiscountValue=@DiscountValue, MinInvoiceAmount=@MinInvoiceAmount, StartDate=@StartDate, EndDate=@EndDate, UsageLimit=@UsageLimit, IsActive=@IsActive WHERE Id=@Id";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Code", voucher.Code);
            cmd.Parameters.AddWithValue("@DiscountType", voucher.DiscountType);
            cmd.Parameters.AddWithValue("@DiscountValue", voucher.DiscountValue);
            cmd.Parameters.AddWithValue("@MinInvoiceAmount", voucher.MinInvoiceAmount);
            cmd.Parameters.AddWithValue("@StartDate", voucher.StartDate);
            cmd.Parameters.AddWithValue("@EndDate", voucher.EndDate);
            cmd.Parameters.AddWithValue("@UsageLimit", voucher.UsageLimit);
            cmd.Parameters.AddWithValue("@IsActive", voucher.IsActive);
            cmd.Parameters.AddWithValue("@Id", voucher.Id);
            try { return cmd.ExecuteNonQuery() > 0; } catch { return false; }
        }

        public static bool DeleteVoucher(int id)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "DELETE FROM Vouchers WHERE Id=@Id";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static Voucher? GetVoucherByCode(string code)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "SELECT * FROM Vouchers WHERE Code = @Code AND IsActive = 1 AND StartDate <= NOW() AND EndDate >= NOW()";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Code", code);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Voucher { Id = reader.GetInt32("Id"), Code = reader.GetString("Code"), DiscountType = reader.GetString("DiscountType"), DiscountValue = reader.GetDecimal("DiscountValue"), MinInvoiceAmount = reader.GetDecimal("MinInvoiceAmount"), StartDate = reader.GetDateTime("StartDate"), EndDate = reader.GetDateTime("EndDate"), UsageLimit = reader.GetInt32("UsageLimit"), UsedCount = reader.GetInt32("UsedCount"), IsActive = reader.GetBoolean("IsActive") };
            }
            return null;
        }

        public static void UpdateVoucherUsage(int voucherId, MySqlConnection connection, MySqlTransaction tx)
        {
            string sql = "UPDATE Vouchers SET UsedCount = UsedCount + 1 WHERE Id = @Id";
            using var cmd = new MySqlCommand(sql, connection, tx);
            cmd.Parameters.AddWithValue("@Id", voucherId);
            cmd.ExecuteNonQuery();
        }

        public static int GetVoucherUsageCountForCustomer(int voucherId, int customerId)
        {
            if (voucherId <= 0 || customerId <= 0) return 0;
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "SELECT COUNT(*) FROM Invoices WHERE VoucherId = @vid AND CustomerId = @cid AND Status != 'Cancelled'";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@vid", voucherId);
            cmd.Parameters.AddWithValue("@cid", customerId);
            try { return Convert.ToInt32(cmd.ExecuteScalar() ?? 0); }
            catch { return 0; }
        }
    }
}

