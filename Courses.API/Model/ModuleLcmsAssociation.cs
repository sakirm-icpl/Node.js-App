using Assessment.API.Models;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class ModuleLcmsAssociation : CommonFields
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public int? LCMSId { get; set; }
        public string LanguageCode { get; set; }
    }
}
