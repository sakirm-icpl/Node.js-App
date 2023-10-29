using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class APIRankingExport
    {
        public int UserId { get; set; }
        public string EUSerId { get; set; }
        public string UserName { get; set; }
        public int TotalPoint { get; set; }
        public string ProfilePicture { get; set; }
        public string Gender { get; set; }
        public int Rank { get; set; }
        public string Level { get; set; }
        public int MaximumLevelPoint { get; set; }
        public string LevelCode { get; set; }
        public string HouseCode { get; set; }
        public string HouseName { get; set; }
        public string eId { get; set; }
        public string UserCategory { get; set; }
        public string SalesOfficer { get; set; }
        public string SalesArea { get; set; }
        public string ControllingOffice { get; set; }
        public string District { get; set; }
        public string Designation { get; set; }
        public bool UserStatus { get; set; }
        public string AreaName { get; set; }
    }
}
