using Gadget.API.APIModel;
using Gadget.API.Data;
using Gadget.API.Helper;
using Gadget.API.Metadata;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;

namespace Gadget.API.Repositories
{
    public class ImageRepository : Repository<Image>, IImageRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ImageRepository));
        private GadgetDbContext context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        private readonly ITokensRepository _tokensRepository;
        IAzureStorage _azurestorage;
        public ImageRepository(GadgetDbContext context, IHttpContextAccessor httpContextAccessor, ITokensRepository tokensRepository, IConfiguration configuration, IAzureStorage azurestorage) : base(context)
        {
            this.context = context;
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
            this._tokensRepository = tokensRepository;
            this._azurestorage = azurestorage;
        }

        public async Task<List<APIImage>> GetImageList()
        {
            return await this.context.Images.Select(x => new APIImage { ImageId = x.ImageId, ImageName = x.ImageName, ImagePath = x.ImagePath, ProductId = x.ProductId }).ToListAsync();
        }

        public async Task<List<Image>> PostImageList(Image imageData)
        {
            context.Add(imageData);
            context.SaveChanges();

            return await this.context.Images.Select(x => x).ToListAsync();
        }

        public async Task<List<Image>> GetImageByIdForDelete(int id)
        {
            return await this.context.Images.Where(x => x.ProductId == id).Select(x => x).ToListAsync();
        }

        public async Task<List<APIImage>> GetImageById(int id)
        {
            return await this.context.Images.Where(x => x.ProductId == id).Select(x => new APIImage { ImageId = x.ImageId, ImageName = x.ImageName, ImagePath = x.ImagePath, ProductId = x.ProductId }).ToListAsync();
        }




        public async Task<int> SaveImage(IFormFile uploadedFile, Image imageData)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string saveFilePath = string.Empty;

            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

            _logger.Info("image EnableBlobStorage : " + EnableBlobStorage);

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {

                string gadgetsPath = _configuration["ApiGatewayWwwroot"];

                if (!Directory.Exists(gadgetsPath))
                {
                    Directory.CreateDirectory(gadgetsPath);
                }
                fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                fileName = string.Concat(fileName.Split(' '));
                filePath = Path.Combine(gadgetsPath, FileType.Image);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }

                saveFilePath = Path.Combine(FileType.Image, fileName);
                saveFilePath = Path.Combine("\\", saveFilePath);
                filePath = Path.Combine(gadgetsPath, FileType.Image, fileName);
                filePath = string.Concat(filePath.Split(' '));
                await SaveFile(uploadedFile, filePath);

                imageData.ImageName = fileName;
                imageData.ImagePath = saveFilePath;

                await PostImageList(imageData);
                return 1;
            }
            else
            {
                try
                {
                    BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, FileType.Image);
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            filePath = res.Blob.Name.ToString();
                            fileName = filePath.Substring(filePath.LastIndexOf('\\') + 1);
                            filePath = Path.Combine("\\", filePath);
                            imageData.ImageName = fileName;
                            imageData.ImagePath = filePath;

                            await PostImageList(imageData);
                            return 1;
                        }
                        else
                        {
                            _logger.Error(res.ToString());
                            
                        }

                    }

                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return 1;
            }
        }

        public async Task<bool> SaveFile(IFormFile uploadedFile, string filePath)
        {
            try
            {
                using (var fs = new FileStream(Path.Combine(filePath), FileMode.Create))
                {
                    await uploadedFile.CopyToAsync(fs);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return false;
        }

        public bool DeleteFile(string filePath)
        {
            try
            {
                string gadgetsPath = _configuration["ApiGatewayWwwroot"];
                File.Delete(gadgetsPath + filePath);

                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return true;
            }
        }
    }
}
