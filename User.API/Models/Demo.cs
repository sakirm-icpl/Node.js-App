using System.ComponentModel.DataAnnotations;

namespace User.API.Models
{
    public class Demo : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
