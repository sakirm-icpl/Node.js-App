using TNA.API.Model;
using System;
using System.ComponentModel.DataAnnotations;

namespace TNA.API.Model
{
    public class BespokeRequest : CommonFields
    {
        public int Id { get; set; }
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
        public DateTime DOJ { get; set; }
        public string CostCode { get; set; }
        public string TrainingRequestDescription { get; set; }
        public string DesiredOutcome { get; set; }
        public string Measure { get; set; }
        public int TotalNumberofParticipants { get; set; }
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
        public string TrainingCostCode { get; set; }

    }
}