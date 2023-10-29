namespace Saml.API.Helper
{
    public class Constants
    {
        public const int BATCH_SIZE = 5000;


        public struct DBTableNames
        {
            public const string TBL_SOCIALMEDIARULE = "[User].[SocialMediaRule]";
            public const string TBL_CUSTOMIZENOTIFICATIONIMPORTEDUSERS = "[User].[CustomizeNotificationImportedUsers]";
            public const string TBL_SocialMediaRejected = "[User].[SocialMediaRejected]";

        }

        public struct CacheKeyNames
        {
            public const string IS_MULTIPLE_LOGIN_ENABLED = "CONFIGURATION_VALUE_MULTIPLE_LOGIN";
            public const string GET_PERMISSIONS = "GET_PERMISSIONS";
            public const string CONFIGURABLE_VALUES = "CONFIGURABLE_VALUES";

            public const string CONNECTION_STRINGS = "CONNECTION_STRINGS";
            public const string SYSTEM_ROLES = "USER_SETTINGS";
            public const string ROLES = "ROLES";


        }
        public struct RegularExpression
        {
            public const string userIdPattern = "[a-zA-Z0-9][a-zA-Z0-9_-]*";
            public const string emailIdPattern = @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
            public const string userNamePattern = @"^[a-zA-Z .-]+$";
            public const string mobileNumberPattern = @"^((\+[1-9]{1,4}[ \-]*)|(\([0-9]{2,3}\)[ \-]*)|([0-9]{2,4})[ \-]*)*?[0-9]{3,4}?[ \-]*[0-9]{3,4}?$";
        }

        public const int CACHE_EXPIRED_TIMEOUT = 15;
    }
}
