using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.APIModel.Competency
{
    public class APICompetencySubSubCategory
    {
        public string CompetencyCode { get; set; }
        public string CompetencyName { get; set; }
        public string CompetencyDescription { get; set; }
    }

    public class APICompetencySubSubCategoryImport
    {
        public string SubsetCode { get; set; }
        public int? CategoryId { get; set; }
        public string SubsetDescription { get; set; }

        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }

        public string CategoryName { get; set; }
        public string SubCategory { get; set; }
       

    }

    public class APICompetencySubSubCategoryImportColumns
    {
        public const string SubCategory   = "SubCategory";
        public const string CategoryName = "CategoryName";
        public const string SubsetDescription = "SubsetDescription";
        public const string SubsetCode = "SubsetCode";

    }
}
