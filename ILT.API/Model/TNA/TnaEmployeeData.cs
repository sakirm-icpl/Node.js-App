using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.Model.TNA
{
    public class TnaEmployeeData
    {
        public int Id { get; set; }
        public string CourseDetails { get; set; }
        public string CategoryType { get; set; }
        public string Grade { get; set; }
        public string DepartmentName { get; set; }
        public string SubDepartmentName { get; set; }
    }
    
    public class TnaEmployeeNominateRequestPayload
    {
        public int Id { get; set; }
        public string CourseDetails { get; set; }
    }

    public class TnaNominateRequestData : BaseModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TnaEmployeeDataId { get; set; }
        public string RequestStatus { get; set; }
        public string Reason { get; set; }
    }

    public class TnaRequestData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string RequestDate { get; set; }
    }

    public class TnaRequestsDetails
    {
        public string Id { get; set; }
        public string CourseDetails { get; set; }
        public string CategoryType { get; set; }
        public string RequestStatus { get; set; }
        public string RequestDate { get; set; }
        public string ActionDate { get; set; }
    }
    public class Business
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
