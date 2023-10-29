using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class ApiAuthoringMaster
    {

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }
        [Required]
        [MaxLength(500)]
        public string Skills { get; set; }
        [Required]
        public string Description { get; set; }
        public bool IsDeleted { get; set; }
        public int LCMSId { get; set; }
        public float Duration { get; set; }
        public int Id { get; set; }
        public string ModuleType { get; set; }
        public string MetaData { get; set; }
    }

    public class ApiAuthoringMasterDetails
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
        public bool IsDeleted { get; set; }
        public List<ApiAuthoringInteractiveVideoPopups> apiAuthoringInteractiveVideoPopups { get; set; }
    }
    public class ApiAuthoringInteractiveVideoPopups
    {
        public int Id { get; set; }
        public string QuestionOrMessage { get; set; }
        public string TimeStamp { get; set; }
        public bool IsQuestion { get; set; }
        public List<ApiAuthoringInteractiveVideoPopupsOptions> apiAuthoringInteractiveVideoPopupsOptions { get; set; }
    }
    public class ApiAuthoringInteractiveVideoPopupsOptions
    {
        public int Id { get; set; }
        public int authoringInteractiveVideoPopupsId { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
    }

    public class ApiAuthoringMasterDetailsUpdate
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
        public bool IsDeleted { get; set; }
        public List<authoringInteractiveVideoPopupsUpdate> authoringInteractiveVideoPopups { get; set; }
    }
    public class authoringInteractiveVideoPopupsUpdate
    {
        public int Id { get; set; }
        public string QuestionOrMessage { get; set; }
        public string TimeStamp { get; set; }
        public bool IsQuestion { get; set; }
        public List<ApiAuthoringInteractiveVideoPopupsOptionsUpdate> authoringInteractiveVideoPopupsOptionsDisplay { get; set; }
    }
    public class ApiAuthoringInteractiveVideoPopupsOptionsUpdate
    {
        public int Id { get; set; }
        public int authoringInteractiveVideoPopupsId { get; set; }
        public string OptionText { get; set; }
        public bool IsCorrectAnswer { get; set; }
    }


    public class ApiAuthoringInteractiveVideoPopupsHistory
    {
        [Required]
        public int CourseId { get; set; }
        [Required]
        public int AuthoringInteractiveVideoPopupsId { get; set; }
        public int AuthoringInteractiveVideoPopupsOptionsId { get; set; }
    }
    public class AuthoringMasterDetailsPPT
    {
        [Required]
        public int AuthoringMasterId { get; set; }
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required]
        public string Path { get; set; }
        [Required]
        public int PageNumber { get; set; }
        public string PageType { get; set; }
        public int IsDesktopLogin { get; set; }

    }
}
