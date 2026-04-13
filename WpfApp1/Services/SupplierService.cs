using Dapper;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using FashionStore.Core;
using FashionStore.Models;

namespace FashionStore.Services
{
    public static class SupplierService
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static List<Supplier> GetAllSuppliers()
        {
            using var connection = new MySqlConnection(ConnectionString);
            return connection.Query<Supplier>("SELECT * FROM Suppliers ORDER BY Id").ToList();
        }

        public static bool AddSupplier(Supplier supplier)
        {
            using var connection = new MySqlConnection(ConnectionString);
            string sql = "INSERT INTO Suppliers (Name, ContactName, Phone, Email, Address) VALUES (@Name, @ContactName, @Phone, @Email, @Address)";
            return connection.Execute(sql, supplier) > 0;
        }

        public static bool UpdateSupplier(Supplier supplier)
        {
            using var connection = new MySqlConnection(ConnectionString);
            string sql = "UPDATE Suppliers SET Name=@Name, ContactName=@ContactName, Phone=@Phone, Email=@Email, Address=@Address WHERE Id=@Id";
            return connection.Execute(sql, supplier) > 0;
        }

        public static bool DeleteSupplier(int id)
        {
            using var connection = new MySqlConnection(ConnectionString);
            string checkSql = "SELECT COUNT(*) FROM Products WHERE SupplierId = @Id";
            long count = connection.ExecuteScalar<long>(checkSql, new { Id = id });
            if (count > 0) return false;

            string sql = "DELETE FROM Suppliers WHERE Id=@Id";
            return connection.Execute(sql, new { Id = id }) > 0;
        }
    }
}
