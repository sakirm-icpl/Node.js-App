using System.ComponentModel.DataAnnotations;

namespace ILT.API.Model.ActivitiesManagement
{
    public class AssignmentsDetail : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        public int AssignmentId { get; set; }
        public bool Accountable { get; set; }
        public Assignments Assignment { get; set; }
    }
}
