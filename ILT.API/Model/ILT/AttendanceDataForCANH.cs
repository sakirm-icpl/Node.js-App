using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.Model.ILT
{
    public class AttendanceDataForCANH : BaseModel
    {
        public int Id { get; set; }
        public int AttendanceId { get; set; }
        public string Message { get; set; }
        public string InsertedJSON { get; set; }
    }
}
