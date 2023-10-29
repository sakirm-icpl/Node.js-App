using log4net;

namespace Feedback.API.Common


{
    public class Metadata
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(Metadata));
        public const string CourseUrl = "CourseUrl";
        public const string CourseNameList = "CourseNameList";
        public const string GetCourseName = "GetCourseName";
        public const string ForwardSlash = "/";
        public const string GetCourseId = "GetCourseId";
    }

    public class Status
    {

        public const string Pass = "pass";
        public const string Fail = "fail";
        public const string Incomplete = "incomplete";
        public const string Complete = "complete";
        public const string Started = "started";
        public const string InProgress = "inprogress";
        public const string Incompleted = "incompleted";
        public const string Completed = "completed";
        public const string Failed = "failed";
        public const string Passed = "passed";
        public const string NotStarted = "notstarted";
        public const string All = "all";

    }
    public class ScormVarName
    {
        public const string cmi_core_score_raw = "cmi.core.score.raw";
        public const string cmi_score_raw = "cmi.score.raw";
        public const string cmi_core_lesson_status = "cmi.core.lesson_status";
        public const string cmi_completion_status = "cmi.completion_status";
    }
    public static class FedbackImportField
    {
        public const string Metadata = "Metadata";
        public const string QuestionText = "QuestionText";
        public const string IsEmoji = "IsEmoji";
        public const string IsSubjective = "IsSubjective";
        public const string QuestionType = "QuestionType";
        public const string Section = "Section";
        public const string IsAllowSkipping = "IsAllowSkipping";
        public const string IsActive = "IsActive";
        public const string NoOfOptions = "NoOfOptions";
        public const string Option1 = "Option1";
        public const string Option2 = "Option2";
        public const string Option3 = "Option3";
        public const string Option4 = "Option4";
        public const string Option5 = "Option5";

        public const string Option6 = "Option6";
        public const string Option7 = "Option7";
        public const string Option8 = "Option8";
        public const string Option9 = "Option9";
        public const string Option10 = "Option10";
        public const string CourseCode = "CourseCode";
        public const string FeedbackQuestion = "FeedbackQuestion";
        public const string FeedbackQuestionxlsx = "FeedbackQuestion.xlsx";
        public const string ImportFeedback = "Feedback.xlsx";

    }
    public class ErrorMessage
    {
        public const string ModelError = "Model Error";
        public const string SameData = "Same Data";
    }
    public enum Message
    {
        InvalidModel,
        Ok,
        SameData,
        NotFound,
        DependencyExist,
        Success,
        Duplicate,
        CannotDelete,
        ApprovedRequestsExists,
        PendingRequestsExists
    }


}
