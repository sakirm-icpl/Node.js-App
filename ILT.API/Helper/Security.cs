using Microsoft.Extensions.Configuration;
using NETCore.Encrypt;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ILT.API.Helper;
using log4net;
namespace ILT.API.Helper
{
    public static class Security
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Security));
        public static IConfigurationRoot Configuration { get; set; }

        static Security()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }
        public static string Encrypt(this string encrypt)
        {
            try
            {
                if (string.IsNullOrEmpty(encrypt))
                    return encrypt;
                var aesKey = EncryptProvider.CreateAesKey();
                var key = Configuration["EncryptionKey"];
                return EncryptProvider.AESEncrypt(encrypt, key);
            }
            catch (Exception ex)
            {
                 return encrypt;
            }
        }

        public static string Decrypt(this string decrypt)
        {
            try
            {
                if (string.IsNullOrEmpty(decrypt))
                    return decrypt;
                var aesKey = EncryptProvider.CreateAesKey();
                var key = Configuration["EncryptionKey"];
                return EncryptProvider.AESDecrypt(decrypt, key);
            }
            catch (Exception ex)
            {
                 return decrypt;
            }
        }

        // SBIL Security //
        public static string EncryptForUI(string jsonData11)
        {
            string Key = "a1b2c3d4e5f6g7h8";
            string dataNew = EncryptNEWMethod(jsonData11.ToString(), Key);

            return dataNew;
        }
        public static string EncryptNEWMethod(string data, string key)
        {
            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC; //remember this parameter
            rijndaelCipher.Padding = PaddingMode.PKCS7; //remember this parameter

            rijndaelCipher.KeySize = 0x80;
            rijndaelCipher.BlockSize = 0x80;
            byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;

            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }

            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
            byte[] plainText = Encoding.UTF8.GetBytes(data);

            return Convert.ToBase64String
            (transform.TransformFinalBlock(plainText, 0, plainText.Length));
        }

        public static string DecryptForUI(string textToDecrypt)
        {
            string Key = "a1b2c3d4e5f6g7h8";
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 0x80;
            rijndaelCipher.BlockSize = 0x80;
            byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
            byte[] pwdBytes = Encoding.UTF8.GetBytes(Key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return encoding.GetString(plainText);
        }
        public static string DecryptWithKey(string textToDecrypt, string _key)
        {
            string Key = _key;
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

            RijndaelManaged rijndaelCipher = new RijndaelManaged();
            rijndaelCipher.Mode = CipherMode.CBC;
            rijndaelCipher.Padding = PaddingMode.PKCS7;

            rijndaelCipher.KeySize = 0x80;
            rijndaelCipher.BlockSize = 0x80;
            byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
            byte[] pwdBytes = Encoding.UTF8.GetBytes(Key);
            byte[] keyBytes = new byte[0x10];
            int len = pwdBytes.Length;
            if (len > keyBytes.Length)
            {
                len = keyBytes.Length;
            }
            Array.Copy(pwdBytes, keyBytes, len);
            rijndaelCipher.Key = keyBytes;
            rijndaelCipher.IV = keyBytes;
            byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            return encoding.GetString(plainText);
        }

    }
}
