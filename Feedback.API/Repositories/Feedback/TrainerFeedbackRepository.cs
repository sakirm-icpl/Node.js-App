using AutoMapper;
using Feedback.API.APIModel;
using Feedback.API.Model;
using Feedback.API.Models;
using Feedback.API.Repositories.Interfaces;

namespace Feedback.API.Repositories
{
    public class TrainerFeedbackRepository : Repository<TrainerFeedback>, ITrainerFeedbackRepository
    {
        private FeedbackContext _db;
        public TrainerFeedbackRepository(FeedbackContext context) : base(context)
        {
            _db = context;
        }
        public int Count(string search = null, string filter = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return _db.TrainerFeedback.Where(r => r.QuestionText.Contains(search) && r.IsDeleted == false).Count();
            return _db.TrainerFeedback.Where(T => T.IsDeleted == false).Count();
        }

        public bool Exists(string name)
        {
            if (_db.TrainerFeedback.Count(y => y.QuestionText == name) > 0)
                return true;
            return false;
        }

        public List<TrainerFeedback> Get(int page, int pageSize, string search = null, string filter = null)
        {
            IQueryable<TrainerFeedback> Query = _db.TrainerFeedback;
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.QuestionText.Contains(search));
            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);
            if (pageSize != -1)
                Query = Query.Take(pageSize);
            return Query.ToList();
        }

        public TrainerFeedbackAPI GetFeedback(int courseId)
        {
            TrainerFeedback trainerFeedback = _db.TrainerFeedback.Where(c => c.IsDeleted == false).SingleOrDefault();
            TrainerFeedbackAPI trainerFeedbackAPI = Mapper.Map<TrainerFeedbackAPI>(trainerFeedback);
            List<Option> questions = new List<Option>();
            if (trainerFeedback.Option1 != null)
                questions.Add(new Option { option = trainerFeedback.Option1 });
            if (trainerFeedback.Option2 != null)
                questions.Add(new Option { option = trainerFeedback.Option2 });
            if (trainerFeedback.Option3 != null)
                questions.Add(new Option { option = trainerFeedback.Option3 });
            if (trainerFeedback.Option4 != null)
                questions.Add(new Option { option = trainerFeedback.Option4 });
            if (trainerFeedback.Option5 != null)
                questions.Add(new Option { option = trainerFeedback.Option5 });
            trainerFeedbackAPI.Options = questions.ToArray();
            return trainerFeedbackAPI;
        }


    }
}
