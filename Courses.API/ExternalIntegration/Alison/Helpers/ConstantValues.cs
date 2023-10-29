using System;
using System.Collections.Generic;
using System.Text;

namespace Alison.Helpers
{
    public static class ConstantValues
    {
        public const string alisonOrgId = "Eh73uDza9fYZV1Y3Tad7";
        public const string alisonOrgKey = "FrFB8SbcMaE9q2UPWlJv";

        public const string  url = "https://alison.com/api/service.php?";

        public const string soapAction = "https://alison.com/api/service.php?";


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
