using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace User.API.APIModel
{
    public class APIGetTopRankingExport
    {
        public int? ranks { get; set; }
        public string configuredColumnName { get; set; }
        public string houseCode { get; set; }
        public string configuredColumnValue { get; set; }
    }
}
