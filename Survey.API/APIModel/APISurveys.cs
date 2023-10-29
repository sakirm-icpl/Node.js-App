using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.API.APIModel
{
    public class APISurveys
    {
        public int Id { get; set; }
        public int SurveyManagementId { get; set; }
        public string SurveySubject { get; set; }
        public bool IsDeleted { get; set; }
    }
}
