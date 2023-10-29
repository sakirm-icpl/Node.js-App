using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class ApiTeamsCred
    {
        public string TeamsEmail { get;set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int defaultAccount { get; set; }
    }
    public class UserWebinarMaster
    {
        public int Id { get; set; }
        public string TeamsEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int CreatedBy { get; set; }
        public int WebinarId { get; set; }
        public int isDefault { get; set; }
        public int isDeleted { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
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
    public class TeamsEmail
    {
        public string Teamsemail { get; set; }
    }
    public class ApiTeamsCredential
    {
        public int Id { get; set; }
        public string TeamsEmail { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int isDefault { get; set; }
    }
    public class ApiZoomCred
    {
        public string ZoomEmail { get; set; }
        public string Username { get; set; }
        public int DefaultAccount { get; set; }
    }
    public class ApiGsuitCred
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public int DefaultAccount { get; set; }
    }
    public class ApiGetZoomCred
    {
        public int Id { get; set; }
        public string TeamsEmail { get; set; }
        public string Username { get; set; }
        public int isDefault { get; set; }
    }
}
