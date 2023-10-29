using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gadget.API.Models
{
    public class Benefit : ProductCommonFields
    {
        public int BenefitId { get; set; }
        public string BenefitName { get; set; }
        public string BenefitDescription { get; set; }
        public int ProductId { get; set; }
    }
}
