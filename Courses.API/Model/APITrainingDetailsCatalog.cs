using Assessment.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model
{
    public class APITrainingDetailsCatalog
    {
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
    }

    public class TrainingDetailsCatalog: CommonFields
    {
        public int Id { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
    }

    public class APITrainingDetailsTypeAhead
    {
        public int Id { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingName { get; set; }
    }
}
