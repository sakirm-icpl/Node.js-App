using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel.TigerhallIntegration
{
    public class APITigerhallCourses
    {
       
        public courses[] courses { get; set; }
        
       
    }
    public class courses
    {
        //public bool Isactive { get; set; }
        public string courseId { get; set; }
        public string courseName { get; set; }
        public int totalContentPieces { get; set; }
        public contents[] contents { get; set; }
    }
    public class contents
    {
        public string contentId { get; set; }
        public string contentType { get; set; }
        public string inAppLink { get; set; }
        public category category { get; set; }
    }
    public class category
    {
        public string id { get; set; }
        public string name { get; set; }
    }
}
