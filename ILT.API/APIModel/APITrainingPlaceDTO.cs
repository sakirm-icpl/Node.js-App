using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ILT.API.APIModel
{
    public class APITrainingPlaceDTO
    {
        public int Id { get; set; }
        [MaxLength(50)]
        [Required]
        public string PlaceCode { get; set; }
        [MaxLength(100)]
        [Required]
        public string PlaceName { get; set; }
        [MaxLength(500)]
        [Required]
        public string PostalAddress { get; set; }
        [MaxLength(50)]
        public string TimeZone { get; set; }
        [MaxLength(10)]
        [Required]
        public string AccommodationCapacity { get; set; }
        [MaxLength(500)]
        public List<Facilitie> Facilities { get; set; }
        [Required]
        [MaxLength(50)]
        public string PlaceType { get; set; }
        [Required]
        [MaxLength(20)]
        public int UserID { get; set; }
        [Required]
        [MaxLength(20)]
        public string ContactNumber { get; set; }
        [MaxLength(20)]
        public string AlternateContact { get; set; }
        public bool Status { get; set; }
    }

    public class Facilitie
    {
        string name { get; set; }
        string value { get; set; }
        bool chk { get; set; }
    }

    public class APITrainingPlaceDetails
    {
        public int page { get; set; }
        public int pageSize { get; set; }
        public string search { get; set; }
        public string columnName { get; set; }
    }
}
