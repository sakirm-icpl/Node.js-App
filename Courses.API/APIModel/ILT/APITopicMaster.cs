using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel.ILT
{
    public class APITopicMaster
    {
        public int ID { get; set; }
        [Required]
        [MaxLength(50)]
        public string TopicName { get; set; }
    }
}
