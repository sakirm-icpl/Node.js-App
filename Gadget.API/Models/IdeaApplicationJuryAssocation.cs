using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Models
{
    public class IdeaApplicationJuryAssocation : CommonFields
    {
        [Required]
        public int Id { get; set; }
        public int JuryId { get; set; }
        public int ApplicationId { get; set; }
        public double JuryScore { get; set; }
        public string JuryComments { get; set; }
    }
}
