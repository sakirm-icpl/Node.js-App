using log4net;
using AzureStorageLibrary.Repositories.Interfaces;
using Assessment.API.Model;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Repositories;
using Assessment.API.Helper;
using Assessment.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Assessment.API.Repositories
{
    public class LCMSRepositrory : Repository<LCMS>, ILCMSRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LCMSRepositrory));
        private AssessmentContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IConfiguration _configuration;
        
        ICustomerConnectionStringRepository _customerConnection;
        
        ICourseRepository _courseRepository;
        IAzureStorage _azurestorage;

        // static string EnableBlobStorage =  _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

        public LCMSRepositrory(AssessmentContext context,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment hostingEnvironment,
            IConfiguration configuration,
             
              ICourseRepository courseRepository,
              IAzureStorage azurestorage,
            ICustomerConnectionStringRepository customerConnection

            ) : base(context)
        {
            _db = context;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _customerConnection = customerConnection;
            
            this._courseRepository = courseRepository;
            this._azurestorage = azurestorage;
           
        }

        public async Task<bool> Exist(string fileName, string contentType, int? id = null)
        {
            if (id == null)
            {
                int count = await _db.LCMS.Where(r =>
                 r.Name.ToLower() == fileName.ToLower()
                 && r.ContentType == contentType
                 && r.IsDeleted == Record.NotDeleted).CountAsync();
                if (count > 0)
                    return true;
            }
            else
            {
                if (await _db.LCMS.Where(r => r.Name.ToLower() == fileName.ToLower() && r.ContentType == contentType && r.Id != id && r.IsDeleted == Record.NotDeleted).CountAsync() > 0)
                    return true;
            }
            return false;
        }

        public Task<LCMS> GetLcmsByAssessmentConfigureId(int AssesmentConfigId)
        {
            return _db.LCMS.Where(a => a.AssessmentSheetConfigID == AssesmentConfigId && a.IsDeleted == Record.NotDeleted)
                   .FirstOrDefaultAsync();
        }
    }
}