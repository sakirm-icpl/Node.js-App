namespace MyCourse.API.APIModel
{
    public class APIUserCourseDetails
    {
        public string UserId { get; set; }
        public int page { get; set; }
        public int pageSize { get; set; }
        public int? categoryId { get; set; }
        public string search { get; set; }
        public string status { get; set; }
        public string courseType { get; set; }
        public int? subCategoryId { get; set; }

        public int? subsubCategoryId { get; set; }
        public bool? isShowCatalogue { get; set; }
        public string sortBy { get; set; }
        public int? CompetencyCategoryID { get; set; }

        // This is added to get UDEMY as provider and get the udemy courses
        public string provider { get; set; }
        public bool? IsMobile { get; set; }
    }

    public class APIGetManagerEvaluationCourses
    {
        
        public int page { get; set; }
        public int pageSize { get; set; }
       
    }

    public class APIAssignMasterTest
    {
        public string UserId { get; set; }      
        public int CourseID { get; set; }
    }
}
