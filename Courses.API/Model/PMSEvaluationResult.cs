using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class PMSEvaluationResult
    {
        public int Id { get; set; }
        public int SubmittedId { get; set; }
        public int QuestionId { get; set; }
        public string QuestionType { get; set; }
        public string QuestionText { get; set; }
        public string UserFeedback { get; set; }
        public string SubjectiveUserFeedback { get; set; }
        public string ManagerFeedback { get; set; }
        public string SubjectiveManagerFeedback { get; set; }
        public string Section { get; set; }
    }


}
