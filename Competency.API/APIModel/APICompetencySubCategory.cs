using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Competency.API.APIModel.Competency
{
    public class APICompetencySubCategory
    {
        public string CompetencyCode { get; set; }
        public string CompetencyName { get; set; }
        public string CompetencyDescription { get; set; }
    }

    public class APICompetencySubCategoryImport
    {
        public string SubCategoryCode { get; set; }
        public int? CategoryId { get; set; }
        public string SubCategoryDescription { get; set; }

        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }

        public string CategoryName { get; set; }
        public string SubCategoryName { get; set; }
       

    }

    public class APICompetencySubCategoryImportColumns
    {
        public const string CompetencyCode = "SubCategoryCode";
        public const string CategoryName = "CategoryName";
        public const string CompetencyDescription = "SubCategoryDescription";

    }
}
