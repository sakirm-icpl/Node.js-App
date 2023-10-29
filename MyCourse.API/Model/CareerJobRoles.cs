using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.Model
{
    public class CareerJobRoles : BaseModel
    {
        public int? Id { get; set; }       
        public int UserId { get; set; }
        [Required]
        public int JobRoleId { get; set; }
       
    }
}

