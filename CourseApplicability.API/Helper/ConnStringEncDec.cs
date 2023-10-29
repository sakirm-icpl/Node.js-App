using log4net;
using Microsoft.Extensions.Configuration;
using NETCore.Encrypt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CourseApplicability.API.Helper
{
    /// <summary>
    /// Helper class to decrypt connection string if it is encrypted.
    /// <CreatedAt>11-Jan-2021</CreatedAt>
    /// </summary>
    public class ConnStringEncDec
    {
        private IConfigurationRoot Configuration { get; set; }
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FileValidation));
        private string partEncryptionString = ")(*&^%$#@!";

        public ConnStringEncDec()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }
        public string GetDefaultConnectionString()
        {
            try
            {
                string defaultDBString = Configuration.GetConnectionString("DefaultConnection");
                if (!defaultDBString.ToLower().Contains("server"))
                {
                    return EncryptProvider.AESDecrypt(defaultDBString, GetNewEncryptionKey());
                }
                else
                    return defaultDBString;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
        }

        public string GetEmpoweredMasterConnectionString()
        {
            try
            {
                string empoweredMasterDBString = Configuration.GetConnectionString("EmpoweredMaster");
                if (!empoweredMasterDBString.ToLower().Contains("server"))
                {
                    return EncryptProvider.AESDecrypt(empoweredMasterDBString, GetNewEncryptionKey());
                }
                else
                    return empoweredMasterDBString;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
        }

        /// <summary>
        /// Prepare new encryption key from key which pass from app settings.json
        /// </summary>
        /// <returns>New Encryption Key</returns>
        private string GetNewEncryptionKey()
        {
            try
            {
                var key = Configuration["EncryptionKey"];
                string NewEncKey = key.Substring(0, 11);
                NewEncKey = NewEncKey + partEncryptionString + key.Substring(key.Length - 11);
                return NewEncKey;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
        }

        private string Encrpt(string connectionString)
        {
            try
            {
                var key = Configuration["EncryptionKey"];
                return EncryptProvider.AESEncrypt(connectionString, key);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw;
            }
        }

    }
}
