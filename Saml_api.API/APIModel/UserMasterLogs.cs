using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Saml.API.Models
{
    public class UserMasterLogs
    {
        public int Id { get; set; }
        [MaxLength(1000)]
        public int UserId { get; set; }
        [MaxLength(1000)]
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public int IsDeleted { get; set; }
        public int IsUpdated { get; set; }
        public int IsInserted { get; set; }
    }
}
