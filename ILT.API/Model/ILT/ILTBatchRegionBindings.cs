using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.Model.ILT
{
    public class ILTBatchRegionBindings
    {
        public int Id { get; set; }
        public int BatchId { get; set; }
        public int RegionId { get; set; }
        public bool IsDeleted { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
