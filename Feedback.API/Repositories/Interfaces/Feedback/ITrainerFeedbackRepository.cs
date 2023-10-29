using Courses.API.Repositories.Interfaces;
using Feedback.API.APIModel;
using Feedback.API.Model;
using System.Collections.Generic;


namespace Feedback.API.Repositories.Interfaces
{
    public interface ITrainerFeedbackRepository : IRepository<TrainerFeedback>
    {
        bool Exists(string name);
        List<TrainerFeedback> Get(int page, int pageSize, string search = null, string filter = null);
        int Count(string search = null, string filter = null);
        TrainerFeedbackAPI GetFeedback(int courseId);


    }
}
