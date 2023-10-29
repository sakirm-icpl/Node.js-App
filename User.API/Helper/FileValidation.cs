using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace User.API.Helper
{
    public static class FileValidation
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(FileValidation));
        public static IConfigurationRoot Configuration { get; set; }

        static FileValidation()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }


        public static bool IsValidImageVideoPdf(this IFormFile fileUpload)
        {
            bool isAllowed = false;
            try
            {
                string fileclass = string.Empty;
                bool isAllowedExt = true;
                string[] filenameCount = fileUpload.FileName.Split(".");

                if (filenameCount.Length > 2)
                {
                    isAllowedExt = false;
                }
                else
                {
                    isAllowedExt = true;

                }
                if (isAllowedExt)
                {
                    using (BinaryReader r = new BinaryReader(fileUpload.OpenReadStream()))
                    {

                        byte buffer = r.ReadByte();
                        fileclass = buffer.ToString();
                        buffer = r.ReadByte();
                        fileclass += buffer.ToString();
                        r.Close();
                    }
                    // allowed pdf,xlsx,video,images
                    if (fileclass == "255216" || fileclass == "7173" || fileclass == "13780" || fileclass == "6677" || fileclass == "3780" || fileclass == "7368" || fileclass == "4838" || fileclass == "8075" || fileclass == "00")//3780=.pdf 208207=.doc 7173=.gif 255216=.jpg 6677=.bmp 13780=.png
                    {
                        isAllowed = true;
                    }
                    else
                    {
                        isAllowed = false;
                    }
                }
                return isAllowed;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                isAllowed = false;
            }
            return isAllowed;
        }
        public static bool IsValidXLSX(this IFormFile fileUpload)
        {
            bool isAllowed = false;
            try
            {
                string fileclass = string.Empty;
                bool isAllowedExt = true;
                string[] filenameCount = fileUpload.FileName.Split(".");

                if (filenameCount.Length > 2)
                {
                    isAllowedExt = false;
                }
                else
                {
                    isAllowedExt = true;

                }
                if (isAllowedExt)
                {
                    using (BinaryReader r = new BinaryReader(fileUpload.OpenReadStream()))
                    {

                        byte buffer = r.ReadByte();
                        fileclass = buffer.ToString();
                        buffer = r.ReadByte();
                        fileclass += buffer.ToString();
                        r.Close();
                    }

                    if (fileclass == "8075") //3780=.pdf 208207=.doc 7173=.gif 255216=.jpg 6677=.bmp 13780=.png
                    {
                        isAllowed = true;
                    }
                    else
                    {
                        isAllowed = false;
                    }
                }
                return isAllowed;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                isAllowed = false;
            }
            return isAllowed;
        }


        public static bool IsValidImage(this IFormFile fileUpload)
        {
            bool isAllowed = false;
            try
            {
                string fileclass = string.Empty;
                bool isAllowedExt = true;
                string[] filenameCount = fileUpload.FileName.Split(".");

                if (filenameCount.Length > 2)
                {
                    isAllowedExt = false;
                }
                else
                {
                    isAllowedExt = true;

                }
                if (isAllowedExt)
                {
                    using (BinaryReader r = new BinaryReader(fileUpload.OpenReadStream()))
                    {

                        byte buffer = r.ReadByte();
                        fileclass = buffer.ToString();
                        buffer = r.ReadByte();
                        fileclass += buffer.ToString();
                        r.Close();
                    }
                    // allowed .pdf,video,images
                    if (fileclass == "255216" || fileclass == "7173" || fileclass == "13780" || fileclass == "6677")//3780=.pdf 208207=.doc 7173=.gif 255216=.jpg 6677=.bmp 13780=.png
                    {
                        isAllowed = true;
                    }
                    else
                    {
                        isAllowed = false;
                    }
                }
                return isAllowed;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                isAllowed = false;
            }
            return isAllowed;
        }

        public static bool IsValidPdf(this IFormFile fileUpload)
        {
            bool isAllowed = false;
            try
            {
                string fileclass = string.Empty;
                bool isAllowedExt = true;
                string[] filenameCount = fileUpload.FileName.Split(".");

                if (filenameCount.Length > 2)
                {
                    isAllowedExt = false;
                }
                else
                {
                    isAllowedExt = true;

                }
                if (isAllowedExt)
                {
                    using (BinaryReader r = new BinaryReader(fileUpload.OpenReadStream()))
                    {

                        byte buffer = r.ReadByte();
                        fileclass = buffer.ToString();
                        buffer = r.ReadByte();
                        fileclass += buffer.ToString();
                        r.Close();
                    }
                    // allowed .pdf,video,images
                    if (fileclass == "3780")//3780=.pdf 208207=.doc 7173=.gif 255216=.jpg 6677=.bmp 13780=.png
                    {
                        isAllowed = true;
                    }
                    else
                    {
                        isAllowed = false;
                    }
                }
                return isAllowed;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                isAllowed = false;
            }
            return isAllowed;
        }

        public static Boolean CheckForSQLInjection(string _value)
        {
            if (!string.IsNullOrEmpty(_value))
            {
                if (_value.StartsWith('+') || _value.StartsWith('-') || _value.StartsWith('=') || _value.StartsWith('@'))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;

        }
    }
}
