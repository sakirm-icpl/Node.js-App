using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APIBespokeRequest
    {

        [Required]
        [MaxLength(200)]
        public string UserName { get; set; }
        [MaxLength(100)]
        public string Grade { get; set; }
        [Required]
        [MaxLength(1000)]
        public string UserId { get; set; }
        [MaxLength(200)]
        public string Department { get; set; }
        public string DateOfJoining { get; set; }
        public string CostCode { get; set; }
        [Required]
        public string TrainingRequestDescription { get; set; }
        [Required]
        public string DesiredOutcome { get; set; }
        [Required]
        public string Measure { get; set; }
        [Required]
        public int TotalNumberofParticipants { get; set; }
        public APIBespokeparticipants[] APIBespokeparticipants { get; set; }
        [Required]
        public DateTime NeedbyDate { get; set; }
        [MaxLength(100)]
        public string TrainingMethod { get; set; }
        [MaxLength(100)]
        public string Competency { get; set; }
        [MaxLength(100)]
        public string ManagerialCompetancy { get; set; }
        public string TrainingName { get; set; }
        public string TrainingCosts { get; set; }
        public string AttachmentPath { get; set; }
        public string Id { get; set; }
        public string TrainingCostCode { get; set; }

    }
}
