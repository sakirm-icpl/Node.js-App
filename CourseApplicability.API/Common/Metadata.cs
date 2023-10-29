using log4net;

namespace CourseApplicability.API.Common
{
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

    public class ErrorMessage
    {
        public const string ModelError = "Model Error";
        public const string SameData = "Same Data";
    }

}
