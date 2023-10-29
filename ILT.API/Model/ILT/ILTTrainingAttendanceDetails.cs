using System;

namespace ILT.API.Model.ILT
{
    public class ILTTrainingAttendanceDetails : BaseModel
    {
        public int Id { get; set; }
        public int AttendanceId { get; set; }
        public DateTime AttendanceDate { get; set; }
        public TimeSpan InTime { get; set; }
        public TimeSpan OutTime { get; set; }
        public string AttendanceStatus { get; set; }
        public int AttendanceBy { get; set; }
        public string OTP { get; set; }
    }
}
