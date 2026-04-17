using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FashionStore.Core.Utils
{
    public static class EncryptionHelper
    {
        // Internal system key - priority: Environment Variable > Secure Static Fallback
        private static byte[] GetKey()
        {
            var envKey = Environment.GetEnvironmentVariable("FASHION_STORE_AES_KEY");
            if (!string.IsNullOrEmpty(envKey))
            {
                var bytes = Encoding.UTF8.GetBytes(envKey);
                if (bytes.Length == 32) return bytes;
            }
            // Fallback for development - IMPORTANT: MUST be changed in Production via Environment Variable
            return Encoding.UTF8.GetBytes("F4sh10nSt0r3_S3cur3_K3y_2026_ERP");
        }

        private static byte[] GetIV()
        {
            var envIv = Environment.GetEnvironmentVariable("FASHION_STORE_AES_IV");
            if (!string.IsNullOrEmpty(envIv))
            {
                var bytes = Encoding.UTF8.GetBytes(envIv);
                if (bytes.Length == 16) return bytes;
            }
            return Encoding.UTF8.GetBytes("F4sh10n_IV_2026!");
        }

        private static readonly byte[] Key = GetKey();
        private static readonly byte[] IV = GetIV();

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText)) return plainText;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText)) return cipherText;

            try
            {
                byte[] fullCipher = Convert.FromBase64String(cipherText);

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = Key;
                    aesAlg.IV = IV;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(fullCipher))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch
            {
                // If decryption fails, return original text (might be plaintext during migration)
                return cipherText;
            }
        }
    }
}
