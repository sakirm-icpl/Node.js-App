using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.APIModel
{
    public class APIAccessibilityRuleImport
    {
        public string Languages { get; set; }
        public string Semester { get; set; }
        public string Course { get; set; }
        public string Subject { get; set; }
        public string Unit { get; set; }
        public string ErrMessage { get; set; }
        public string UserId { get; set; }
    }
}
