using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Courses.API.Model.Competency
{
    public class CompetencySubSubCategory: BaseModel
    {

        public int Id { get; set; }
        public int CategoryId { get; set; }
        public int SubCategoryId { get; set; }

        public string SubSubcategoryCode { get; set; }

        public string SubSubcategoryDescription { get; set; } 
       
    }
    public class CompetencySubSubCategoryResult
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }      
        public int SubCategoryId { get; set; }
        public string SubCategoryName { get; set; }

        public string SubSubcategoryCode { get; set; }

        public string SubSubcategoryDescription { get; set; }
        public bool IsDeleted { get; set; }
    }
}
