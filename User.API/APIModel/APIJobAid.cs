using System.ComponentModel.DataAnnotations;

namespace User.API.APIModel
{
    public class APIJobAid
    {

        [Range(0, int.MaxValue)]
        public int Id { get; set; }
        public string ContentId { get; set; }
        public string CustomerCode { get; set; }
        [Required]
        [MaxLength(100)]
        public string Title { get; set; }
        public string FileType { get; set; }
        [MaxLength(500)]
        public string AdditionalDescription { get; set; }
        public string Content { get; set; }
        [MaxLength(50)]
        public string KeywordForSearch { get; set; }
        public string UserName { get; set; }
    }


}
