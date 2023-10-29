
using System.ComponentModel.DataAnnotations.Schema;

namespace Courses.API.Model
{
    [Table("DistributedAdminFieldAssociation", Schema = "User")]
    public class DistributedAdminFieldAssociation 
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FieldId { get; set; }
        public int IsDeleted { get; set; }
    }
}
