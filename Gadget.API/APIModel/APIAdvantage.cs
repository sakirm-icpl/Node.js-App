using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.APIModel
{
    public class APIAdvantage
    {
        public int AdvantageId { get; set; }
        public string AdvantageName { get; set; }
        public string AdvantageDescription { get; set; }
        public int ProductId { get; set; }
    }
}
