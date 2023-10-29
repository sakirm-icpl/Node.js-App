using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.APIModel
{
    public class APIIdeaGetAllAppToJury
    {
        public int ApplicationId { get; set; }
        public string ApplicationCode { get; set; }
        public string UserRegion { get; set; }
        public string UserBusiness { get; set; }
        public string JuryRegion { get; set; }
    }
    public class APIIdeaJuryScore
    {
        public double JuryScore { get; set; }
    }
}
