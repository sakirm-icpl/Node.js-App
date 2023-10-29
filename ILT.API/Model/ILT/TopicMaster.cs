using System.ComponentModel.DataAnnotations;

namespace ILT.API.Model.ILT
{
    public class TopicMaster : BaseModel
    {
        public int ID { get; set; }
        [Required]
        [MaxLength(50)]
        public string TopicName { get; set; }
    }
}
