using AspNet.Security.OAuth.Introspection;
using AutoMapper;
using Gadget.API.APIModel;
using Gadget.API.Helper;
using Gadget.API.Metadata;
using Gadget.API.Models;
using Gadget.API.Repositories.Interfaces;
using Gadget.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Gadget.API.Common.TokenPermissions;
using System.Text.Json;
using log4net;
using Gadget.API.Helper.Log_API_Count;

namespace Gadget.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/g/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
    [Authorize]
    //added to check expired token 
    [TokenRequired()]
    public class ProductController : IdentityController
    {
    
        IProductCategoryRepository _productCategoryRepository;
        IProductRepository _productRepository;
        IFeatureRepository _featureRepository;
        IAdvantageRepository _advantageRepository;
        IBenefitRepository _benefitRepository;
        IImageRepository _imageRepository;
        IProductAccessibilityRepository _productAccessibilityRepository;

        private static readonly ILog _logger = LogManager.GetLogger(typeof(ProductController));

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITokensRepository _tokensRepository;
        public ProductController(IProductCategoryRepository productCategoryRepository,
                                IProductRepository productRepository,
                                IFeatureRepository featureRepository,
                                IAdvantageRepository advantageRepository,
                                IBenefitRepository benefitRepository,
                                IImageRepository imageRepository,
                                IProductAccessibilityRepository productAccessibilityRepository,
                                IIdentityService identitySvc,
                                IHttpContextAccessor httpContextAccessor,
                                ITokensRepository tokensRepository
                               ) : base(identitySvc)
        {
            this._productCategoryRepository = productCategoryRepository;
            this._productRepository = productRepository;
            this._featureRepository = featureRepository;
            this._advantageRepository = advantageRepository;
            this._benefitRepository = benefitRepository;
            this._imageRepository = imageRepository;
            this._productAccessibilityRepository = productAccessibilityRepository;
            this._httpContextAccessor = httpContextAccessor;
            this._tokensRepository = tokensRepository;
           
        }

        // GET: Product
        [HttpGet]
        public async Task<IActionResult> GetProduct()
        {
            try
            {
                return Ok(await _productRepository.GetProductList());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // GET: Product/GetProductByProductId/id
        [HttpGet]
        [Route("GetProductByProductId/{id}")]
        public async Task<IActionResult> GetProductByProductId(int id)
        {
            try
            {
                Product productData = await _productRepository.Get(id);

                APIProduct apiProduct = new APIProduct();

                apiProduct.ProductId = productData.ProductId;
                apiProduct.ProductName = productData.ProductName;
                apiProduct.ProductDescription = productData.ProductDescription;
                apiProduct.ProductCategoryId = productData.ProductCategoryId;
                apiProduct.Thumbnail = productData.Thumbnail;

                return Ok(apiProduct);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // GET: Product/GetProductByCategoryId/id
        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetProductByCategoryId(int id)
        {
            try
            {
                List<APIProduct> ProductList = await _productRepository.GetProductByCategoryId(id);

                if (ProductList.Count == 0)
                {
                    return NotFound();
                }
                else
                {
                    return Ok(ProductList);
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // GET: Product/GetProductByUserId
        [HttpGet]
        [Route("GetProductByUserId/{Id}")]
        public async Task<IActionResult> GetProductByUserId(int Id)
        {
            try
            {
                return Ok(await _productRepository.GetProductByUserId(Id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // POST: Product
        [HttpPost]
        public async Task<IActionResult> PostProduct([FromForm] int temp, string JsonString)
        {
            try
            {
                Product ProductObj = JsonSerializer.Deserialize<Product>(JsonString);

                ProductObj.CreatedDate = DateTime.UtcNow;
                ProductObj.CreatedBy = UserId;
                ProductObj.ModifiedBy = UserId;

                var request = _httpContextAccessor.HttpContext.Request;
                if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile uploadedFile in request.Form.Files)
                    {

                        if ((uploadedFile.ContentType.Contains(FileType.Image)) && (FileValidation.IsValidImage(uploadedFile)))
                        {
                            Product Obj = await this._productRepository.SaveImage(uploadedFile, ProductObj,OrgCode);
                            await _productRepository.Add(Obj);
                            return Ok("Success");
                        }
                    }
                }
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        //PUT : Product
        [HttpPost]
        [Route("UpdateProduct")]
        public async Task<IActionResult> PutProduct([FromForm] int temp, string JsonString)
        {
            try
            {

                Product ProductObj = JsonSerializer.Deserialize<Product>(JsonString);

                ProductObj.ModifiedDate = DateTime.UtcNow;
                //  ProductObj.ProductName = JsonString.;
                ProductObj.ProductId = ProductObj.ProductId;
                ProductObj.ModifiedBy = UserId;

                var request = _httpContextAccessor.HttpContext.Request;
                if (request.Form.Files.Count == 0)
                {
                    // Product Obj = await this._productRepository.SaveImage(null, ProductObj);
                    ProductObj.Thumbnail = "null";
                    _productRepository.EditProduct(ProductObj);
                    return Ok("Success");
                }
                else if (request.Form.Files.Count > 0)
                {
                    foreach (IFormFile uploadedFile in request.Form.Files)
                    {

                        if ((uploadedFile.ContentType.Contains(FileType.Image)) && (FileValidation.IsValidImage(uploadedFile)))
                        {
                            Product Obj = await this._productRepository.SaveImage(uploadedFile, ProductObj,OrgCode);
                            _productRepository.EditProduct(Obj);
                            return Ok("Success");
                        }
                    }
                }
                return Ok();

                //Product OldProduct = await this._productRepository.Get(productData.ProductId);
                //if (OldProduct == null)
                //    return NotFound();
                //OldProduct.ProductName = productData.ProductName;
                //OldProduct.ProductDescription = productData.ProductDescription;
                //OldProduct.Thumbnail = productData.Thumbnail;

                //OldProduct.ModifiedDate = DateTime.UtcNow;
                //OldProduct.ModifiedBy = UserId;

                //await this._productRepository.Update(OldProduct);
                //return Ok("Success");

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // DELETE : Product/id
        [HttpDelete]
        public async Task<IActionResult> DeleteProduct([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Product product = await _productRepository.Get(DecryptedId);
                int ProductId = product.ProductId;
                if (product == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                this._productRepository.DeleteProductWithFAB(ProductId);
                string EncryptedProductId = Security.Encrypt(Convert.ToString(ProductId));
                await DeleteImageByProductId(EncryptedProductId);
                
                this._productRepository.DeleteFile(product.Thumbnail);
                await _productRepository.Remove(product);

                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                   new ResponseMessage
                   {
                       StatusCode = 500,
                       Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                       Description = ex.StackTrace
                   });
            }
        }




        // GET: Product/Feature
        [HttpGet]
        [Route("GetFeature")]
        public async Task<IActionResult> GetFeatures()
        {
            try
            {
                return Ok(await _featureRepository.GetFeatureList());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // GET: Product/Feature/ProductId
        [HttpGet]
        [Route("GetFeatureByProductId/{id}")]
        public async Task<IActionResult> GetFeatureById(int id)
        {
            try
            {
                return Ok(await _featureRepository.GetFeatureById(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // POST: Product/Feature
        [HttpPost]
        [Route("PostFeature")]
        public async Task<IActionResult> PostFeatures([FromBody] Feature featuredata)
        {
            try
            {
                Feature FeatureObj = Mapper.Map<Feature>(featuredata);
                FeatureObj.CreatedDate = DateTime.UtcNow;
                FeatureObj.CreatedBy = UserId;
                FeatureObj.ModifiedBy = UserId;

                await _featureRepository.Add(featuredata);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        //PUT: Product/Feature
        [HttpPost]
        [Route("PutFeature")]
        public async Task<IActionResult> PutFeature([FromBody] Feature featureData)
        {
            try
            {
                Feature OldFeature = await this._featureRepository.Get(featureData.FeatureId);
                if (OldFeature == null)
                    return NotFound();
                OldFeature.FeatureName = featureData.FeatureName;
                OldFeature.FeatureDescription = featureData.FeatureDescription;

                OldFeature.ModifiedDate = DateTime.UtcNow;
                OldFeature.ModifiedBy = UserId;

                await this._featureRepository.Update(OldFeature);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // DELETE : Product/DeleteFeature/id
        [HttpDelete("DeleteFeature")]
        public async Task<IActionResult> DeleteFeature([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Feature feature = await _featureRepository.Get(DecryptedId);
                if (feature == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                await _featureRepository.Remove(feature);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }




        // GET: Product/Advantage
        [HttpGet]
        [Route("GetAdvantage")]
        public async Task<IActionResult> GetAdvantages()
        {
            try
            {
                return Ok(await _advantageRepository.GetAdvantageList());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // GET: Product/GetAdvantageByProductId/id
        [HttpGet]
        [Route("GetAdvantageByProductId/{id}")]
        public async Task<IActionResult> GetAdvantageById(int id)
        {
            try
            {
                return Ok(await _advantageRepository.GetAdvantageById(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // POST: Product/Advantage
        [HttpPost]
        [Route("PostAdvantage")]
        public async Task<IActionResult> PostAdvantages([FromBody] Advantage advantageData)
        {
            try
            {
                Advantage AdvantageObj = Mapper.Map<Advantage>(advantageData);
                AdvantageObj.CreatedDate = DateTime.UtcNow;
                AdvantageObj.CreatedBy = UserId;

                await _advantageRepository.Add(advantageData);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // PUT: Product/Advantage
        [HttpPost]
        [Route("PutAdvantage")]
        public async Task<IActionResult> PutAdvantage([FromBody] Advantage advantageData)
        {
            try
            {
                Advantage OldAdvantage = await this._advantageRepository.Get(advantageData.AdvantageId);
                if (OldAdvantage == null)
                    return NotFound();
                OldAdvantage.AdvantageName = advantageData.AdvantageName;
                OldAdvantage.AdvantageDescription = advantageData.AdvantageDescription;
                OldAdvantage.ModifiedDate = DateTime.UtcNow;
                OldAdvantage.ModifiedBy = UserId;

                await this._advantageRepository.Update(OldAdvantage);

                return Ok("Success");

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // DELETE : Product/DeleteAdvantage/id
        [HttpDelete("DeleteAdvantage")]
        public async Task<IActionResult> DeleteAdvantage([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Advantage advantage = await _advantageRepository.Get(DecryptedId);
                if (advantage == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                await _advantageRepository.Remove(advantage);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                   new ResponseMessage
                   {
                       StatusCode = 500,
                       Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                       Description = ex.StackTrace
                   });
            }
        }




        // GET: Product/Benefit
        [HttpGet]
        [Route("GetBenefit")]
        public async Task<IActionResult> GetBenefits()
        {
            try
            {
                return Ok(await _benefitRepository.GetBenefitList());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // GET: Product/GetBenefitByProductId/id
        [HttpGet]
        [Route("GetBenefitByProductId/{id}")]
        public async Task<IActionResult> GetBenefitById(int id)
        {
            try
            {
                return Ok(await _benefitRepository.GetBenefitById(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // POST: Product/Benefit
        [HttpPost]
        [Route("PostBenefit")]
        public async Task<IActionResult> PostBenefits([FromBody] Benefit benefitdata)
        {
            try
            {
                Benefit BenefitObj = Mapper.Map<Benefit>(benefitdata);
                BenefitObj.CreatedDate = DateTime.UtcNow;
                BenefitObj.CreatedBy = UserId;
                BenefitObj.ModifiedBy = UserId;

                await _benefitRepository.Add(benefitdata);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        //PUT: Product/Benefit
        [HttpPost]
        [Route("PutBenefit")]
        public async Task<IActionResult> PutBenefit([FromBody] Benefit benefitData)
        {
            try
            {
                Benefit OldBenefit = await this._benefitRepository.Get(benefitData.BenefitId);
                if (OldBenefit == null)
                    return NotFound();
                OldBenefit.BenefitName = benefitData.BenefitName;
                OldBenefit.BenefitDescription = benefitData.BenefitDescription;
                OldBenefit.ModifiedDate = DateTime.UtcNow;
                OldBenefit.ModifiedBy = UserId;

                await this._benefitRepository.Update(OldBenefit);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // DELETE : Product/DeleteBenefit/id
        [HttpDelete("DeleteBenefit")]
        public async Task<IActionResult> DeleteBenefit([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Benefit benefit = await _benefitRepository.Get(DecryptedId);
                if (benefit == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                await _benefitRepository.Remove(benefit);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                   new ResponseMessage
                   {
                       StatusCode = 500,
                       Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                       Description = ex.StackTrace
                   });
            }
        }




        // GET: Product/Image
        [HttpGet]
        [Route("GetImage")]
        public async Task<IActionResult> GetImages()
        {
            try
            {
                return Ok(await _imageRepository.GetImageList());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // GET: Product/GetImageByProductId/id
        [HttpGet]
        [Route("GetImageByProductId/{id}")]
        public async Task<IActionResult> GetImageById(int id)
        {
            try
            {
                return Ok(await _imageRepository.GetImageById(id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // POST: Product/Image
        [HttpPost]
        [Route("PostImage")]
        public async Task<IActionResult> PostImage([FromForm] Image imageData)
        {
            try
            {
                string CoursesPath = string.Empty;
                string FilePath = string.Empty;
                string FileName = string.Empty;

                if (ModelState.IsValid)
                {
                    var request = _httpContextAccessor.HttpContext.Request;
                    if (request.Form.Files.Count > 0)
                    {
                        foreach (IFormFile uploadedFile in request.Form.Files)
                        {
                            Image image = new Image();
                            image.CreatedDate = DateTime.UtcNow;
                            image.CreatedBy = UserId;
                            image.ModifiedBy = UserId;
                            image.ProductId = imageData.ProductId;

                            if ((uploadedFile.ContentType.Contains(FileType.Image)) && (FileValidation.IsValidImage(uploadedFile)))
                            {
                                return Ok(await this._imageRepository.SaveImage(uploadedFile, image));
                            }
                        }
                    }
                    else
                    {
                        return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InvalidFile), Description = EnumHelper.GetEnumDescription(MessageType.InvalidFile) });
                    }
                }
                return BadRequest(ModelState);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // PUT: Product/Image
        [HttpPost]
        [Route("PutImage")]
        public async Task<IActionResult> PutImage([FromBody] Image imageData)
        {
            try
            {
                Image OldImage = await this._imageRepository.Get(imageData.ImageId);
                if (OldImage == null)
                    return NotFound();
                OldImage.ImageName = imageData.ImageName;
                OldImage.ImagePath = imageData.ImagePath;
                OldImage.ModifiedDate = DateTime.UtcNow;
                OldImage.ModifiedBy = UserId;

                await this._imageRepository.Update(OldImage);

                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // DELETE : Product/DeleteImageByImageId/id
        [HttpDelete("DeleteImageByImageId")]
        public async Task<IActionResult> DeleteImage([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                Image image = await _imageRepository.Get(DecryptedId);
                if (image == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                _imageRepository.DeleteFile(image.ImagePath);
                await _imageRepository.Remove(image);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                   new ResponseMessage
                   {
                       StatusCode = 500,
                       Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                       Description = ex.StackTrace
                   });
            }
        }

        [HttpDelete("Image/DeleteImageByProductId")]
        public async Task<IActionResult> DeleteImageByProductId([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                List<Image> image = await _imageRepository.GetImageByIdForDelete(DecryptedId);
                if (image == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                for (int i = 0; i < image.Count; i++)
                {
                    _imageRepository.DeleteFile(image[i].ImagePath);
                    await _imageRepository.Remove(image[i]);
                }
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                   new ResponseMessage
                   {
                       StatusCode = 500,
                       Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                       Description = ex.StackTrace
                   });
            }
        }

        // GET: Product/Category
        [HttpGet]
        [Route("GetCategory")]
        public async Task<IActionResult> GetProductCategory()
        {
            try
            {
                return Ok(await _productCategoryRepository.GetProductCategoryList());
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }
        // GET: Product/Category/CategoryId
        [HttpGet]
        [Route("GetCategorybyCategoryId/{categoryid}")]
        public async Task<IActionResult> GetProductCategorybyCategoryId(int categoryid)
        {
            try
            {
                return Ok(await _productCategoryRepository.ProductCategorywithCategoryId(categoryid));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // GET: Product/Category/GetProductCategoryByUserId
        [HttpGet]
        [Route("Category/GetCategoryByUserId/{Id}")]
        public async Task<IActionResult> GetProductCategoryByUserId(int Id)
        {
            try
            {
                return Ok(await _productCategoryRepository.GetProductCategoryListByUserId(Id));
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // POST: Product/Category
        [HttpPost]
        [Route("PostCategory")]
        public async Task<IActionResult> PostProductCategory([FromBody] ProductCategory productCategoryData)
        {

            try
            {
                ProductCategory ProductCategoryObj = Mapper.Map<ProductCategory>(productCategoryData);
                ProductCategoryObj.CreatedDate = DateTime.UtcNow;
                ProductCategoryObj.CreatedBy = UserId;
                ProductCategoryObj.ModifiedBy = UserId;

                await _productCategoryRepository.Add(productCategoryData);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }
        [HttpPost]
        [Route("PostCategoryName")]
        public async Task<IActionResult> IsCategoryNameExists([FromBody] ProductCategory productCategoryData)
        {

            try
            {
                if (await this._productCategoryRepository.IsCategoryNameExists(productCategoryData.ProductCategoryName))
                    return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.Success), Description = EnumHelper.GetEnumDescription(MessageType.Duplicate) });
                return this.Ok(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        //PUT : Product/Category
        [HttpPost]
        [Route("PutCategory")]
        public async Task<IActionResult> PutProductCategory([FromBody] ProductCategory productCategoryData)
        {
            try
            {
                ProductCategory OldProductCategory = await this._productCategoryRepository.Get(productCategoryData.ProductCategoryId);
                if (OldProductCategory == null)
                    return NotFound();
                OldProductCategory.ProductCategoryName = productCategoryData.ProductCategoryName;

                OldProductCategory.ModifiedDate = DateTime.UtcNow;
                OldProductCategory.ModifiedBy = UserId;

                await this._productCategoryRepository.Update(OldProductCategory);

                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // DELETE : Product/DeleteCategory/id
        [HttpDelete("DeleteCategory")]
        public async Task<IActionResult> DeleteProductCategory([FromQuery]string id)
        {
            try
            {
                int DecryptedId = Convert.ToInt32(Security.Decrypt(id));
                ProductCategory productCategory = await _productCategoryRepository.Get(DecryptedId);
                if (productCategory == null)
                    return BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.NotFound), Description = EnumHelper.GetEnumDescription(MessageType.NotFound) });

                List<APIProduct> products = await _productRepository.GetProductByCategoryId(productCategory.ProductCategoryId);

                for (int i = 0; i < products.Count; i++)
                {
                    string EncryptedId = Security.Encrypt(Convert.ToString(products[i].ProductId));
                    await DeleteProduct(EncryptedId);
                }

                await _productCategoryRepository.Remove(productCategory);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                   new ResponseMessage
                   {
                       StatusCode = 500,
                       Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                       Description = ex.StackTrace
                   });
            }
        }

        [HttpGet]
        [Route("GetUserId")]
        public IActionResult GetUserId()
        {
            try
            {
                return Ok(UserId);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                   new ResponseMessage
                   {
                       StatusCode = 500,
                       Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                       Description = ex.StackTrace
                   });
            }
        }

        // POST: Product/ProductAccessibilityEntry
        [HttpPost]
        [Route("ProductAccessibilityEntry")]
        public async Task<IActionResult> PostProductAccessibilityEntry([FromBody] ProductAccessibility productAccessibilityData)
        {
            try
            {
                ProductAccessibility productAccessibilityObj = Mapper.Map<ProductAccessibility>(productAccessibilityData);
                productAccessibilityObj.UserId = UserId;
                productAccessibilityObj.FromDate = DateTime.Now;

                await _productAccessibilityRepository.Add(productAccessibilityObj);
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // POST: Product/ProductAccessibilityEntries
        [HttpPost]
        [Route("ProductAccessibilityEntries")]
        public async Task<IActionResult> PostProductAccessibilityEntries([FromBody] ProductAccessibility[] productAccessibilityDatas)
        {
            try
            {
                for(int i = 0; i < productAccessibilityDatas.Length; i++)
                {
                    ProductAccessibility productAccessibilityObj = Mapper.Map<ProductAccessibility>(productAccessibilityDatas[i]);
                    productAccessibilityObj.UserId = UserId;
                    productAccessibilityObj.FromDate = DateTime.Now;

                    await _productAccessibilityRepository.Add(productAccessibilityObj);
                }
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // POST: Product/ProductAccessibilityExit
        [HttpPost]
        [Route("ProductAccessibilityExit")]
        public async Task<IActionResult> PostProductAccessibilityExit([FromBody] ProductAccessibility productAccessibilityData)
        {
            try
            {
                ProductAccessibility productAccessibilityObj = await this._productAccessibilityRepository.GetLatestProductEntry(productAccessibilityData.ProductId, UserId);
                if (productAccessibilityObj == null)
                    return NotFound();
                productAccessibilityObj.ToDate = DateTime.Now;

                await this._productAccessibilityRepository.Update(productAccessibilityObj);

                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }

        // POST: Product/ProductAccessibilityExits
        [HttpPost]
        [Route("ProductAccessibilityExits")]
        public async Task<IActionResult> PostProductAccessibilityExits([FromBody] ProductAccessibility[] productAccessibilityDatas)
        {
            try
            {
                for (int i = 0; i < productAccessibilityDatas.Length; i++)
                {
                    ProductAccessibility productAccessibilityObj = await this._productAccessibilityRepository.GetLatestProductEntry(productAccessibilityDatas[i].ProductId, UserId);
                    if (productAccessibilityObj == null)
                        return NotFound();
                    productAccessibilityObj.ToDate = DateTime.Now;

                    await this._productAccessibilityRepository.Update(productAccessibilityObj);
                }
                return Ok("Success");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return BadRequest(
                    new ResponseMessage
                    {
                        StatusCode = 500,
                        Message = EnumHelper.GetEnumName(MessageType.InternalServerError),
                        Description = ex.StackTrace
                    });
            }
        }
    }
}
