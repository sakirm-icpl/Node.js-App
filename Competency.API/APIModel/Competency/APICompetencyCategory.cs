using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Competency.API.APIModel.Competency
{
    public class APICompetencyCategory
    {
        public string CompetencyCode { get; set; }
        public string CompetencyName { get; set; }
    }

    public class APICompetencyCategoryImport
    {
        public string CompetencyCode { get; set; }
        public string CompetencyName { get; set; }


        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }


    }

    public class APICompetencyCategoryImportColumns
    {
        public const string CompetencyCode = "CategoryCode";
        public const string CompetencyName = "CategoryName";
    }
}
