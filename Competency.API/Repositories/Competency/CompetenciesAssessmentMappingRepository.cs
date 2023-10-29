//// ======================================
//// <copyright file="CompetenciesMappingRepository.cs" company="Enthralltech Pvt. Ltd.">
////     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
//// </copyright>
//// ======================================

//using Courses.API.APIModel.Competency;
//using Courses.API.Helper;
//using Courses.API.Model.Competency;
//using Courses.API.Models;
//using Courses.API.Repositories.Interfaces;
//using Courses.API.Repositories.Interfaces.Competency;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Linq;
//using System.Threading.Tasks;
//using log4net;


//namespace Courses.API.Repositories.Competency
//{
//    public class CompetenciesAssessmentMappingRepository : Repository<AssessmentCompetenciesMapping>, ICompetenciesAssessmentMappingRepository
//    {
//        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetenciesAssessmentMappingRepository));
//        private CourseContext db;
//        private ICustomerConnectionStringRepository _customerConnectionStringRepository;

//        public CompetenciesAssessmentMappingRepository(CourseContext context, ICustomerConnectionStringRepository customerConnectionStringRepository) : base(context)
//        {
//            this.db = context;
//            this._customerConnectionStringRepository = customerConnectionStringRepository;
//        }
   
//        public async Task<IEnumerable<APICompetenciesMapping>> GetAllCompetenciesMappingByCourse(int courseid)
//        {
//            try
//            {
//                using (var context = this.db)
//                {
//                    var result = (from competenciesMapping in context.CompetenciesMapping
//                                  join competencyCategory in context.CompetencyCategory on competenciesMapping.CompetencyCategoryId equals competencyCategory.Id
//                                  into TempCat
//                                  from competencyCategory in TempCat.DefaultIfEmpty()
//                                  join competenciesMaster in context.CompetenciesMaster on competenciesMapping.CompetencyId equals competenciesMaster.Id
//                                  join course in context.Course on competenciesMapping.CourseId equals course.Id
//                                  join category in context.Category on competenciesMapping.CourseCategoryId equals category.Id
//                                  join module in context.Module on competenciesMapping.ModuleId equals module.Id
//                                  where competenciesMapping.IsDeleted == .NotDeleted && competenciesMapping.CourseId == courseid
//                                  select new APICompetenciesMapping
//                                  {
//                                      Id = competenciesMapping.Id,
//                                      CourseCategoryId = competenciesMapping.CourseCategoryId,
//                                      CompetencyId = competenciesMapping.CompetencyId,
//                                      CompetencyCategoryId = competenciesMapping.CompetencyCategoryId,
//                                      ModuleId = competenciesMapping.ModuleId.Value,
//                                      CourseId = competenciesMapping.CourseId,
//                                      Competency = competenciesMaster.CompetencyDescription,
//                                      CourseCategory = category.Name,
//                                      Course = course.Title,
//                                      Module = module.Name,
//                                      CompetencyCategory = competencyCategory.Category,
//                                      TrainingType = module.ModuleType

//                                  });


//                    return await result.ToListAsync();
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error(Utilities.GetDetailedException(ex));
//                string exception = ex.Message;
//            }
//            return null;
//        }      

   
//        public async Task<IEnumerable<AssessmentCompetenciesMapping>> GetassessmentCompetency(int AssessmentQuestionId)
//        {
//            try
//            {
//                using (var context = this.db)
//                {
//                    var result = (from asscomp in context.AssessmentCompetenciesMapping
//                                  where (asscomp.IsDeleted == Record.NotDeleted && asscomp.AssessmentQuestionId == AssessmentQuestionId)
//                                  select new AssessmentCompetenciesMapping
//                                  {
//                                      Id = asscomp.Id,                                    
//                                      CompetencyId = asscomp.CompetencyId,                                     
//                                      AssessmentQuestionId = asscomp.AssessmentQuestionId                                  
//                                  });

//                    return await result.ToListAsync();
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.Error(Utilities.GetDetailedException(ex));
//                string exception = ex.Message;
//            }
//            return null;
//        }
       

//        public async Task<bool> Exists(int AssessmentQuestionId,  int comId, int? id = null)
//        {
//            var count = await this.db.AssessmentCompetenciesMapping.Where(p => ((p.AssessmentQuestionId == AssessmentQuestionId) && (p.CompetencyId == comId) && (p.IsDeleted == Record.NotDeleted) && (p.Id != id || id == null))).CountAsync();

//            if (count > 0)
//                return true;
//            return false;
//        }

      

//        public async void FindElementsNotInArray(int[] CurrentCompetencies, int[] aPIOldCompetenciesId, int CourseId)
//        {
//            var result = aPIOldCompetenciesId.Except(CurrentCompetencies);
//            foreach (var res in result)
//            {
//                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
//                {
//                    using (var connection = dbContext.Database.GetDbConnection())
//                    {
//                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
//                            connection.Open();
//                        using (var cmd = connection.CreateCommand())
//                        {
//                            dbContext.Database.ExecuteSqlCommand("Update Course.AssessmentCompetenciesMapping set IsDeleted = 1 where CompetencyId = " + res + " and CourseId=" + CourseId);

//                        }
//                    }
//                }

//            }
//            return;
//        }

//    }
//}
