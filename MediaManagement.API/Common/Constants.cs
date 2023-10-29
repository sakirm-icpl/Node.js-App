namespace MediaManagement.API.Common
{
    public class Constants
    {
        public const string RATING_REWARD = "RAREW";
        public const int CACHE_EXPIRED_TIMEOUT = 15;
        public const string Image = "image";
        public const string Video = "video";
        public const string Document = "pdf";
        public const string Text = "text";
        public const string SURVEY_QUESTIONS = "SURVEYQ";
        public struct CacheKeyNames
        {
            public const string IS_MULTIPLE_LOGIN_ENABLED = "CONFIGURATION_VALUE_MULTIPLE_LOGIN";
        }        
        public static class AppSetting
        {
            public const string ApiGatewayWwwRootFolderName = "ApiGatewayWwwroot";
            public const string ApiGatewayUrlValue = "ApiGatewayUrl";
        }
        public static class ImportBackupFolder
        {
            public const string SurveyApplicabilityImportFolder = "SurveyApplicabilityImport";
           
        }
        public static class ImportBackupFile
        {
            public const string SurveyApplicabilityImportFile = "SurveyApplicabilityImport.csv";

        }
        public static class ImportFile
        {
            public const string SurveyApplicabilityDownloadFile = "SurveyApplicabilityImport.xlsx";

        }
        
    }
}
