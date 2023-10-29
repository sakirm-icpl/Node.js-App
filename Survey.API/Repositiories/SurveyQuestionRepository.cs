// ======================================
// <copyright file="SurveyQuestionRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using Survey.API.APIModel;
using Survey.API.Data;
using Survey.API.Helper;
using Survey.API.Metadata;
using Survey.API.Models;
using Survey.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Survey.API.Common;

namespace Survey.API.Repositories
{
    public class SurveyQuestionRepository : Repository<SurveyQuestion>, ISurveyQuestionRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SurveyQuestionRepository));
        private GadgetDbContext db;
        private ISurveyOptionRepository surveyOptionRepository;
        private ISurveyManagementRepository surveyManagementRepository;
        public SurveyQuestionRepository(GadgetDbContext context, ISurveyOptionRepository surveyOptionRepositorys, ISurveyManagementRepository surveyManagementRepositorys) : base(context)
        {
            this.db = context;
            this.surveyOptionRepository = surveyOptionRepositorys;
            this.surveyManagementRepository = surveyManagementRepositorys;
        }
        public async Task<List<APISurveyMergeredModel>> GetAllSurveyQuestion(int UserId, string UserRole, int page, int pageSize, string search = null)
        {
            if (UserRole == Record.LoginUserRole)
            {
                IQueryable<APISurveyMergeredModel> Query = (from surveyQuestions in db.SurveyQuestion
                                                            where surveyQuestions.IsDeleted == false
                                                            select new APISurveyMergeredModel
                                                            {
                                                                Section = surveyQuestions.Section,
                                                                Question = surveyQuestions.Question,
                                                                NoOfOption = db.SurveyOption.Where(c => c.QuestionId == surveyQuestions.Id).Count(),
                                                                Status = surveyQuestions.Status,
                                                                AllowSkipAnswering = surveyQuestions.AllowSkipAswering,
                                                                Id = surveyQuestions.Id,
                                                                ModifiedDate = surveyQuestions.ModifiedDate
                                                            });

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(r => r.Question.Contains(search));
                }
                Query = Query.OrderByDescending(r => r.ModifiedDate);
                if (page != -1)
                    Query = Query.Skip((page - 1) * pageSize);
                if (pageSize != -1)
                    Query = Query.Take(pageSize);

                List<APISurveyMergeredModel> surveyMergeredModel = await Query.ToListAsync();

                return surveyMergeredModel;
            }
            else
            {
                IQueryable<APISurveyMergeredModel> Query = (from surveyQuestions in db.SurveyQuestion
                                                            where surveyQuestions.IsDeleted == false
                                                            select new APISurveyMergeredModel
                                                            {
                                                                Section = surveyQuestions.Section,
                                                                Question = surveyQuestions.Question,
                                                                NoOfOption = db.SurveyOption.Where(c => c.QuestionId == surveyQuestions.Id).Count(),
                                                                Status = surveyQuestions.Status,
                                                                AllowSkipAnswering = surveyQuestions.AllowSkipAswering,
                                                                Id = surveyQuestions.Id,
                                                                ModifiedDate = surveyQuestions.ModifiedDate
                                                            });

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(r => r.Question.Contains(search));
                }
                Query = Query.OrderByDescending(r => r.ModifiedDate);
                if (page != -1)
                    Query = Query.Skip((page - 1) * pageSize);
                if (pageSize != -1)
                    Query = Query.Take(pageSize);

                List<APISurveyMergeredModel> surveyMergeredModel = await Query.ToListAsync();

                return surveyMergeredModel;
            }
        }
        public async Task<List<APISurveyMergeredModel>> GetNestedSurveyQuestion(int UserId, string UserRole, int page, int pageSize, string search = null)
        {
            IQueryable<APISurveyMergeredModel> Query = (from surveyQuestions in db.SurveyQuestion
                                                            where surveyQuestions.IsDeleted == false && surveyQuestions.IsMultipleChoice == false && surveyQuestions.Section == "Objective" && surveyQuestions.OptionType != "MultipleSelection" && surveyQuestions.AllowSkipAswering == false && db.SurveyOption.Where(c => c.QuestionId == surveyQuestions.Id).Count() < 6
                                                            select new APISurveyMergeredModel
                                                            {
                                                                Section = surveyQuestions.Section,
                                                                Question = surveyQuestions.Question,
                                                                NoOfOption = db.SurveyOption.Where(c => c.QuestionId == surveyQuestions.Id).Count(),
                                                                Status = surveyQuestions.Status,
                                                                AllowSkipAnswering = surveyQuestions.AllowSkipAswering,
                                                                Id = surveyQuestions.Id,
                                                                ModifiedDate = surveyQuestions.ModifiedDate
                                                            });

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Question.Contains(search));
            }
            Query = Query.OrderByDescending(r => r.ModifiedDate);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);

            List<APISurveyMergeredModel> surveyMergeredModel = await Query.ToListAsync();

            return surveyMergeredModel;
        }
        public async Task<int> Count(int UserId, string UserRole, string search = null)
        {
            if (UserRole == Record.LoginUserRole)
            {
                if (!string.IsNullOrWhiteSpace(search))
                    return await this.db.SurveyQuestion.Where(r => r.Question.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
                return await this.db.SurveyQuestion.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(search))
                    return await this.db.SurveyQuestion.Where(r => r.Question.Contains(search) && r.IsDeleted == Record.NotDeleted).CountAsync();
                return await this.db.SurveyQuestion.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
            }
        }
        public async Task<int> NestedCount(int UserId, string UserRole, string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
            {
                    return await this.db.SurveyQuestion.Where(r => r.Question.Contains(search) && r.IsDeleted == Record.NotDeleted && r.IsMultipleChoice == false && r.Section == "Objective" && r.OptionType != "MultipleSelection" && r.AllowSkipAswering == false && db.SurveyOption.Where(c => c.QuestionId == r.Id).Count() < 6).CountAsync();
            }
            return await this.db.SurveyQuestion.Where(r => r.IsDeleted == Record.NotDeleted && r.IsMultipleChoice == false && r.Section == "Objective" && r.OptionType != "MultipleSelection" && r.AllowSkipAswering == false && db.SurveyOption.Where(c => c.QuestionId == r.Id).Count() < 6).CountAsync();
        }
        public async Task<bool> Exist(int id, string search)
        {
            int count = await this.db.SurveyQuestion.Where(p => (p.Question.ToLower() == search.ToLower()) && p.IsDeleted == Record.NotDeleted).CountAsync();
            if (count > 0)
                return true;
            return false;
        }
        public async Task<bool> QuestionExist(string question, string section)
        {
            int count = await this.db.SurveyQuestion
                .Where(p =>
                   (p.Question.ToLower() == question.ToLower())
                && (p.Section.ToLower() == section.ToLower())
                && p.IsDeleted == Record.NotDeleted)
                .CountAsync();
            if (count > 0)
                return true;
            return false;
        }
        public async Task<IEnumerable<SurveyQuestion>> Search(string query)
        {
            Task<List<SurveyQuestion>> SurveyQuestionList = (from surveyQuestion in this.db.SurveyQuestion
                                                             where
                                                        (surveyQuestion.Question.StartsWith(query))
                                                        && surveyQuestion.IsDeleted == false
                                                             select surveyQuestion).ToListAsync();
            return await SurveyQuestionList;
        }
        public async Task<IEnumerable<SurveyQuestion>> GetAllSurveyQuestion(int surveyId)
        {
            try
            {
                IQueryable<Models.SurveyQuestion> Query = this.db.SurveyQuestion;


                Query = Query.Where(v => v.IsDeleted == Record.NotDeleted && (v.Status == true));
                Query = Query.OrderByDescending(v => v.Id);
                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<APIResponse> GetSurveyBySurveyId(int SurveyId, int userid)
        {

            APIResponse Response = new APIResponse();

            bool applicabletouser = false;

            IEnumerable<SurveyManagement> SurveyManagementList = await this.surveyManagementRepository.GetAllSurveyManagement(userid);
            foreach (SurveyManagement obj in SurveyManagementList)
            {
                if (obj.Id == SurveyId && applicabletouser == false)
                {
                    applicabletouser = true;
                }
            }

            if (applicabletouser == true)
            {
                int TargetResponseCount = 0;
                TargetResponseCount = db.SurveyManagement.Where(c => c.Id == SurveyId).Select(c => c.TargetResponseCount).FirstOrDefault();

                int CompletedCount = 0;
                try
                {
                    CompletedCount = db.SurveyResult.Where(c => c.SurveyId == SurveyId).Select(c => c.Id).Count();
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    CompletedCount = 0;
                }
                //int ResponseCount = 0;
                //ResponseCount = db.SurveyManagement.Where(c => c.IsApplicableToAll == true).Select(c => c.ResponseCount).FirstOrDefault();
                int count = 0;
                try
                {
                    count = this.db.SurveyResult.Where(p => p.SurveyId == SurveyId && p.UserId == userid).Count();
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                    count = 0;
                }
                if ((TargetResponseCount > CompletedCount) || (count > 0))
                {

                    IEnumerable<SurveyQuestion> surveyQuestions = await (from surveyManagement in db.SurveyManagement
                                                                         join surveyConfiguration in db.SurveyConfiguration on surveyManagement.LcmsId equals surveyConfiguration.LcmsId
                                                                         join surveyQuestion in this.db.SurveyQuestion on surveyConfiguration.QuestionId equals surveyQuestion.Id
                                                                         where surveyManagement.Id == SurveyId
                                                                         //orderby surveyQuestion.CreatedDate
                                                                         select surveyQuestion
                                                             ).ToListAsync();
                    List<APISurveyQuestionMergered> aPISurveyQuestionMergereds = new List<APISurveyQuestionMergered>();
                    foreach (SurveyQuestion ObjSurveyQuestion in surveyQuestions)
                    {
                        if (ObjSurveyQuestion != null)
                        {
                            APISurveyQuestionMergered aPISurveyQuestionMergered = new APISurveyQuestionMergered
                            {
                                Question = ObjSurveyQuestion.Question,
                                Section = ObjSurveyQuestion.Section,
                                AllowSkipAswering = ObjSurveyQuestion.AllowSkipAswering,
                                Status = ObjSurveyQuestion.Status,
                                Id = ObjSurveyQuestion.Id,
                                OptionType = ObjSurveyQuestion.OptionType
                            };

                            List<APISurveyOption> aPISurveyOptions = new List<APISurveyOption>();
                            List<Models.SurveyOption> surveyOptions = await surveyOptionRepository.GetAll(o => o.QuestionId == ObjSurveyQuestion.Id);
                            foreach (Models.SurveyOption option in surveyOptions)
                            {
                                APISurveyOption opt = new APISurveyOption
                                {
                                    Id = option.Id,
                                    OptionText = option.OptionText,
                                    QuestionId = option.QuestionId
                                };
                                aPISurveyOptions.Add(opt);
                            }
                            aPISurveyQuestionMergered.aPISurveyOption = aPISurveyOptions.ToArray();
                            aPISurveyQuestionMergereds.Add(aPISurveyQuestionMergered);
                        }
                    }
                    if (aPISurveyQuestionMergereds.Count > 0)
                    // return aPISurveyQuestionMergereds;
                    {
                        Response.ResponseObject = aPISurveyQuestionMergereds;
                        Response.StatusCode = 200;
                        return Response;
                    }
                    else
                    {
                        Response.ResponseObject = null;
                        Response.StatusCode = 119;
                        Response.Description = "";
                        return Response;
                    }
                }
                else
                {
                    Response.ResponseObject = null;
                    Response.StatusCode = 304;
                    Response.Description = "The limit of responses is exceed, you can not give the Survey";
                    return Response;
                }
            }
            else
            {
                Response.ResponseObject = null;
                Response.StatusCode = 304;
                Response.Description = "Survey is not applicable";
                return Response;
            }

        }

        public async Task<APISurveyMergeredModel> GetSurvey(int questionId)
        {
            SurveyQuestion surveyQuestion = db.SurveyQuestion.Where(c => c.Id == questionId).SingleOrDefault();
            APISurveyMergeredModel surveyMergeredModelapi = new APISurveyMergeredModel
            {
                Section = surveyQuestion.Section,
                Id = surveyQuestion.Id,
                Question = surveyQuestion.Question,
                AllowSkipAnswering = surveyQuestion.AllowSkipAswering,
                Status = surveyQuestion.Status,
                IsMultipleChoice = surveyQuestion.IsMultipleChoice,
                OptionType = surveyQuestion.OptionType,
                NoOfOption = db.SurveyOption.Where(c => c.QuestionId == questionId).Count()
            };
            List<APISurveyOption> options = new List<APISurveyOption>();
            List<SurveyOption> surveyOptions = await surveyOptionRepository.GetAll(o => o.QuestionId == surveyQuestion.Id);
            foreach (SurveyOption option in surveyOptions)
            {
                APISurveyOption opt = new APISurveyOption
                {
                    Id = option.Id,
                    OptionText = option.OptionText,
                    QuestionId = option.QuestionId
                };
                options.Add(opt);
            }

            surveyMergeredModelapi.aPISurveyOption = options.ToArray();

            return surveyMergeredModelapi;
        }

        public async Task<List<APISurveyMergeredModel>> GetActiveQuestions(int page, int pageSize, string search = null)
        {
            IQueryable<APISurveyMergeredModel> Query = (from surveyQuestions in db.SurveyQuestion
                                                        where surveyQuestions.IsDeleted == false
                                                        select new APISurveyMergeredModel
                                                        {
                                                            Section = surveyQuestions.Section,
                                                            Question = surveyQuestions.Question,
                                                            NoOfOption = db.SurveyOption.Where(c => c.QuestionId == surveyQuestions.Id).Count(),
                                                            Status = surveyQuestions.Status,
                                                            AllowSkipAnswering = surveyQuestions.AllowSkipAswering,
                                                            Id = surveyQuestions.Id
                                                        });

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Question.Contains(search));
            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);

            List<APISurveyMergeredModel> surveyMergeredModel = await Query.ToListAsync();

            return surveyMergeredModel;
        }

        public async Task<int> GetActiveQuestionsCount(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.SurveyQuestion.Where(r => (r.Question.Contains(search) && r.IsDeleted == Record.NotDeleted)).CountAsync();
            return await this.db.SurveyQuestion.Where(r => (r.IsDeleted == Record.NotDeleted)).CountAsync();
        }

        public async Task<bool> existsQuestion(string question, int? questionId)
        {
            int count = await this.db.SurveyQuestion.Where(p => (p.Question.ToLower() == question.ToLower()) && p.Id != questionId && p.IsDeleted == Record.NotDeleted).CountAsync();

            if (count > 0)
                return true;
            return false;
        }

        public async Task<APIResponse> AddSurvey(List<APISurveyMergeredModel> aPISurveyMergeredModel, int userId)
        {
            APIResponse response = new APIResponse();
            List<APISurveyMergeredModel> InvalidSurveys = new List<APISurveyMergeredModel>();
            foreach (APISurveyMergeredModel surveyMergeredModelAPI in aPISurveyMergeredModel)
            {


                SurveyQuestion surveyQuestion = new SurveyQuestion
                {
                    Section = surveyMergeredModelAPI.Section,
                    Question = surveyMergeredModelAPI.Question,
                    AllowSkipAswering = surveyMergeredModelAPI.AllowSkipAnswering,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow,
                    ModifiedBy = userId,
                    ModifiedDate = DateTime.UtcNow,
                    Status = surveyMergeredModelAPI.Status,
                    IsMultipleChoice = surveyMergeredModelAPI.IsMultipleChoice

                };
                ;
                if (surveyQuestion.IsMultipleChoice == true)
                {
                    surveyQuestion.OptionType = "MultipleSelection";
                }
                else
                {
                    surveyQuestion.OptionType = "SingleSelection";
                }
                if (surveyQuestion.Section.ToLower() == "objective")
                {
                    if (surveyMergeredModelAPI.aPISurveyOption.Count(o => !string.IsNullOrEmpty(o.OptionText)) < 2)
                    {
                        surveyMergeredModelAPI.Error = "Minimum 2 options required";
                        InvalidSurveys.Add(surveyMergeredModelAPI);
                    }
                    else
                    {
                        await this.Add(surveyQuestion);
                        int OptionCount = surveyMergeredModelAPI.aPISurveyOption.Where(o => !string.IsNullOrEmpty(o.OptionText)).Count();
                        SurveyOption[] surveyOptions = new SurveyOption[OptionCount];
                        int i = 0;
                        foreach (APISurveyOption opt in surveyMergeredModelAPI.aPISurveyOption)
                        {
                            if (!string.IsNullOrEmpty(opt.OptionText))
                            {
                                //bool valid1 = FileValidation.CheckForSQLInjection(opt.OptionText);
                                //if (valid1 == true)
                                //{
                                //    response.StatusCode = 400;
                                //    return response;
                                //}
                                SurveyOption surveyOption = new SurveyOption
                                {
                                    OptionText = opt.OptionText,
                                    QuestionId = surveyQuestion.Id,
                                    CreatedBy = userId,
                                    CreatedDate = DateTime.UtcNow,
                                    ModifiedBy = userId,
                                    ModifiedDate = DateTime.UtcNow
                                };
                                surveyOptions[i] = surveyOption;
                                i++;
                            }
                        }
                        await surveyOptionRepository.AddRange(surveyOptions);
                    }
                }
                if (surveyQuestion.Section.ToLower() == "subjective")
                {
                    await this.Add(surveyQuestion);
                }
            }

            if (InvalidSurveys.Count != 0)
            {
                response.StatusCode = 400;
                response.Description = "Invalid Data";
                response.ResponseObject = InvalidSurveys;
                return response;
            }
            response.StatusCode = 200;
            response.Description = "Success";
            return response;

        }
        public bool QuestionExists(string question)
        {
            if (this.db.SurveyQuestion.Count(x => x.Question == question && x.IsDeleted == false) > 0)
                return true;
            return false;
        }
        //public bool QuestionExists(string question, int? id = null)
        //{
        //    if (this.db.SurveyQuestion.Count(Q =>
        //    Q.Question.Trim().ToLower().Equals(question.Trim().ToLower())
        //    && Q.IsDeleted == false && (id == null || Q.Id != id)) > 0)
        //        return true;
        //    return false;
        //}
        public async Task<bool> IsQuestionAssigned(int questionId)
        {
            int Count = await this.db.SurveyResultDetail.Where(c => c.ServeyQuestionId == questionId && c.IsDeleted == Record.NotDeleted).CountAsync();

            //int QuestionCount = await this.db.SurveyResult.Where(c => c.SurveyId == surveyId && c.IsDeleted==Record.NotDeleted).CountAsync();
            if (Count > 0)
                return true;
            return false;
        }
        public async Task<APIResponse> MultiDeleteSurveyQuestion(APIDeleteSurveyQuestion[] apideletemultipleques)
        {
            int totalRecordForDelete = apideletemultipleques.Count();
            int totalRecordRejected = 0;

            List<APIDeleteSurveyQuestion> QueStatusList = new List<APIDeleteSurveyQuestion>();
            foreach (APIDeleteSurveyQuestion deletemultipleque in apideletemultipleques)
            {
                APIDeleteSurveyQuestion surveyqueStatus = new APIDeleteSurveyQuestion();
                SurveyQuestion Question = await this.Get(deletemultipleque.Id);
                bool result = await IsQuestionAssigned(deletemultipleque.Id);
                if (result == false)
                {

                    Question.IsDeleted = true;
                }

                else
                {
                    surveyqueStatus.ErrMessage = Message.DependencyExist.ToString();
                    surveyqueStatus.QuestionText = Question.Question.ToString();
                    surveyqueStatus.Id = Question.Id;
                    totalRecordRejected++;
                    QueStatusList.Add(surveyqueStatus);


                }
                await this.Update(Question);

            }

            string resultstring = (totalRecordForDelete - totalRecordRejected) + " record deleted out of " + totalRecordForDelete;
            APIResponse response = new APIResponse
            {
                StatusCode = 200,
                ResponseObject = new { resultstring, QueStatusList }
            };
            return response;

        }

        public async Task<APIResponse> GetMultipleBySurveyId(int SurveyId, int userid, string OrgCode)
        {

            APIResponse Response = new APIResponse();
            int AverageRespondTime = 0;
            bool applicabletouser = false;

            IEnumerable<SurveyManagement> SurveyManagementList = await this.surveyManagementRepository.GetAllSurveyManagement(userid);
            foreach (SurveyManagement obj in SurveyManagementList)
            {
                if (obj.Id == SurveyId && applicabletouser == false)
                {
                    applicabletouser = true;
                }
            }

            if (applicabletouser == true)
            {
                int TargetResponseCount = 0;
                int Durations = 0;

                var surveyDetails = db.SurveyManagement.Where(c => c.Id == SurveyId).Select(c => new
                {
                    c.TargetResponseCount,
                    c.AverageRespondTime
                }).FirstOrDefault();

                TargetResponseCount = surveyDetails.TargetResponseCount;
                Durations = surveyDetails.AverageRespondTime;

                int CompletedCount = 0;
                CompletedCount = db.SurveyResult.Where(c => c.SurveyId == SurveyId).Select(c => c.Id).Count();

                //int ResponseCount = 0;
                //ResponseCount = db.SurveyManagement.Where(c => c.IsApplicableToAll == true).Select(c => c.ResponseCount).FirstOrDefault();

                int count = await this.db.SurveyResult.Where(p => p.SurveyId == SurveyId && p.UserId == userid).CountAsync();

                if ((TargetResponseCount > CompletedCount) || (count > 0))
                {
                    List<APISurveyQuestionMergeredForMultiple> aPISurveyQuestionMergereds = new List<APISurveyQuestionMergeredForMultiple>();
                    string cacheKey = Constants.SURVEY_QUESTIONS + "_" + OrgCode + "_" + Convert.ToString(SurveyId);
                    var cache = new CacheManager.CacheManager();
                    if (cache.IsAdded(cacheKey))
                    {
                        aPISurveyQuestionMergereds = cache.Get<List<APISurveyQuestionMergeredForMultiple>>(cacheKey);
                    }
                    else
                    {
                        IEnumerable<SurveyQuestion> surveyQuestions = await (from surveyManagement in db.SurveyManagement
                                                                             join surveyConfiguration in db.SurveyConfiguration on surveyManagement.LcmsId equals surveyConfiguration.LcmsId
                                                                             join surveyQuestion in this.db.SurveyQuestion on surveyConfiguration.QuestionId equals surveyQuestion.Id
                                                                             where surveyManagement.Id == SurveyId
                                                                             orderby surveyConfiguration.SequenceNumber ascending
                                                                             select surveyQuestion
                                                             ).ToListAsync();

                        foreach (SurveyQuestion ObjSurveyQuestion in surveyQuestions)
                        {
                            if (ObjSurveyQuestion != null)
                            {
                                APISurveyQuestionMergeredForMultiple aPISurveyQuestionMergered = new APISurveyQuestionMergeredForMultiple
                                {
                                    Question = ObjSurveyQuestion.Question,
                                    Section = ObjSurveyQuestion.Section,
                                    AllowSkipAswering = ObjSurveyQuestion.AllowSkipAswering,
                                    Status = ObjSurveyQuestion.Status,
                                    Id = ObjSurveyQuestion.Id,
                                    OptionType = ObjSurveyQuestion.OptionType,
                                    AverageRespondTime = Durations

                                };

                                List<APISurveyOption> aPISurveyOptions = new List<APISurveyOption>();
                                List<Models.SurveyOption> surveyOptions = await surveyOptionRepository.GetAll(o => o.QuestionId == ObjSurveyQuestion.Id);
                                foreach (Models.SurveyOption option in surveyOptions)
                                {
                                    APISurveyOption opt = new APISurveyOption
                                    {
                                        Id = option.Id,
                                        OptionText = option.OptionText,
                                        QuestionId = option.QuestionId
                                    };
                                    aPISurveyOptions.Add(opt);
                                }
                                aPISurveyQuestionMergered.aPISurveyOption = aPISurveyOptions.ToArray();
                                aPISurveyQuestionMergereds.Add(aPISurveyQuestionMergered);
                            }
                        }
                        cache.Add(cacheKey, aPISurveyQuestionMergereds, DateTimeOffset.Now.AddMinutes(Constants.CACHE_EXPIRED_TIMEOUT));
                    }
                    if (aPISurveyQuestionMergereds.Count > 0)
                    // return aPISurveyQuestionMergereds;
                    {
                        Response.ResponseObject = aPISurveyQuestionMergereds;
                        Response.StatusCode = 200;
                        return Response;
                    }
                    else
                    {
                        Response.ResponseObject = null;
                        Response.StatusCode = 119;
                        Response.Description = "";
                        return Response;
                    }
                }
                else
                {
                    Response.ResponseObject = null;
                    Response.StatusCode = 304;
                    Response.Description = "The limit of responses is exceed, you can not give the Survey";
                    return Response;
                }
            }
            else
            {
                Response.ResponseObject = null;
                Response.StatusCode = 304;
                Response.Description = "Survey is not applicable";
                return Response;
            }

        }
    }
}
