using System;
using System.Collections.Generic;
using System.Text;

namespace ILT.API.ExternalIntegration.EdCast
{
    public static class ConstantEdCast
    {
        public const string grant_type = "client_credentials";
        //public const string clientID = "ClientID";
        //public const string clientSecrete = "ClientSecrete";
        //public const string lmsHost = "LmsHost";
        //public const string EmpoweredHost = "EmpoweredHost";
       // public const string DarwinboxHost = "DarwinboxHost";
       // public const string EmpoweredCourseUrl = "EmpoweredCourseUrl";
        //public const string DarwinboxCourseUrl = "DarwinboxCourseUrl";

        //public const string Username = "Username";
        //public const string Password = "Password";
        //public const string create_LA = "create_LA";
        //public const string update_LA = "update_LA";
        //public const string AllowedOrgCode = "AllowedOrgCode";
        //public const string Program = "Program";
        //public const string JsonFilePath = "ExternalIntegration/EdCast/AppDetails.json";


        public const string HTTPMETHOD = "POST";
        public const string Trans_Success = "SUCCESS";
        public const string Trans_Error = "ERROR";


        #region "Action per methods
        public const string loginAction = "login";
        public const string availableCourseAction = "getAvailableCourses";
        public const string myUserIDAction = "getUserId";
        public const string getAvailableCategoriesAction = "getAvailableCategories";
        public const string getCoursesByCategoryAction = "getCoursesByCategory";

        public const string getMyCoursesDetailedAction = "getMyCoursesDetailed ";
        #endregion
    }
}
