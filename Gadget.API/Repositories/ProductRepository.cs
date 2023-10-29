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
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using AzureStorageLibrary.Model;
using AzureStorageLibrary.Repositories.Interfaces;

namespace Gadget.API.Repositories
{
    public class ProductRepository : Repository<Product>, IProductRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProductRepository));
        private ICustomerConnectionStringRepository _customerConnectionString;
        private GadgetDbContext context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private IConfiguration _configuration;
        private readonly ITokensRepository _tokensRepository;
        IAzureStorage _azurestorage;
        public ProductRepository(GadgetDbContext context, 
            IHttpContextAccessor httpContextAccessor, 
            IConfiguration configuration,
            ICustomerConnectionStringRepository customerConnectionString,
            ITokensRepository tokensRepository,
            IAzureStorage azurestorage) : base(context)
        {
            this.context = context;
            this._httpContextAccessor = httpContextAccessor;
            this._configuration = configuration;
            this._customerConnectionString = customerConnectionString;
            this._tokensRepository = tokensRepository;
            this._azurestorage = azurestorage;
        }

        public async Task<List<APIProduct>> GetProductList()
        {
            return await this.context.Products.Select(x => new APIProduct { ProductId = x.ProductId, ProductName = x.ProductName, ProductDescription = x.ProductDescription, ProductCategoryId = x.ProductCategoryId, Thumbnail = x.Thumbnail }).ToListAsync();
        }

        public async Task<List<APIProduct>> GetProductByCategoryId (int id)
        {
            return await this.context.Products.Where(x => x.ProductCategoryId == id).Select(x => new APIProduct { ProductId = x.ProductId, ProductName = x.ProductName, ProductDescription = x.ProductDescription, ProductCategoryId = x.ProductCategoryId, Thumbnail = x.Thumbnail }).ToListAsync();
        }

        public async Task<List<APIProduct>> GetProductByUserId (int id)
        {
            return await this.context.Products.Where(x => x.CreatedBy == id).Select(x => new APIProduct { ProductId = x.ProductId, ProductName = x.ProductName, ProductDescription = x.ProductDescription, ProductCategoryId = x.ProductCategoryId, Thumbnail = x.Thumbnail }).ToListAsync();
        }

        public void DeleteProductWithFAB(int ProductId)
        {
            try
            {
                using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
                {
                    using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "DeleteProductWithFAB";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = ProductId });
                        dbContext.Database.OpenConnection();
                        cmd.ExecuteReader();
                        dbContext.Database.CloseConnection();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
        }





        public async Task<Product> SaveImage(IFormFile uploadedFile, Product ProductObj,string orgCode)
        {
            string filePath = string.Empty;
            string fileName = string.Empty;
            var EnableBlobStorage = await _tokensRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

            _logger.Info("image EnableBlobStorage : " + EnableBlobStorage);

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                var request = _httpContextAccessor.HttpContext.Request;
                
                string saveFilePath = string.Empty;
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

                ProductObj.Thumbnail = saveFilePath;

                return ProductObj;
            }
            else
            {
                try
                {
                    BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, FileType.Image);
                    _logger.Info("response1 : " + res);
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            _logger.Info("response1 : " + res);
                            filePath = res.Blob.Name.ToString();
                            filePath = Path.Combine("\\", filePath);
                            ProductObj.Thumbnail = filePath;
                            _logger.Info("ProductObj1 : " + res);
                            return ProductObj;
                        }
                        else
                        {
                            _logger.Error(res.ToString());
                            return null;
                        }
                        
                    }
               
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return ProductObj;
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
        public void EditProduct(Product ProductObj)
        {
            using (GadgetDbContext dbContext = this._customerConnectionString.GetDbContext())
            {
                using (DbCommand cmd = dbContext.Database.GetDbConnection().CreateCommand())
                {



                    cmd.CommandText = "EditProduct";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@ProductId", SqlDbType.Int) { Value = ProductObj.ProductId });
                    cmd.Parameters.Add(new SqlParameter("@ProductName", SqlDbType.NVarChar) { Value = ProductObj.ProductName });
                    cmd.Parameters.Add(new SqlParameter("@ProductDescription", SqlDbType.NVarChar) { Value = ProductObj.ProductDescription });
                    cmd.Parameters.Add(new SqlParameter("@Thumbnail", SqlDbType.NVarChar) { Value = ProductObj.Thumbnail });

                    dbContext.Database.OpenConnection();
                    cmd.ExecuteReader();
                    dbContext.Database.CloseConnection();


                }
            }
        }
    }
}
