using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using log4net;
using Suggestion.API.Helper;

namespace Suggestion.API.Helper
{
    public static class FileValidation
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(FileValidation));
        public static IConfigurationRoot Configuration { get; set; }

        static FileValidation()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
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
                    // allowed .pdf,video,images
                    if (fileclass == "00" || fileclass == "255216" || fileclass == "7173" || fileclass == "13780" || fileclass == "6677" || fileclass == "3780" || fileclass == "7368" || fileclass == "79103" || fileclass == "2669")
                    //3780=.pdf 208207=.doc 7173=.gif 255216=.jpg 6677=.bmp 13780=.png  79103=.ogv/ogg 2269=webm
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
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                isAllowed = false;
            }
            return isAllowed;
        }


        public static bool IsValidImage(this IFormFile fileUpload)
        {
            bool isAllowed = false;
            bool isAllowedExt = true;
            try
            {
                string fileclass = string.Empty;

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
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                isAllowed = false;
            }
            return isAllowed;
        }


        public static bool IsValidVideo(this IFormFile fileUpload)
        {
            bool isAllowed = false;
            bool isAllowedExt = true;
            try
            {
                string fileclass = string.Empty;

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
                    // allowed .mp4,.wma
                    if (fileclass == "00" || fileclass == "4838" || fileclass == "2669" || fileclass == "79103")
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
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
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

                if (filenameCount.Length > 4)
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
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
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
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                isAllowed = false;
            }
            return isAllowed;
        }
        public static bool IsValidLCMSDocument(this IFormFile fileUpload)
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

                    if (fileclass == "208207" || fileclass == "8075" || fileclass == "67111" || fileclass == "239187" || fileclass == "3780" || fileclass == "8583" || fileclass == "12392") //ppt,pptx,csv,xls,pdf,rtf
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
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                isAllowed = false;
            }
            return isAllowed;
        }
        public static Boolean CheckForSQLInjection(string _value = null)
        {
            if (!string.IsNullOrEmpty(_value))
            {
                if (_value.StartsWith('+') || _value.StartsWith('-') || _value.StartsWith('=') || _value.StartsWith('@'))
                    return true;
                return false;
            }
            return false;
        }
    }
}
