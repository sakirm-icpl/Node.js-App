using System.ComponentModel.DataAnnotations;

namespace ILT.API.APIModel
{
    public class APITopicMaster
    {
        public int ID { get; set; }
        [Required]
        [MaxLength(50)]
        public string TopicName { get; set; }
    }
}
