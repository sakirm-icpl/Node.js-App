using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model.Process
{
    public class ProcessConfiguration : CommonFields
    {
        public int Id { get; set; }
        public int ProcessManagementId { get; set; }

    }
}
