using Competency.API.APIModel;
using Competency.API.Helper;
using Competency.API.Model;
using Competency.API.Model.Competency;
using Competency.API.Models;
using Competency.API.Repositories.Interfaces;
using Competency.API.Repositories.Interfaces.Competency;
using log4net;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using static Competency.API.Model.ResponseModels;

namespace Competency.API.Repositories.Competency
{
    public class CompetencyReviewParametersRepository : Repository<CompetencyReviewParameters>, ICompetencyReviewParametersRepository
    {

        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetencyReviewParametersRepository));
        private CourseContext db;
        private ICourseRepository _courseRepository;

        public CompetencyReviewParametersRepository(CourseContext context, ICourseRepository courseRepository) : base(context)
        {
            this.db = context;
            this._courseRepository = courseRepository;

        }

        public async Task<bool> Exists(string review)
        {
            review = review.ToLower().Trim();
            //subcategory = subcategory.ToLower().Trim();
            int Count = 0;


            Count = await (from c in this.db.CompetencyReviewParameters
                           where c.IsDeleted == false && (c.ReviewParameter.ToLower().Equals(review))
                           select new
                           { c.Id }).CountAsync();


            if (Count > 0)
                return true;
            return false;

        }

        public async Task<IApiResponse> GetCompetencyReviewParametersOptions()
        {
            APIResponse<CompetencyReviewParametersOptions> aPIResponse = new APIResponse<CompetencyReviewParametersOptions>();
            List<CompetencyReviewParametersOptions> aPICompetencies = new List<CompetencyReviewParametersOptions>();

            try
            {
                aPICompetencies = this.db.CompetencyReviewParametersOptions.Where(s => s.IsDeleted == Record.NotDeleted && s.IsSupervisor == false).ToList();

            }
            catch (Exception ex)
            {
                _logger.Error(ex.InnerException);
            }

            aPIResponse.Data.Records = aPICompetencies;
            aPIResponse.Data.RecordCount = aPICompetencies.Count();


            return aPIResponse;
        }

        public async Task<IApiResponse> getCompetencyReviewParametersSupervisorOptions()
        {
            APIResponse<CompetencyReviewParametersOptions> aPIResponse = new APIResponse<CompetencyReviewParametersOptions>();
            List<CompetencyReviewParametersOptions> aPICompetencies = new List<CompetencyReviewParametersOptions>();

            try
            {
                aPICompetencies = this.db.CompetencyReviewParametersOptions.Where(s => s.IsDeleted == Record.NotDeleted && s.IsSupervisor == true).ToList();

            }
            catch (Exception ex)
            {
                _logger.Error(ex.InnerException);
            }

            aPIResponse.Data.Records = aPICompetencies;
            aPIResponse.Data.RecordCount = aPICompetencies.Count();


            return aPIResponse;
        }

        public async Task<IApiResponse> SaveAssessment(APICompetencyReviewParametersSelfAssessment postModel, int UserId)
        {
            APIResponse<GetReviewParametersPostModelResult> aPIResponse = new APIResponse<GetReviewParametersPostModelResult>();
            try
            {
                foreach (APISelfAssessmentQuestions item in postModel.assessmentQuestions)
                {
                    CompetencyReviewParametersAssessment competencyReviewParametersAssessment = db.CompetencyReviewParametersResult.Where(a => a.UserId == UserId
                    && a.ReviewParameterID == item.ReviewParameterID && a.IsDeleted == false && a.IsActive == true
                    ).FirstOrDefault();
                    if (competencyReviewParametersAssessment == null)
                    {
                        CompetencyReviewParametersAssessment competencyReview = new CompetencyReviewParametersAssessment();
                        competencyReview.AssessmentType = "SingleSelection";
                        competencyReview.CompetencyId = postModel.CompetencyId;
                        competencyReview.CreatedBy = UserId;
                        competencyReview.IsDeleted = false;
                        competencyReview.ModifiedBy = UserId;
                        competencyReview.IsActive = true;
                        competencyReview.ModifiedBy = UserId;
                        competencyReview.CreatedDate = DateTime.Now;
                        competencyReview.ModifiedDate = DateTime.Now;
                        competencyReview.ReviewParameterID = item.ReviewParameterID;
                        competencyReview.UserResponse = item.UserResponse;
                        competencyReview.UserLevel = postModel.UserLevel;
                        competencyReview.UserId = UserId;
                        this.db.CompetencyReviewParametersResult.Add(competencyReview);
                        this.db.SaveChanges();
                    }
                    else
                    {
                        return new APIResposeNo { Message = "Already Submitted Assessment" };
                    }
                }

                return new APIResposeYes { Message = "Record Added Successfully" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.InnerException);
            }
            return new APIResposeNo { };
        }

        public async Task<IApiResponse> SaveAssessmentUpdate(APICompetencyReviewParametersSelfAssessment postModel, int UserId)
        {
            APIResponse<GetReviewParametersPostModelResult> aPIResponse = new APIResponse<GetReviewParametersPostModelResult>();
            try
            {
                foreach (APISelfAssessmentQuestions item in postModel.assessmentQuestions)
                {
                    CompetencyReviewParametersAssessment competencyReviewParametersAssessment = db.CompetencyReviewParametersResult.Where(a => a.UserId == UserId
                    && a.ReviewParameterID == item.ReviewParameterID && a.IsDeleted == false && a.IsActive == true
                    ).FirstOrDefault();
                    if (competencyReviewParametersAssessment != null)
                    {
                        CompetencyReviewParametersAssessment competencyReview = new CompetencyReviewParametersAssessment();
                        competencyReview.AssessmentType = "SingleSelection";
                        competencyReview.CompetencyId = postModel.CompetencyId;
                        competencyReview.CreatedBy = UserId;
                        competencyReview.IsDeleted = false;
                        competencyReview.ModifiedBy = UserId;
                        competencyReview.IsActive = true;
                        competencyReview.ModifiedBy = UserId;
                        competencyReview.CreatedDate = DateTime.Now;
                        competencyReview.ModifiedDate = DateTime.Now;
                        competencyReview.ReviewParameterID = item.ReviewParameterID;
                        competencyReview.UserResponse = item.UserResponse;
                        competencyReview.UserLevel = postModel.UserLevel;
                        competencyReview.UserId = UserId;
                        this.db.CompetencyReviewParametersResult.Update(competencyReview);
                        this.db.SaveChanges();
                    }
                    else
                    {
                        return new APIResposeNo { Message = "Already Submitted Assessment" };
                    }
                }

                return new APIResposeYes { Message = "Record Updated Successfully" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.InnerException);
            }
            return new APIResposeNo { };
        }

        public async Task<IApiResponse> SaveSupervisorAssessment(CompetencySupervisorUpdate postModel, int UserId)
        {
            APIResponse<GetReviewParametersPostModelResult> aPIResponse = new APIResponse<GetReviewParametersPostModelResult>();
            try
            {
                foreach (APISelfAssessmentQuestionsSupervisor item in postModel.assessmentQuestions)
                {
                    CompetencyReviewParametersAssessment competencyReviewParametersAssessment = 
                        db.CompetencyReviewParametersResult.Where(a => a.Id == item.Id
                    ).FirstOrDefault();
                    if (competencyReviewParametersAssessment != null)
                    {

                        competencyReviewParametersAssessment.SupervisorLevel = postModel.SupervisorLevel;
                        competencyReviewParametersAssessment.SupervisorId = UserId;
                        competencyReviewParametersAssessment.ModifiedDate = DateTime.Now;
                        competencyReviewParametersAssessment.SupervisorDate = DateTime.Now;
                        competencyReviewParametersAssessment.ReviewParameterID = item.ReviewParameterID;
                        competencyReviewParametersAssessment.SupervisorResponse = item.UserResponse;
                        competencyReviewParametersAssessment.SupervisorOverallRemark = postModel.SupervisorOverallRemark;
                        this.db.CompetencyReviewParametersResult.Update(competencyReviewParametersAssessment);
                        this.db.SaveChanges();
                    }
                    else
                    {
                        return new APIResposeNo { Message = "Already Submitted Assessment" };
                    }
                }

                return new APIResposeYes { Message = "Record Added Successfully" };
            }
            catch (Exception ex)
            {
                _logger.Error(ex.InnerException);
            }
            return new APIResposeNo { };
        }

        public async Task<IApiResponse> GetLastAssessmentDate(UserIdPayload selfRatingForSupervisor)
        {
            APIResponseSingle<CompetencyReviewParametersAssessment> aPIResponse = new APIResponseSingle<CompetencyReviewParametersAssessment>();
            UserMaster userMaster = db.UserMaster.Where(a => a.UserId == selfRatingForSupervisor.Id && a.IsDeleted == false).FirstOrDefault();

            CompetencyReviewParametersAssessment competencyReviewParametersAssessment = db.CompetencyReviewParametersResult.Where
                (a => a.UserId == userMaster.Id).OrderByDescending(a => a.CreatedDate).FirstOrDefault();

            if(competencyReviewParametersAssessment!= null)
            {
                aPIResponse.Record = competencyReviewParametersAssessment;
            }else
            {
                aPIResponse.Record = null;
            }


            return aPIResponse;
        }

        public async Task<IApiResponse> GetSelfRatingforSupervisor(SelfRatingForSupervisor selfRatingForSupervisor)
        {
            APIResponse<CompetencyReviewParametersAssessment> aPIResponse = new APIResponse<CompetencyReviewParametersAssessment>();
            UserMaster userMaster = db.UserMaster.Where(a => a.UserId == selfRatingForSupervisor.UserId && a.IsDeleted == false).FirstOrDefault();
            if(userMaster == null)
            {
                return new APIResposeNo { Message = "No User Found" };
            }
            List<CompetencyReviewParametersAssessment> competencyReviewParametersAssessment = db.CompetencyReviewParametersResult.Where
                (a => a.UserId == userMaster.Id && a.CompetencyId == selfRatingForSupervisor.CompetencyId).ToList();
            foreach (CompetencyReviewParametersAssessment item in competencyReviewParametersAssessment)
            {
                if (item.UserLevel!= null)
                {
                    int level = Convert.ToInt32(item.UserLevel);
                    CompetencyLevels levels = db.CompetencyLevels.Where(v => v.Id == level).FirstOrDefault();
                    if(levels == null)
                    {
                        break;
                    }
                    item.UserLevel = levels.LevelName;
                }
                
            }

            aPIResponse.Data.Records = competencyReviewParametersAssessment;
            aPIResponse.Data.RecordCount = competencyReviewParametersAssessment.Count();
            return aPIResponse;
        }
        public async Task<IApiResponse> GetCompetencyReviewParameters(GetReviewParametersPostModel postModel)
        {
            APIResponse<GetReviewParametersPostModelResult> aPIResponse = new APIResponse<GetReviewParametersPostModelResult>();
            List<GetReviewParametersPostModelResult> aPICompetencies = new List<GetReviewParametersPostModelResult>();
            //var enable_subset = await _courseRepository.GetMasterConfigurableParameterValue("COMP_SUB_SUBCATEGORY");
            //if (Convert.ToString(enable_subset).ToLower() == "yes")
            //{
                IQueryable<GetReviewParametersPostModelResult> result = null;
                //using (var context = this.db)
                //{
                result = (from competencyReviewParameters in this.db.CompetencyReviewParameters
                          join compLevel in this.db.CompetencyLevels on competencyReviewParameters.Id equals compLevel.Id
                          into competencyCategory1
                          from competenciesMasterSub in competencyCategory1.DefaultIfEmpty()
                          where competencyReviewParameters.IsDeleted == Record.NotDeleted
                            && competencyReviewParameters.JobRoleId == postModel.JobRoleID 
                            && competencyReviewParameters.CompetencyId == postModel.CompetencyID
                          select new GetReviewParametersPostModelResult
                          {
                              Id = competencyReviewParameters.Id,
                              LevelID = competencyReviewParameters.Level,
                              LevelName = competenciesMasterSub.LevelName,
                              ReviewParameter = competencyReviewParameters.ReviewParameter
                          });



                aPIResponse.Data.RecordCount = await result.CountAsync();


                result = result.OrderByDescending(v => v.Id);
                

                aPICompetencies = await result.ToListAsync();

                aPIResponse.Data.Records = aPICompetencies;


                return aPIResponse;
                //}
            //}
            //else
            //{
            //    return aPIResponse;
            //}
           
        }
        public async Task<IApiResponse> GetAllCompetencyReviewParameters(ReviewParametersPostModel postModel)
        {
            APIResponse<CompetencyReviewParametersResult> aPIResponse = new APIResponse<CompetencyReviewParametersResult>();
            List<CompetencyReviewParametersResult> reviewParametersResults = new List<CompetencyReviewParametersResult>();
            var connection = this.db.Database.GetDbConnection();
            if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                connection.Open();
            using (var cmd = connection.CreateCommand())
            {
                cmd.CommandText = "GetCompetencyReviewParameters";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new SqlParameter("@Page", SqlDbType.VarChar) { Value = postModel.Page });
                cmd.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.VarChar) { Value = postModel.PageSize });
                cmd.Parameters.Add(new SqlParameter("@search", SqlDbType.VarChar) { Value = postModel.FilterName });
                cmd.Parameters.Add(new SqlParameter("@columnName", SqlDbType.VarChar) { Value = postModel.FilterValue });


                DbDataReader reader = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                dt.Load(reader);
                try
                {
                    if (dt.Rows.Count > 0)
                    {

                        foreach (DataRow row in dt.Rows)
                        {
                            CompetencyReviewParametersResult result = new CompetencyReviewParametersResult();
                            result.Id = Convert.ToInt32(row["Id"].ToString());
                            result.CompetencyId = Convert.ToInt32(row["CompetencyId"].ToString());
                            result.CompetencyName = Convert.ToString(row["CompetencyName"]);
                            result.CategoryId = Convert.ToInt32(row["CompetencyCategoryId"].ToString());
                            result.CategoryName = Convert.ToString(row["Category"]);
                            result.SubcategoryId = Convert.ToString(row["CompetencySubcategoryId"].ToString());
                            result.SubcategoryName = Convert.ToString(row["SubcategoryDescription"]);
                            result.SubSubcategoryId = Convert.ToString(row["CompetencySubSubcategoryId"].ToString());
                            result.SubSubcategoryName = Convert.ToString(row["SubSubcategoryDescription"]);
                            result.CompetencyLevelId = Convert.ToString(row["Level"].ToString());
                            result.CompetencyLevelDescription = Convert.ToString(row["LevelName"]);
                            result.JobRoleId = Convert.ToString(row["JobRoleId"].ToString());
                            result.JobRoleName = Convert.ToString(row["Name"]);
                            result.ReviewParameter = Convert.ToString(row["ReviewParameter"]);

                            reviewParametersResults.Add(result);
                        }

                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }
               
                reader.Dispose();
            }
            connection.Close();

            if(reviewParametersResults.Count>0)
            {
                IQueryable<CompetencyReviewParametersResult> results = reviewParametersResults.AsQueryable<CompetencyReviewParametersResult>();
                aPIResponse.Data.RecordCount = reviewParametersResults.Count;
                if (!string.IsNullOrEmpty(postModel.FilterValue) && string.IsNullOrEmpty(postModel.FilterName))
                {
                    results = results.Where(r => r.ReviewParameter.Contains(postModel.FilterValue));
                    aPIResponse.Data.RecordCount = await results.CountAsync();
                }

                if (!string.IsNullOrEmpty(postModel.FilterName))
                {
                    if (postModel.FilterName.Equals("Category"))
                    {
                        results = results.Where(r => r.CategoryName.Contains(postModel.FilterValue));
                    }
                    if (postModel.FilterName.Equals("Subcategory"))
                    {
                        results = results.Where(r => r.SubcategoryName.Contains(postModel.FilterValue));
                    }
                    if (postModel.FilterName.Equals("Competency"))
                    {
                        results = results.Where(r => r.CompetencyName.Contains(postModel.FilterValue));
                    }
                    aPIResponse.Data.RecordCount = await results.CountAsync();


                }
                results = results.OrderByDescending(v => v.Id);
                var reviews = results.ToList();
               

                if (postModel.Page != -1)
                    results = results.Skip((postModel.Page - 1) * postModel.PageSize);
                if (postModel.PageSize != -1)
                    results = results.Take(postModel.PageSize);
                results = results.OrderByDescending(v => v.Id);
                reviews = results.ToList();

                aPIResponse.Data.Records = reviews;
               
                return aPIResponse;
            }
            else
            {
                return new APIResposeNo { Message = "No Record Found", Content = "No Records Found" };
            }
           
        

        }
        public async Task<IApiResponse> GetUserSelfAssessment(int userid)
        {
            APIResponse<APICompetencySkillNameV2> aPIResponse = new APIResponse<APICompetencySkillNameV2>();
            try
            {
                List<APICompetencySkillNameV2> CompetencySkillSetList1 = await (from CompetencyReviewParametersResult in db.CompetencyReviewParametersResult
                                                                          join competencymaster in db.CompetenciesMaster on CompetencyReviewParametersResult.CompetencyId equals competencymaster.Id
                                                                          where CompetencyReviewParametersResult.IsDeleted == false && CompetencyReviewParametersResult.UserId == userid
                                                                          select new APICompetencySkillNameV2
                                                                          {
                                                                              CompetencySkill = competencymaster.CompetencyName,
                                                                              CompetencySkillId = CompetencyReviewParametersResult.Id,

                                                                          }).ToListAsync();

                
                aPIResponse.Data.Records = CompetencySkillSetList1;
                aPIResponse.Data.RecordCount = CompetencySkillSetList1.Count();

                return aPIResponse;
            }
            catch (Exception ex)
            {
                _logger.Error(ex.InnerException);
            }
            return new APIResposeNo { };
        }
    }
}
