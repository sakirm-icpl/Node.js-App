using Assessment.API.APIModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace Courses.API.APIModel
{
    public class APIDataMigration
    {
        public string EmployeeCode { get; set; }
        public string CourseCode { get; set; }
        public string ScheduleCode { get; set; }
        public string ModifiedDate { get; set; }
        public string StartDate { get; set; }
        public string AssessmentResult { get; set; }
        public string MarksObtained { get; set; }
        public int? NoOfAttempts { get; set; }
        public int? ScormScore { get; set; }
        public string ScormResult { get; set; }
        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }

    }
    public class APIDataMigrationRejected
    {
        public string EmployeeCode { get; set; }
        public string CourseCode { get; set; }
        public string ScheduleCode { get; set; }
        public string ErrMessage { get; set; }


    }
    public class APIDataMigrationFilePath
    {
        public string Path { get; set; }
    }
    public class APIAssDataMigration
    {
        public string QuestionText { get; set; }
        public string OptionType { get; set; }
        public int Marks { get; set; }
        public string OptionText { get; set; }

        public bool IsCorrectAnswer { get; set; }

        public string DifficultyLevel { get; set; }
        public string QuestionType { get; set; }
        public string Section { get; set; }

        public AssessmentOptions[] aPIassessmentOptions { get; set; }

        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }
    }
    public class APICourseModuleDataMigration
    {
        public string CourseName { get; set; }
        public string CourseType { get; set; }
        public string LoginUserID { get; set; }
        public string ContentType { get; set; }
        public string MetaData { get; set; }
        public string CourseCode { get; set; }
        public string CourseCatagoryName { get; set; }
        public string CourseSubCatagoryName { get; set; }
        public string CourseDescription { get; set; }
        public string ModuleName { get; set; }
        public string ModuleCode { get; set; }
        public string IsActive { get; set; }
        public string InternalName { get; set; }
        public string OriginalFileName { get; set; }
        public string Path { get; set; }
        public string ZipPath { get; set; }
        public string ModuleType { get; set; }
        public string IsApplicableToAll { get; set; }
        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }
    }
    public class APIILTScheduleDataMigration
    {
        public string ScheduleCode { get; set; }
        public string ModuleCode { get; set; }
        public string PlaceName { get; set; }
        public string TrainerType { get; set; }
        public int? AcademyTrainerUserId { get; set; }
        public string AcademyAgencyCode { get; set; }
        public string AgencyTrainerName { get; set; }
        public string AcademyTrainerName { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime RegistrationEndDate { get; set; }
        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }
        public string ScheduleID { get; set; }
        public string TrainerID { get; set; }
        public string TrainerName { get; set; }
        public string UserName { get; set; }
        public string ScheduleType { get; set; }
    }
    public class APICompetencyImport
    {
        public string CategoryCode { get; set; }
        public string CourseCode { get; set; }
        public string CategoryName { get; set; }
        public string CompetencyName { get; set; }
        public string CompetencyDescription { get; set; }
        public string CompetencyLevel { get; set; }
        public string LevelDescription { get; set; }
        public int? InsertedID { get; set; }
        public string InsertedCode { get; set; }
        public string IsInserted { get; set; }
        public string IsUpdated { get; set; }
        public string notInsertedCode { get; set; }
        public string ErrMessage { get; set; }
    }
    public class APIDataMigrationCompetencyRejected
    {
        public string CategoryCode { get; set; }
        public string CompetencyName { get; set; }
        public string CompetencyLevel { get; set; }
        public string ErrMessage { get; set; }
    }
    public class APIILTScheduleMigrationImportColumns
    {
        public const string CourseCode = "CourseCode";
        public const string ModuleName = "ModuleName";
        public const string ScheduleCode = "ScheduleCode";
        public const string StartDate = "StartDate";
        public const string EndDate = "EndDate";
        public const string StartTime = "StartTime";
        public const string EndTime = "EndTime";
        public const string RegistrationEndDate = "RegistrationEndDate";
        public const string TrainerType = "TrainerType";
        public const string TrainerName = "TrainerName";
        public const string TrainerNameEncrypted = "TrainerNameEncrypted";
        public const string TrainingPlaceType = "TrainingPlaceType";
        public const string AcademyAgencyName = "AcademyAgencyName";
        public const string PlaceName = "PlaceName";
        public const string SeatCapacity = "SeatCapacity";
        public const string City = "City";
        public const string PostalAddress = "PostalAddress";
        public const string CoordinatorName = "CoordinatorName";
        public const string ContactNumber = "ContactNumber";
        public const string Currency = "Currency";
        public const string Cost = "Cost";
    }
    public class APITrainingNeedImportColumns
    {
       
        public const string JobRole = "JobRole";
        public const string Department = "Department";
        public const string Section = "Section";
        public const string Level = "Level";
        public const string Status = "Status";
        public const string TraniningProgram = "TraniningProgram";
        public const string Category = "Category";
    }
    public class APIILTScheduleMigrationRejected
    {
        public string CourseCode { get; set; }
        public string ModuleCode { get; set; }
        public string ScheduleCode { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string RegistrationEndDate { get; set; }
        public string TrainerType { get; set; }
        public string AgencyTrainerName { get; set; }
        public string TrainingPlaceType { get; set; }
        public string AcademyAgencyName { get; set; }
        public string PlaceName { get; set; }
        public string SeatCapacity { get; set; }
        public string City { get; set; }
        public string PostalAddress { get; set; }
        public string CoordinatorName { get; set; }
        public string ContactNumber { get; set; }
        public string Currency { get; set; }
        public string Cost { get; set; }
        public string Status { get; set; }
        public string ErrMessage { get; set; }
    }

    public class RejectedTrainingReommendationNeeds 
    {        
        public string JobRole { get; set; }
        public string Department { get; set; }      
        public string Section { get; set; }       
        public string Level { get; set; }        
        public string Status { get; set; }       
        public string TrainingProgram { get; set; }        
        public string Category { get; set; }
        public string Year { get; set; }
        public string ErrorMessage { get; set; }
        public string RecordStatus { get; set; }

    }
}
