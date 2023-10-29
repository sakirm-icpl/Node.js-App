using System.ComponentModel.DataAnnotations;

namespace Gadget.API.Models
{
    public class OrganizationMessages : CommonFields
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(500)]
        public string MessageHeading { get; set; }
        [Required]
        public string MessageDescription { get; set; }
        [MaxLength(500)]
        public string ProfilePicture { get; set; }
        [MaxLength(50)]
        public string MessageFrom { get; set; }
        public bool ShowToAll { get; set; }
        public bool Status { get; set; }

    }
}
