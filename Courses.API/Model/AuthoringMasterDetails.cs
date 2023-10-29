using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.Model
{
    public class AuthoringMasterDetails : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int AuthoringMasterId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        public int PageNumber { get; set; }
        public string PageType { get; set; }
        [Required]
        public string Content { get; set; }
        public string Path { get; set; }
    }

    public class AuthoringInteractiveVideoPopups : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int AuthoringMasterDetailsId { get; set; }
        [Required]
        public string QuestionOrMessage { get; set; }
        [Required]
        public string TimeStamp { get; set; }
        [Required]
        public bool IsQuestion { get; set; }
    }

    public class AuthoringInteractiveVideoPopupsOptions : BaseModel
    {
        public int Id { get; set; }
        [Required]
        public int AuthoringInteractiveVideoPopupsId { get; set; }
        [Required]
        public string OptionText { get; set; }
        [Required]
        public bool IsCorrectAnswer { get; set; } 
    }

    public class AuthoringInteractiveVideoPopupsHistory
    {
        [Required]
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int AuthoringInteractiveVideoPopupsId { get; set; }
        public int AuthoringInteractiveVideoPopupsOptionsId { get; set; }
        public int UserMasterId { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedDate { get; set; }
    }



    public class AuthoringMasterDetailsDisplay
    {
        public int Id { get; set; }
        [Required]
        public int AuthoringMasterId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        public int PageNumber { get; set; }
        public string PageType { get; set; }
        [Required]
        public string Content { get; set; }
        public string Path { get; set; }
        public List<AuthoringInteractiveVideoPopupsDisplay> authoringInteractiveVideoPopups { get; set; }
    }
    public class AuthoringInteractiveVideoPopupsDisplay
    {
        public int Id { get; set; }
        public int AuthoringMasterDetailsId { get; set; }
        public string QuestionOrMessage { get; set; }
        public string TimeStamp { get; set; }
        public bool IsQuestion { get; set; }
        public bool IsCompleted { get; set; }
        public List<AuthoringInteractiveVideoPopupsOptionsDisplay> authoringInteractiveVideoPopupsOptionsDisplay { get; set; }
    }
    public class AuthoringInteractiveVideoPopupsOptionsDisplay
    {
        public int Id { get; set; }
        public int AuthoringInteractiveVideoPopupsId { get; set; }
        public string OptionText { get; set; }
    }
}
