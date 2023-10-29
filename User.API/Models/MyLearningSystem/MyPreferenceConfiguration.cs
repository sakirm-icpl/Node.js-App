using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.Models.MyLearningSystem
{
    public class MyPreferenceConfiguration : CommonFields
    {
        public int Id { get; set; }    
        public bool IsActive { get; set; }
        [MaxLength(100)]
        public string LandingPageName { get; set; }

        [MaxLength(50)]
        public string Code { get; set; }
    }
}
