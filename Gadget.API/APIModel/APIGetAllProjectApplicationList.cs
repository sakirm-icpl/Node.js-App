using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.APIModel
{
    public class APIGetAllProjectApplicationList
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public string ApplicationCode { get; set; }
        public string UserName { get; set; }
        public string UserId { get; set; }
        public string Business { get; set; }
        public string UserRegion { get; set; }
        public string StoreCode { get; set; }
        public string FinalStatus { get; set; }
        public string FirstJuryName { get; set; }
        public double FirstScore { get; set; }
        public string SecondJuryName { get; set; }
        public double SecondScore { get; set; }
        public string ThirdJuryName { get; set; }
        public double ThirdScore { get; set; }
        public double AvgScore { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }
}
