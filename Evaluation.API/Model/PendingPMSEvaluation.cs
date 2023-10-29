using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Evaluation.API.Model
{
    public class PendingPMSEvaluation
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Username { get; set; }
        public int ManagerId { get; set; }
        public DateTime Date { get; set; }
        public string Status { get; set; }
    }
}
