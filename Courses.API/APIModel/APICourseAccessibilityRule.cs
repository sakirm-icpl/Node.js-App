using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel
{
    public class APICourseAccessibilityRule
    {
        public int Id { get; set; }
        public int? CourseId { get; set; }
        public string Title { get; set; }
        public string courseCode { get; set; }
        public bool courseStatus { get; set; }       
        public int CreatedBy { get; set; }
        public int? AreaId { get; set; }
        public int? BusinessId { get; set; }
        public int? GroupId { get; set; }
        public int? LocationId { get; set; }
    }

    public class ApplicabilityTotalAPI
    {
        public List<APICourseAccessibilityRule> Data { get; set; }
        public int TotalRecords { get; set; }
    }

    public class APICategoryAccessibilityRule
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; }
        public string CategoryCode { get; set; }
        public bool CategoryStatus { get; set; }
        public string SubCategoryCode { get; set; }
        public string SubCategory { get; set; }
        public int? SubCategoryId { get; set; }
    }
    public class APICategorySubCategoryAccessibilityRules
    {
        public int Id { get; set; }
        public int? CategoryId { get; set; }
        public string Name { get; set; }
        public string CategoryCode { get; set; }
        public bool CategoryStatus { get; set; }
        public string SubCategoryCode { get; set; }
        public string SubCategory { get; set; }
        public int? SubCategoryId { get; set; }
        public int? UserID { get; set; }
        [MaxLength(100)]
        public string EmailID { get; set; }
        [MaxLength(25)]
        public string MobileNumber { get; set; }
        [MaxLength(100)]
        public string ReportsTo { get; set; }
        [MaxLength(10)]
        public string UserType { get; set; }
        public int? Business { get; set; }
        public int? Group { get; set; }
        public int? Area { get; set; }
        public int? Location { get; set; }
        public int? ConfigurationColumn1 { get; set; }
        public int? ConfigurationColumn2 { get; set; }
        public int? ConfigurationColumn3 { get; set; }
        public int? ConfigurationColumn4 { get; set; }
        public int? ConfigurationColumn5 { get; set; }
        public int? ConfigurationColumn6 { get; set; }
        public int? ConfigurationColumn7 { get; set; }
        public int? ConfigurationColumn8 { get; set; }
        public int? ConfigurationColumn9 { get; set; }
        public int? ConfigurationColumn10 { get; set; }
        public int? ConfigurationColumn11 { get; set; }
        public int? ConfigurationColumn12 { get; set; }
        [MaxLength(100)]
        public string RuleAnticipation { get; set; }
        public int? TargetPeriod { get; set; }
        [DefaultValue(false)]
        public bool IsCourseFee { get; set; }
        [MaxLength(10)]
        public string ConditionForRules { get; set; }
        public int? CourseId { get; set; }
        public int? GroupTemplateId { get; set; }
        public string Reason { get; set; }

        public string RowGUID { get; set; }
    }

    public class APIScheduleRegiLimit
    {
        public int Id { get; set; }
        public int? ScheduleId { get; set; }
        public string Title { get; set; }
        public string scheduleCode { get; set; }
        public bool courseStatus { get; set; }
    }


    public class APIGetAllSchedule
    {      
        public int ScheduleId { get; set; }
        public string Title { get; set; }
        public string CourseCode { get; set; }
        public string Schedulecode { get; set; }
        public string ModuleNamne { get; set; }
        public string StartDate { get; set; }
        public string Type { get; set; }
        public string Venue { get; set; }
        public bool usercount { get; set; }
    }



    public class ApiGetSchedules
    {
        [Required]
        public int page { get; set; } = 1;
        [Required]
        public int pageSize { get; set; } = 10;
        public string search { get; set; }
        public string columnName { get; set; }
        public string filter { get; set; }
        public bool showAllData { get; set; }
    }
    public class APIGetAllSchedulewithusers
    {
        public int ScheduleId { get; set; }       
        public string Title { get; set; }
        public string CourseCode { get; set; }
        public string Schedulecode { get; set; }
        public string ModuleNamne { get; set; }
        public string StartDate { get; set; }
        public string Type { get; set; }
        public string Venue { get; set; }
        public bool usercount { get; set; }
        public string UserName { get; set; }
        public int CreatedBy { get; set; }
        public int? AreaId { get; set; }
        public int? BusinessId { get; set; }
        public int? GroupId { get; set; }
        public int? LocationId { get; set; }
        public bool UserCreated { get; set; }
    }
    public class APITotalGetAllSchedule
    {
        public List<APIGetAllSchedulewithusers> Data { get; set; }
        public int TotalRecords { get; set; }
    }
}
