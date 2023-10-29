using System.ComponentModel.DataAnnotations;

namespace MyCourse.API.APIModel
{
    public class ApiNotification
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Message { get; set; }
        public string Type { get; set; }
        public string Url { get; set; }
        public bool IsRead { get; set; }
        public int UserId { get; set; }
        public int? CourseId { get; set; }

    }
}
