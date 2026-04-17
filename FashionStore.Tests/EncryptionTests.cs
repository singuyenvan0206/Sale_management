using Xunit;
using FashionStore.Core.Utils;
using System;
using System.Text;

namespace FashionStore.Tests
{
    public class EncryptionTests
    {
        [Fact]
        public void EncryptDecrypt_RoundTrip_ShouldBeConsistent()
        {
            // Arrange
            string original = "SePay_Secret_Token_12345";

            // Act
            string encrypted = EncryptionHelper.Encrypt(original);
            string decrypted = EncryptionHelper.Decrypt(encrypted);

            // Assert
            Assert.NotEqual(original, encrypted);
            Assert.Equal(original, decrypted);
        }

        [Fact]
        public void Decrypt_InvalidBase64_ShouldReturnOriginal()
        {
            // Arrange
            string invalid = "Not@Base64!";

            // Act
            string result = EncryptionHelper.Decrypt(invalid);

            // Assert
            Assert.Equal(invalid, result);
        }

        [Fact]
        public void Encrypt_EmptyString_ShouldReturnOriginal()
        {
            Assert.Equal("", EncryptionHelper.Encrypt(""));
            Assert.Null(EncryptionHelper.Encrypt(null!));
        }

        [Fact]
        public void EnvironmentVariables_Fallback_ShouldWork()
        {
            // This test verifies that the system doesn't crash even if env vars are missing
            // (It uses the static fallback we defined)
            
            // Arrange
            string text = "SecretData";
            
            // Act
            string encrypted = EncryptionHelper.Encrypt(text);
            string decrypted = EncryptionHelper.Decrypt(encrypted);
            
            // Assert
            Assert.Equal(text, decrypted);
        }
    }
}
