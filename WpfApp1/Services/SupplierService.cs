using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using FashionStore.Core;
using FashionStore.Models;

namespace FashionStore.Services
{
    public static class SupplierService
    {
        private static string ConnectionString => SettingsManager.BuildConnectionString();

        public static List<Supplier> GetAllSuppliers()
        {
            var suppliers = new List<Supplier>();
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "SELECT * FROM Suppliers ORDER BY Name";
            using var cmd = new MySqlCommand(sql, connection);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                suppliers.Add(new Supplier
                {
                    Id = reader.GetInt32("Id"),
                    Name = reader.GetString("Name"),
                    ContactName = reader.IsDBNull(reader.GetOrdinal("ContactName")) ? "" : reader.GetString("ContactName"),
                    Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? "" : reader.GetString("Phone"),
                    Email = reader.IsDBNull(reader.GetOrdinal("Email")) ? "" : reader.GetString("Email"),
                    Address = reader.IsDBNull(reader.GetOrdinal("Address")) ? "" : reader.GetString("Address")
                });
            }
            return suppliers;
        }

        public static bool AddSupplier(Supplier supplier)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "INSERT INTO Suppliers (Name, ContactName, Phone, Email, Address) VALUES (@Name, @ContactName, @Phone, @Email, @Address)";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Name", supplier.Name);
            cmd.Parameters.AddWithValue("@ContactName", supplier.ContactName);
            cmd.Parameters.AddWithValue("@Phone", supplier.Phone);
            cmd.Parameters.AddWithValue("@Email", supplier.Email);
            cmd.Parameters.AddWithValue("@Address", supplier.Address);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool UpdateSupplier(Supplier supplier)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string sql = "UPDATE Suppliers SET Name=@Name, ContactName=@ContactName, Phone=@Phone, Email=@Email, Address=@Address WHERE Id=@Id";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Name", supplier.Name);
            cmd.Parameters.AddWithValue("@ContactName", supplier.ContactName);
            cmd.Parameters.AddWithValue("@Phone", supplier.Phone);
            cmd.Parameters.AddWithValue("@Email", supplier.Email);
            cmd.Parameters.AddWithValue("@Address", supplier.Address);
            cmd.Parameters.AddWithValue("@Id", supplier.Id);
            return cmd.ExecuteNonQuery() > 0;
        }

        public static bool DeleteSupplier(int id)
        {
            using var connection = new MySqlConnection(ConnectionString);
            connection.Open();
            string checkSql = "SELECT COUNT(*) FROM Products WHERE SupplierId = @Id";
            using var checkCmd = new MySqlCommand(checkSql, connection);
            checkCmd.Parameters.AddWithValue("@Id", id);
            long count = (long)checkCmd.ExecuteScalar();
            if (count > 0) return false;

            string sql = "DELETE FROM Suppliers WHERE Id=@Id";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}

