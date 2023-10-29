using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class BookCategory : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Category { get; set; }
    }
}
