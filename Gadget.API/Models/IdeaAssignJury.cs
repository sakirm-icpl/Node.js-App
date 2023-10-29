using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Models
{
    public class IdeaAssignJury : CommonFields
    {
        public int Id { get; set; }
        [Required]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Region { get; set; }
        public string Jurylevel { get; set; }

    }
}
