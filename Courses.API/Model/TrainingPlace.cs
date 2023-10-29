using Courses.API.Validations;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class TrainingPlace : BaseModel
    {
        public int Id { get; set; }
        [MaxLength(50)]
        public string PlaceCode { get; set; }
        [MaxLength(50)]
        [Required]
        [CSVInjection]
        public string Cityname { get; set; }
        [MaxLength(100)]
        [Required]
        [CSVInjection]
        public string PlaceName { get; set; }
        [MaxLength(500)]
        [Required]
        [CSVInjection]
        public string PostalAddress { get; set; }
        [MaxLength(50)]
        public string TimeZone { get; set; }
        [MaxLength(10)]
        [Required]
        [CSVInjection]
        public string AccommodationCapacity { get; set; }
        [MaxLength(500)]
        public string Facilities { get; set; }
        [Required]
        [MaxLength(50)]
        public string PlaceType { get; set; }
       public int UserID { get; set; }
        [MaxLength(20)]
        [CSVInjection]
        [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$")]
        public string ContactNumber { get; set; }

        [CSVInjection]
        [MaxLength(20)]
        [RegularExpression(@"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$")]
        public string AlternateContact { get; set; }
        [MaxLength(50)]
        [CSVInjection]
        public string ContactPerson { get; set; }
        public bool Status { get; set; }

    }
}
