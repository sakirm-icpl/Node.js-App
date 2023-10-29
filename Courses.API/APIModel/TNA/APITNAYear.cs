using System;

namespace Courses.API.APIModel.TNA
{
    public class APITNAYear
    {
        public int Id { get; set; }
        public string Year { get; set; }
        public bool Status { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
