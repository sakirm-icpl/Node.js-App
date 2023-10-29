using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using Courses.API.Helper;
using log4net;
using System.Text.RegularExpressions;
using System.Linq;

namespace Courses.API.Helper
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

        public static byte[] ReadFileHead(IFormFile file)
        {
            using var fs = new BinaryReader(file.OpenReadStream());
            var bytes = new byte[20];
            fs.Read(bytes, 0, 20);
            return bytes;
        }

        public static bool IsValidExtension(this IFormFile fileUpload, string[] supportedTypes)
       {
        bool isAllowed = false;
        try
        {
            var fileExt = System.IO.Path.GetExtension(fileUpload.FileName).Substring(1).ToLower();
            if (supportedTypes.Contains(fileExt))
            {
                isAllowed = true;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(Utilities.GetDetailedException(ex));
            isAllowed = false;
        }
        return isAllowed;
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
                    if (fileclass == "255216" || fileclass == "7173" || fileclass == "13780" || fileclass == "6677" || fileclass == "3780" || fileclass == "7368" || fileclass == "4838")//3780=.pdf 208207=.doc 7173=.gif 255216=.jpg 6677=.bmp 13780=.png
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
                    if (fileclass == "3780" || fileclass == "208207" || fileclass == "8075")//3780=.pdf 208207=.doc 7173=.gif 255216=.jpg 6677=.bmp 13780=.png 8075 =.docx
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


        public static bool IsValidLCMSVideo(this IFormFile fileUpload)
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

                    if (fileclass == "00" || fileclass == "2669" || fileclass == "79103") //mp4,webm,ogg
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

        public static bool IsValidLCMSAudio(this IFormFile fileUpload)
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

                    if (fileclass == "00" || fileclass == "8273" || fileclass == "79103" || fileclass == "7368" || fileclass == "255251" || fileclass == "255227") //mpeg,wav,ogg,mp3,mp4
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

        public static bool IsValidLCMSH5P(this IFormFile fileUpload)
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
                    //using (BinaryReader r = new BinaryReader(fileUpload.OpenReadStream()))
                    //{

                    //    byte buffer = r.ReadByte();
                    //    fileclass = buffer.ToString();
                    //    buffer = r.ReadByte();
                    //    fileclass += buffer.ToString();
                    //    r.Close();
                    //}

                    //if (fileclass == "8075") //mpeg,wav,ogg
                    //{
                    //    isAllowed = true;
                    //}
                    //else
                    //{
                    //    isAllowed = false;
                    //}
                    string ext = Path.GetExtension(fileUpload.FileName);
                    if(ext == "h5p")
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

        public static bool IsValidH5PZip(this IFormFile fileUpload)
        {
            bool isAllowed = false;
            try
            {
                //TODO: Add validation to check valid h5p file
                isAllowed = true;
                return isAllowed;
            }
            catch (Exception)
            {
                isAllowed = false;
            }
            return isAllowed;
        }

        public static bool IsValidLCMSZip(this IFormFile fileUpload)
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

                    if (fileclass == "8075") //mpeg,wav,ogg
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

                    if (fileclass == "208207" || fileclass == "8075" || fileclass == "67111" || fileclass == "239187" || fileclass == "3780" || fileclass == "8583") //ppt,pptx,csv,xls,pdf
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

        public static Boolean CheckForSQLInjectionWithRegEx(string _value = null)
        {
            if (!string.IsNullOrEmpty(_value))
            {
                Regex rx = new Regex(@"([+='\t\r\n\v]|[-]{2,})+",RegexOptions.Compiled);
                if (rx.IsMatch(_value))
                    return true;
            }
            return false;
        }

    }

   
}
