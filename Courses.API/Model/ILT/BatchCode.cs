using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model.ILT
{
    public class BatchCode
    {
        public int Id { get; set; }
        public string Prefix { get; set; }
        public bool? IsDeleted { get; set; }
        public int? UserId { get; set; }
    }
}
