using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.APIModel
{
    public class APIILTBatch
    {
        public int? Id { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public int CourseId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public List<APIILTBatchRegionBindings> RegionIds { get; set; }
        public int? SeatCapacity { get; set; }
        public string Description { get; set; }
        public string BatchType { get; set; }
        public string ReasonForCancellation { get; set; }
    }
    public class APIILTBatchRegionBindings
    {
        public int RegionId { get; set; }
        public string RegionName { get; set; }
    }
    public class APIILTBatchDetails
    {
        public int? Id { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public int CourseId { get; set; }
        public string CourseCode { get; set; }
        public string CourseTitle { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string RegionId { get; set; }
        public string RegionName { get; set; }
        public List<APIILTBatchRegionBindings> RegionIds { get; set; }
        public int? SeatCapacity { get; set; }
        public string Description { get; set; }
        public string BatchType { get; set; }
        public string ReasonForCancellation { get; set; }
    }
    public class APIILTBatchImport
    {
        public string Path { get; set; }
    }
    public class APIILTBatchImportColumns
    {
        public const string CourseCode = "CourseCode";
        public const string BatchCode = "BatchCode";
        public const string BatchName = "BatchName";
        public const string StartDate = "StartDate";
        public const string EndDate = "EndDate";
        public const string StartTime = "StartTime";
        public const string EndTime = "EndTime";
        public const string RegionName = "RegionName";
        public const string SeatCapacity = "SeatCapacity";
        public const string Description = "Description";
    }
    public class APIILTBatchRejected
    {
        public string CourseCode { get; set; }
        public string BatchCode { get; set; }
        public string BatchName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string RegionName { get; set; }
        public string? SeatCapacity { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
    }
    public class APIILTBatchDelete
    {
        public int Id { get; set; }
        [Required]
        public string ReasonForCancellation { get; set; }
    }
}
