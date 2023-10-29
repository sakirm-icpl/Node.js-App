using System;

namespace User.API.APIModel
{
    public class APIGetBespokeUserDetails
    {
        public string UserName { get; set; }
        public string UserId { get; set; }

        public DateTime? DateOfJoining { get; set; }

        public string Grade { get; set; }

        public string Department { get; set; }

        public string CostCode { get; set; }

        public string Id { get; set; }

        public string EmployeeCode { get; set; }


    }
}
