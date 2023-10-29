using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Models
{
    public class Advantage : ProductCommonFields
    {
        public int AdvantageId { get; set; }
        public string AdvantageName { get; set; }
        public string AdvantageDescription { get; set; }
        public int ProductId { get; set; }
    }
}
