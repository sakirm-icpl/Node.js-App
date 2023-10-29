using System;

namespace Courses.API.APIModel.ActivitiesManagement
{
    public class APIAssignments
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string PurposeOfExercise { get; set; }
        public string DesirableFormOutput { get; set; }
        public DateTime DateOfSubmission { get; set; }
        public string ReferenceDocumentPath { get; set; }
        public string AdditionalReferences { get; set; }
        public string Status { get; set; }
    }
    public class APIAssignmentsDetail
    {
        public int? Id { get; set; }
        public int UserId { get; set; }
        public int AssignmentId { get; set; }
        public bool Accountable { get; set; }
    }
    public class APIAssignmentsMerged
    {
        public int? Id { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public string PurposeOfExercise { get; set; }
        public string DesirableFormOutput { get; set; }
        public DateTime DateOfSubmission { get; set; }
        public string ReferenceDocumentPath { get; set; }
        public string AdditionalReferences { get; set; }
        public string Status { get; set; }
        public APIAssignmentsDetail[] aPIAssignmentsDetail { get; set; }
    }

}
