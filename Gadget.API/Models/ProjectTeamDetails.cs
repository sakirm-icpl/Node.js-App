using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Models
{
    public class ProjectTeamDetails
    {
       
            public int Id { get; set; }
            [Required]
            public int UserId { get; set; }
            [Required]
            public string ApplicationCode { get; set; }
            [Required]
            public bool IsDeleted { get; set; }
            public int CreatedBy { get; set; }
            public DateTime CreatedDate { get; set; }
            public int? TeamMember1 { get; set; }
            public int? TeamMember2 { get; set; }
            public int? TeamMember3 { get; set; }
            public int? TeamMember4 { get; set; }


        
    }
}
