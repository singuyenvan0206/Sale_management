using Xunit;
using FashionStore.Core.Settings;
using System;

namespace FashionStore.Tests
{
    public class SecurityTests
    {
        [Fact]
        public void PasswordHashing_NewHash_ShouldVerify()
        {
            // Arrange
            string password = "StrongPassword123!";

            // Act
            string hash = PasswordHelper.HashPassword(password);
            bool isValid = PasswordHelper.VerifyPassword(password, hash);

            // Assert
            Assert.True(isValid);
            Assert.StartsWith("$2", hash); // BCrypt format
        }

        [Fact]
        public void PasswordHashing_WrongPassword_ShouldFail()
        {
            // Arrange
            string password = "CorrectPassword";
            string wrongPassword = "WrongPassword";

            // Act
            string hash = PasswordHelper.HashPassword(password);
            bool isValid = PasswordHelper.VerifyPassword(wrongPassword, hash);

            // Assert
            Assert.False(isValid);
        }

        [Fact]
        public void PasswordHashing_LegacyHash_ShouldVerify()
        {
            // This represents a hash created with the old SHA256 method
            // Arrange
            string password = "LegacyPassword";
            // Manually created legacy hash (emulating old PasswordHelper)
            // salt(16) + hash(32) = 48 bytes base64
            byte[] salt = new byte[16];
            new Random().NextBytes(salt);
            byte[] hashBytes;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] pwdBytes = System.Text.Encoding.UTF8.GetBytes(password);
                byte[] salted = new byte[pwdBytes.Length + salt.Length];
                Buffer.BlockCopy(pwdBytes, 0, salted, 0, pwdBytes.Length);
                Buffer.BlockCopy(salt, 0, salted, pwdBytes.Length, salt.Length);
                hashBytes = sha256.ComputeHash(salted);
            }
            byte[] result = new byte[48];
            Buffer.BlockCopy(salt, 0, result, 0, 16);
            Buffer.BlockCopy(hashBytes, 0, result, 16, 32);
            string legacyHash = Convert.ToBase64String(result);

            // Act
            bool isValid = PasswordHelper.VerifyPassword(password, legacyHash);
            bool needsUpgrade = PasswordHelper.NeedsUpgrade(legacyHash);

            // Assert
            Assert.True(isValid);
            Assert.True(needsUpgrade);
        }

        [Fact]
        public void PasswordHashing_EmptyPassword_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() => PasswordHelper.HashPassword(""));
            Assert.Throws<ArgumentException>(() => PasswordHelper.HashPassword(null!));
        }
    }
}
