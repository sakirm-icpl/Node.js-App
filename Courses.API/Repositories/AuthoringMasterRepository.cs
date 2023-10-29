using AutoMapper;
using Courses.API.APIModel;
using Courses.API.Helper;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using log4net;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Courses.API.Repositories
{
    public class AuthoringMasterRepository : Repository<AuthoringMaster>, IAuthoringMaster
    {
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        ILCMSRepository _lcmsRepository;
        IModuleRepository _moduleRepository;
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AuthoringMasterRepository));
        public AuthoringMasterRepository(CourseContext context, ICustomerConnectionStringRepository customerConnection, ILCMSRepository lcmsRepository, IModuleRepository moduleRepository) : base(context)
        {
            this._db = context;
            this._customerConnection = customerConnection;
            this._lcmsRepository = lcmsRepository;
            this._moduleRepository = moduleRepository;
        }

        public async Task<bool> Exists(string Name, string Skills, int? Authoringid = null)
        {
            Skills = Skills.ToLower().Trim();
            Name = Name.ToLower().Trim();
            int Count = 0;

            if (Authoringid != null)
            {
                Count = await (from c in this._db.AuthoringMaster
                               where c.Id != Authoringid && c.IsDeleted == false && (c.Name.ToLower().Equals(Name) || c.Skills.ToLower().Equals(Skills))
                               select new
                               { c.Id }).CountAsync();
            }
            else
            {
                Count = await (from c in this._db.AuthoringMaster
                               where c.IsDeleted == false && (c.Name.ToLower().Equals(Name) || c.Skills.ToLower().Equals(Skills))
                               select new
                               { c.Id }).CountAsync();
            }

            if (Count > 0)
                return true;
            return false;
        }

        public async Task<bool> NameExists(string Name, int? Authoringid = null)
        {
            Name = Name.ToLower().Trim();
            int Count = 0;

            if (Authoringid != null)
            {
                Count = await (from c in this._db.AuthoringMaster
                               where c.Id != Authoringid && c.IsDeleted == false && (c.Name.ToLower().Equals(Name))
                               select new
                               { c.Id }).CountAsync();
            }
            else
            {
                Count = await (from c in this._db.AuthoringMaster
                               where c.IsDeleted == false && (c.Name.ToLower().Equals(Name))
                               select new
                               { c.Id }).CountAsync();
            }

            if (Count > 0)
                return true;
            return false;
        }

        public async Task<bool> SkillExists(string Skills, int? Authoringid = null)
        {
            Skills = Skills.ToLower().Trim();
            int Count = 0;

            if (Authoringid != null)
            {
                Count = await (from c in this._db.AuthoringMaster
                               where c.Id != Authoringid && c.IsDeleted == false && (c.Skills.ToLower().Equals(Skills))
                               select new
                               { c.Id }).CountAsync();
            }
            else
            {
                Count = await (from c in this._db.AuthoringMaster
                               where c.IsDeleted == false && (c.Skills.ToLower().Equals(Skills))
                               select new
                               { c.Id }).CountAsync();
            }

            if (Count > 0)
                return true;
            return false;
        }

        public async Task<List<object>> GetAll(int page, int pageSize, string search = null)
        {
            var Query= (from authoringmaster in _db.AuthoringMaster
                        join lcms in _db.LCMS on authoringmaster.LCMSId equals lcms.Id
                        into c
                        from lcms in c.DefaultIfEmpty()
                        where authoringmaster.IsDeleted == false
                       select new ApiAuthoringMaster
                       {
                           Duration=lcms.Duration,
                           Name= authoringmaster.Name,
                           Skills=authoringmaster.Skills,
                           Description=authoringmaster.Description,
                           Id=authoringmaster.Id,
                           ModuleType=authoringmaster.ModuleType,
                           MetaData = authoringmaster.MetaData
                       }
                       );
        
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Name.Contains(search));  // add skill for search
            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                Query = Query.Take(pageSize);
            List<ApiAuthoringMaster> AuthoringMasterList = await Query.ToListAsync();

            List<object> Result = new List<object>();
            foreach (ApiAuthoringMaster authoringMaster in AuthoringMasterList)
            {
                object authoringMasterobj = new
                {
                    authoringMaster.Id,
                    authoringMaster.Name,
                    authoringMaster.Skills,
                    authoringMaster.Description,
                    authoringMaster.Duration,
                    authoringMaster.ModuleType,
                    authoringMaster.MetaData
                };
                Result.Add(authoringMasterobj);

            }
            return Result;
        }
        public async Task<int> Count(string search = null)
        {
            var Query = _db.AuthoringMaster.Where(r => r.IsDeleted == Record.NotDeleted);
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Name.Contains(search));
            }
            return await Query.CountAsync();
        }

        public async Task<List<object>> GetDetailsByAuthoringId(int id, int page, int pageSize, int UserId, int courseId = 0, string search = null)
        {
            var Query = _db.AuthoringMasterDetails.Where(r => r.IsDeleted == Record.NotDeleted && r.AuthoringMasterId == id);

            Query = Query.OrderBy(r => r.PageNumber);

            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                Query = Query.Take(pageSize);

            List<AuthoringMasterDetails> AuthoringMasterDetailsList = await Query.ToListAsync();

            List<object> Result = new List<object>();
            int NewPagenumber = 1;
            foreach (AuthoringMasterDetails authoringMasterDetails in AuthoringMasterDetailsList)
            {
                AuthoringMasterDetailsDisplay authoringMasterDetailsobj = new AuthoringMasterDetailsDisplay();

                authoringMasterDetailsobj.Id = authoringMasterDetails.Id;
                authoringMasterDetailsobj.AuthoringMasterId = authoringMasterDetails.AuthoringMasterId;
                authoringMasterDetailsobj.Title = authoringMasterDetails.Title;
                authoringMasterDetailsobj.PageNumber = NewPagenumber;
                authoringMasterDetailsobj.PageType = authoringMasterDetails.PageType;
                authoringMasterDetailsobj.Content = authoringMasterDetails.Content;
                authoringMasterDetailsobj.Path = authoringMasterDetails.Path;
                var Query2 = _db.AuthoringInteractiveVideoPopups.Where(r => r.IsDeleted == Record.NotDeleted && r.AuthoringMasterDetailsId == authoringMasterDetails.Id);
                List<AuthoringInteractiveVideoPopups> AuthoringInteractiveVideoPopups = await Query2.ToListAsync();

                List<AuthoringInteractiveVideoPopupsDisplay> AuthoringInteractiveVideoPopupsDisplays = new List<AuthoringInteractiveVideoPopupsDisplay>();

                foreach (AuthoringInteractiveVideoPopups AuthoringInteractiveVideoPopup in AuthoringInteractiveVideoPopups)
                {
                    AuthoringInteractiveVideoPopupsDisplay AuthoringInteractiveVideoPopupsDisplay = new AuthoringInteractiveVideoPopupsDisplay();
                    AuthoringInteractiveVideoPopupsDisplay.Id = AuthoringInteractiveVideoPopup.Id;
                    AuthoringInteractiveVideoPopupsDisplay.AuthoringMasterDetailsId = AuthoringInteractiveVideoPopup.AuthoringMasterDetailsId;
                    AuthoringInteractiveVideoPopupsDisplay.QuestionOrMessage = AuthoringInteractiveVideoPopup.QuestionOrMessage;
                    AuthoringInteractiveVideoPopupsDisplay.TimeStamp = AuthoringInteractiveVideoPopup.TimeStamp;
                    AuthoringInteractiveVideoPopupsDisplay.IsQuestion = AuthoringInteractiveVideoPopup.IsQuestion;

                    var Query3 = _db.AuthoringInteractiveVideoPopupsHistory.Where(r => r.AuthoringInteractiveVideoPopupsId == AuthoringInteractiveVideoPopup.Id && r.UserMasterId == UserId && r.CourseId == courseId);

                    List<AuthoringInteractiveVideoPopupsHistory> AuthoringInteractiveVideoPopupsHistorys = await Query3.ToListAsync();

                    if(AuthoringInteractiveVideoPopupsHistorys.Count != 0 && courseId != 0)
                    {
                        AuthoringInteractiveVideoPopupsHistory AuthoringInteractiveVideoPopupsHistory = AuthoringInteractiveVideoPopupsHistorys.LastOrDefault();
                        AuthoringInteractiveVideoPopupsDisplay.IsCompleted = AuthoringInteractiveVideoPopupsHistory.IsCompleted;
                    }
                    else
                    {
                        AuthoringInteractiveVideoPopupsDisplay.IsCompleted = false;
                    }

                    var Query4 = _db.AuthoringInteractiveVideoPopupsOptions.Where(r => r.AuthoringInteractiveVideoPopupsId == AuthoringInteractiveVideoPopup.Id && r.IsDeleted == Record.NotDeleted);
                    List<AuthoringInteractiveVideoPopupsOptions> AuthoringInteractiveVideoPopupsOptions = await Query4.ToListAsync();

                    List<AuthoringInteractiveVideoPopupsOptionsDisplay> AuthoringInteractiveVideoPopupsOptionsDisplay = new List<AuthoringInteractiveVideoPopupsOptionsDisplay>();

                    if (AuthoringInteractiveVideoPopupsOptions != null)
                    {
                        foreach (AuthoringInteractiveVideoPopupsOptions AuthoringInteractiveVideoPopupsOption in AuthoringInteractiveVideoPopupsOptions)
                        {
                            AuthoringInteractiveVideoPopupsOptionsDisplay authoringInteractiveVideoPopupsOptionsDisplay = new AuthoringInteractiveVideoPopupsOptionsDisplay();
                            authoringInteractiveVideoPopupsOptionsDisplay.Id = AuthoringInteractiveVideoPopupsOption.Id;
                            authoringInteractiveVideoPopupsOptionsDisplay.AuthoringInteractiveVideoPopupsId = AuthoringInteractiveVideoPopupsOption.AuthoringInteractiveVideoPopupsId;
                            authoringInteractiveVideoPopupsOptionsDisplay.OptionText = AuthoringInteractiveVideoPopupsOption.OptionText;

                            AuthoringInteractiveVideoPopupsOptionsDisplay.Add(authoringInteractiveVideoPopupsOptionsDisplay);
                        }
                    }

                    AuthoringInteractiveVideoPopupsDisplay.authoringInteractiveVideoPopupsOptionsDisplay = AuthoringInteractiveVideoPopupsOptionsDisplay;

                    AuthoringInteractiveVideoPopupsDisplays.Add(AuthoringInteractiveVideoPopupsDisplay);
                }

                authoringMasterDetailsobj.authoringInteractiveVideoPopups = AuthoringInteractiveVideoPopupsDisplays;

                Result.Add(authoringMasterDetailsobj);

                NewPagenumber += 1;
            }
            return Result;
        }


        public async Task<ApiAuthoringMasterDetails> PostAuthoringDetails(ApiAuthoringMasterDetails apiAuthoringMasterDetails, int UserId)
        {

            var title = apiAuthoringMasterDetails.Title;
            var Text = (from x in _db.AuthoringMasterDetails.Where(x => x.Title == title && x.AuthoringMasterId == apiAuthoringMasterDetails.AuthoringMasterId)
                        select x).ToList();
            if (Text.Count > 0)
            {
                return null;
            }
            else
            {
                AuthoringMasterDetails authoringDetailsSave = new AuthoringMasterDetails();
                AuthoringMasterDetails authoringDetails = new AuthoringMasterDetails();
                authoringDetails.AuthoringMasterId = apiAuthoringMasterDetails.AuthoringMasterId;
                authoringDetails.Title = apiAuthoringMasterDetails.Title;
                authoringDetails.PageNumber = apiAuthoringMasterDetails.PageNumber;
                authoringDetails.PageType = apiAuthoringMasterDetails.PageType;
                authoringDetails.Content = apiAuthoringMasterDetails.Content;
                authoringDetails.Path = apiAuthoringMasterDetails.Path;
                authoringDetails.IsDeleted = false;
                authoringDetails.IsActive = true;
                authoringDetails.CreatedBy = UserId;
                authoringDetails.CreatedDate = DateTime.UtcNow;

                await this._db.AuthoringMasterDetails.AddAsync(authoringDetails);
                await this._db.SaveChangesAsync();

                apiAuthoringMasterDetails.Id = authoringDetails.Id;

                if (apiAuthoringMasterDetails.PageType == "InteractiveVideo" && apiAuthoringMasterDetails.apiAuthoringInteractiveVideoPopups != null)
                {
                    foreach (var videoPopup in apiAuthoringMasterDetails.apiAuthoringInteractiveVideoPopups)
                    {
                        AuthoringInteractiveVideoPopups authoringInteractiveVideoPopups = new AuthoringInteractiveVideoPopups();
                        authoringInteractiveVideoPopups.AuthoringMasterDetailsId = authoringDetails.Id;
                        authoringInteractiveVideoPopups.QuestionOrMessage = videoPopup.QuestionOrMessage;
                        if (videoPopup.TimeStamp != null)
                        {
                            authoringInteractiveVideoPopups.TimeStamp = videoPopup.TimeStamp;
                        }
                        else
                        {
                            authoringInteractiveVideoPopups.TimeStamp = "0";
                        }
                        authoringInteractiveVideoPopups.IsQuestion = videoPopup.IsQuestion;
                        authoringInteractiveVideoPopups.IsDeleted = false;
                        authoringInteractiveVideoPopups.IsActive = true;
                        authoringInteractiveVideoPopups.CreatedBy = UserId;
                        authoringInteractiveVideoPopups.CreatedDate = DateTime.UtcNow;

                        await this._db.AuthoringInteractiveVideoPopups.AddAsync(authoringInteractiveVideoPopups);
                        await this._db.SaveChangesAsync();

                        videoPopup.Id = authoringInteractiveVideoPopups.Id;

                        if (videoPopup.IsQuestion == true && videoPopup.apiAuthoringInteractiveVideoPopupsOptions != null)
                        {
                            foreach (var option in videoPopup.apiAuthoringInteractiveVideoPopupsOptions)
                            {
                                AuthoringInteractiveVideoPopupsOptions authoringInteractiveVideoPopupsOptions = new AuthoringInteractiveVideoPopupsOptions();
                                authoringInteractiveVideoPopupsOptions.AuthoringInteractiveVideoPopupsId = authoringInteractiveVideoPopups.Id;
                                authoringInteractiveVideoPopupsOptions.OptionText = option.OptionText;
                                authoringInteractiveVideoPopupsOptions.IsCorrectAnswer = option.IsCorrectAnswer;
                                authoringInteractiveVideoPopupsOptions.IsDeleted = false;
                                authoringInteractiveVideoPopupsOptions.IsActive = true;
                                authoringInteractiveVideoPopupsOptions.CreatedBy = UserId;
                                authoringInteractiveVideoPopupsOptions.CreatedDate = DateTime.UtcNow;

                                await this._db.AuthoringInteractiveVideoPopupsOptions.AddAsync(authoringInteractiveVideoPopupsOptions);
                                await this._db.SaveChangesAsync();

                                option.Id = authoringInteractiveVideoPopupsOptions.Id;
                                option.authoringInteractiveVideoPopupsId = authoringInteractiveVideoPopups.Id;
                            }
                        }
                    }
                }

                return apiAuthoringMasterDetails;
            }
        }

        public async Task<AuthoringInteractiveVideoPopupsHistory> PostAuthoringInteractiveVideoPopupsHistory(ApiAuthoringInteractiveVideoPopupsHistory apiAuthoringInteractiveVideoPopupsHistory, int UserId)
        {
            List<AuthoringInteractiveVideoPopups> popups = await _db.AuthoringInteractiveVideoPopups.Where(x => x.Id == apiAuthoringInteractiveVideoPopupsHistory.AuthoringInteractiveVideoPopupsId).ToListAsync();
            List<AuthoringInteractiveVideoPopupsOptions> options = new List<AuthoringInteractiveVideoPopupsOptions>();
            if (apiAuthoringInteractiveVideoPopupsHistory.AuthoringInteractiveVideoPopupsOptionsId != 0)
            {
                options = await _db.AuthoringInteractiveVideoPopupsOptions.Where(x => x.Id == apiAuthoringInteractiveVideoPopupsHistory.AuthoringInteractiveVideoPopupsOptionsId).ToListAsync();
            }

            AuthoringInteractiveVideoPopupsHistory authoringInteractiveVideoPopupsHistory = new AuthoringInteractiveVideoPopupsHistory();
            authoringInteractiveVideoPopupsHistory.CourseId = apiAuthoringInteractiveVideoPopupsHistory.CourseId;
            authoringInteractiveVideoPopupsHistory.AuthoringInteractiveVideoPopupsId = apiAuthoringInteractiveVideoPopupsHistory.AuthoringInteractiveVideoPopupsId;
            authoringInteractiveVideoPopupsHistory.AuthoringInteractiveVideoPopupsOptionsId = apiAuthoringInteractiveVideoPopupsHistory.AuthoringInteractiveVideoPopupsOptionsId;
            authoringInteractiveVideoPopupsHistory.UserMasterId = UserId;
            authoringInteractiveVideoPopupsHistory.CreatedDate = DateTime.UtcNow;

            if((options.Count != 0 && options.FirstOrDefault().IsCorrectAnswer == true) || popups.FirstOrDefault().IsQuestion == false)
            {
                authoringInteractiveVideoPopupsHistory.IsCompleted = true;
            }
            else
            {
                authoringInteractiveVideoPopupsHistory.IsCompleted = false;
            }

            if(authoringInteractiveVideoPopupsHistory.CourseId != 0)
            {
                await this._db.AuthoringInteractiveVideoPopupsHistory.AddAsync(authoringInteractiveVideoPopupsHistory);
                await this._db.SaveChangesAsync();
            }

            return authoringInteractiveVideoPopupsHistory;
        }

        public async Task<ApiResponse> PostAuthoringDetailsToLcms(ApiAuthoringMaster apiAuthoringMaster, int UserId)
        {
            ApiResponse responce = new ApiResponse();

            if (await _lcmsRepository.FileExist(apiAuthoringMaster.Name.Trim(), "1.0"))
            {
                responce.StatusCode = 409;
                responce.Description = "Duplicate";

            }


            LCMS lcms = new LCMS();
            lcms.Description = apiAuthoringMaster.Description.Trim();
            lcms.MetaData = apiAuthoringMaster.Skills;
            lcms.Name = apiAuthoringMaster.Name;
            lcms.Duration = apiAuthoringMaster.Duration;
            lcms.Version = "1.0";
            lcms.ContentType = "Microlearning";
            lcms.CreatedBy = UserId;
            lcms.ModifiedBy = UserId;
            lcms.CreatedDate = DateTime.UtcNow;
            lcms.IsActive = true;
            await _lcmsRepository.Add(lcms);


            Module module = new Module();
            module.IsActive = true;
            module.LCMSId = lcms.Id;
            module.Name = apiAuthoringMaster.Name;
            module.Description = apiAuthoringMaster.Description;
            module.CourseType = "elearning";
            module.ModuleType = "Microlearning";
            module.CreatedDate = DateTime.UtcNow;
            module.ModifiedDate = DateTime.UtcNow;
            module.CreatedBy = UserId;
            module.ModifiedBy = UserId;
            await _moduleRepository.Add(module);
            responce.StatusCode = 200;
            responce.ResponseObject = lcms.Id;
            return responce;

        }

        public async Task<AuthoringMasterDetails> GetAuthoringMasterDetails(int id)
        {
            var Query = _db.AuthoringMasterDetails.Where(r => r.IsDeleted == Record.NotDeleted && r.Id == id);
            AuthoringMasterDetails AuthoringMasterDetails = await Query.FirstOrDefaultAsync();
            return AuthoringMasterDetails;
        }

        public async Task<bool> IsDependacyExist(int authoringid)
        {
            int count = await (from authoring in _db.AuthoringMaster
                               join lcms in _db.LCMS on authoring.LCMSId equals lcms.Id
                               join module in _db.Module on authoring.LCMSId equals module.LCMSId
                               join coursemodule in _db.CourseModuleAssociation on module.Id equals coursemodule.ModuleId
                               where (authoring.Id == authoringid && authoring.IsDeleted == false && module.IsDeleted == false && lcms.IsDeleted == false)
                               select new { authoring.Id }).CountAsync();
            if (count > 0)
                return true;
            return false;
        }


        public async Task<object> DeleteAuthoringMasterDetails(AuthoringMasterDetails authoringMasterDetails)
        {
            authoringMasterDetails.IsDeleted = true;
            authoringMasterDetails.ModifiedDate = DateTime.UtcNow;
            _db.AuthoringMasterDetails.Update(authoringMasterDetails);

            await this._db.SaveChangesAsync();
            return authoringMasterDetails;
        }

        public async Task<object> DeleteAuthoringInteractiveVideoPopups(int id, int UserId)
        {
            var Query = _db.AuthoringInteractiveVideoPopups.Where(r => r.Id == id);
            AuthoringInteractiveVideoPopups AuthoringInteractiveVideoPopups = await Query.FirstOrDefaultAsync();

            AuthoringInteractiveVideoPopups.IsDeleted = true;
            AuthoringInteractiveVideoPopups.ModifiedBy = UserId;
            AuthoringInteractiveVideoPopups.ModifiedDate = DateTime.UtcNow;
            _db.AuthoringInteractiveVideoPopups.Update(AuthoringInteractiveVideoPopups);

            await this._db.SaveChangesAsync();

            var Query2 = _db.AuthoringInteractiveVideoPopups.Where(r => r.IsDeleted == Record.NotDeleted && r.AuthoringMasterDetailsId == AuthoringInteractiveVideoPopups.AuthoringMasterDetailsId);
            List<AuthoringInteractiveVideoPopups> authoringInteractiveVideoPopups = await Query2.ToListAsync();

            List<AuthoringInteractiveVideoPopupsDisplay> AuthoringInteractiveVideoPopupsDisplays = new List<AuthoringInteractiveVideoPopupsDisplay>();

            foreach (AuthoringInteractiveVideoPopups AuthoringInteractiveVideoPopup in authoringInteractiveVideoPopups)
            {
                AuthoringInteractiveVideoPopupsDisplay AuthoringInteractiveVideoPopupsDisplay = new AuthoringInteractiveVideoPopupsDisplay();
                AuthoringInteractiveVideoPopupsDisplay.Id = AuthoringInteractiveVideoPopup.Id;
                AuthoringInteractiveVideoPopupsDisplay.AuthoringMasterDetailsId = AuthoringInteractiveVideoPopup.AuthoringMasterDetailsId;
                AuthoringInteractiveVideoPopupsDisplay.QuestionOrMessage = AuthoringInteractiveVideoPopup.QuestionOrMessage;
                AuthoringInteractiveVideoPopupsDisplay.TimeStamp = AuthoringInteractiveVideoPopup.TimeStamp;
                AuthoringInteractiveVideoPopupsDisplay.IsQuestion = AuthoringInteractiveVideoPopup.IsQuestion;
                AuthoringInteractiveVideoPopupsDisplay.IsCompleted = false;

                var Query4 = _db.AuthoringInteractiveVideoPopupsOptions.Where(r => r.AuthoringInteractiveVideoPopupsId == AuthoringInteractiveVideoPopup.Id && r.IsDeleted == Record.NotDeleted);
                List<AuthoringInteractiveVideoPopupsOptions> AuthoringInteractiveVideoPopupsOptions = await Query4.ToListAsync();

                List<AuthoringInteractiveVideoPopupsOptionsDisplay> AuthoringInteractiveVideoPopupsOptionsDisplay = new List<AuthoringInteractiveVideoPopupsOptionsDisplay>();

                if (AuthoringInteractiveVideoPopupsOptions != null)
                {
                    foreach (AuthoringInteractiveVideoPopupsOptions AuthoringInteractiveVideoPopupsOption in AuthoringInteractiveVideoPopupsOptions)
                    {
                        AuthoringInteractiveVideoPopupsOptionsDisplay authoringInteractiveVideoPopupsOptionsDisplay = new AuthoringInteractiveVideoPopupsOptionsDisplay();
                        authoringInteractiveVideoPopupsOptionsDisplay.Id = AuthoringInteractiveVideoPopupsOption.Id;
                        authoringInteractiveVideoPopupsOptionsDisplay.AuthoringInteractiveVideoPopupsId = AuthoringInteractiveVideoPopupsOption.AuthoringInteractiveVideoPopupsId;
                        authoringInteractiveVideoPopupsOptionsDisplay.OptionText = AuthoringInteractiveVideoPopupsOption.OptionText;

                        AuthoringInteractiveVideoPopupsOptionsDisplay.Add(authoringInteractiveVideoPopupsOptionsDisplay);
                    }
                }

                AuthoringInteractiveVideoPopupsDisplay.authoringInteractiveVideoPopupsOptionsDisplay = AuthoringInteractiveVideoPopupsOptionsDisplay;

                AuthoringInteractiveVideoPopupsDisplays.Add(AuthoringInteractiveVideoPopupsDisplay);
            }
            return AuthoringInteractiveVideoPopupsDisplays;
        }

        public async Task<object> UpdateAuthoringMasterDetails(AuthoringMasterDetails authoringMasterDetails, ApiAuthoringMasterDetailsUpdate apiAuthoringMasterDetails, int UserId)
        {
            authoringMasterDetails.Content = apiAuthoringMasterDetails.Content;
            authoringMasterDetails.PageNumber = apiAuthoringMasterDetails.PageNumber;
            authoringMasterDetails.PageType = apiAuthoringMasterDetails.PageType;
            authoringMasterDetails.Title = apiAuthoringMasterDetails.Title;
            authoringMasterDetails.Path = apiAuthoringMasterDetails.Path;
            authoringMasterDetails.ModifiedDate = DateTime.UtcNow;
            _db.AuthoringMasterDetails.Update(authoringMasterDetails);
            await this._db.SaveChangesAsync();

            apiAuthoringMasterDetails.Id = authoringMasterDetails.Id;

            foreach (var videoPopup in apiAuthoringMasterDetails.authoringInteractiveVideoPopups)
            {
                var Query = _db.AuthoringInteractiveVideoPopups.Where(r => r.IsDeleted == Record.NotDeleted && r.Id == videoPopup.Id);
                AuthoringInteractiveVideoPopups authoringInteractiveVideoPopupsOld = await Query.FirstOrDefaultAsync();

                authoringInteractiveVideoPopupsOld.QuestionOrMessage = videoPopup.QuestionOrMessage;
                authoringInteractiveVideoPopupsOld.TimeStamp = videoPopup.TimeStamp;
                authoringInteractiveVideoPopupsOld.ModifiedBy = UserId;
                authoringInteractiveVideoPopupsOld.ModifiedDate = DateTime.UtcNow;

                this._db.AuthoringInteractiveVideoPopups.Update(authoringInteractiveVideoPopupsOld);
                await this._db.SaveChangesAsync();

                if (videoPopup.IsQuestion == true && videoPopup.authoringInteractiveVideoPopupsOptionsDisplay != null)
                {
                    foreach (var option in videoPopup.authoringInteractiveVideoPopupsOptionsDisplay)
                    {
                        var Query2 = _db.AuthoringInteractiveVideoPopupsOptions.Where(r => r.IsDeleted == Record.NotDeleted && r.Id == option.Id);
                        AuthoringInteractiveVideoPopupsOptions authoringInteractiveVideoPopupsOptionsOld = await Query2.FirstOrDefaultAsync();

                        authoringInteractiveVideoPopupsOptionsOld.OptionText = option.OptionText;
                        authoringInteractiveVideoPopupsOptionsOld.ModifiedBy = UserId;
                        authoringInteractiveVideoPopupsOptionsOld.ModifiedDate = DateTime.UtcNow;

                        this._db.AuthoringInteractiveVideoPopupsOptions.Update(authoringInteractiveVideoPopupsOptionsOld);
                        await this._db.SaveChangesAsync();

                        option.authoringInteractiveVideoPopupsId = authoringInteractiveVideoPopupsOld.Id;
                    }
                }
            }

            return apiAuthoringMasterDetails;
        }

        public async Task<List<AuthoringMasterDetails>> UpdatePageSequence(int AuthoringMasterId, List<ApiAuthoringMasterDetails> authoringMasterDetailsList, int userId)
        {
            try
            {
                List<AuthoringMasterDetails> authoringMasterDetails = await this._db.AuthoringMasterDetails.Where(x => x.IsDeleted == Record.NotDeleted && x.AuthoringMasterId == AuthoringMasterId).ToListAsync();

                foreach (var page in authoringMasterDetailsList)
                {
                    AuthoringMasterDetails updatePage = new AuthoringMasterDetails();
                    updatePage = authoringMasterDetails.Find(x => x.Title == page.Title);
                    updatePage.PageNumber = page.PageNumber;
                    updatePage.ModifiedBy = userId;
                    updatePage.ModifiedDate = DateTime.UtcNow;
                    _db.AuthoringMasterDetails.Update(updatePage);
                    await this._db.SaveChangesAsync();
                }
                return authoringMasterDetails;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetPageNumber(int id)
        {
            var Query = _db.AuthoringMasterDetails.Where(r => r.IsDeleted == Record.NotDeleted && r.AuthoringMasterId == id);
            return await Query.CountAsync();
        }


        public async Task<int?> GetAuthoringMasterIdByModuleID(int ModuleId)

        {
            int? authoringMasterId =
                  (from module in _db.Module
                   join lcms in _db.LCMS on module.LCMSId equals lcms.Id
                   join authoringMaster in _db.AuthoringMaster on lcms.Id equals authoringMaster.LCMSId
                   where (module.Id == ModuleId && authoringMaster.IsDeleted == false && authoringMaster.IsActive == true &&
                     module.IsDeleted == false)
                   select (authoringMaster.Id)).FirstOrDefault();
            return authoringMasterId;

        }

    }
}

