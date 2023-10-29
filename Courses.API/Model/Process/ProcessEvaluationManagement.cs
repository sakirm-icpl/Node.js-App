using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model.Process
{
    public class ProcessEvaluationManagement : CommonFields
    {
        public int Id { get; set; }
        public string Title { get; set; }
    
        public string Objective { get; set; }
    }
}
