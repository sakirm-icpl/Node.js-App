using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Suggestion.API.APIModel
{
    public class APIUpdateSuggestions
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
      
        public string ContextualAreaofBusiness { get; set; }
       
        public bool Status { get; set; }
        public string ApprovalStatus { get; set; }
        public string UserName { get; set; }
        public string BriefDescription { get; set; }
        public string FilePath { get; set; }
        public string FileType { get; set; }
    }
}
