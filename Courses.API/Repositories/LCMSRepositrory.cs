using Courses.API.APIModel;
using Courses.API.Common;
using Courses.API.Helper;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Assessment.API.Models;
using log4net;
using Microsoft.Data.SqlClient;
using Courses.API.Helper.Metadata;
using AzureStorageLibrary.Repositories.Interfaces;
using AzureStorageLibrary.Model;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System.Security.Cryptography;
using System.Text;
using Courses.API.APIModel.ThirdPartyIntegration;
using Microsoft.AspNetCore.Mvc;
using Azure;

namespace Courses.API.Repositories
{
    public class LCMSRepositrory : Repository<LCMS>, ILCMSRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LCMSRepositrory));
        private CourseContext _db;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private IConfiguration _configuration;
        ISystemSettingRepository _systemSettingRepository;
        ICustomerConnectionStringRepository _customerConnection;
        IModuleRepository _moduleRepository;
        ICourseRepository _courseRepository;
        IAzureStorage _azurestorage;

      // static string EnableBlobStorage =  _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

        public LCMSRepositrory(CourseContext context,
            IHttpContextAccessor httpContextAccessor,
            IWebHostEnvironment hostingEnvironment,
            IConfiguration configuration,
             IModuleRepository moduleRepository,
              ICourseRepository courseRepository,
              IAzureStorage azurestorage,
            ICustomerConnectionStringRepository customerConnection,

            ISystemSettingRepository systemSettingRepository) : base(context)
        {
            _db = context;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
            _configuration = configuration;
            _customerConnection = customerConnection;
            this._moduleRepository = moduleRepository;
            this._courseRepository = courseRepository;
           this._azurestorage = azurestorage;
            this._systemSettingRepository = systemSettingRepository;
        }
        public async Task<int> Count(string search = null, string contentType = null)
        {

            if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(contentType))
            {
                if (contentType == "xapi")
                    return await _db.LCMS.Where(l => l.ContentType.ToLower().StartsWith(contentType.ToLower()) || l.ContentType.ToLower().StartsWith("cmi5")).CountAsync();
                else
                    return await _db.LCMS.Where(r => r.ContentType.Contains(contentType) && r.Name.Contains(search)).CountAsync();
            }
            else if (!string.IsNullOrWhiteSpace(search))
                return await _db.LCMS.Where(r => r.Name.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();

            return await _db.LCMS.Where(l => l.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<bool> Exists(string name)
        {
            if (await _db.LCMS.CountAsync(y => y.Name == name) > 0)
                return true;
            return false;
        }

        public async Task<List<LCMS>> Get(int page, int pageSize, string search = null, string contentType = null)
        {
            IQueryable<LCMS> Query = _db.LCMS.Where(l => l.IsDeleted == false);
            if (!string.IsNullOrEmpty(search) && !string.IsNullOrEmpty(contentType))
                Query = Query.Where(r => r.ContentType.Contains(contentType) && r.Name.Contains(search));

            else if (!string.IsNullOrEmpty(search))
                Query = Query.Where(r => r.Name.Contains(search));

            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            return await Query.ToListAsync();
        }

        public async Task<Object> MediaCount(int userId, bool showAllData = false)
        {
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetLcmsCount";
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@UserId ", SqlDbType.NVarChar) { Value = userId });
                        cmd.Parameters.Add(new SqlParameter("@ShowAllData", SqlDbType.NVarChar) { Value = showAllData });
                       
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return null;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            var result = new
                            {
                                h5p = string.IsNullOrEmpty(row["h5p"].ToString()) ? 0 : int.Parse(row["h5p"].ToString()),
                                image = string.IsNullOrEmpty(row["image"].ToString()) ? 0 : int.Parse(row["image"].ToString()),
                                video = string.IsNullOrEmpty(row["video"].ToString()) ? 0 : int.Parse(row["video"].ToString()),
                                scorm = string.IsNullOrEmpty(row["scorm"].ToString()) ? 0 : int.Parse(row["scorm"].ToString()),
                                nonscorm = string.IsNullOrEmpty(row["nonscorm"].ToString()) ? 0 : int.Parse(row["nonscorm"].ToString()),
                                youtube = string.IsNullOrEmpty(row["youtube"].ToString()) ? 0 : int.Parse(row["youtube"].ToString()),
                                document = string.IsNullOrEmpty(row["document"].ToString()) ? 0 : int.Parse(row["document"].ToString()),
                                assessment = string.IsNullOrEmpty(row["assessment"].ToString()) ? 0 : int.Parse(row["assessment"].ToString()),
                                assignment = string.IsNullOrEmpty(row["assignment"].ToString()) ? 0 : int.Parse(row["assignment"].ToString()),
                                feedback = string.IsNullOrEmpty(row["feedback"].ToString()) ? 0 : int.Parse(row["feedback"].ToString()),
                                emojiFeedback = string.IsNullOrEmpty(row["emojifeedback"].ToString()) ? 0 : int.Parse(row["emojifeedback"].ToString()),
                                audio = string.IsNullOrEmpty(row["audio"].ToString()) ? 0 : int.Parse(row["audio"].ToString()),
                                Faq = string.IsNullOrEmpty(row["faq"].ToString()) ? 0 : int.Parse(row["faq"].ToString()),
                                externalLink = string.IsNullOrEmpty(row["externallink"].ToString()) ? 0 : int.Parse(row["externallink"].ToString()),
                                memo = string.IsNullOrEmpty(row["memo"].ToString()) ? 0 : int.Parse(row["memo"].ToString()),
                                survey = string.IsNullOrEmpty(row["survey"].ToString()) ? 0 : int.Parse(row["survey"].ToString()),
                                scorm12 = string.IsNullOrEmpty(row["scorm12"].ToString()) ? 0 : int.Parse(row["scorm12"].ToString()),
                                scorm2000 = string.IsNullOrEmpty(row["scorm2000"].ToString()) ? 0 : int.Parse(row["scorm2000"].ToString()),
                                Microlearning = string.IsNullOrEmpty(row["authoring"].ToString()) ? 0 : int.Parse(row["authoring"].ToString()),
                                xAPI = string.IsNullOrEmpty(row["xAPI"].ToString()) ? 0 : int.Parse(row["xAPI"].ToString()),
                                kpoint = string.IsNullOrEmpty(row["kpoint"].ToString()) ? 0 : int.Parse(row["kpoint"].ToString()),
                                knovel = string.IsNullOrEmpty(row["knovel"].ToString()) ? 0 : int.Parse(row["knovel"].ToString())
                            };
                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return null;
        }

        public async Task<IEnumerable<object>> Media(int page, int pageSize, string search = null, string metaData = null)
        {
            var result = (from lcms in _db.LCMS
                          where lcms.IsDeleted == false
                          select new
                          {
                              Id = lcms.Id,
                              Name = lcms.Name,
                              Path = lcms.Path,
                              ThumbnailPath = lcms.ThumbnailPath,
                              Version = lcms.Version,
                              IsBuiltInAssesment = lcms.IsBuiltInAssesment,
                              OriginalFileName= lcms.OriginalFileName,
                              IsMobileCompatible= lcms.IsMobileCompatible,
                              MetaData=lcms.MetaData,
                              ContentType=lcms.ContentType,
                              YoutubeVideoId=lcms.YoutubeVideoId,
                              AssessmentSheetConfigID=lcms.AssessmentSheetConfigID,
                              FeedbackSheetConfigID= lcms.FeedbackSheetConfigID,
                              Duration =lcms.Duration,
                              Language = lcms.Language
                          });
            
            if (search != null)
            {
                switch (search.ToLower())
                {
                    case "all":
                        if (metaData != null)
                            result = result.Where(l => l.MetaData.ToLower().Contains(metaData.ToLower()));
                        break;
                    case "feedback":
                        result = (from feedback in result
                                  join feedbackSheet in _db.FeedbackSheetConfiguration on feedback.FeedbackSheetConfigID equals feedbackSheet.Id
                                  where feedbackSheet.IsEmoji == false && feedback.ContentType.ToLower().Equals("feedback")
                                  select feedback);
                        if (metaData != null)
                            result = result.Where(f => f.MetaData.ToLower().Contains(metaData) || f.Name.ToLower().Contains(metaData.ToLower()));
                        break;

                    case "authoring":
                        result = (from authoring in result
                                  join authoringMaster in _db.AuthoringMaster on authoring.Id equals authoringMaster.LCMSId
                                  where authoringMaster.IsDeleted == false && authoring.ContentType.ToLower().Equals("microlearning")
                                  select authoring);
                        if (metaData != null)
                            result = result.Where(f => f.MetaData.ToLower().Contains(metaData) || f.Name.ToLower().Contains(metaData.ToLower()));
                        break;

                    case "emojifeedback":

                        result = (from feedback in result
                                  join feedbackSheet in _db.FeedbackSheetConfiguration on feedback.FeedbackSheetConfigID equals feedbackSheet.Id
                                  where feedbackSheet.IsEmoji == true && feedback.ContentType.ToLower().Equals("feedback")
                                  select feedback);
                        if (metaData != null)
                            result = result.Where(f => f.MetaData.ToLower().Contains(metaData) || f.Name.ToLower().Contains(metaData.ToLower()));
                        break;
                    case "xapi":
                        if (metaData == null)
                            result = result.Where(l => l.ContentType.ToLower().StartsWith(search.ToLower()) || l.ContentType.ToLower().StartsWith("cmi5"));
                        break;

                    default:
                        if (metaData == null)
                            result = result.Where(l => l.ContentType.ToLower().StartsWith(search.ToLower()));
                        else
                            result = result.Where(l => l.ContentType.ToLower().StartsWith(search.ToLower()) && (l.MetaData.ToLower().Contains(metaData.ToLower()) || l.Name.ToLower().Contains(metaData.ToLower())));
                        break;
                }
            }
            
            result = result.OrderByDescending(r => r.Id);
            if (page != -1)
                result = result.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                result = result.Take(pageSize);
            return await result.ToListAsync();
        }


        public async Task<IEnumerable<APILCMSMedia>> SurveyMedia(int page, int pageSize, string search = null, string metaData = null)
        {

            DataTable surveyLcms = new DataTable();
            if (search != null)
            {
                search = search.ToLower();

                    try
                    {
                        using (var dbContext = this._customerConnection.GetDbContext())
                        {
                            using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                            {
                                cmd.CommandText = "LCMSSurveyFilterNestedWithoutRoot";
                                cmd.CommandType = CommandType.StoredProcedure;
                                await dbContext.Database.OpenConnectionAsync();
                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                surveyLcms.Load(reader);
                                if (surveyLcms.Rows.Count <= 0)
                                {
                                    reader.Dispose();

                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        throw ex;
                    }
                
                    
            }

                IEnumerable<APILCMSMedia> res = DatatableToIEnumList(surveyLcms);
            if (metaData != null)
            {
                res = res.Where(l => l.MetaData.ToLower().Contains(metaData.ToLower()) || l.Name.ToLower().Contains(metaData.ToLower()));
            }
                
                res = res.OrderByDescending(r => r.Id);
                if (page != -1)
                    res = res.Skip((page - 1) * pageSize);
                if (pageSize != -1)
                    res = res.Take(pageSize);
                return res;

        }

        public IEnumerable<APILCMSMedia> DatatableToIEnumList(DataTable dataTable)
        {
            var lcmsList = new List<APILCMSMedia>();
            foreach (DataRow row in dataTable.Rows)
            {
                var apiLCMS =
                      new APILCMSMedia
                      {
                          Id = Convert.ToInt32(row["Id"]),
                          Name = Convert.ToString(row["Name"]),
                          Path = string.IsNullOrEmpty(row["Path"].ToString()) ? null : Convert.ToString(row["Path"]),
                          ThumbnailPath = string.IsNullOrEmpty(row["ThumbnailPath"].ToString()) ? null : Convert.ToString(row["ThumbnailPath"]),
                          Version = string.IsNullOrEmpty(row["Version"].ToString()) ? null : Convert.ToString(row["Version"]),
                          IsBuiltInAssesment = Convert.ToBoolean(row["IsBuiltInAssesment"]),
                          OriginalFileName = string.IsNullOrEmpty(row["OriginalFileName"].ToString()) ? null : row["OriginalFileName"].ToString(),
                          IsMobileCompatible = Convert.ToBoolean(row["IsMobileCompatible"]),
                          MetaData = Convert.ToString(row["MetaData"]),
                          ContentType = Convert.ToString(row["ContentType"]),
                          YoutubeVideoId = string.IsNullOrEmpty(row["YoutubeVideoId"].ToString()) ? null : Convert.ToString(row["YoutubeVideoId"]),
                          AssessmentSheetConfigID = string.IsNullOrEmpty(row["AssessmentSheetConfigID"].ToString()) ? 0 : Convert.ToInt32(row["AssessmentSheetConfigID"]),
                          FeedbackSheetConfigID = string.IsNullOrEmpty(row["FeedbackSheetConfigID"].ToString()) ? 0 : Convert.ToInt32(row["FeedbackSheetConfigID"]),
                          Duration = Convert.ToDouble(row["Duration"]),
                          Language = string.IsNullOrEmpty(row["YoutubeVideoId"].ToString()) ? null : Convert.ToString(row["Language"]),
                      };

                lcmsList.Add(apiLCMS);
            }
            return lcmsList.AsEnumerable();
        }

        public async Task<bool> ExistByType(string name, string contentType = null, int? id = null)
        {
            name = name.Trim();

            if (id == null)
            {
                if (await _db.LCMS.Where(r =>
                (r.Name.ToLower() == name.Trim().ToLower())
                && (r.ContentType==contentType)
                && r.IsDeleted == Record.NotDeleted).CountAsync() > 0)
                    return true;
            }
            else
            {
                if (await _db.LCMS.Where(r =>
               (r.Name.ToLower()== name.Trim().ToLower())
               && (r.ContentType.ToLower()== contentType.ToLower())
               && r.Id != id
               && r.IsDeleted == Record.NotDeleted).CountAsync() > 0)
                    return true;
            }
            return false;
        }


        public async Task<bool> ExistByTypeExternalLink(string name, string contentType = null, string Path = null, int? id = null)
        {
            name = name.Trim();
            Path = Path.Trim();
            if (id == null)
            {

                if (await _db.LCMS.Where(r =>
            ((r.Name.ToLower()== name.Trim().ToLower())
            || r.Path.ToLower()== Path.Trim().ToLower()
            && (r.ContentType.ToLower()== contentType.ToLower())
            && r.IsDeleted == Record.NotDeleted)).CountAsync() > 0)
                    return true;
            }
            else
            {
                if (await _db.LCMS.Where(r => ((r.Name.ToLower()==name.Trim().ToLower()
                || (r.Path.ToLower()== Path.Trim().ToLower()))
                && (r.ContentType.ToLower()== contentType.ToLower())
                && r.Id != id
                && r.IsDeleted == Record.NotDeleted)).CountAsync() > 0)
                    return true;
            }
            return false;
        }

        public async Task<bool> ExistYouTubeLink(string name, string contentType = null, string YoutubeVideoId = null, int? id = null)
        {
            name = name.Trim().ToLower();
            YoutubeVideoId = YoutubeVideoId.Trim();
            if (id == null)
            {



                if (await _db.LCMS.Where(r =>
                 ((r.Name.ToLower() == name.Trim().ToLower())
                )
                 && (r.ContentType.ToLower() == contentType.ToLower())
                 && r.IsDeleted == Record.NotDeleted).CountAsync() > 0)
                    return true;
            }
            else
            {
                if (await _db.LCMS.Where(r =>
               ( r.Name.ToLower() == name.Trim().ToLower())
               && (r.ContentType.ToLower()==contentType.ToLower())
               && r.Id != id
               && r.IsDeleted == Record.NotDeleted).CountAsync() > 0)
                    return true;
            }
            return false;
        }


        public async Task<bool> Exist(string fileName ,string contentType, int? id = null)
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
                if (await _db.LCMS.Where(r => r.Name.ToLower()==fileName.ToLower() && r.ContentType==contentType && r.Id != id && r.IsDeleted == Record.NotDeleted).CountAsync() > 0)
                    return true;
            }
            return false;
        }
        public async Task<bool> FileExist(string fileName, string version)
        {
            if (await _db.LCMS.Where(r =>
            r.Name == fileName
            && r.ContentType == version
            && r.IsDeleted == Record.NotDeleted)
            .CountAsync() > 0)
                return true;
            return false;
        }
        public async Task<int> GetMediaCount(string search = null, string metaData = null)
        {
            var result = (from lcms in _db.LCMS
                          where lcms.IsDeleted == false
                          select new
                          {
                              Id = lcms.Id,
                              lcms.MetaData,
                              lcms.Name,
                              lcms.ContentType,
                              lcms.FeedbackSheetConfigID
                          });
            if (search != null)
            {
                switch (search.ToLower())
                {
                    case "all":
                        if (metaData != null)
                            result = result.Where(l => l.MetaData.ToLower().Contains(metaData.ToLower()));
                        break;
                    case "feedback":
                        result = (from feedback in result
                                  join feedbackSheet in _db.FeedbackSheetConfiguration on feedback.FeedbackSheetConfigID equals feedbackSheet.Id
                                  where feedbackSheet.IsEmoji == false && feedback.ContentType.ToLower().Equals("feedback")
                                  select feedback);
                        if (metaData != null)
                            result = result.Where(f => f.MetaData.ToLower().Contains(metaData) || f.Name.ToLower().Contains(metaData.ToLower()));
                        break;
                    case "authoring":
                        result = (from authoring in result
                                  join authoringMaster in _db.AuthoringMaster on authoring.Id equals authoringMaster.LCMSId
                                  where authoringMaster.IsDeleted == false && authoring.ContentType.ToLower().Equals("microlearning")
                                  select authoring);
                        if (metaData != null)
                            result = result.Where(f => f.MetaData.ToLower().Contains(metaData) || f.Name.ToLower().Contains(metaData.ToLower()));
                        break;
                    case "emojifeedback":

                        result = (from feedback in result
                                  join feedbackSheet in _db.FeedbackSheetConfiguration on feedback.FeedbackSheetConfigID equals feedbackSheet.Id
                                  where feedbackSheet.IsEmoji == true && feedback.ContentType.ToLower().Equals("feedback")
                                  select feedback);
                        if (metaData != null)
                            result = result.Where(f => f.MetaData.ToLower().Contains(metaData) || f.Name.ToLower().Contains(metaData.ToLower()));
                        break;
                    case "xapi":                     
                        if (metaData == null)
                            result = result.Where(l => l.ContentType.ToLower().StartsWith(search.ToLower()) || l.ContentType.ToLower().StartsWith("cmi5"));
                        break;
                    default:
                        if (metaData == null)
                            result = result.Where(l => l.ContentType.ToLower().Contains(search.ToLower()));
                        else
                            result = result.Where(l => l.ContentType.ToLower().Contains(search.ToLower()) && (l.MetaData.ToLower().Contains(metaData.ToLower()) || l.Name.ToLower().Contains(metaData.ToLower())));
                        break;
                }
            }
            return await result.CountAsync();
        }
        public async Task<int> GetSurveyMediaCount(string search = null, string metaData = null)
        {
            DataTable surveyLcms = new DataTable();
            if (search != null)
            {
                search = search.ToLower();
                    try
                    {
                        using (var dbContext = this._customerConnection.GetDbContext())
                        {
                            using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                            {
                                cmd.CommandText = "LCMSSurveyFilterNestedWithoutRoot";
                                cmd.CommandType = CommandType.StoredProcedure;
                                await dbContext.Database.OpenConnectionAsync();
                                DbDataReader reader = await cmd.ExecuteReaderAsync();
                                surveyLcms.Load(reader);
                                if (surveyLcms.Rows.Count <= 0)
                                {
                                    reader.Dispose();

                                }

                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                        throw ex;
                    }                

            }

            IEnumerable<APILCMSMedia> res = DatatableToIEnumList(surveyLcms);
            if (metaData != null)
            {
                res = res.Where(l => l.MetaData.ToLower().Contains(metaData.ToLower()) || l.Name.ToLower().Contains(metaData.ToLower()));
            }

            return res.Count();
        }
        public Task<LCMS> GetLcmsByAssessmentConfigureId(int AssesmentConfigId)
        {
            return _db.LCMS.Where(a => a.AssessmentSheetConfigID == AssesmentConfigId && a.IsDeleted == Record.NotDeleted)
                   .FirstOrDefaultAsync();
        }
        public async Task<ApiResponse> SaveH5P(IFormFile uploadedFile, LCMSAPI lcmsApi, int UserId, string orgCode)
        {
            //String log = String.Empty;
            //log += "File name:" + uploadedFile.FileName;
            ApiResponse response = new ApiResponse();
            int FileSizeInMb = await _systemSettingRepository.GetScormFileMaxSizeInMb();
            long UploadedFileSizeInByte = uploadedFile.Length;
            int UploadedFileSizeInMb = (int)(UploadedFileSizeInByte / 1048576);
            //log += "\n\rFile size:" + UploadedFileSizeInMb;
            //log += "\n\rFile size max:" + FileSizeInMb;
            if (UploadedFileSizeInMb > FileSizeInMb)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Description = "File size must be less than " + FileSizeInMb + " Mb";
                return response;
            }

            // Save in secure folder

            string secure_filePath;
            string secure_fileName = string.Empty;

            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

            //log += "\n\rIn Blob:" + EnableBlobStorage;

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string secure_CoursePath = this._configuration["CoursePath"];
                secure_CoursePath = Path.Combine(secure_CoursePath, orgCode, Record.Courses, "zip");
                if (!Directory.Exists(secure_CoursePath))
                {
                    Directory.CreateDirectory(secure_CoursePath);
                }

                secure_fileName = Path.Combine(DateTime.Now.Ticks + lcmsApi.OriginalFileName);
                secure_fileName = string.Concat(secure_fileName.Split(' '));
                secure_filePath = Path.Combine(secure_CoursePath, secure_fileName);
                secure_filePath = string.Concat(secure_filePath.Split(' '));
                //log += "\n\rFile save path:" + secure_filePath;
                bool saved = await SaveFile(uploadedFile, secure_filePath);
                //log += "\n\rFile saved:" + saved;
            }
            else
            {                
                BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, orgCode, "zip", Record.Courses);                
                if (res != null)
                {
                    //log += "\n\rFile blob store:" + res.ToString() + " " + res.Error;
                    if (res.Error == false)
                    {
                        secure_filePath = res.Blob.Name.ToString();
                        string[] name = res.Blob.Name.Split('\\');
                        secure_fileName = name[name.Length - 1];
                    }
                    else
                    {
                        _logger.Error(res.ToString());
                    }
                }
            }           

            // Savee & Extract file for angular

            string coursesPath = string.Empty;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string CoursePath = this._configuration["CoursePathForExtract"];
            coursesPath = Path.Combine(CoursePath, "courses");
            if (!Directory.Exists(coursesPath))
            {
                Directory.CreateDirectory(coursesPath);
            }
            fileName = Path.Combine(DateTime.Now.Ticks + lcmsApi.OriginalFileName);
            fileName = string.Concat(fileName.Split(' '));
            filePath = Path.Combine(coursesPath, fileName);
            filePath = string.Concat(filePath.Split(' '));
            //log += "\n\rSaving at:" + filePath;
            bool filesaved = await SaveFile(uploadedFile, filePath);
            //log += "\n\rFile saved for extract:" + filesaved;


            string extractPath = Path.Combine(this._configuration["CoursePathForExtract"], "h5pcourses", Path.GetFileNameWithoutExtension(fileName));
            ZipExtactor extactor = new ZipExtactor();
            extactor.UnzipFile(extractPath, filePath);
            //log += "\n\rFile extracted:" + extractPath;
            File.Delete(filePath);            

            string ModuleType = "H5P";
            string startPage = string.Empty;
            string LaunchData = string.Empty;
            string activityid = string.Empty;
            if (lcmsApi.ContentType == "h5p")
            {
                string contentpath = Path.Combine(extractPath, "content");
                startPage = Path.Combine(contentpath, "content.json");
                LaunchData = extractPath;
                //log += "\n\rH5P validating:" + startPage;

                if (!File.Exists(startPage))
                {
                    File_Operation.DeleteDirectory(extractPath);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Description = "(Validation) Invalid h5p file";
                    return response;
                }

                lcmsApi.ScormType = "h5p";
                //log += "\n\rH5P validated:" + startPage;
            }
            else
            {
                File_Operation.DeleteDirectory(extractPath);
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Description = "(Validation) Invalid file type";
                return response;
            }
            
            LCMS Lcms = new LCMS();
            Lcms.ContentType = lcmsApi.ScormType;
            Lcms.Description = lcmsApi.Description;
            Lcms.Language = lcmsApi.Language;
            Lcms.MetaData = lcmsApi.MetaData;
            Lcms.Name = lcmsApi.Name;
            Lcms.Version = lcmsApi.Version;
            Lcms.Duration = lcmsApi.Duration == null ? 0 : float.Parse(lcmsApi.Duration.ToString());
            Lcms.IsBuiltInAssesment = lcmsApi.IsBuiltInAssesment == null ? false : Convert.ToBoolean(lcmsApi.IsBuiltInAssesment.ToString());
            Lcms.IsMobileCompatible = lcmsApi.IsMobileCompatible == null ? false : Convert.ToBoolean(lcmsApi.IsMobileCompatible.ToString());
            Lcms.CreatedBy = UserId;
            Lcms.CreatedDate = DateTime.UtcNow;
            Lcms.IsActive = lcmsApi.IsActive;
            Lcms.MimeType = uploadedFile.ContentType;
            Lcms.CreatedBy = UserId;
            Lcms.ModifiedBy = UserId;
            Lcms.CreatedDate = DateTime.UtcNow;
            Lcms.InternalName = secure_fileName;
            Lcms.IsActive = true;
            Lcms.OriginalFileName = uploadedFile.FileName;
            Lcms.LaunchData = LaunchData;
            Lcms.ActivityID = activityid;            

            Lcms.Path = startPage;
            Lcms.ZipPath = "";

            string FPath = "";

            FPath = WebUtility.UrlDecode(new System.Uri(extractPath).AbsoluteUri);
            Lcms.Path = string.Concat('/', FPath.Substring(FPath.LastIndexOf(Record.Assets)));
            await this.Add(Lcms);

            //log += "\n\rLcms Data:" + Lcms.ToString();

            if (lcmsApi.Ismodulecreate)
            {
                Module module = new Module();
                module.IsActive = true;
                module.LCMSId = Lcms.Id;
                module.Name = lcmsApi.Name;
                module.Description = lcmsApi.Name;
                module.CourseType = "elearning";
                module.ModuleType = ModuleType;
                module.CreatedDate = DateTime.UtcNow;
                module.ModifiedDate = DateTime.UtcNow;
                module.CreatedBy = UserId;
                module.ModifiedBy = UserId;
                module.IsMultilingual = false;
                await _moduleRepository.Add(module);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Description = "Success";
            //response.Description = log;
            return response;
        }
        public async Task<ApiResponse> SaveZip(IFormFile uploadedFile, LCMSAPI lcmsApi, int UserId, string orgCode)
        {
            if(lcmsApi.ScormType == "SCORM2004" || lcmsApi.ScormType == "SCORM1.2")
            {
                LCMS lCMS = _db.LCMS.Where(a => a.Name.ToLower() == lcmsApi.Name.ToLower()
                && a.Version == lcmsApi.Version && a.Language == lcmsApi.Language && a.IsDeleted == false
                   ).FirstOrDefault();

                ApiResponse response1 = new ApiResponse();

                if (lCMS != null)
                {
                    response1.StatusCode = StatusCodes.Status400BadRequest;
                    response1.Description = "Duplicate Data";
                    return response1;
                }
            }
           

            ApiResponse response = new ApiResponse();
            int FileSizeInMb = await _systemSettingRepository.GetScormFileMaxSizeInMb();
            long UploadedFileSizeInByte = uploadedFile.Length;
            int UploadedFileSizeInMb = (int)(UploadedFileSizeInByte / 1048576);
            if (UploadedFileSizeInMb > FileSizeInMb)
            {
                response.StatusCode = StatusCodes.Status400BadRequest;
                response.Description = "File size must be less than " + FileSizeInMb + " Mb";
                return response;
            }
           
            // Save in secure folder

            string secure_filePath = string.Empty;
            string secure_fileName = string.Empty;

            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string secure_CoursePath = this._configuration["CoursePath"];
                secure_CoursePath = Path.Combine(secure_CoursePath, orgCode, Record.Courses, "zip");
                if (!Directory.Exists(secure_CoursePath))
                {
                    Directory.CreateDirectory(secure_CoursePath);
                }

                secure_fileName = Path.Combine(DateTime.Now.Ticks + lcmsApi.OriginalFileName);
                secure_fileName = string.Concat(secure_fileName.Split(' '));
                secure_filePath = Path.Combine(secure_CoursePath, secure_fileName);
                secure_filePath = string.Concat(secure_filePath.Split(' '));
                await SaveFile(uploadedFile, secure_filePath);

            }
            else
            {
                BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, orgCode, "zip", Record.Courses);
                if (res != null)
                {
                    if (res.Error == false)
                    {
                        secure_filePath = res.Blob.Name.ToString();
                        string[] name = res.Blob.Name.Split('\\');                        
                        secure_fileName = name[name.Length - 1];
                    }
                    else
                    {
                        _logger.Error(res.ToString());
                    }
                }
            }

            // Savee & Extract file for angular

            string coursesPath = string.Empty;
            string filePath = string.Empty;
            string fileName = string.Empty;
            string CoursePath = this._configuration["CoursePathForExtract"];
            coursesPath = Path.Combine(CoursePath, "courses");
            if (!Directory.Exists(coursesPath))
            {
                Directory.CreateDirectory(coursesPath);
            }
            fileName = Path.Combine(DateTime.Now.Ticks + lcmsApi.OriginalFileName);
            fileName = string.Concat(fileName.Split(' '));
            filePath = Path.Combine(coursesPath, fileName);
            filePath = string.Concat(filePath.Split(' '));
            await SaveFile(uploadedFile, filePath);


            string extractPath = Path.Combine(this._configuration["CoursePathForExtract"], "courses", Path.GetFileNameWithoutExtension(fileName));
            ZipExtactor extactor = new ZipExtactor();
            extactor.UnzipFile(extractPath, filePath);

            File.Delete(filePath);


            string ModuleType = "SCORM";
            string startPage = string.Empty;
            string LaunchData = string.Empty;
            string activityid = string.Empty;
            if (lcmsApi.IsScorm)
            {
                startPage = extactor.GetRelativePath(extractPath, "");
                LaunchData = extactor.GetLaunchData(extractPath);
                if (string.IsNullOrEmpty(startPage))
                {
                    File_Operation.DeleteDirectory(extractPath);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Description = "Invalid Scorm file";
                    return response;
                }
            }
            if (!lcmsApi.IsScorm)
            {
                if (lcmsApi.StartPagePath.Length > 0 && !string.IsNullOrEmpty(lcmsApi.StartPagePath))
                {
                    lcmsApi.StartPagePath = lcmsApi.StartPagePath == "undefined"? null: lcmsApi.StartPagePath;
                }
               
                activityid = extactor.GetxAPIData(extractPath);               
                if (string.IsNullOrEmpty(activityid) && !string.IsNullOrEmpty(lcmsApi.StartPagePath))
                {
                    startPage = extactor.GetRelativePath(extractPath, lcmsApi.StartPagePath);                   
                    lcmsApi.ContentType = "nonscorm";
                    lcmsApi.ScormType = "nonscorm";
                    ModuleType = "nonSCORM";
                }
                else if(string.IsNullOrEmpty(activityid) && string.IsNullOrEmpty(lcmsApi.StartPagePath))
                {
                    activityid = extactor.Getcmi5ActivityId(extractPath);
                    startPage = extactor.GetRelativePath_CMI5(extractPath, lcmsApi.StartPagePath);
                    lcmsApi.ContentType = "cmi5";
                    lcmsApi.ScormType = "cmi5";
                    ModuleType = "cmi5";
                }
                else
                {
                    startPage = extactor.GetRelativePath(extractPath,extactor.GetxAPIDataPath(extractPath));
                    lcmsApi.ContentType = "xAPI";
                    lcmsApi.ScormType = "xAPI";
                    ModuleType = "xAPI";
                }

                if (string.IsNullOrEmpty(lcmsApi.StartPagePath) && string.IsNullOrEmpty(startPage))
                {
                    File_Operation.DeleteDirectory(extractPath);
                    response.StatusCode = StatusCodes.Status400BadRequest;
                    response.Description = "Invalid Scorm file";
                    return response;
                }
               
            }
            LCMS Lcms = new LCMS();
            Lcms.ContentType = lcmsApi.ScormType;
            Lcms.Description = lcmsApi.Description;
            Lcms.Language = lcmsApi.Language;
            Lcms.MetaData = lcmsApi.MetaData;
            Lcms.Name = lcmsApi.Name;
            Lcms.Version = lcmsApi.Version;
            Lcms.Duration = lcmsApi.Duration == null ? 0 : float.Parse(lcmsApi.Duration.ToString());
            Lcms.IsBuiltInAssesment = lcmsApi.IsBuiltInAssesment == null ? false : Convert.ToBoolean(lcmsApi.IsBuiltInAssesment.ToString());
            Lcms.IsMobileCompatible = lcmsApi.IsMobileCompatible == null ? false : Convert.ToBoolean(lcmsApi.IsMobileCompatible.ToString());
            Lcms.CreatedBy = UserId;
            Lcms.CreatedDate = DateTime.UtcNow;
            Lcms.IsActive = lcmsApi.IsActive;
            Lcms.MimeType = uploadedFile.ContentType;
            Lcms.CreatedBy = UserId;
            Lcms.ModifiedBy = UserId;
            Lcms.CreatedDate = DateTime.UtcNow;
            Lcms.InternalName = secure_fileName;
            Lcms.IsActive = true;
            Lcms.OriginalFileName = uploadedFile.FileName;
            Lcms.LaunchData = LaunchData;
            Lcms.ActivityID = activityid;

            string FPath = "";
           
                 FPath = WebUtility.UrlDecode(new System.Uri(startPage).AbsoluteUri);
          
            string ZipPath = new System.Uri(filePath).AbsoluteUri;
            Lcms.Path = string.Concat('/', FPath.Substring(FPath.LastIndexOf(Record.Assets)));
            Lcms.Path = Lcms.Path.Replace("HTML5/player/APIWrapper.js", "HTML5/demo.html");


         Lcms.ZipPath = string.Concat('/', ZipPath.Substring(ZipPath.LastIndexOf(Record.Assets)));
            await this.Add(Lcms);

            if (lcmsApi.Ismodulecreate)
            {
                Module module = new Module();
                module.IsActive = true;
                module.LCMSId = Lcms.Id;
                module.Name = lcmsApi.Name;
                module.Description = lcmsApi.Name;
                module.CourseType = "elearning";
                module.ModuleType = ModuleType;
                module.CreatedDate = DateTime.UtcNow;
                module.ModifiedDate = DateTime.UtcNow;
                module.CreatedBy = UserId;
                module.ModifiedBy = UserId;
                module.IsMultilingual = false;
                await _moduleRepository.Add(module);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Description = "Success";
            return response;
        }
        public async Task<ApiResponse> SaveVideo(IFormFile uploadedFile, LCMS lcms, int UserId, string ordCode)
        {
            LCMS lCMS = _db.LCMS.Where(a => a.Name.ToLower() == lcms.Name.ToLower()
              && a.Version == lcms.Version && a.Language == lcms.Language && a.IsDeleted == false
              && a.MetaData == lcms.MetaData && a.ContentType == FileType.Video
                 ).FirstOrDefault();

            ApiResponse response = new ApiResponse();

            if (lCMS != null)
            {
                response.StatusCode = StatusCodes.Status409Conflict;
                response.Description = "Duplicate Data";
                return response;
            }
            string filePath = string.Empty;
            string fileName = string.Empty;
            
            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

            _logger.Info("video EnableBlobStorage : " + EnableBlobStorage);

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string coursesPath = this._configuration["CoursePath"];
                coursesPath = Path.Combine(coursesPath, ordCode, Record.Courses);
                if (!Directory.Exists(coursesPath))
                {
                    Directory.CreateDirectory(coursesPath);
                }
                fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                fileName = string.Concat(fileName.Split(' '));
                filePath = Path.Combine(coursesPath, FileType.Video);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                filePath = Path.Combine(coursesPath, FileType.Video, fileName);
                filePath = string.Concat(filePath.Split(' '));
                await SaveFile(uploadedFile, filePath);
            }
            else
            {
                try
                {
                    BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, ordCode, FileType.Video, Record.Courses);
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            filePath = res.Blob.Name.ToString();
                            string[] name = res.Blob.Name.Split('\\');
                           fileName = name[name.Length - 1];
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
            }
            
            lcms.MimeType = uploadedFile.ContentType;
            lcms.CreatedBy = UserId;
            lcms.ModifiedBy = UserId;
            lcms.CreatedDate = DateTime.UtcNow;
            lcms.InternalName = fileName;
            lcms.IsActive = true;
            lcms.ContentType = FileType.Video;
            lcms.OriginalFileName = uploadedFile.FileName;

            string DomainName = this._configuration["ApiGatewayUrl"];
            

            string orgc = "org-content/";
            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                var uri = new System.Uri(filePath);
                filePath = uri.AbsoluteUri;
                string FPath = new System.Uri(filePath).AbsoluteUri;
                lcms.Path = string.Concat(DomainName, orgc, ordCode, '/', FPath.Substring(FPath.LastIndexOf(Record.Courses)));
            }
            else
            {
                lcms.Path = string.Concat(DomainName, orgc, filePath);
            }

            await this.Add(lcms);

            if (lcms.Ismodulecreate)
            {
                Module module = new Module();
                module.IsActive = true;
                module.LCMSId = lcms.Id;
                module.Name = lcms.Name;
                module.Description = lcms.Name;
                module.CourseType = "elearning";
                module.ModuleType = "Video";
                module.CreatedDate = DateTime.UtcNow;
                module.ModifiedDate = DateTime.UtcNow;
                module.CreatedBy = UserId;
                module.ModifiedBy = UserId;
                module.IsMultilingual = false;
                await _moduleRepository.Add(module);
            }
            response.StatusCode = StatusCodes.Status200OK;
            response.Description = "Success";
            return response;
        }
        public async Task<int> SaveVimeoLink(VimeoVideo vimeoVideo,int UserId)
        {
            if(vimeoVideo == null)
            {
                return -1;
            }
            LCMS lCMS = _db.LCMS.Where(a => a.Name == vimeoVideo.name && a.SubContentType == "vimeo").FirstOrDefault();
            if(lCMS != null)
            {
                return -1;
            }
            LCMS Lcms = new LCMS();
            Lcms.ContentType = "video";
            Lcms.Description = vimeoVideo.description;
            Lcms.Language = vimeoVideo.language;
            Lcms.MetaData = vimeoVideo.metaData;
            Lcms.Name = vimeoVideo.name;
            Lcms.Version = vimeoVideo.version;
            Lcms.Duration = vimeoVideo.duration == null ? 0 : float.Parse(vimeoVideo.duration.ToString());
            Lcms.IsBuiltInAssesment = vimeoVideo.isBuiltInAssesment == null ? false : Convert.ToBoolean(vimeoVideo.isBuiltInAssesment.ToString());
            Lcms.IsMobileCompatible = vimeoVideo.isMobileCompatible == null ? false : Convert.ToBoolean(vimeoVideo.isMobileCompatible.ToString());
            Lcms.CreatedBy = UserId;
            Lcms.ModifiedBy = UserId;
            Lcms.CreatedDate = DateTime.UtcNow;
            Lcms.IsActive = true;
            Lcms.Ismodulecreate = vimeoVideo.ismodulecreate;
            Lcms.Path = vimeoVideo.VideoLink;

            Lcms.MimeType = "video";
            Lcms.CreatedBy = UserId;
            Lcms.ModifiedBy = UserId;
            Lcms.CreatedDate = DateTime.UtcNow;
            Lcms.InternalName = null;
            Lcms.IsActive = true;
            Lcms.ContentType = FileType.Video;
            Lcms.OriginalFileName = vimeoVideo.name;
            Lcms.SubContentType = "vimeo";

            await this.Add(Lcms);

            if (Lcms.Ismodulecreate)
            {
                SaveModule(Lcms, UserId);
            }
            return 1;
        }
        private async void SaveModule(LCMS lcms, int UserId)
        {
            Module module = new Module();
            module.IsActive = true;
            module.LCMSId = lcms.Id;
            module.Name = lcms.Name;
            module.Description = lcms.Name;
            module.CourseType = "elearning";
            module.ModuleType = "Video";
            module.CreatedDate = DateTime.UtcNow;
            module.ModifiedDate = DateTime.UtcNow;
            module.CreatedBy = UserId;
            module.ModifiedBy = UserId;
            module.IsMultilingual = false;
            await _moduleRepository.Add(module);
        }
        public async Task<VimeoConfiguration> GetVimeoToken()
        {
            VimeoConfiguration vimeoConfiguration = await _db.VimeoConfiguration.FirstOrDefaultAsync();
            return vimeoConfiguration;
        }
        public async Task<VimeoLink> GetVimeoVideo(LCMSID lCMSID)
        {
            if(lCMSID == null)
            {
                return null;
            }
            LCMS lCMS = await _db.LCMS.Where(a => a.Id == lCMSID.lcmsId).FirstOrDefaultAsync();
            if(lCMS == null)
            {
                return null;
            }
            VimeoLink vimeoLink = new VimeoLink();
            vimeoLink.Link = lCMS.Path;
            return vimeoLink;
        }
        public async Task<ApiResponse> SaveAudio(IFormFile uploadedFile, LCMS lcms, int UserId, string ordCode)
        {
            LCMS lCMS = _db.LCMS.Where(a => a.Name.ToLower() == lcms.Name.ToLower()
              && a.Version == lcms.Version && a.Language == lcms.Language && a.IsDeleted == false
              && a.MetaData == lcms.MetaData && a.ContentType == FileType.Audio
                 ).FirstOrDefault();

            ApiResponse response = new ApiResponse();

            if (lCMS != null)
            {
                response.StatusCode = StatusCodes.Status409Conflict;
                response.Description = "Duplicate Data";
                return response;
            }
            string filePath = string.Empty;
            string fileName = string.Empty;

            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
            _logger.Info("audio EnableBlobStorage : " + EnableBlobStorage);

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string coursesPath = this._configuration["CoursePath"];
                coursesPath = Path.Combine(coursesPath, ordCode, Record.Courses);
                if (!Directory.Exists(coursesPath))
                {
                    Directory.CreateDirectory(coursesPath);
                }
                fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                fileName = string.Concat(fileName.Split(' '));
                filePath = Path.Combine(coursesPath, FileType.Audio);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                filePath = Path.Combine(coursesPath, FileType.Audio, fileName);
                filePath = string.Concat(filePath.Split(' '));
                await SaveFile(uploadedFile, filePath);
            }
            else
            {
                try
                {
                    BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, ordCode, FileType.Audio, Record.Courses);
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            filePath = res.Blob.Name.ToString();
                            string[] name = res.Blob.Name.Split('\\');
                           fileName = name[name.Length - 1];
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
            }

            
            lcms.MimeType = uploadedFile.ContentType;
            lcms.CreatedBy = UserId;
            lcms.ModifiedBy = UserId;
            lcms.CreatedDate = DateTime.UtcNow;
            lcms.InternalName = fileName;
            lcms.IsActive = true;
            lcms.ContentType = FileType.Audio;
            lcms.OriginalFileName = uploadedFile.FileName;
            string DomainName = this._configuration["ApiGatewayUrl"];

            string orgc = "org-content/";
            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                var uri = new System.Uri(filePath);
                filePath = uri.AbsoluteUri;
                string FPath = new System.Uri(filePath).AbsoluteUri;
                lcms.Path = string.Concat(DomainName, orgc, ordCode, '/', FPath.Substring(FPath.LastIndexOf(Record.Courses)));
            }
            else
            {
                lcms.Path = string.Concat(DomainName, orgc, filePath);                
            }
           
            await this.Add(lcms);

            if (lcms.Ismodulecreate)
            {
                Module module = new Module();
                module.IsActive = true;
                module.LCMSId = lcms.Id;
                module.Name = lcms.Name;
                module.Description = lcms.Name;
                module.CourseType = "elearning";
                module.ModuleType = "Audio";
                module.CreatedDate = DateTime.UtcNow;
                module.ModifiedDate = DateTime.UtcNow;
                module.CreatedBy = UserId;
                module.ModifiedBy = UserId;
                module.IsMultilingual = false;
                await _moduleRepository.Add(module);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Description = "Success";
            return response;
        }
        public async Task<ApiResponse> SavePdf(IFormFile uploadedFile, LCMS lcms, int UserId, string ordCode)
        {
            LCMS lCMS = _db.LCMS.Where(a => a.Name.ToLower() == lcms.Name.ToLower()
             && a.Version == lcms.Version && a.Language == lcms.Language && a.IsDeleted == false
             && a.MetaData == lcms.MetaData && a.ContentType == FileType.Document
                ).FirstOrDefault();

            ApiResponse response = new ApiResponse();

            if (lCMS != null)
            {
                response.StatusCode = StatusCodes.Status409Conflict;
                response.Description = "Duplicate Data";
                return response;
            }
            string filePath = string.Empty;
            string fileName = string.Empty;

            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");
            _logger.Info("pdf EnableBlobStorage : " + EnableBlobStorage);

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                var request = _httpContextAccessor.HttpContext.Request;
                string coursesPath = this._configuration["CoursePath"];
                coursesPath = Path.Combine(coursesPath, ordCode, Record.Courses);
                if (!Directory.Exists(coursesPath))
                {
                    Directory.CreateDirectory(coursesPath);
                }
                fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                fileName = string.Concat(fileName.Split(' '));
                filePath = Path.Combine(coursesPath, FileType.Pdf);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                filePath = Path.Combine(coursesPath, FileType.Pdf, fileName);
                filePath = string.Concat(filePath.Split(' '));
                await SaveFile(uploadedFile, filePath);
            }
            else
            {
                try
                {
                    BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, ordCode, FileType.Pdf, Record.Courses);
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                           filePath = res.Blob.Name.ToString();
                           string[] name = res.Blob.Name.Split('\\');
                           fileName = name[name.Length - 1];
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
            }
           
            lcms.MimeType = uploadedFile.ContentType;
            lcms.CreatedBy = UserId;
            lcms.ModifiedBy = UserId;
            lcms.CreatedDate = DateTime.UtcNow;
            lcms.InternalName = fileName;
            lcms.IsActive = true;
            lcms.OriginalFileName = uploadedFile.FileName;
            lcms.ContentType = FileType.Document;
            string DomainName = this._configuration["ApiGatewayUrl"];     
            string orgc = "org-content/";
            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                var uri = new System.Uri(filePath);
                filePath = uri.AbsoluteUri;
                string FPath = new System.Uri(filePath).AbsoluteUri;
                lcms.Path = string.Concat(DomainName, orgc, ordCode, '/', FPath.Substring(FPath.LastIndexOf(Record.Courses)));
            }
            else
            {
                lcms.Path = string.Concat(DomainName, orgc, filePath);
            }
            await this.Add(lcms);

            if (lcms.Ismodulecreate)
            {
                Module module = new Module();
                module.IsActive = true;
                module.LCMSId = lcms.Id;
                module.Name = lcms.Name;
                module.Description = lcms.Name;
                module.CourseType = "elearning";
                module.ModuleType = "Document";
                module.CreatedDate = DateTime.UtcNow;
                module.ModifiedDate = DateTime.UtcNow;
                module.CreatedBy = UserId;
                module.ModifiedBy = UserId;
                module.IsMultilingual = false;
                await _moduleRepository.Add(module);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Description = "Success";
            return response;
        }
        public async Task<ApiResponse> SaveImage(IFormFile uploadedFile, LCMS lcms, int UserId, string ordCode)
        {
            LCMS lCMS = _db.LCMS.Where(a => a.Name.ToLower() == lcms.Name.ToLower()
              && a.Version == lcms.Version && a.Language == lcms.Language && a.IsDeleted == false
              && a.MetaData == lcms.MetaData && a.ContentType == FileType.Image
                 ).FirstOrDefault();

            ApiResponse response = new ApiResponse();

            if (lCMS != null)
            {
                response.StatusCode = StatusCodes.Status409Conflict;
                response.Description = "Duplicate Data";
                return response;
            }
            string filePath = string.Empty;
            string fileName = string.Empty;
            var EnableBlobStorage = await _courseRepository.GetMasterConfigurableParameterValue("Enable_BlobStorage");

            _logger.Info("image EnableBlobStorage : " + EnableBlobStorage);

            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                _logger.Info("image");

                var request = _httpContextAccessor.HttpContext.Request;
                string coursesPath = this._configuration["CoursePath"];
                coursesPath = Path.Combine(coursesPath, ordCode, Record.Courses);
                if (!Directory.Exists(coursesPath))
                {
                    Directory.CreateDirectory(coursesPath);
                }
                fileName = Path.Combine(DateTime.Now.Ticks + uploadedFile.FileName.Trim());
                fileName = string.Concat(fileName.Split(' '));
                filePath = Path.Combine(coursesPath, FileType.Image);
                if (!Directory.Exists(filePath))
                {
                    Directory.CreateDirectory(filePath);
                }
                filePath = Path.Combine(coursesPath, FileType.Image, fileName);
                filePath = string.Concat(filePath.Split(' '));
                await SaveFile(uploadedFile, filePath);
                if (!string.IsNullOrEmpty(lcms.ThumbnailPath))
                {
                    string ThumbnailPath = Path.Combine(coursesPath, FileType.Image, Record.Thumbnail);
                    if (!Directory.Exists(ThumbnailPath))
                    {
                        Directory.CreateDirectory(ThumbnailPath);
                    }
                    ThumbnailPath = Path.Combine(ThumbnailPath, fileName);
                    if (GetThumbnail(filePath, ThumbnailPath))
                        lcms.ThumbnailPath = ThumbnailPath;
                }
            }
            else
            {
                try
                {
                    BlobResponseDto res = await _azurestorage.UploadAsync(uploadedFile, ordCode, FileType.Image, Record.Courses);
                    if (res != null)
                    {
                        if (res.Error == false)
                        {
                            filePath = res.Blob.Name.ToString();
                            string[] name = res.Blob.Name.Split('\\');
                           fileName = name[name.Length - 1];
                        }
                        else
                        {

                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }

            
            lcms.MimeType = uploadedFile.ContentType;
            lcms.CreatedBy = UserId;
            lcms.ModifiedBy = UserId;
            lcms.CreatedDate = DateTime.UtcNow;
            lcms.InternalName = fileName;
            lcms.ContentType = FileType.Image;
            lcms.IsActive = true;
            lcms.OriginalFileName = uploadedFile.FileName;
            string DomainName = this._configuration["ApiGatewayUrl"];            
            string orgc = "org-content/";
            if (Convert.ToString(string.IsNullOrEmpty(EnableBlobStorage) ? "no" : EnableBlobStorage).ToLower() == "no")
            {
                var uri = new System.Uri(filePath);
                filePath = uri.AbsoluteUri;
                string FPath = new System.Uri(filePath).AbsoluteUri;
                lcms.Path = string.Concat(DomainName, orgc, ordCode, '/', FPath.Substring(FPath.LastIndexOf(Record.Courses)));
            }
            else
            {
                lcms.Path = string.Concat(DomainName, orgc, filePath);
            }
           
            await this.Add(lcms);

            if (lcms.Ismodulecreate)
            {
                Module module = new Module();
                module.IsActive = true;
                module.LCMSId = lcms.Id;
                module.Name = lcms.Name;
                module.Description = lcms.Name;
                module.CourseType = "elearning";
                module.ModuleType = "Document";
                module.CreatedDate = DateTime.UtcNow;
                module.ModifiedDate = DateTime.UtcNow;
                module.CreatedBy = UserId;
                module.ModifiedBy = UserId;
                module.IsMultilingual = false;
                await _moduleRepository.Add(module);
            }

            response.StatusCode = StatusCodes.Status200OK;
            response.Description = "Success";
            return response;
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
       
        public async Task<int> AddYoutubeFile(LCMSAPI lcmsApi, int UserId)
        {
            if (lcmsApi.YoutubeVideoId.Length != 11)
            {
                return 0;
            }
            LCMS lcms = new LCMS();
            lcms.ContentType = lcmsApi.ContentType;
            lcms.Description = lcmsApi.Description;
            lcms.Language = lcmsApi.Language;
            lcms.MetaData = lcmsApi.MetaData;
            lcms.Name = lcmsApi.Name;
            lcms.Version = lcmsApi.Version;
            lcms.Duration = float.Parse(lcmsApi.Duration.ToString());
            lcms.IsBuiltInAssesment = Convert.ToBoolean(lcmsApi.IsBuiltInAssesment.ToString());
            lcms.IsMobileCompatible = Convert.ToBoolean(lcmsApi.IsMobileCompatible.ToString());
            lcms.ContentType = "youtube";
            lcms.CreatedBy = UserId;
            lcms.ModifiedBy = UserId;
            lcms.CreatedDate = DateTime.UtcNow;
            lcms.IsActive = true;
            lcms.Path = lcmsApi.Path;
            lcms.YoutubeVideoId = lcmsApi.YoutubeVideoId;
            await this.Add(lcms);

            if (lcmsApi.Ismodulecreate)
            {
                Module module = new Module();
                module.IsActive = true;
                module.LCMSId = lcms.Id;
                module.Name = lcms.Name;
                module.Description = lcms.Name;
                module.CourseType = "elearning";
                module.ModuleType = "YouTube";
                module.CreatedDate = DateTime.UtcNow;
                module.ModifiedDate = DateTime.UtcNow;
                module.CreatedBy = UserId;
                module.ModifiedBy = UserId;
                module.IsMultilingual = false;
                await _moduleRepository.Add(module);
            }

            return 1;
        }
        public async Task<int> AddAssesment(LCMSAPI lcmsApi, int UserId)
        {
            try
            {
                LCMS lcms = new LCMS();
                lcms.Description = lcmsApi.Description;
                lcms.MetaData = lcmsApi.MetaData;
                lcms.Name = lcmsApi.Name;
                lcms.AssessmentSheetConfigID = lcmsApi.AssessmentSheetConfigID;
                lcms.ContentType = "assesment";
                lcms.CreatedBy = UserId;
                lcms.ModifiedBy = UserId;
                lcms.CreatedDate = DateTime.UtcNow;
                lcms.IsActive = true;
                await this.Add(lcms);
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return 0;
            }
        }
        public async Task<int> AddFeedback(LCMSAPI lcmsApi, int UserId)
        {
            try
            {
                LCMS lcms = new LCMS();
                lcms.Description = lcmsApi.Description;
                lcms.MetaData = lcmsApi.MetaData;
                lcms.Name = lcmsApi.Name;
                lcms.FeedbackSheetConfigID = lcmsApi.AssessmentSheetConfigID;
                lcms.ContentType = "feedback";
                lcms.CreatedBy = UserId;
                lcms.ModifiedBy = UserId;
                lcms.CreatedDate = DateTime.UtcNow;
                lcms.IsActive = true;
                await this.Add(lcms);

                if (lcmsApi.Ismodulecreate)
                {
                    Module module = new Module();
                    module.IsActive = true;
                    module.LCMSId = lcms.Id;
                    module.Name = lcmsApi.Name;
                    module.Description = lcmsApi.MetaData;
                    module.CourseType = "Feedback";
                    module.ModuleType = "Feedback";
                    module.CreatedDate = DateTime.UtcNow;
                    module.ModifiedDate = DateTime.UtcNow;
                    module.CreatedBy = UserId;
                    module.ModifiedBy = UserId;
                    module.IsMultilingual = false;
                    await _moduleRepository.Add(module);
                }

                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return 0;
            }
        }


        public async Task<int> AddSurvey(LCMSAPI lcmsApi, int UserId)
        {
            try
            {
                LCMS lcms = new LCMS();
                lcms.Description = lcmsApi.Description;
                lcms.MetaData = lcmsApi.MetaData;
                lcms.Name = lcmsApi.Name;
                lcms.ContentType = "survey";
                lcms.CreatedBy = UserId;
                lcms.ModifiedBy = UserId;
                lcms.CreatedDate = DateTime.UtcNow;
                lcms.IsActive = true;
                await this.Add(lcms);
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                return 0;
            }
        }
        public bool GetThumbnail(string imagePath, string thumbnailPath)
        {
            System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath);
            int srcWidth = image.Width;
            int srcHeight = image.Height;
            int thumbWidth = 200;
            int thumbHeight;
            Bitmap bmp;
            if (srcHeight > srcWidth)
            {
                thumbHeight = (srcHeight / srcWidth) * thumbWidth;
                bmp = new Bitmap(thumbWidth, thumbHeight);
            }
            else
            {
                thumbHeight = thumbWidth;
                thumbWidth = (srcWidth / srcHeight) * thumbHeight;
                bmp = new Bitmap(thumbWidth, thumbHeight);
            }
            try
            {
                System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(bmp);
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                gr.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;
                System.Drawing.Rectangle rectDestination =
                       new System.Drawing.Rectangle(0, 0, thumbWidth, thumbHeight);
                gr.DrawImage(image, rectDestination, 0, 0, srcWidth, srcHeight, GraphicsUnit.Pixel);
                bmp.Save(thumbnailPath);
                bmp.Dispose();
                image.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                bmp.Dispose();
                image.Dispose();
                return false;
            }
        }

        public async Task<bool> IsLcmsDependacyExist(int lcmsId)
        {
            int LcmsCount = await this._db.Module.Where(m => m.LCMSId == lcmsId && m.IsDeleted == Record.NotDeleted).Select(m => m.LCMSId).CountAsync();
            int CourseAssessmentCount = await (from courses in _db.Course
                                               join assessment in _db.Module on courses.AssessmentId equals assessment.Id
                                               join lcms in this._db.LCMS on assessment.LCMSId equals lcms.Id
                                               join assesmentConfig in this._db.AssessmentSheetConfiguration on lcms.AssessmentSheetConfigID equals assesmentConfig.ID
                                               where
                                               lcms.Id == lcmsId
                                               && assesmentConfig.IsDeleted == Record.NotDeleted
                                               && courses.IsDeleted == false
                                               && assessment.IsDeleted == false
                                               select lcms.Id).CountAsync();
            int ModuleAssessmentCount = await (from CourseModuleAssociation in _db.CourseModuleAssociation
                                               join module in _db.Module on CourseModuleAssociation.ModuleId equals module.Id
                                               join assessment in _db.Module on CourseModuleAssociation.AssessmentId equals assessment.Id
                                               join lcms in this._db.LCMS on assessment.LCMSId equals lcms.Id
                                               join assesmentConfig in this._db.AssessmentSheetConfiguration on lcms.AssessmentSheetConfigID equals assesmentConfig.ID
                                               where
                                               lcms.Id == lcmsId
                                               && assesmentConfig.IsDeleted == Record.NotDeleted
                                               && module.IsDeleted == false
                                               && assessment.IsDeleted == false
                                               select lcms.Id).CountAsync();
            int CourseFeedbackCount = await (from courses in _db.Course
                                             join Feedback in _db.Module on courses.FeedbackId equals Feedback.Id
                                             join lcms in this._db.LCMS on Feedback.LCMSId equals lcms.Id
                                             join feedbackconfig in this._db.FeedbackSheetConfiguration on lcms.FeedbackSheetConfigID equals feedbackconfig.Id
                                             where
                                             lcms.Id == lcmsId
                                             && feedbackconfig.IsDeleted == Record.NotDeleted
                                             && courses.IsDeleted == false
                                             && Feedback.IsDeleted == false
                                             select lcms.Id).CountAsync();
            int ModuleFeedbackCount = await (from CourseModuleAssociation in _db.CourseModuleAssociation
                                             join module in _db.Module on CourseModuleAssociation.ModuleId equals module.Id
                                             join Feedback in _db.Module on CourseModuleAssociation.FeedbackId equals Feedback.Id
                                             join lcms in this._db.LCMS on Feedback.LCMSId equals lcms.Id
                                             join feedbackconfig in this._db.FeedbackSheetConfiguration on lcms.FeedbackSheetConfigID equals feedbackconfig.Id
                                             where
                                             lcms.Id == lcmsId
                                             && feedbackconfig.IsDeleted == Record.NotDeleted
                                             && module.IsDeleted == false
                                             && Feedback.IsDeleted == false
                                             select lcms.Id).CountAsync();
            if (LcmsCount > 0 || ModuleAssessmentCount > 0 || CourseAssessmentCount > 0 || ModuleFeedbackCount > 0 || CourseFeedbackCount > 0)
                return true;
            return false;
        }
        public async Task<Message> DeleteLcms(int lcmsId)
        {
            LCMS Lcms = await this._db.LCMS.Where(l => l.Id == lcmsId && l.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
            if (Lcms == null)
                return Message.NotFound;
            if (await IsLcmsDependacyExist(lcmsId))
                return Message.DependencyExist;
            Lcms.IsDeleted = Record.Deleted;
            await this.Update(Lcms);

            AssessmentSheetConfiguration assessmentSheetConfiguration = await this._db.AssessmentSheetConfiguration.Where(l => l.ID == Lcms.AssessmentSheetConfigID && l.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();
            if (assessmentSheetConfiguration != null)
            {
                assessmentSheetConfiguration.IsDeleted = Record.Deleted;
                _db.AssessmentSheetConfiguration.Update(assessmentSheetConfiguration);
                await _db.SaveChangesAsync();
            }
            List<AssessmentSheetConfigurationDetails> assessmentSheetConfigurationd = await this._db.AssessmentSheetConfigurationDetails.Where(l => l.AssessmentSheetConfigID == Lcms.AssessmentSheetConfigID && l.IsDeleted == Record.NotDeleted).ToListAsync();

            foreach (AssessmentSheetConfigurationDetails item in assessmentSheetConfigurationd)
            {
                item.IsDeleted = Record.Deleted;
            }
            _db.AssessmentSheetConfigurationDetails.UpdateRange(assessmentSheetConfigurationd);
            await _db.SaveChangesAsync();
            return Message.Success;
        }


        public async Task<IEnumerable<object>> GetxAPILaunchData(int moduleid,string orgnizationCode, int UserId)
        {

            string UserUrl = _configuration[APIHelper.UserAPI];
            string NameById = "GetNameById";
            string ColumnName = "username";
            int Value = UserId;
            HttpResponseMessage response = await APIHelper.CallGetAPI(UserUrl + NameById + "/" + orgnizationCode + "/" + ColumnName + "/" + Value);
            xAPIUserDetails _xAPIUserDetails = new xAPIUserDetails();
            if (response.IsSuccessStatusCode)
            {
                var username = await response.Content.ReadAsStringAsync();
                _xAPIUserDetails = JsonConvert.DeserializeObject<xAPIUserDetails>(username);
              
            }

            var result = (from lcms in _db.LCMS
                          join module in _db.Module on lcms.Id equals module.LCMSId
                          where lcms.IsDeleted == false && lcms.Id == moduleid && lcms.ContentType == "xAPI"
                          select new
                          {
                              Path = lcms.Path,
                              activityid = lcms.ActivityID,
                              UserName = _xAPIUserDetails.Name,
                              EmailId = _xAPIUserDetails.EmailId,
                              endpoint = _configuration[APIHelper.xAPIEndPoint],
                              basic = _configuration[APIHelper.xAPIBasic],
                          });


            return await result.ToListAsync();
        }

        public async Task<APITotalLCMsView> MediaV2(ApiGetLCMSMedia apiGetLCMSMedia, int userId, string userRole)
        {
            APITotalLCMsView apITotalLCMsView = new APITotalLCMsView();
            UserMasterDetails userdetails = await _db.UserMasterDetails.Where(r => r.UserMasterId == userId).FirstOrDefaultAsync();
            var result = (from lcms in _db.LCMS
                          join user in _db.UserMaster on lcms.ModifiedBy equals user.Id into um
                          from user in um.DefaultIfEmpty()
                          join umd in _db.UserMasterDetails on lcms.CreatedBy equals umd.UserMasterId into umddetails
                          from umd in umddetails.DefaultIfEmpty()

                          where lcms.IsDeleted == false
                          select new LCMSData
                          {
                              Id = lcms.Id,
                              Name = lcms.Name,
                              Path = lcms.Path,
                              ThumbnailPath = lcms.ThumbnailPath,
                              Version = lcms.Version,
                              IsBuiltInAssesment = lcms.IsBuiltInAssesment,
                              OriginalFileName = lcms.OriginalFileName,
                              IsMobileCompatible = lcms.IsMobileCompatible,
                              MetaData = lcms.MetaData,
                              ContentType = lcms.ContentType,
                              YoutubeVideoId = lcms.YoutubeVideoId,
                              AssessmentSheetConfigID = lcms.AssessmentSheetConfigID,
                              FeedbackSheetConfigID = lcms.FeedbackSheetConfigID,
                              Duration = lcms.Duration,
                              Language = lcms.Language,
                              UserName = String.IsNullOrEmpty(user.UserName) ? "System" : user.UserName,
                              AreaId = umd.AreaId,
                              LocationId = umd.LocationId,
                              GroupId = umd.GroupId,
                              BusinessId = umd.BusinessId,
                              CreatedBy = lcms.CreatedBy,
                              UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (lcms.CreatedBy == userId) ? true : false : true,
                              ExternalLCMSId = lcms.ExternalLCMSId,
                              IsExternalContent = lcms.IsExternalContent,
                              SubContentType = lcms.SubContentType
                          });

            var authorQuery = (from course in _db.Course
                               join author in _db.CourseAuthorAssociation on course.Id equals author.CourseId
                               join cma in _db.CourseModuleAssociation on course.Id equals cma.CourseId 
                               join module in _db.Module on cma.ModuleId equals module.Id
                               join  lcms in _db.LCMS on module.LCMSId equals lcms.Id                               
                          join user in _db.UserMaster on lcms.ModifiedBy equals user.Id into um
                          from user in um.DefaultIfEmpty()
                          join umd in _db.UserMasterDetails on lcms.CreatedBy equals umd.UserMasterId into umddetails
                          from umd in umddetails.DefaultIfEmpty()

                          where lcms.IsDeleted == false && author.UserId == userId
                          select new LCMSData
                          {
                              Id = lcms.Id,
                              Name = lcms.Name,
                              Path = lcms.Path,
                              ThumbnailPath = lcms.ThumbnailPath,
                              Version = lcms.Version,
                              IsBuiltInAssesment = lcms.IsBuiltInAssesment,
                              OriginalFileName = lcms.OriginalFileName,
                              IsMobileCompatible = lcms.IsMobileCompatible,
                              MetaData = lcms.MetaData,
                              ContentType = lcms.ContentType,
                              YoutubeVideoId = lcms.YoutubeVideoId,
                              AssessmentSheetConfigID = lcms.AssessmentSheetConfigID,
                              FeedbackSheetConfigID = lcms.FeedbackSheetConfigID,
                              Duration = lcms.Duration,
                              Language = lcms.Language,
                              UserName = String.IsNullOrEmpty(user.UserName) ? "System" : user.UserName,
                              AreaId = umd.AreaId,
                              LocationId = umd.LocationId,
                              GroupId = umd.GroupId,
                              BusinessId = umd.BusinessId,
                              CreatedBy = lcms.CreatedBy,
                              UserCreated = (userRole == UserRoles.LA || userRole == UserRoles.AA || userRole == UserRoles.GA || userRole == UserRoles.BA) ? (lcms.CreatedBy == userId) ? true : false : true,
                              ExternalLCMSId= lcms.ExternalLCMSId,
                              IsExternalContent = lcms.IsExternalContent,
                              SubContentType = lcms.SubContentType
                          });

            if (apiGetLCMSMedia.search != null)
            {
                switch (apiGetLCMSMedia.search.ToLower())
                {
                    case "all":
                        if (apiGetLCMSMedia.metaData != null)
                            result = result.Where(l => l.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()));
                        break;
                    case "feedback":
                        result = (from feedback in result
                                  join feedbackSheet in _db.FeedbackSheetConfiguration on feedback.FeedbackSheetConfigID equals feedbackSheet.Id
                                  where feedbackSheet.IsEmoji == false && feedback.ContentType.ToLower().Equals("feedback")
                                  select feedback);
                        if (apiGetLCMSMedia.metaData != null)
                            result = result.Where(f => f.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData) || f.Name.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()));
                        break;

                    case "authoring":
                        result = (from authoring in result
                                  join authoringMaster in _db.AuthoringMaster on authoring.Id equals authoringMaster.LCMSId
                                  where authoringMaster.IsDeleted == false && authoring.ContentType.ToLower().Equals("microlearning")
                                  select authoring);
                        if (apiGetLCMSMedia.metaData != null)
                            result = result.Where(f => f.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData) || f.Name.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()));
                        break;

                    case "emojifeedback":

                        result = (from feedback in result
                                  join feedbackSheet in _db.FeedbackSheetConfiguration on feedback.FeedbackSheetConfigID equals feedbackSheet.Id
                                  where feedbackSheet.IsEmoji == true && feedback.ContentType.ToLower().Equals("feedback")
                                  select feedback);
                        if (apiGetLCMSMedia.metaData != null)
                            result = result.Where(f => f.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData) || f.Name.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()));
                        break;
                    case "xapi":
                        if (apiGetLCMSMedia.metaData == null)
                            result = result.Where(l => l.ContentType.ToLower().StartsWith(apiGetLCMSMedia.search.ToLower()) || l.ContentType.ToLower().StartsWith("cmi5"));
                        break;
                    case "udemy": 
                    case "linkedin":
                    case "coursera":
                    case "skillsoft":
                    case "zobble":
                    case "external-link":
                        if (apiGetLCMSMedia.metaData == null)
                            result = result.Where(l => l.SubContentType.ToLower().StartsWith(apiGetLCMSMedia.search.ToLower()) && l.IsExternalContent == true) ;
                        if (apiGetLCMSMedia.metaData != null)
                            result = result.Where(f => (f.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData) || f.Name.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()))
                            && f.SubContentType.ToLower().StartsWith(apiGetLCMSMedia.search.ToLower()) && f.IsExternalContent == true
                            );
                        break;
                    default:
                        if (apiGetLCMSMedia.metaData == null)
                            result = result.Where(l => l.ContentType.ToLower().StartsWith(apiGetLCMSMedia.search.ToLower()));
                        else
                            result = result.Where(l => l.ContentType.ToLower().StartsWith(apiGetLCMSMedia.search.ToLower()) && (l.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()) || l.Name.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower())));
                        break;
                }
            }

            if (apiGetLCMSMedia.search != null)
            {
                switch (apiGetLCMSMedia.search.ToLower())
                {
                    case "all":
                        if (apiGetLCMSMedia.metaData != null)
                            authorQuery = authorQuery.Where(l => l.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()));
                        break;
                    case "feedback":
                        authorQuery = (from feedback in authorQuery
                                       join feedbackSheet in _db.FeedbackSheetConfiguration on feedback.FeedbackSheetConfigID equals feedbackSheet.Id
                                  where feedbackSheet.IsEmoji == false && feedback.ContentType.ToLower().Equals("feedback")
                                  select feedback);
                        if (apiGetLCMSMedia.metaData != null)
                            authorQuery = authorQuery.Where(f => f.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData) || f.Name.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()));
                        break;

                    case "authoring":
                        authorQuery = (from authoring in authorQuery
                                       join authoringMaster in _db.AuthoringMaster on authoring.Id equals authoringMaster.LCMSId
                                  where authoringMaster.IsDeleted == false && authoring.ContentType.ToLower().Equals("microlearning")
                                  select authoring);
                        if (apiGetLCMSMedia.metaData != null)
                            authorQuery = authorQuery.Where(f => f.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData) || f.Name.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()));
                        break;

                    case "emojifeedback":

                        authorQuery = (from feedback in authorQuery
                                       join feedbackSheet in _db.FeedbackSheetConfiguration on feedback.FeedbackSheetConfigID equals feedbackSheet.Id
                                  where feedbackSheet.IsEmoji == true && feedback.ContentType.ToLower().Equals("feedback")
                                  select feedback);
                        if (apiGetLCMSMedia.metaData != null)
                            authorQuery = authorQuery.Where(f => f.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData) || f.Name.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()));
                        break;
                    case "xapi":
                        if (apiGetLCMSMedia.metaData == null)
                            authorQuery = authorQuery.Where(l => l.ContentType.ToLower().StartsWith(apiGetLCMSMedia.search.ToLower()) || l.ContentType.ToLower().StartsWith("cmi5"));
                        break;
                    case "udemy":
                    case "linkedin":
                    case "coursera":
                    case "skillsoft":
                    case "zobble":
                    case "external-link":
                        if (apiGetLCMSMedia.metaData == null)
                            authorQuery = authorQuery.Where(l => l.SubContentType.ToLower().StartsWith(apiGetLCMSMedia.search.ToLower()) && l.IsExternalContent == true);
                        if (apiGetLCMSMedia.metaData != null)
                            authorQuery = authorQuery.Where(f => (f.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData) || f.Name.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()))
                            && f.SubContentType.ToLower().StartsWith(apiGetLCMSMedia.search.ToLower()) && f.IsExternalContent == true
                            );
                        break;
                    default:
                        if (apiGetLCMSMedia.metaData == null)
                            authorQuery = authorQuery.Where(l => l.ContentType.ToLower().StartsWith(apiGetLCMSMedia.search.ToLower()));
                        else
                            authorQuery = authorQuery.Where(l => l.ContentType.ToLower().StartsWith(apiGetLCMSMedia.search.ToLower()) && (l.MetaData.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower()) || l.Name.ToLower().Contains(apiGetLCMSMedia.metaData.ToLower())));
                        break;
                }
            }

            if (userRole == UserRoles.BA)
            {
                result = result.Where(r => r.BusinessId == userdetails.BusinessId);
            }
            if (userRole == UserRoles.GA)
            {
                result = result.Where(r => r.GroupId == userdetails.GroupId);
            }
            if (userRole == UserRoles.LA)
            {
                result = result.Where(r => r.LocationId == userdetails.LocationId);
            }
            if (userRole == UserRoles.AA)
            {
                result = result.Where(r => r.AreaId == userdetails.AreaId);
            }
            if (apiGetLCMSMedia.showAllData == false && (userRole != UserRoles.CA))
            {
                result = result.Where(r => r.CreatedBy == userId);
            }

            var queryResult = result.Union(authorQuery);

            apITotalLCMsView.TotalRecords= await queryResult.Distinct().CountAsync();
            queryResult = queryResult.OrderByDescending(r => r.Id);
            if (apiGetLCMSMedia.page != -1)
                queryResult = queryResult.Skip((apiGetLCMSMedia.page - 1) * apiGetLCMSMedia.pageSize);
            if (apiGetLCMSMedia.pageSize != -1)
                queryResult = queryResult.Take(apiGetLCMSMedia.pageSize);
            var data= await queryResult.Distinct().ToListAsync();
            apITotalLCMsView.Data=data;
            return apITotalLCMsView;
        }
        public async Task<List<string>> GetExternalLinkVendor()
        {
           
            List<string> data = await _db.LCMS.Where(a => a.IsExternalContent == true).Select(a => a.SubContentType).Distinct().ToListAsync();
            return data;
        }
        public async Task<List<string>> GetVideoSubContent()
        {

            List<string> data = await _db.LCMS.Where(a => a.ContentType == "video" && a.SubContentType != null).Select(a => a.SubContentType).Distinct().ToListAsync();
            return data;
        }
        public async Task<string> GetMasterConfigurableParameterValue(string configurationCode)
        {
            string value = null; //default value
            try
            {
                using (var dbContext = _customerConnection.GetDbContext())
                {
                    var connection = dbContext.Database.GetDbConnection();
                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                        connection.Open();
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "GetConfigurableParameterValue";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@ConfigurationCode", SqlDbType.VarChar) { Value = configurationCode });
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count > 0)
                        {
                            value = dt.Rows[0]["Value"].ToString();
                        }
                        reader.Dispose();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return value;
        }
        public string KPointToken(int UserId)
        {
            ExternalCoursesConfiguration externalCoursesConfiguration = _db.ExternalCoursesConfiguration.Where(
                a => a.Vendor == "kpoint"
                ).FirstOrDefault();
            if(externalCoursesConfiguration == null)
            {
                return null;
            }
            UserMaster userMaster = _db.UserMaster.Where(a => a.Id == UserId).FirstOrDefault();
            string email = null;
            string displayname = null;
            if (userMaster != null)
            {
                email = Security.Decrypt(userMaster.EmailId);
                displayname = userMaster.UserName;
            }
             string SECRET_KEY = externalCoursesConfiguration.ClientSecret;


            string CLIENT_ID = externalCoursesConfiguration.ClientId;
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            string Dttime = Convert.ToInt64((DateTime.UtcNow.ToUniversalTime() - epoch).TotalSeconds).ToString();
            
            string xt = "";
            Encoding encoding = Encoding.UTF8;

            string message = CLIENT_ID + ":" + email + ":" + displayname + ":" + Dttime;

            var keyByte = encoding.GetBytes(SECRET_KEY);
            using (var hmacmd5 = new HMACMD5(keyByte))
            {
                byte[] hash = hmacmd5.ComputeHash(encoding.GetBytes(message));
                string xauthToken = Base64Encode(hash);
                xauthToken = xauthToken.Replace("=", "");
                xauthToken = xauthToken.Replace("+", "-");
                xauthToken = xauthToken.Replace("/", "_");
                string xtEncode = "client_id=" + CLIENT_ID + "&user_email=" + email
                        + "&user_name=" + displayname + "&challenge=" + Dttime
                        + "&xauth_token=" + xauthToken;
                xt = Base64Encode(Encoding.UTF8.GetBytes(xtEncode));
                xt = xt.Replace("=", "");
                xt = xt.Replace("+", "-");
                xt = xt.Replace("/", "_");
            }
            return xt;
        }
        public string KPointTokenForAdmin()
        {
            ExternalCoursesConfiguration externalCoursesConfiguration = _db.ExternalCoursesConfiguration.Where(
                a => a.Vendor == "kpoint"
                ).FirstOrDefault();
            if (externalCoursesConfiguration == null)
            {
                return null;
            }
            
            string SECRET_KEY = externalCoursesConfiguration.ClientSecret;


            string CLIENT_ID = externalCoursesConfiguration.ClientId;
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            string Dttime = Convert.ToInt64((DateTime.UtcNow.ToUniversalTime() - epoch).TotalSeconds).ToString();

            string xt = "";
            Encoding encoding = Encoding.UTF8;

            string message = CLIENT_ID + ":" + externalCoursesConfiguration.EmailId + ":" + externalCoursesConfiguration.OrgId + ":" + Dttime;

            var keyByte = encoding.GetBytes(SECRET_KEY);
            using (var hmacmd5 = new HMACMD5(keyByte))
            {
                byte[] hash = hmacmd5.ComputeHash(encoding.GetBytes(message));
                string xauthToken = Base64Encode(hash);
                xauthToken = xauthToken.Replace("=", "");
                xauthToken = xauthToken.Replace("+", "-");
                xauthToken = xauthToken.Replace("/", "_");
                string xtEncode = "client_id=" + CLIENT_ID + "&user_email=" + externalCoursesConfiguration.EmailId
                        + "&user_name=" + externalCoursesConfiguration.OrgId + "&challenge=" + Dttime
                        + "&xauth_token=" + xauthToken;
                xt = Base64Encode(Encoding.UTF8.GetBytes(xtEncode));
                xt = xt.Replace("=", "");
                xt = xt.Replace("+", "-");
                xt = xt.Replace("/", "_");
            }
            return xt;
        }
        private string Base64Encode(byte[] plainText)
        {
            return Convert.ToBase64String(plainText);
        }

        public async Task<int> GetAuthenticationKpoint(IFormFile formFile,string fileName, string name,string Description,LCMS lCMS, int UserId)
        {
            try
            {
                string xauth_Token = KPointToken(UserId);

                string URL = "https://enthralltech.zencite.com/api/v3/media?method=options&type=video&xt=" + xauth_Token;

                HttpResponseMessage response = await ApiHelper.CallGetAPI(URL);
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;

                    AuthenticationKpoint authenticationKpoint = new AuthenticationKpoint();
                    authenticationKpoint = JsonConvert.DeserializeObject<AuthenticationKpoint>(result);
                    int id = await SendVideoToKPoint(authenticationKpoint, fileName, name, formFile,xauth_Token, Description, lCMS, UserId);

                    return id;
                }
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex.InnerException);
            }
            return 0;
        }
        public async Task<int> SendVideoToKPoint(AuthenticationKpoint authenticationKpoint,string fileName, string name, IFormFile formFile, string xauth_Token,string Description,LCMS lCMS, int UserId)
        {
            try
            {
                HttpResponseMessage response = await ApiHelper.CallKPointAPI(authenticationKpoint, fileName, name,formFile);

                if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.NoContent)
                {
                    int ret = await PostKPointVideoUpload(authenticationKpoint, "https://enthralltech.zencite.com/api/v3/media?xt=", xauth_Token, fileName, Description, lCMS, UserId);

                    return ret;
                }
                return 1;
            }
            catch(Exception ex)
            {
                _logger.Error(ex.Message, ex.InnerException);
                return -1;
            }
        }
        public async Task<int> PostKPointVideoUpload(AuthenticationKpoint authenticationKpoint,string URL,string XT,string kapsule_name, string description,LCMS lCMS,int UserId)
        {
            string upload_URL = authenticationKpoint.bucket + "/" + authenticationKpoint.key + kapsule_name;
            URL = URL + XT;
            HttpResponseMessage response = ApiHelper.CallPostKPointAPI(upload_URL, kapsule_name, description,URL);

            if (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Accepted || response.StatusCode == HttpStatusCode.Created)
            {
                var result = response.Content.ReadAsStringAsync().Result;
                KPointResponse kPointResponse = JsonConvert.DeserializeObject<KPointResponse>(result);

                lCMS.ExternalLCMSId = kPointResponse.kapsule_id;
                lCMS.ContentType = "kpoint";
                lCMS.InternalName = kapsule_name;
                lCMS.IsActive = true;
                lCMS.ModifiedDate = DateTime.Now;
                lCMS.OriginalFileName = kapsule_name;

                await this.Add(lCMS);

                if (lCMS.Ismodulecreate)
                {
                    Module module = new Module();
                    module.IsActive = true;
                    module.LCMSId = lCMS.Id;
                    module.Name = lCMS.Name;
                    module.Description = lCMS.Name;
                    module.CourseType = "elearning";
                    module.ModuleType = "kpoint";
                    module.CreatedDate = DateTime.UtcNow;
                    module.ModifiedDate = DateTime.UtcNow;
                    module.CreatedBy = UserId;
                    module.ModifiedBy = UserId;
                    module.IsMultilingual = false;
                    await _moduleRepository.Add(module);
                }

                return 1;
            }
            return 0;
        }
    }
}
