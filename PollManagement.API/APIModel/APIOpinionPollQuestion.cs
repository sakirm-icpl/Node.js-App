using System.ComponentModel.DataAnnotations;

namespace PollManagement.API.APIModel
{
    public class APIOpinionPollQuestion
    {
        public int? Id { get; set; }

        [MaxLength(500)]
        [Required]
        public string Title { get; set; }
    }
}
