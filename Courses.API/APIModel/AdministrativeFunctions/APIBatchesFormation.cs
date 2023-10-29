namespace Courses.API.APIModel.AdministrativeFunctions
{
    public class APIBatchesFormation
    {
        public int? Id { get; set; }
        public int CourseId { get; set; }
        public int BatchId { get; set; }
        public int ModuleId { get; set; }
        public int SessionId { get; set; }
    }
    public class APIBatchesFormationDetail
    {
        public int? Id { get; set; }
        public int UserId { get; set; }
        public string Status { get; set; }
        public string RoleCode { get; set; }
        public int BatchesFormationId { get; set; }
        public int BatchCount { get; set; }
        public string BatchSize { get; set; }
    }

    public class APIBatchesFormationDetailGetBatch
    {
        public int Id { get; set; }
        public string AccommodationCapacity { get; set; }
        public string Status { get; set; }
        public string RoleCode { get; set; }
    }
}
