using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Models
{
    public class WorkDiary : CommonFields
    {
        public int Id { get; set; }
        public DateTime WorkDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Accounts { get; set; }
    }
}
