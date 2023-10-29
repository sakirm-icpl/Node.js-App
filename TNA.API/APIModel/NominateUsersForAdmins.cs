using TNA.API.APIModel;
using System.Collections.Generic;

namespace TNA.API.APIModel
{
    public class NominateUsersForAdmins
    {
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
        public int SchduleId { get; set; }
        public string Status { get; set; }
        public string RequestFrom { get; set; }
        public int RequestFromLevel { get; set; }
        public string Commnet { get; set; }
        public List<APIUserData> APIUserData { get; set; }
    }
}
