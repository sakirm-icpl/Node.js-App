using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APIUserTrainingAdminAssociation
    {
        public int Id { get; set; }
        [Required]
        public int DepartmentId { get; set; }
        [Required]
        [MaxLength(200)]
        public string DepartmentName { get; set; }
        [Required]
        public int UserMasterId { get; set; }
        [Required]
        [MaxLength(200)]
        public string UserName { get; set; }
    }
}
