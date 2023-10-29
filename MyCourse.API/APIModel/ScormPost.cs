using System.Collections.Generic;

namespace MyCourse.API.APIModel
{
    public class ApiScormPost
    {
        public string VarName { get; set; }
        public string VarValue { get; set; }
        public int CourseId { get; set; }
        public int ModuleId { get; set; }
    }
    public class PostApi
    {
        public List<ApiScormPost> Result { get; set; }
    }
}
