using Microsoft.Extensions.Configuration;
using NETCore.Encrypt;
using System;
using System.IO;
using log4net;
using PollManagement.API.Helper;

namespace PollManagement.API.Helper
{
    public static class Security
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Security));
        public static IConfigurationRoot Configuration { get; set; }

        static Security()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
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
                NETCore.Encrypt.Internal.AESKey aesKey = EncryptProvider.CreateAesKey();
                string key = Configuration["EncryptionKey"];
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
                NETCore.Encrypt.Internal.AESKey aesKey = EncryptProvider.CreateAesKey();
                string key = Configuration["EncryptionKey"];
                return EncryptProvider.AESDecrypt(decrypt, key);
            }
            catch (Exception ex)
            {
                return decrypt;
            }
        }

        public static string EncryptSHA512(this string encrypt)
        {
            try
            {
                return EncryptProvider.Sha512(encrypt);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return encrypt;
            }
        }


    }
}
