// ======================================
// <copyright file="QuizQuestionMasterRepository.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using QuizManagement.API.APIModel;
using QuizManagement.API.Data;
using QuizManagement.API.Helper;
using QuizManagement.API.Models;
using QuizManagement.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizManagement.API.Repositories
{
    public class QuizQuestionMasterRepository : Repository<QuizQuestionMaster>, IQuizQuestionMasterRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(QuizQuestionMasterRepository));
        private GadgetDbContext db;
        private IQuizOptionMasterRepository quizOptionMasterRepository;
        public QuizQuestionMasterRepository(GadgetDbContext context, IQuizOptionMasterRepository quizOptionMasterrepository) : base(context)
        {
            this.db = context;
            this.quizOptionMasterRepository = quizOptionMasterrepository;
        }
        public async Task<IEnumerable<APIQuizQuestionMergered>> GetAllQuizQuestionMaster(int page, int pageSize, string search = null)
        {
            try
            {
                IQueryable<APIQuizQuestionMergered> Query = (from quizQuestionMaster in db.QuizQuestionMaster
                                                             join quizzManagement in db.QuizzesManagement on quizQuestionMaster.QuizId equals quizzManagement.Id
                                                             where quizQuestionMaster.IsDeleted == false
                                                             select new APIQuizQuestionMergered
                                                             {
                                                                 PicturePath = quizQuestionMaster.PicturePath,
                                                                 Question = quizQuestionMaster.Question,
                                                                 NoOfOption = db.QuizOptionMaster.Where(c => c.QuizQuestionId == quizQuestionMaster.Id).Count(),
                                                                 QuizId = quizQuestionMaster.QuizId,
                                                                 Hint = quizQuestionMaster.Hint,
                                                                 Mark = quizQuestionMaster.Mark,
                                                                 AnswersArePictures = quizQuestionMaster.AnswersArePictures,
                                                                 RandomizeSequence = quizQuestionMaster.RandomizeSequence,
                                                                 Id = quizQuestionMaster.Id,
                                                                 QuizTitle = quizzManagement.QuizTitle
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

                List<APIQuizQuestionMergered> quizzMergeredModel = await Query.ToListAsync();

                return quizzMergeredModel;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<IEnumerable<APIQuizQuestionMergered>> GetAllQuizQuestionMaster()
        {
            try
            {
                IQueryable<APIQuizQuestionMergered> Query = (from quizQuestionMaster in db.QuizQuestionMaster
                                                             join quizzManagement in db.QuizzesManagement on quizQuestionMaster.QuizId equals quizzManagement.Id
                                                             where quizQuestionMaster.IsDeleted == false
                                                             select new APIQuizQuestionMergered
                                                             {
                                                                 PicturePath = quizQuestionMaster.PicturePath,
                                                                 Question = quizQuestionMaster.Question,
                                                                 NoOfOption = db.QuizOptionMaster.Where(c => c.QuizQuestionId == quizQuestionMaster.Id).Count(),
                                                                 QuizId = quizQuestionMaster.QuizId,
                                                                 Hint = quizQuestionMaster.Hint,
                                                                 Mark = quizQuestionMaster.Mark,
                                                                 AnswersArePictures = quizQuestionMaster.AnswersArePictures,
                                                                 RandomizeSequence = quizQuestionMaster.RandomizeSequence,
                                                                 Id = quizQuestionMaster.Id,
                                                                 QuizTitle = quizzManagement.QuizTitle
                                                             });



                List<APIQuizQuestionMergered> quizzMergeredModel = await Query.ToListAsync();

                return quizzMergeredModel;
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<int> Count(string search = null)
        {
            IQueryable<QuizQuestionMaster> Query = (from quizQuestionMaster in db.QuizQuestionMaster
                                                    join quizzManagement in db.QuizzesManagement on quizQuestionMaster.QuizId equals quizzManagement.Id
                                                    where quizQuestionMaster.IsDeleted == false
                                                    select quizQuestionMaster);

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Question.Contains(search));
            }
            return await Query.CountAsync();
        }
        public async Task<bool> Exist(int quizzId, string search)
        {
            QuizQuestionMaster obj = new QuizQuestionMaster();
            obj = await this.db.QuizQuestionMaster.Where(p => p.Question.ToLower() == search.ToLower() && p.QuizId == quizzId && p.IsDeleted == Record.NotDeleted).FirstOrDefaultAsync();

            if (obj != null)
                return true;
            else return false;
        }
        public async Task<IEnumerable<QuizQuestionMaster>> Search(string query)
        {
            Task<List<QuizQuestionMaster>> quizQuestionMasterList = (from quizQuestionMaster in this.db.QuizQuestionMaster
                                                                     where
                                                                (quizQuestionMaster.Question.StartsWith(query) ||
                                                               Convert.ToString(quizQuestionMaster.QuizId).StartsWith(query)
                                                               )
                                                                && quizQuestionMaster.IsDeleted == false
                                                                     select quizQuestionMaster).ToListAsync();
            return await quizQuestionMasterList;
        }
        public async Task<IEnumerable<QuizQuestionMaster>> GetAllQuizQuestion(int quizId)
        {
            try
            {
                IQueryable<Models.QuizQuestionMaster> Query = this.db.QuizQuestionMaster;


                Query = Query.Where(v => v.IsDeleted == Record.NotDeleted && (v.QuizId == quizId));
                Query = Query.OrderByDescending(v => v.Id);
                return await Query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<APIResponse> GetQuizByQuizId(int QuizId, int userid)
        {
            APIResponse Response = new APIResponse();

            Boolean isApplicableToAll = await db.QuizzesManagement.Where(c => c.Id == QuizId).Select(c => c.IsApplicableToAll).FirstOrDefaultAsync();

            int TargetResponseCount = 0;
            TargetResponseCount = await db.QuizzesManagement.Where(c => c.Id == QuizId).Select(c => c.TargetResponseCount).FirstOrDefaultAsync();

            int CompletedCount = 0;
            if(isApplicableToAll == true)
            {
                CompletedCount = await db.QuizResult.Where(c => c.QuizId == QuizId && c.UserId == userid).Select(c => c.Id).CountAsync();
            }
            else
            {
                CompletedCount = await db.QuizResult.Where(c => c.QuizId == QuizId).Select(c => c.Id).CountAsync();
            }


            int count = await this.db.QuizResult.Where(p => p.QuizId == QuizId && p.UserId == userid).CountAsync();



            if ((TargetResponseCount > CompletedCount) && (count == 0))
            {
                QuizzesManagement objQuizzesManagement = db.QuizzesManagement.Where(c => c.Id == QuizId).FirstOrDefault();

                IEnumerable<QuizQuestionMaster> quizQuestionMasters = this.db.QuizQuestionMaster.Where(c => c.QuizId == objQuizzesManagement.Id && c.IsDeleted == Record.NotDeleted).ToList();
                List<APIQuizQuestionMergered> aPIQuizQuestionMergereds = new List<APIQuizQuestionMergered>();
                foreach (QuizQuestionMaster ObjQuizQuestionMaster in quizQuestionMasters)
                {
                    if (ObjQuizQuestionMaster != null)
                    {
                        APIQuizQuestionMergered aPIQuizQuestionMergered = new APIQuizQuestionMergered
                        {
                            Question = ObjQuizQuestionMaster.Question,
                            PicturePath = ObjQuizQuestionMaster.PicturePath,
                            QuizId = ObjQuizQuestionMaster.QuizId,
                            Hint = ObjQuizQuestionMaster.Hint,
                            Mark = ObjQuizQuestionMaster.Mark,
                            AnswersArePictures = ObjQuizQuestionMaster.AnswersArePictures,
                            RandomizeSequence = ObjQuizQuestionMaster.RandomizeSequence,
                            Id = ObjQuizQuestionMaster.Id
                        };

                        List<APIQuizOptionMaster> quizOptionMasters = new List<APIQuizOptionMaster>();
                        List<Models.QuizOptionMaster> AssessmentOptions = await quizOptionMasterRepository.GetAll(o => o.QuizQuestionId == ObjQuizQuestionMaster.Id);
                        foreach (Models.QuizOptionMaster option in AssessmentOptions)
                        {
                            APIQuizOptionMaster opt = new APIQuizOptionMaster
                            {
                                Id = option.Id,
                                QuizQuestionId = option.QuizQuestionId,
                                AnswerText = option.AnswerText,
                                IsCorrectAnswer = option.IsCorrectAnswer,
                                AnswerPicturePath = option.AnswerPicturePath
                            };
                            quizOptionMasters.Add(opt);
                        }
                        aPIQuizQuestionMergered.aPIQuizOptionMaster = quizOptionMasters.ToArray();
                        aPIQuizQuestionMergereds.Add(aPIQuizQuestionMergered);
                    }
                }
                if (aPIQuizQuestionMergereds.Count > 0)
                // return aPIQuizQuestionMergereds;
                {
                    Response.ResponseObject = aPIQuizQuestionMergereds;
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
                Response.Description = "The limit of responses is exceed, you can not give the Quiz";
                return Response;
            }
            //return null;
        }

        public async Task<APIQuizQuestionMergered> GetQuiz(int questionId)
        {
            QuizQuestionMaster quizQuestionMaster = db.QuizQuestionMaster.Where(c => c.Id == questionId).SingleOrDefault();
            APIQuizQuestionMergered quizQuestionMergeredapi = new APIQuizQuestionMergered
            {
                PicturePath = quizQuestionMaster.PicturePath,
                Id = quizQuestionMaster.Id,
                Question = quizQuestionMaster.Question,
                Hint = quizQuestionMaster.Hint,
                Mark = quizQuestionMaster.Mark,
                AnswersArePictures = quizQuestionMaster.AnswersArePictures,
                RandomizeSequence = quizQuestionMaster.RandomizeSequence,
                QuizId = quizQuestionMaster.QuizId
            };

            QuizzesManagement quizzesManagement = db.QuizzesManagement.Where(c => c.Id == quizQuestionMergeredapi.QuizId).SingleOrDefault();
            quizQuestionMergeredapi.QuizTitle = quizzesManagement.QuizTitle;

            //quizQuestionMergeredapi.NoOfOption = db.SurveyOption.Where(c => c.QuestionId == questionId).Count();
            List<APIQuizOptionMaster> options = new List<APIQuizOptionMaster>();
            List<QuizOptionMaster> quizOptionMasters = await quizOptionMasterRepository.GetAll(o => o.QuizQuestionId == quizQuestionMaster.Id);
            foreach (QuizOptionMaster option in quizOptionMasters)
            {
                APIQuizOptionMaster opt = new APIQuizOptionMaster
                {
                    Id = option.Id,
                    AnswerText = option.AnswerText,
                    AnswerPicturePath = option.AnswerPicturePath,
                    QuizQuestionId = option.QuizQuestionId,
                    IsCorrectAnswer = option.IsCorrectAnswer
                };
                options.Add(opt);
            }
            quizQuestionMergeredapi.NoOfOption = quizOptionMasters.Count();
            quizQuestionMergeredapi.aPIQuizOptionMaster = options.ToArray();

            return quizQuestionMergeredapi;
        }

    }

}
