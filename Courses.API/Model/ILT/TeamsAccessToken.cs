using System;

namespace Courses.API.Model.ILT
{
    public class TeamsAccessToken
    {
        public int ID { get; set; }
        public string TeamsToken { get; set; }
    }
    public class ConfigurableValues
    {
        public int ID { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public Boolean IsDeleted { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        public Guid RowGuid { get; set; }
        public int Sequence { get; set; }
        public string TypeCode { get; set; }
        public string TypeName { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
        public string ConfigurableParameter1 { get; set; }
        public string BaseUrl { get; set; }

        public string Authority { get; set; } 
    }
    public class UserWebinarMaster
    {
        public int Id { get; set; }
        public int WebinarID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int CreatedBy { get; set; }
        public int isDeleted { get; set; }
        public int isDefault { get; set; }
        public string TeamsEmail { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
