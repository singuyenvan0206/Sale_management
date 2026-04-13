using System;
using BCrypt.Net;

namespace FashionStore.Core
{
    public static class PasswordHelper
    {
        /// <summary>
        /// Mã hóa mật khẩu sử dụng BCrypt (an toàn hơn SHA256)
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Password cannot be null or empty", nameof(password));

            // Sử dụng BCrypt với work factor mặc định (11)
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        /// <summary>
        /// Xác minh mật khẩu có khớp với hash đã lưu không
        /// Hỗ trợ cả hash BCrypt mới và SHA256 cũ để migration
        /// </summary>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(hashedPassword))
                return false;

            try
            {
                // Kiểm tra xem có phải là format BCrypt không ($2a$, $2b$, $2y$)
                if (hashedPassword.StartsWith("$2"))
                {
                    return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
                }

                // Nếu không phải BCrypt, thử verify bằng SHA256 cũ (Legacy)
                return VerifyLegacySha256(password, hashedPassword);
            }
            catch
            {
                // Fallback cuối cùng cho plain text (nếu có)
                return password == hashedPassword;
            }
        }

        /// <summary>
        /// Xác minh mật khẩu sử dụng phương pháp SHA256 + Salt cũ
        /// </summary>
        private static bool VerifyLegacySha256(string password, string hashedPassword)
        {
            try
            {
                byte[] storedBytes = Convert.FromBase64String(hashedPassword);
                if (storedBytes.Length != 48) return false; // 16 bytes salt + 32 bytes hash

                byte[] salt = new byte[16];
                Buffer.BlockCopy(storedBytes, 0, salt, 0, 16);

                byte[] storedHash = new byte[32];
                Buffer.BlockCopy(storedBytes, 16, storedHash, 0, 32);

                using (var sha256 = System.Security.Cryptography.SHA256.Create())
                {
                    byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
                    byte[] saltedPassword = new byte[passwordBytes.Length + salt.Length];
                    Buffer.BlockCopy(passwordBytes, 0, saltedPassword, 0, passwordBytes.Length);
                    Buffer.BlockCopy(salt, 0, saltedPassword, passwordBytes.Length, salt.Length);

                    byte[] computedHash = sha256.ComputeHash(saltedPassword);
                    return CompareByteArrays(storedHash, computedHash);
                }
            }
            catch
            {
                return false;
            }
        }

        private static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length) return false;
            int result = 0;
            for (int i = 0; i < array1.Length; i++)
                result |= array1[i] ^ array2[i];
            return result == 0;
        }

        /// <summary>
        /// Kiểm tra xem hash có cần được nâng cấp (re-hash) không
        /// </summary>
        public static bool NeedsUpgrade(string hashedPassword)
        {
            // Nếu không phải BCrypt, cần upgrade
            return !string.IsNullOrEmpty(hashedPassword) && !hashedPassword.StartsWith("$2");
        }

        /// <summary>
        /// Kiểm tra xem một chuỗi có vẻ là đã được hash theo bất kỳ format nào không
        /// </summary>
        public static bool IsHashed(string password)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            // BCrypt format
            if (password.StartsWith("$2")) return true;
            // Legacy SHA256 format (Base64 và độ dài 48 bytes sau decode)
            try
            {
                byte[] bytes = Convert.FromBase64String(password);
                return bytes.Length == 48;
            }
            catch { return false; }
        }
    }
}
