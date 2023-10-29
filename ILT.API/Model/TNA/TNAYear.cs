using System;

namespace ILT.API.Model.TNA
{
    public class TNAYear : BaseModel
    {
        public int Id { get; set; }
        public string Year { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
