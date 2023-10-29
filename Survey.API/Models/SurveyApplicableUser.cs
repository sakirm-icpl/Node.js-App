using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Survey.API.Models
{
    public class SurveyApplicableUser
    {
        public static int SurveyManagementId { get; internal set; }
        public string UserID { get; internal set; }
        public string UserName { get; internal set; }
    }
}
