using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Competency.API.Model.Competency
{
    public class CompetencySubCategory: BaseModel
    {

        public int Id { get; set; }
        public int CategoryId { get; set; }

        public string SubcategoryCode { get; set; }

        public string SubcategoryDescription { get; set; }
    }

    public class CompetencySubCategoryResult
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }

        public string SubcategoryCode { get; set; }

        public string SubcategoryDescription { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CompetencySubCategoryFilter
    {
        public int id { get; set; }

        public string name{ get; set; }

    }
}
