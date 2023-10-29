using Assessment.API.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APIAdditionalResourceForCourse : CommonFields
    {
        public int Id { get; set; }
        public string CourseCode { get; set; }
        public string PathLink { get; set; }
        public string ContentType { get; set; }

    }
    
}
