using System;

namespace ILT.API.Model.ThirdPartyIntegration
{
    public class ExternalCourseCategory
    {
        public int ID { get; set; }
        public string CategoryName { get; set; }
        public int VendorId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set;}
    }
    public class ExternalCourseCategoryV3
    {
        public string Category { get; set; }
    }
    public class ExternalCourseProviders
    {
        public string Providers { get; set; }
    }
    public class ExternalCoursesLanguage
    {
        public string language { get; set; }
    }
    public class ExternalCourseCategoryV2
    {
        public string Category { get; set; }
    }
    public class ExternalSubCategory
    {
        public string SubCategory { get; set; }
    }
    public class ExternalSubSubCategory
    {
        public string SubSubCategory { get; set; }
    }
    public class ZobbleCourseDetails
    {
        public int ID { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
    public class APIExternalCourseCheck
    {
        public bool IsExternalCourse { get; set; }
        public string CourseLink { get; set; }
    }
}
