using System;

namespace TNA.API.Model
{
    public class TNAYear : BaseModel
    {
        public int Id { get; set; }
        public string Year { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
}
