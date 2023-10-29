using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Courses.API.APIModel;
using Microsoft.EntityFrameworkCore;
using Process.API.APIModel;
using Courses.API.APIModel.TigerhallIntegration;

namespace Courses.API.Repositories
{
    public class OpenAIRepository : Repository<OpenAIQuestion>, IOpenAIRepository
    {
        private CourseContext _db;
        public OpenAIRepository(CourseContext context) : base(context)
        {
            _db = context;
        }
        public async Task<List<APIOpenAIQuestion>> GetQuestions()
        {
            return await (from openAIQuestion in this._db.OpenAIQuestion
                          where openAIQuestion.IsDeleted == false
                          select new APIOpenAIQuestion
                          {
                              Id = openAIQuestion.Id,
                              QuestionText = openAIQuestion.QuestionText,
                              AnswerText = openAIQuestion.AnswerText,
                              Metadata = openAIQuestion.Metadata,
                              Industry = openAIQuestion.Industry
                          }).ToListAsync();
        }
        public async Task<List<APIOpenAIQuestion>> GetQuestionsByCourseId(string CourseCode)
        {
            return await (from openAIQuestion in this._db.OpenAIQuestion
                          join openAICourseQuestionAssociation in this._db.OpenAICourseQuestionAssociation on openAIQuestion.Id equals openAICourseQuestionAssociation.QuestionId
                          where openAIQuestion.IsDeleted == false && openAICourseQuestionAssociation.CourseCode == CourseCode
                          select new APIOpenAIQuestion
                          {
                              Id = openAIQuestion.Id,
                              QuestionText = openAIQuestion.QuestionText,
                              AnswerText = openAIQuestion.AnswerText,
                              Metadata = openAIQuestion.Metadata,
                              Industry = openAIQuestion.Industry
                          }).ToListAsync();

        }

        public async Task<int> GetMappingId(string CourseCode, int QuestionId)
        {
            return await (from openAIQuestion in this._db.OpenAIQuestion
                          join openAICourseQuestionAssociation in this._db.OpenAICourseQuestionAssociation on openAIQuestion.Id equals openAICourseQuestionAssociation.QuestionId
                          where openAIQuestion.IsDeleted == false && openAICourseQuestionAssociation.CourseCode == CourseCode && openAIQuestion.Id == QuestionId
                          select openAICourseQuestionAssociation.Id
                          ).FirstAsync();

        }
    }

    public class OpenAICourseQuestionAssociationRepository : Repository<OpenAICourseQuestionAssociation>, IOpenAICourseQuestionAssociationRepository
    {
        private CourseContext _db;
        public OpenAICourseQuestionAssociationRepository(CourseContext context) : base(context)
        {
            _db = context;
        }
    }
}
