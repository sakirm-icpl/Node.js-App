using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class ApiConfiguration1
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
