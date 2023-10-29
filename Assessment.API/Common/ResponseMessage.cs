using System.ComponentModel;

namespace Assessment.API.Common
{
    public class ResponseMessage
    {
        public string? Message { get; set; }
        public string? Description { get; set; }
        public int StatusCode { get; set; }
    }
    public enum MessageType
    {
        [Description("Users not mapped to team")]
        TeamEmpty,
        [Description("Record was saved successfully")]
        Success,
        [Description("Failed to save")]
        Fail,
        [Description("Record with same values already exists.")]
        Duplicate,
        [Description("Deleted record")]
        Delete,
        [Description("No record found!")]
        NotFound,
        [Description("Record does not exist!")]
        NotExist,
        [Description("Data not available")]
        DataNotAvailable,
        [Description("Internal Server Error")]
        InternalServerError,
        [Description("Invalid data!")]
        InvalidData,
        [Description("Invalid Request !")]
        InvalidRequest,
        [Description("Invalid file !")]
        InvalidFile,
        [Description("Dependancy exist!")]
        DependancyExist,
        [Description("Unexpected error!")]
        Unexpected,
        [Description("User does not have access to this function!")]
        NoAccess,
        [Description("Your course is not Completed")]
        NotCompleted,
        [Description("No content added! To enable course , you need to select contents.")]
        NoContentAdded,
        [Description("Duplicate Option Text")]
        DuplicateOptionText,
        [Description("Incomplete activities")]
        Incomplete,
        [Description("Please submit assignment with at least one submission type.")]
        AssignmentNotSubmitted,
        [Description("Please enter Numeric values.")]
        NotNumeric,
        [Description("Module can not be deleted, as some user(s) have already started this course.")]
        NotAllowedDelete,
        [Description("Primary Attempt Not Exhausted.")]
        AttemptNotExhausted,
        [Description("Attempts are already available to the user.")]
        AttemptExist,
        [Description("User has already passed the assessment.")]
        UserPassed,
        [Description("User has not started the assessment.")]
        UsernotStarted,
        [Description("Duplicate!.The content name is already exist in the system.")]
        DuplicateContent,
        [Description("Invalid post request")]
        InvalidPostRequest,
        [Description("Course code or title already exist in the system.")]
        DuplicateCourseCodeOrTitle,
        [Description("Cannot delete active course.")]
        CannotDelete,
        [Description("Cannot change Is Feedback Optional, as some user(s) have already started this course.")]
        CannotUpdate,
        [Description("Cannot cancel batch as dependancy exist!")]
        BatchDependancyExist,
        [Description("Unsafe data present in Input Option")]
        InvalidCharacterInputOption,
        [Description("This user is not reporting to you.")]
        UserNotInTeam,
        [Description("Course auto assignment dates required.")]
        CourseAssignmentDatesRequired,
        [Description("End date must be greater than start date.")]
        InvalidCourseAssignmentDates,
        [Description("Start date and time must not be past date and time.")]
        PastCourseAssignmentDates,
        [Description("Can't complete content! Please read content till the end.")]
        ContentNotCompleted,
        [Description("Cannot change Learning Approach, as some user(s) have already started this course.")]
        CannotUpdateLearningApproach,
        [Description("Learning activity code does not exists.")]
        CodeNotExists,
    }

    enum FileMessages
    {
        [Description("Excel file is not in required format. Please check sample excel file format and try uploading again.")]
        FileFormatError,
        [Description("Imported file does not contain data.")]
        FileEmpty,
        [Description("Error while importing course applicability file.Please contact support.")]
        FileErrorInImportUnassignCourse

    }




}
