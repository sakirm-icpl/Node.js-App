using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class RoleCompetency : BaseModel
    {
        public int Id { get; set; }
        public int JobRoleId { get; set; }
        public int? CompetencyCategoryId { get; set; }
      
        public int? CompetencyId { get; set; }
        public int CompetencyLevelId { get; set; }
      


    }
}
