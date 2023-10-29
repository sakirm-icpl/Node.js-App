using System.ComponentModel.DataAnnotations;

namespace Competency.API.Model.Competency
{
    public class CareerJobRoles : BaseModel
    {
        public int? Id { get; set; }       
        public int UserId { get; set; }
        [Required]
        public int JobRoleId { get; set; }
       
    }
}

