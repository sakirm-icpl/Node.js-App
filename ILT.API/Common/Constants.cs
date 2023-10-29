using System.ComponentModel;

namespace ILT.API.Common
{
    public static class Constants
    {
        public const string ASSESSMENT_QUESTIONS = "ASMTQ";
        public const string ASSESSMENT_CONFIG = "ASMTC";
        public const string REWARD_POINTS = "REWTS";
        public const string RATING_REWARD = "RAREW";
        public const string ASSESSMENT_HEADER = "ASMH";
        public const string COMPETENCY_CATEGORY = "Categories";
        public const string AssessmentSheetConfigurationId = "AssessmentSheetConfigurationId";
        public const string DarwinboxConfiguration = "DarwinboxConfiguration";
        public const string CONFIGURABLE_VALUES = "CONFIGURABLE_VALUES";

        public const string FEEDBACK_QUESTIONS = "FEEDBACKQ";
        public const int BATCH_SIZE = 5000;
        public const int CACHE_EXPIRED_TIMEOUT = 15;
    }
    public static class AppSetting
    {
        public const string ApiGatewayWwwRootFolderName = "ApiGatewayWwwroot";
    }

    public enum TrainingType
    {
        [Description("E-Learning")]
        elearning,
        [Description("Classroom")]
        classroom,
        [Description("Webinar")]
        webinar,
        [Description("Certification")]
        Certification,
        [Description("Resource")]
        Resource,

    }

    public class BBBConstant
    {
        public const string SHARED_SECRET = "SharedSecred";
        public const string BBB_BASE_URL = "BBServerURL";

        public const string AttendeePassword = "Ap";
        public const string ModeratorPassword = "Mp";
    }

    public struct CacheKeyNames
    {
        public const string IS_MULTIPLE_LOGIN_ENABLED = "CONFIGURATION_VALUE_MULTIPLE_LOGIN";
        public const string COURSE_MODULE_COUNT = "MODULE_COURSE_COUNT";
        public const string COURSE_MAX_ATTEMPTS = "COURSE_MAX_ATTEMPTS";
        public const string TOTAL_QUESTIONS_IN_ASSESSMENT = "TOTAL_QUESTIONS_IN_ASSESSMENT";
        public const string ASSESSMENT_PASSING_PERCENTAGE = "ASSESSMENT_PASSING_PERCENTAGE";
        public const string CONFIGURATION_VALUE = "CONFIGURATION_VALUE";
    }





}
