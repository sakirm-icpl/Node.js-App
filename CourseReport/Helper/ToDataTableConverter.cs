using log4net;
using log4net.Core;
using CourseReport.API.APIModel;
using CourseReport.API.Controllers;
using CourseReport.API.Helper.MetaData;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace CourseReport.API.Helper
{
    public class ToDataTableConverter : IToDataTableConverter
    {

        //private static readonly ILog _logger = LogManager.GetLogger(typeof(ILTReportController));
        public ToDataTableConverter()
        {

        }
        #region IEnumerable To DataTable
        public DataTable ToDataTableOpinionPollReport<APIOpinionPollReport>(IEnumerable<APIOpinionPollReport> items)
        {
            var tb = new DataTable(typeof(APIOpinionPollReport).Name);

            PropertyInfo[] props = typeof(APIOpinionPollReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("Search");

            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["Username"].ColumnName = HeaderName.UserName;
            tb.Columns["Question"].ColumnName = HeaderName.Question;
            tb.Columns["Option1"].ColumnName = HeaderName.OptionText1;
            tb.Columns["Option2"].ColumnName = HeaderName.OptionText2;
            tb.Columns["Option3"].ColumnName = HeaderName.OptionText3;
            tb.Columns["Option4"].ColumnName = HeaderName.OptionText4;
            tb.Columns["Option5"].ColumnName = HeaderName.OptionText5;
            tb.Columns["SelectedAnswer"].ColumnName = HeaderName.SelectedAnswer;
            tb.Columns["PollDate"].ColumnName = HeaderName.PollDate;

            return tb;
        }

        public DataTable ToDataTableAssessmentResultSheetReport<APIAssessmentResultSheetReport>(IEnumerable<APIAssessmentResultSheetReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIAssessmentResultSheetReport).Name);

            PropertyInfo[] props = typeof(APIAssessmentResultSheetReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            if (OrgCode.ToLower() == "ujjivan")
            {
                var removeCols = new string[] { "id", "AssessmentTimeDiffrance", "AssessmentEndTime", "OrgCode", "AssessmentStartTime",
                    "TotalRecordCount"};
                foreach (string rmcol in removeCols)
                {
                    tb.Columns.Remove(rmcol);
                }

                tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
                tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
                tb.Columns["UserId"].ColumnName = HeaderName.UserId;
                tb.Columns["UserName"].ColumnName = HeaderName.UserName;
                tb.Columns["MobileNumber"].ColumnName = HeaderName.MobileNo;
                tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
                tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
                tb.Columns["NoOfAttempts"].ColumnName = HeaderName.NoOfAttempts;
                tb.Columns["AssessmentResult"].ColumnName = HeaderName.AssessmentResult;
                tb.Columns["AssessmentPercentage"].ColumnName = HeaderName.AssessmentPercentage;
                tb.Columns["AssessmentDate"].ColumnName = HeaderName.AssessmentDate;

                tb.Columns["WorkLocation"].ColumnName = HeaderName.WorkLocation;
                tb.Columns["Position"].ColumnName = HeaderName.Position;
                tb.Columns["Department"].ColumnName = HeaderName.Department;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
                tb.Columns["Division"].ColumnName = HeaderName.Division;
                tb.Columns["EmployeeState"].ColumnName = HeaderName.EmployeeState;
                tb.Columns["DateofJoining"].ColumnName = HeaderName.DateofJoining;
                tb.Columns["IsAdaptiveAssessment"].ColumnName = HeaderName.AdaptiveLearning;
            }
            else if (OrgCode.ToLower() == "sbil")
            {
                var removeCols = new string[] { "id", "AssessmentEndTime", "OrgCode", "AssessmentStartTime",
                    "TotalRecordCount", "MobileNumber", "EmailId","WorkLocation","Position","Department","Division","Region","EmployeeState","DateofJoining","IsAdaptiveAssessment"};
                foreach (string rmcol in removeCols)
                {
                    tb.Columns.Remove(rmcol);
                }

                /*  tb.Columns.Remove("id");
                  tb.Columns.Remove("AssessmentEndTime");
                  tb.Columns.Remove("OrgCode");
                  tb.Columns.Remove("AssessmentStartTime");
                  tb.Columns.Remove("TotalRecordCount");
                  tb.Columns.Remove("MobileNumber");
                  tb.Columns.Remove("EmailId");
                  tb.Columns.Remove("WorkLocation");
                  tb.Columns.Remove("Position");
                  tb.Columns.Remove("Department");
                  tb.Columns.Remove("Division");
                  tb.Columns.Remove("Region");
                  tb.Columns.Remove("EmployeeState");
                  tb.Columns.Remove("DateofJoining");*/


                tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
                tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
                tb.Columns["UserId"].ColumnName = HeaderName.UserId;
                tb.Columns["UserName"].ColumnName = HeaderName.UserName;
                tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
                tb.Columns["NoOfAttempts"].ColumnName = HeaderName.NoOfAttempts;
                tb.Columns["AssessmentResult"].ColumnName = HeaderName.AssessmentResult;
                tb.Columns["AssessmentPercentage"].ColumnName = HeaderName.AssessmentPercentage;
                tb.Columns["AssessmentDate"].ColumnName = HeaderName.AssessmentDate;
                tb.Columns["AssessmentTimeDiffrance"].ColumnName = HeaderName.AssessmentTimeDiffrance;

            }
            else if (OrgCode.ToLower() == "lenexis" || OrgCode == "ent")
            {
                var removeCols = new string[] { "id", "OrgCode","TotalRecordCount", "MobileNumber", "EmailId","WorkLocation","Position",
                    "Department","Division","EmployeeState","DateofJoining","AssessmentStartTime","AssessmentEndTime","AssessmentTimeDiffrance",
                    "CircleZone","ReportingManager","Status","Level"
                };
                foreach (string rmcol in removeCols)
                {
                    tb.Columns.Remove(rmcol);
                }
                tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
                tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
                tb.Columns["UserId"].ColumnName = HeaderName.UserId;
                tb.Columns["UserName"].ColumnName = HeaderName.UserName;
                tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
                tb.Columns["NoOfAttempts"].ColumnName = HeaderName.NoOfAttempts;

                tb.Columns["MarksObtained"].ColumnName = HeaderName.MarksObtained;
                tb.Columns["TotalMarks"].ColumnName = HeaderName.TotalMarks;
                tb.Columns["AssessmentPercentage"].ColumnName = HeaderName.AssessmentPercentage;

                tb.Columns["AssessmentResult"].ColumnName = HeaderName.AssessmentResult;
                tb.Columns["AssessmentDate"].ColumnName = HeaderName.AssessmentDate;

                tb.Columns["RestaurantId"].ColumnName = HeaderName.RestaurantId;
                tb.Columns["CurrentRestaurant"].ColumnName = HeaderName.CurrentRestaurant;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
                tb.Columns["State"].ColumnName = HeaderName.State;
                tb.Columns["City"].ColumnName = HeaderName.City;
                tb.Columns["ClusterManager"].ColumnName = HeaderName.ClusterManager;
                tb.Columns["AreaManager"].ColumnName = HeaderName.AreaManager;
                tb.Columns["IsAdaptiveAssessment"].ColumnName = HeaderName.AdaptiveLearning;

            }
            else if (OrgCode.ToLower() != "sbig" && OrgCode.ToLower() != "sbiguat")
            {
                var removeCols = new string[] { "id", "AssessmentTimeDiffrance", "AssessmentEndTime", "OrgCode", "AssessmentStartTime",
                    "TotalRecordCount", "MobileNumber", "EmailId","WorkLocation","Position","Department","Division","Region","EmployeeState",
                    "CircleZone","ReportingManager","Status","Level","RestaurantId","CurrentRestaurant",
                    "State","City","ClusterManager","AreaManager"
                };
                foreach (string rmcol in removeCols)
                {
                    tb.Columns.Remove(rmcol);
                }


                tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
                tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
                tb.Columns["UserId"].ColumnName = HeaderName.UserId;
                tb.Columns["UserName"].ColumnName = HeaderName.UserName;
                tb.Columns["DateofJoining"].ColumnName = HeaderName.DateofJoining;
                tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
                tb.Columns["NoOfAttempts"].ColumnName = HeaderName.NoOfAttempts;
                tb.Columns["MarksObtained"].ColumnName = HeaderName.MarksObtained;
                tb.Columns["TotalMarks"].ColumnName = HeaderName.TotalMarks;
                tb.Columns["AssessmentResult"].ColumnName = HeaderName.AssessmentResult;
                tb.Columns["AssessmentPercentage"].ColumnName = HeaderName.AssessmentPercentage;
                tb.Columns["AssessmentDate"].ColumnName = HeaderName.AssessmentDate;
                tb.Columns["IsAdaptiveAssessment"].ColumnName = HeaderName.AdaptiveLearning;
            }
            else
            {
                var removeCols = new string[] { "id", "OrgCode","TotalRecordCount", "MobileNumber", "EmailId","WorkLocation","Position",
                    "Department","Division","Region","EmployeeState","DateofJoining","CourseCode"};
                foreach (string rmcol in removeCols)
                {
                    tb.Columns.Remove(rmcol);
                }

                tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
                tb.Columns["UserId"].ColumnName = HeaderName.UserId;
                tb.Columns["UserName"].ColumnName = HeaderName.UserName;
                tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
                tb.Columns["NoOfAttempts"].ColumnName = HeaderName.NoOfAttempts;
                tb.Columns["AssessmentResult"].ColumnName = HeaderName.AssessmentResult;
                tb.Columns["AssessmentPercentage"].ColumnName = HeaderName.AssessmentPercentage;
                tb.Columns["AssessmentDate"].ColumnName = HeaderName.AssessmentDate;

                tb.Columns["AssessmentTimeDiffrance"].ColumnName = HeaderName.AssessmentTimeDiffrance;
                tb.Columns["AssessmentStartTime"].ColumnName = HeaderName.AssessmentStartTime;
                tb.Columns["AssessmentEndTime"].ColumnName = HeaderName.AssessmentEndTime;
                tb.Columns["IsAdaptiveAssessment"].ColumnName = HeaderName.AdaptiveLearning;
            }

            return tb;
        }

        public DataTable ToDataTableTrainingProgramReport<APIRecommondedTrainingReport>(IEnumerable<APIRecommondedTrainingReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIRecommondedTrainingReport).Name);

            PropertyInfo[] props = typeof(APIRecommondedTrainingReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            var removeCols = new string[] { "id" };
            foreach (string rmcol in removeCols)
            {
                tb.Columns.Remove(rmcol);
            }


            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["JobRole"].ColumnName = HeaderName.JobRole;
            tb.Columns["Department"].ColumnName = HeaderName.Department;
            tb.Columns["Section"].ColumnName = HeaderName.Section;
            tb.Columns["Level"].ColumnName = HeaderName.Level;
            tb.Columns["Status"].ColumnName = HeaderName.Status;
            tb.Columns["TrainingProgram"].ColumnName = HeaderName.TrainingProgram;
            tb.Columns["Category"].ColumnName = HeaderName.Category;



            return tb;
        }

        public DataTable ToDataTableUserAssessmentSheetReport<APIUserAssessmentSheet>(IEnumerable<APIUserAssessmentSheet> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIUserAssessmentSheet).Name);

            PropertyInfo[] props = typeof(APIUserAssessmentSheet).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("id");
            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["QuestionText"].ColumnName = HeaderName.QuestionText;
            tb.Columns["SelectedAnswer"].ColumnName = HeaderName.SelectedAnswer;
            tb.Columns["CorrectAnswer"].ColumnName = HeaderName.CorrectAnswer;
            tb.Columns["ObtainedMarks"].ColumnName = HeaderName.MarksObtained;
            tb.Columns["TotalMarks"].ColumnName = HeaderName.Marks;
            tb.Columns["NoOfAttempts"].ColumnName = HeaderName.NoOfAttempts;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;

            if (OrgCode == "ent" || OrgCode.ToLower() == "lenexis")
            {
                tb.Columns["RestaurantId"].ColumnName = HeaderName.RestaurantId;
                tb.Columns["CurrentRestaurant"].ColumnName = HeaderName.CurrentRestaurant;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
                tb.Columns["State"].ColumnName = HeaderName.State;
                tb.Columns["City"].ColumnName = HeaderName.City;
                tb.Columns["ClusterManager"].ColumnName = HeaderName.ClusterManager;
                tb.Columns["AreaManager"].ColumnName = HeaderName.AreaManager;
            }

            return tb;
        }


        public DataTable ToDataTableUserManagerEvaluationReport<APIUserAssessmentSheet>(List<APIManagerEvaluation> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIUserAssessmentSheet).Name);

            PropertyInfo[] props = typeof(APIUserAssessmentSheet).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("id");
            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["AssessedBy"].ColumnName = HeaderName.AssessedBy;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["QuestionText"].ColumnName = HeaderName.QuestionText;
            tb.Columns["SelectedAnswer"].ColumnName = HeaderName.SelectedAnswer;
            tb.Columns["CorrectAnswer"].ColumnName = HeaderName.CorrectAnswer;
            tb.Columns["ObtainedMarks"].ColumnName = HeaderName.MarksObtained;
            tb.Columns["TotalMarks"].ColumnName = HeaderName.Marks;
            tb.Columns["NoOfAttempts"].ColumnName = HeaderName.NoOfAttempts;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;

            if (OrgCode == "ent" || OrgCode.ToLower() == "lenexis")
            {
                tb.Columns["RestaurantId"].ColumnName = HeaderName.RestaurantId;
                tb.Columns["CurrentRestaurant"].ColumnName = HeaderName.CurrentRestaurant;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
                tb.Columns["State"].ColumnName = HeaderName.State;
                tb.Columns["City"].ColumnName = HeaderName.City;
                tb.Columns["ClusterManager"].ColumnName = HeaderName.ClusterManager;
                tb.Columns["AreaManager"].ColumnName = HeaderName.AreaManager;
            }

            return tb;
        }

        public DataTable ToDataTableTestResultReport<APITestResultReport>(IEnumerable<APITestResultReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APITestResultReport).Name);

            PropertyInfo[] props = typeof(APITestResultReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["QuizTitle"].ColumnName = HeaderName.QuizTitle;
            tb.Columns["Mark"].ColumnName = HeaderName.Marks;
            tb.Columns["QuizResultStatus"].ColumnName = HeaderName.QuizResultStatus;

            return tb;
        }


        public DataTable ToDataTableSurveyReport<APISurveyModel>(IEnumerable<APISurveyModel> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APISurveyModel).Name);

            PropertyInfo[] props = typeof(APISurveyModel).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["UserID"].ColumnName = HeaderName.UserID;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["SurveyStatus"].ColumnName = HeaderName.SurveyStatus;
            tb.Columns["CompletionDate"].ColumnName = HeaderName.CompletionDate;
            tb.Columns["SurveyName"].ColumnName = HeaderName.SurveyName;

            return tb;
        }


        public DataTable ToDataTableUserSurveySheetReport<APIUserSurveySheetReport>(IEnumerable<APIUserSurveySheetReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIUserSurveySheetReport).Name);

            PropertyInfo[] props = typeof(APIUserSurveySheetReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            var removeCols = new string[] { "TotalRecordCount", "OptionText6", "OptionText7","OptionText8","OptionText9",
                    "OptionText10"};
            foreach (string rmcol in removeCols)
            {
                tb.Columns.Remove(rmcol);
            }



            tb.Columns["SurveySubject"].ColumnName = HeaderName.SurveyName;
            tb.Columns["Question"].ColumnName = HeaderName.Question;
            tb.Columns["Section"].ColumnName = HeaderName.SurveyType;
            tb.Columns["OptionText1"].ColumnName = HeaderName.OptionText1;
            tb.Columns["OptionText2"].ColumnName = HeaderName.OptionText2;
            tb.Columns["OptionText3"].ColumnName = HeaderName.OptionText3;
            tb.Columns["OptionText4"].ColumnName = HeaderName.OptionText4;
            tb.Columns["OptionText5"].ColumnName = HeaderName.OptionText5;
            tb.Columns["SelectedAnswer"].ColumnName = HeaderName.SelectedAnswer;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["SurvySubmitionDate"].ColumnName = HeaderName.SurvySubmitionDate;
            tb.Columns["SurveyResultStatus"].ColumnName = HeaderName.SurveyResultStatus;

            return tb;
        }

        public DataTable ToDataTableSurveyApplicabilityReport<APISurveyApplicabilityReport>(IEnumerable<APISurveyApplicabilityReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APISurveyApplicabilityReport).Name);

            PropertyInfo[] props = typeof(APISurveyApplicabilityReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }
            if (OrgCode != "ent" && OrgCode.ToLower() != "lenexis")
            {
                tb.Columns.Remove("RestaurantId");
                tb.Columns.Remove("CurrentRestaurant");
                tb.Columns.Remove("Region");
                tb.Columns.Remove("State");
                tb.Columns.Remove("City");
                tb.Columns.Remove("ClusterManager");
                tb.Columns.Remove("AreaManager");

            }
            if (OrgCode.ToLower() == "sbil")
            {
                tb.Columns.Remove("UserId");
                tb.Columns.Remove("UserName");
            }
            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["SurveyName"].ColumnName = HeaderName.SurveyName;
            tb.Columns["Parameter"].ColumnName = HeaderName.Parameter;
            tb.Columns["ParameterValue"].ColumnName = HeaderName.ParameterValue;

            return tb;
        }

        public DataTable ToDataTableUserWiseLoginDetailsReport<APIUserWiseLoginDetails>(IEnumerable<APIUserWiseLoginDetails> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIUserWiseLoginDetails).Name);

            PropertyInfo[] props = typeof(APIUserWiseLoginDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");


            if (OrgCode == "ent" || OrgCode.ToLower() == "lenexis")
            {
                tb.Columns["RestaurantId"].ColumnName = HeaderName.RestaurantId;
                tb.Columns["CurrentRestaurant"].ColumnName = HeaderName.CurrentRestaurant;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
                tb.Columns["State"].ColumnName = HeaderName.State;
                tb.Columns["City"].ColumnName = HeaderName.City;
                tb.Columns["ClusterManager"].ColumnName = HeaderName.ClusterManager;
                tb.Columns["AreaManager"].ColumnName = HeaderName.AreaManager;
            }
            else
            {
                tb.Columns["EmployeeId"].ColumnName = HeaderName.UserId;
                tb.Columns["UserName"].ColumnName = HeaderName.UserName;
                tb.Columns["LoginFrom"].ColumnName = HeaderName.LoginFrom;
                tb.Columns["LoginDate"].ColumnName = HeaderName.LoginDate;
                tb.Columns["LogInTime"].ColumnName = HeaderName.LoginTime;
                tb.Columns["LogOutTime"].ColumnName = HeaderName.LogOutTime;
                tb.Columns.Remove("RestaurantId");
                tb.Columns.Remove("CurrentRestaurant");
                tb.Columns.Remove("Region");
                tb.Columns.Remove("State");
                tb.Columns.Remove("City");
                tb.Columns.Remove("ClusterManager");
                tb.Columns.Remove("AreaManager");
            }

            return tb;
        }

        public DataTable ToDataTableUserLoginReport<APIUserLoginReport>(IEnumerable<APIUserLoginReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIUserLoginReport).Name);

            PropertyInfo[] props = typeof(APIUserLoginReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("LoginFrom");

            tb.Columns["EmployeeId"].ColumnName = HeaderName.EmployeeId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["UsageTimeInMinute"].ColumnName = HeaderName.UsageTimeInMinute;


            return tb;
        }

        public DataTable ToDataTableUserDatewiseLoginFromDetailsReport<APIUserDatewiseLoginFromDetails>(IEnumerable<APIUserDatewiseLoginFromDetails> items, string OrgCode)
        {

            var tb = new DataTable(typeof(APIUserDatewiseLoginFromDetails).Name);

            PropertyInfo[] props = typeof(APIUserDatewiseLoginFromDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            if (OrgCode == "ujjivan")
            {
                tb.Columns.Remove("TotalRecordCount");

                tb.Columns["UserId"].ColumnName = HeaderName.UserId;
                tb.Columns["UserName"].ColumnName = HeaderName.UserName;
                tb.Columns["MobileNumber"].ColumnName = HeaderName.Phonenumber;
                tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
                tb.Columns["IsActive"].ColumnName = HeaderName.IsActive;
                tb.Columns["IsLoggedInWeb"].ColumnName = HeaderName.IsLoggedInPlatform;
                tb.Columns["Date"].ColumnName = HeaderName.Date;
                tb.Columns["TotalSignIns"].ColumnName = HeaderName.TotalLogIn;
                tb.Columns["WebSignIns"].ColumnName = HeaderName.WebLogIn;
                tb.Columns["AppSignIns"].ColumnName = HeaderName.AppLogIn;
                tb.Columns["LastLoginDate"].ColumnName = HeaderName.LastLoginDate;
                tb.Columns["Area"].ColumnName = HeaderName.office_area;
                tb.Columns["City"].ColumnName = HeaderName.office_city;
                tb.Columns["State"].ColumnName = HeaderName.office_state;
                tb.Columns["Department"].ColumnName = HeaderName.department_name;
                tb.Columns["Designation"].ColumnName = HeaderName.designation_name;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
                tb.Columns["DateOfJoining"].ColumnName = HeaderName.DateOfJoining1;
            }
            else
            {
                var removeCols = new string[] { "TotalRecordCount", "TotalSignIns", "WebSignIns", "AppSignIns", "LastLoginDate", "Area", "City",
                    "State","Department","Designation","Region","DateOfJoining","MobileNumber","EmailId" };
                foreach (string rmcol in removeCols)
                {
                    tb.Columns.Remove(rmcol);
                }

                tb.Columns["UserId"].ColumnName = HeaderName.UserId;
                tb.Columns["UserName"].ColumnName = HeaderName.UserName;
                tb.Columns["IsActive"].ColumnName = HeaderName.IsActive;
                tb.Columns["IsLoggedInWeb"].ColumnName = HeaderName.IsLoggedInWeb;
                tb.Columns["Date"].ColumnName = HeaderName.Date;
            }

            return tb;
        }

        public DataTable ToDataTableUserLogin2Report<APIUserLoginModuleReport>(IEnumerable<APIUserLoginModuleReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIUserLoginModuleReport).Name);

            PropertyInfo[] props = typeof(APIUserLoginModuleReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["UserID"].ColumnName = HeaderName.UserID;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["IsLoggedInWeb"].ColumnName = HeaderName.IsLoggedInWeb;
            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["SubRegion"].ColumnName = HeaderName.SubRegion;
            tb.Columns["Month"].ColumnName = HeaderName.Month;
            tb.Columns["Year"].ColumnName = HeaderName.Year;

            return tb;
        }

        public DataTable ToDataTableUserTotalTimeSpentReport<APIUserTotalTimeSpent>(IEnumerable<APIUserTotalTimeSpent> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIUserTotalTimeSpent).Name);

            PropertyInfo[] props = typeof(APIUserTotalTimeSpent).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecord");
            tb.Columns.Remove("Id");

            tb.Columns["TotalTimeSpent"].ColumnName = HeaderName.TotalTimeSpent;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;

            return tb;
        }

        public DataTable ToDataTableUserNotLoginReport<APIUserNotLoginReport>(IEnumerable<APIUserNotLoginReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIUserNotLoginReport).Name);

            PropertyInfo[] props = typeof(APIUserNotLoginReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            var removeCols = new string[] { "TotalRecordCount", "FunctionName", "SubfunctionName", "BusinessUnitName", "DesignationName" };
            foreach (string rmcol in removeCols)
            {
                tb.Columns.Remove(rmcol);
            }

            tb.Columns["UserID"].ColumnName = HeaderName.UserID;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["OrgCode"].ColumnName = HeaderName.OrgCode;
            tb.Columns["EmailID"].ColumnName = HeaderName.EmailID;
            tb.Columns["MobileNumber"].ColumnName = HeaderName.MobileNo;
            tb.Columns["IsActive"].ColumnName = HeaderName.IsActive;


            return tb;
        }

        public DataTable ToDataTableDealerDetailLoginMonitoringReport<APIDealerDetailLoginMonitoringReport>(IEnumerable<APIDealerDetailLoginMonitoringReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIDealerDetailLoginMonitoringReport).Name);

            PropertyInfo[] props = typeof(APIDealerDetailLoginMonitoringReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["ControllingOffice"].ColumnName = HeaderName.Controllingoffice;
            tb.Columns["SalesOffice"].ColumnName = HeaderName.Salesoffice;
            tb.Columns["SalesArea"].ColumnName = HeaderName.SalesArea;
            tb.Columns["ReportingManger"].ColumnName = HeaderName.ReportingManager;
            tb.Columns["LastloginDate"].ColumnName = HeaderName.LastLogInDate;

            return tb;
        }

        public DataTable ToDataTableUserFeedbackReport<APIUserFeedbackReport>(IEnumerable<APIUserFeedbackReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIUserFeedbackReport).Name);

            PropertyInfo[] props = typeof(APIUserFeedbackReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            tb.Columns["UserID"].ColumnName = HeaderName.UserID;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["QuestionText"].ColumnName = HeaderName.QuestionText;
            tb.Columns["LearnerFeedback"].ColumnName = HeaderName.LearnerFeedback;
            if (OrgCode == "ent" || OrgCode.ToLower() == "lenexis")
            {
                tb.Columns["RestaurantId"].ColumnName = HeaderName.RestaurantId;
                tb.Columns["CurrentRestaurant"].ColumnName = HeaderName.CurrentRestaurant;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
                tb.Columns["State"].ColumnName = HeaderName.State;
                tb.Columns["City"].ColumnName = HeaderName.City;
                tb.Columns["ClusterManager"].ColumnName = HeaderName.ClusterManager;
                tb.Columns["AreaManager"].ColumnName = HeaderName.AreaManager;

                tb.Columns.Remove("DateOfFeedback");
                tb.Columns.Remove("schedulecode");
                tb.Columns.Remove("TrainerName");
                tb.Columns.Remove("PlaceName");
                tb.Columns.Remove("Venue");
                tb.Columns.Remove("TotalRecordCount");
                tb.Columns.Remove("Status");

            }
            if (OrgCode == "bandhan" || OrgCode == "canh" || OrgCode.ToLower() == "bandhanbank" || OrgCode.ToLower() == "evalueserve")
            {

                tb.Columns["DateOfFeedback"].ColumnName = HeaderName.FeedbackDate;
                tb.Columns["schedulecode"].ColumnName = HeaderName.ScheduleCode;
                tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerName;
                tb.Columns["PlaceName"].ColumnName = HeaderName.PlaceName;
                tb.Columns["Venue"].ColumnName = HeaderName.Venue;
                tb.Columns.Remove("RestaurantId");
                tb.Columns.Remove("CurrentRestaurant");
                tb.Columns.Remove("Region");
                tb.Columns.Remove("State");
                tb.Columns.Remove("City");
                tb.Columns.Remove("ClusterManager");
                tb.Columns.Remove("AreaManager");
                tb.Columns.Remove("TotalRecordCount");
                tb.Columns.Remove("Status");

            }
            if (OrgCode.ToLower() == "pplcpd")
            {
                tb.Columns["BU"].ColumnName = HeaderName.Bu;
                tb.Columns["ManagerEmployeeCode"].ColumnName = HeaderName.ManagerEmployeeCode;
                tb.Columns["ManagerName"].ColumnName = HeaderName.ManagerName;
                tb.Columns["ZoneName"].ColumnName = HeaderName.Zone;
                tb.Columns["HQ"].ColumnName = HeaderName.HeadQuarter;

                tb.Columns.Remove("RestaurantId");
                tb.Columns.Remove("CurrentRestaurant");
                tb.Columns.Remove("Region");
                tb.Columns.Remove("State");
                tb.Columns.Remove("City");
                tb.Columns.Remove("ClusterManager");
                tb.Columns.Remove("AreaManager");
                tb.Columns.Remove("DateOfFeedback");
                tb.Columns.Remove("schedulecode");
                tb.Columns.Remove("TrainerName");
                tb.Columns.Remove("PlaceName");
                tb.Columns.Remove("Venue");
                tb.Columns.Remove("TotalRecordCount");
                tb.Columns.Remove("Status");
            }
            else
            {
                tb.Columns.Remove("TotalRecordCount");
                tb.Columns.Remove("Status");
                tb.Columns.Remove("RestaurantId");
                tb.Columns.Remove("CurrentRestaurant");
                tb.Columns.Remove("Region");
                tb.Columns.Remove("State");
                tb.Columns.Remove("City");
                tb.Columns.Remove("ClusterManager");
                tb.Columns.Remove("AreaManager");
                tb.Columns.Remove("DateOfFeedback");
                tb.Columns.Remove("schedulecode");
                tb.Columns.Remove("TrainerName");
                tb.Columns.Remove("PlaceName");
                tb.Columns.Remove("Venue");

            }


            return tb;
        }

        public DataTable ToDataTableDealerReport<APIDealerReport>(IEnumerable<APIDealerReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIDealerReport).Name);

            PropertyInfo[] props = typeof(APIDealerReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["Controllingoffice"].ColumnName = HeaderName.Controllingoffice;
            tb.Columns["Salesoffice"].ColumnName = HeaderName.Salesoffice;
            tb.Columns["SalesArea"].ColumnName = HeaderName.SalesArea;
            tb.Columns["TotalcountofLoginuse"].ColumnName = HeaderName.TotalcountofLoginuse;
            tb.Columns["TotalcountofDealer"].ColumnName = HeaderName.TotalcountofDealer;

            return tb;
        }

        public DataTable ToDataTableFeedbackStatusReport<APIFeedbackStatusReport>(IEnumerable<APIFeedbackStatusReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIFeedbackStatusReport).Name);

            PropertyInfo[] props = typeof(APIFeedbackStatusReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("ModuleId");

            tb.Columns["EmployeeId"].ColumnName = HeaderName.UserID;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;

            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["DateCompleted"].ColumnName = HeaderName.DateCompleted;
            tb.Columns["Status"].ColumnName = HeaderName.Status;

            if (OrgCode == "ent" || OrgCode.ToLower() == "lenexis")
            {
                tb.Columns["RestaurantId"].ColumnName = HeaderName.RestaurantId;
                tb.Columns["CurrentRestaurant"].ColumnName = HeaderName.CurrentRestaurant;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
                tb.Columns["State"].ColumnName = HeaderName.State;
                tb.Columns["City"].ColumnName = HeaderName.City;
                tb.Columns["ClusterManager"].ColumnName = HeaderName.ClusterManager;
                tb.Columns["AreaManager"].ColumnName = HeaderName.AreaManager;

            }

            return tb;
        }

        public DataTable ToDataTableCourseRatingReport<APICourseRatingReport>(IEnumerable<APICourseRatingReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APICourseRatingReport).Name);

            PropertyInfo[] props = typeof(APICourseRatingReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("Count");
            tb.Columns.Remove("StartIndex");
            tb.Columns.Remove("PageSize");
            tb.Columns.Remove("CourseID");
            tb.Columns.Remove("ExportAs");

            tb.Columns["CourseName"].ColumnName = HeaderName.CourseName;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UseName"].ColumnName = HeaderName.UserName;
            tb.Columns["ReviewRating"].ColumnName = HeaderName.CourseRating;
            tb.Columns["ReviewText"].ColumnName = HeaderName.ReviewText;
            tb.Columns["ModifiedDate"].ColumnName = HeaderName.RatingDate;

            return tb;
        }

        public DataTable ToDataTableFeedbackAggregationReport<APIFeedbackAggregationReport>(IEnumerable<APIFeedbackAggregationReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIFeedbackAggregationReport).Name);

            PropertyInfo[] props = typeof(APIFeedbackAggregationReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("ModuleName");
            tb.Columns.Remove("ScheduleCode");

            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["QuestionText"].ColumnName = HeaderName.QuestionText;
            tb.Columns["OptionText1"].ColumnName = HeaderName.OptionText1;
            tb.Columns["OptionTextper1"].ColumnName = HeaderName.OptionText1Percentage;
            tb.Columns["OptionText2"].ColumnName = HeaderName.OptionText2;
            tb.Columns["OptionTextper2"].ColumnName = HeaderName.OptionText2Percentage;
            tb.Columns["OptionText3"].ColumnName = HeaderName.OptionText3;
            tb.Columns["OptionTextper3"].ColumnName = HeaderName.OptionText3Percentage;
            tb.Columns["OptionText4"].ColumnName = HeaderName.OptionText4;
            tb.Columns["OptionTextper4"].ColumnName = HeaderName.OptionText4Percentage;
            tb.Columns["OptionText5"].ColumnName = HeaderName.OptionText5;
            tb.Columns["OptionTextper5"].ColumnName = HeaderName.OptionText5Percentage;

            return tb;
        }

        public DataTable ToDataTableDataMigrationReport<APIDataMigrationReport>(IEnumerable<APIDataMigrationReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIDataMigrationReport).Name);

            PropertyInfo[] props = typeof(APIDataMigrationReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("Date");
            tb.Columns.Remove("ExportAs");
            tb.Columns.Remove("StackTrace");

            tb.Columns["UserID"].ColumnName = HeaderName.UserID;
            tb.Columns["Status"].ColumnName = HeaderName.Status;

            return tb;
        }

        public DataTable ToDataTableBatchWiseAttendanceDataViewReport<APIBatchWiseAttendanceDataView>(IEnumerable<APIBatchWiseAttendanceDataView> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIBatchWiseAttendanceDataView).Name);

            PropertyInfo[] props = typeof(APIBatchWiseAttendanceDataView).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("courseCode");
            tb.Columns.Remove("batchId");

            tb.Columns["userId"].ColumnName = HeaderName.UserID;
            tb.Columns["userName"].ColumnName = HeaderName.UserName;
            tb.Columns["batchCode"].ColumnName = HeaderName.BatchCode;
            tb.Columns["batchName"].ColumnName = HeaderName.BatchName;
            tb.Columns["courseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["scheduleCode"].ColumnName = HeaderName.ScheduleCode;
            tb.Columns["moduleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["topicName"].ColumnName = HeaderName.TopicName;
            tb.Columns["traninigDate"].ColumnName = HeaderName.TrainingDate;
            tb.Columns["attendanceDate"].ColumnName = HeaderName.AttendanceDate;
            tb.Columns["status"].ColumnName = HeaderName.AttendanceStatus;

            return tb;
        }

        public DataTable ToDataTableScheduleWiseAttendanceViewReport<APIScheduleWiseAttendanceView>(IEnumerable<APIScheduleWiseAttendanceView> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIScheduleWiseAttendanceView).Name);

            PropertyInfo[] props = typeof(APIScheduleWiseAttendanceView).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            if (OrgCode.ToLower() != "sbig" && OrgCode.ToLower() != "sbiguat")
            {

                tb.Columns.Remove("TotalRecordCount");
                tb.Columns.Remove("TopicName");

                tb.Columns["UserId"].ColumnName = HeaderName.UserId;
                tb.Columns["UserName"].ColumnName = HeaderName.UserName;
                tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
                tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
                tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
                tb.Columns["TraninigDate"].ColumnName = HeaderName.TrainingDate;
                tb.Columns["Status"].ColumnName = HeaderName.AttendanceStatus;
                tb.Columns["AttendanceDate"].ColumnName = HeaderName.AttendanceDate;
            }
            else
            {
                tb.Columns.Remove("TotalRecordCount");

                tb.Columns["UserId"].ColumnName = HeaderName.UserId;
                tb.Columns["UserName"].ColumnName = HeaderName.UserName;
                tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
                tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
                tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
                tb.Columns["TopicName"].ColumnName = HeaderName.TopicName;
                tb.Columns["TraninigDate"].ColumnName = HeaderName.TrainingDate;
                tb.Columns["Status"].ColumnName = HeaderName.AttendanceStatus;
                tb.Columns["AttendanceDate"].ColumnName = HeaderName.AttendanceDate;
            }


            return tb;
        }

        public DataTable ToDataTableAttendanceSummaryReport<ApiAttendanceSummuryReport>(IEnumerable<ApiAttendanceSummuryReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(ApiAttendanceSummuryReport).Name);

            PropertyInfo[] props = typeof(ApiAttendanceSummuryReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("ID");

            tb.Columns["Title"].ColumnName = HeaderName.CourseName;
            tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
            tb.Columns["StartDate"].ColumnName = HeaderName.StartDate;
            tb.Columns["EndDate"].ColumnName = HeaderName.EndDate;
            tb.Columns["PlaceName"].ColumnName = HeaderName.PlaceName;
            tb.Columns["TotalRegistrations"].ColumnName = HeaderName.TotalRegistrations;
            tb.Columns["TotalPresent"].ColumnName = HeaderName.TotalPresent;
            tb.Columns["TotalAbsent"].ColumnName = HeaderName.TotalAbsent;
            tb.Columns["AttendancePercentage"].ColumnName = HeaderName.AttendancePercentage;
            tb.Columns["AttendanceStatus"].ColumnName = HeaderName.AttendanceStatus;
            tb.Columns["AttendanceDate"].ColumnName = HeaderName.AttendanceDate;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;

            return tb;
        }

        public DataTable ToDataTableInternalTrainersScheduleReport<APIInternalTrainersScheduleReport>(IEnumerable<APIInternalTrainersScheduleReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIInternalTrainersScheduleReport).Name);

            PropertyInfo[] props = typeof(APIInternalTrainersScheduleReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType);
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerName;
            tb.Columns["PlaceName"].ColumnName = HeaderName.PlaceName;
            tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
            tb.Columns["RegionName"].ColumnName = HeaderName.Region;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["NumberofSchedules"].ColumnName = HeaderName.NumberofSchedules;

            return tb;
        }

        public DataTable ToDataTableTrainerWiseCourseDetailsReport<APITrainerWiseCourseDetails>(IEnumerable<APITrainerWiseCourseDetails> items, string OrgCode)
        {


            var tb = new DataTable(typeof(APITrainerWiseCourseDetails).Name);

            PropertyInfo[] props = typeof(APITrainerWiseCourseDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("CourseId");
            tb.Columns.Remove("ScheduleID");
            tb.Columns.Remove("Title");
            tb.Columns.Remove("DurationOfModule");

            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
            tb.Columns["CourseName"].ColumnName = HeaderName.CourseName;
            tb.Columns["PlaceName"].ColumnName = HeaderName.PlaceName;
            tb.Columns["SkipLevelManager"].ColumnName = HeaderName.SkipLevelManager;
            tb.Columns["DurationInMinutes"].ColumnName = HeaderName.DurationInMinutes;
            return tb;
        }

        public DataTable ToDataTableTopicAttendanceReport<APITopicAttendanceReport>(IEnumerable<APITopicAttendanceReport> items, string OrgCode)
        {

            var tb = new DataTable(typeof(APITopicAttendanceReport).Name);

            PropertyInfo[] props = typeof(APITopicAttendanceReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            var removeCols = new string[] { "TotalRecordCount", "ConsultantTrainer", "ScheduleCode", "StartTime", "EndTime" };
            foreach (string rmcol in removeCols)
            {
                tb.Columns.Remove(rmcol);
            }

            tb.Columns["ProgramNo"].ColumnName = HeaderName.ProgramNo;
            tb.Columns["ProgramName"].ColumnName = HeaderName.ProgramName;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["TopicName"].ColumnName = HeaderName.TopicName;
            tb.Columns["EmpNo"].ColumnName = HeaderName.EmpNo;
            tb.Columns["EmployeeName"].ColumnName = HeaderName.EmployeeName;
            tb.Columns["StartDate"].ColumnName = HeaderName.StartDate;
            tb.Columns["EndDate"].ColumnName = HeaderName.EndDate;
            tb.Columns["CreatedBy"].ColumnName = HeaderName.CreatedBy;
            tb.Columns["ConductedBy"].ColumnName = HeaderName.ConductedBy;
            tb.Columns["AttendanceDate"].ColumnName = HeaderName.AttendanceDate;
            tb.Columns["Status"].ColumnName = HeaderName.Status;




            return tb;
        }

        public DataTable ToDataTableTopicFeedbackReport<APITopicFeedbackReport>(IEnumerable<APITopicFeedbackReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APITopicFeedbackReport).Name);

            PropertyInfo[] props = typeof(APITopicFeedbackReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["UserID"].ColumnName = HeaderName.UserID;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
            tb.Columns["TopicName"].ColumnName = HeaderName.TopicName;
            tb.Columns["QuestionText"].ColumnName = HeaderName.QuestionText;
            tb.Columns["LearnerFeedback"].ColumnName = HeaderName.LearnerFeedback;
            tb.Columns["FeedbackSubmittedDate"].ColumnName = HeaderName.FeedbackSubmittedDate;


            return tb;
        }

        public DataTable ToDataTableAttendanceDetailsReport<APIAttendanceDetailsReport>(IEnumerable<APIAttendanceDetailsReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIAttendanceDetailsReport).Name);

            PropertyInfo[] props = typeof(APIAttendanceDetailsReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
            tb.Columns["AttendanceDate"].ColumnName = HeaderName.AttendanceDate;
            tb.Columns["AttendanceByOTP"].ColumnName = HeaderName.AttendanceByOTP;
            tb.Columns["AttendanceByQRCode"].ColumnName = HeaderName.AttendanceByQRCode;
            tb.Columns["TotalAttendance"].ColumnName = HeaderName.TotalAttendance;


            return tb;
        }

        public DataTable ToDataTableILTTrainingReport<APIILTTrainingReport>(IEnumerable<APIILTTrainingReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIILTTrainingReport).Name);

            PropertyInfo[] props = typeof(APIILTTrainingReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            tb.Columns["TrainerEmpCode1"].ColumnName = HeaderName.TrainerEmpCode1;
            tb.Columns["TrainerName1"].ColumnName = HeaderName.TrainerName1;
            tb.Columns["TrainingStrtDate"].ColumnName = HeaderName.TrainingStartDate_;
            tb.Columns["TrainingEndDate"].ColumnName = HeaderName.TrainingEndDate;
            tb.Columns["Attendance"].ColumnName = HeaderName.Attendance;
            tb.Columns["AttendanceDate"].ColumnName = HeaderName.AttendanceDate;
            tb.Columns["AttendiesEMPID"].ColumnName = HeaderName.AttendiesEMPID;
            tb.Columns["NameOfParticipant"].ColumnName = HeaderName.NameOfParticipant;
            tb.Columns["ActivityType"].ColumnName = HeaderName.ActivityType;
            tb.Columns["ActivityName"].ColumnName = HeaderName.ActivityName;
            tb.Columns["TrainingLocation"].ColumnName = HeaderName.TrainingLocation;
            tb.Columns["ChannelPartner"].ColumnName = HeaderName.ChannelPartner;

            return tb;
        }

        public DataTable ToDataTableRewardPointsSummeryReport<APIRewardPointsSummeryReport>(IEnumerable<APIRewardPointsSummeryReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIRewardPointsSummeryReport).Name);

            PropertyInfo[] props = typeof(APIRewardPointsSummeryReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("Category");
            tb.Columns.Remove("Description");

            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["Points"].ColumnName = HeaderName.Points;

            if (OrgCode == "ent" || OrgCode.ToLower() == "lenexis")
            {
                tb.Columns["RestaurantId"].ColumnName = HeaderName.RestaurantId;
                tb.Columns["CurrentRestaurant"].ColumnName = HeaderName.CurrentRestaurant;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
                tb.Columns["State"].ColumnName = HeaderName.State;
                tb.Columns["City"].ColumnName = HeaderName.City;
                tb.Columns["ClusterManager"].ColumnName = HeaderName.ClusterManager;
                tb.Columns["AreaManager"].ColumnName = HeaderName.AreaManager;

                tb.Columns.Remove("CircleZone");
                tb.Columns.Remove("Department");
                tb.Columns.Remove("Division");
                tb.Columns.Remove("ReportingManager");
            }
            return tb;
        }

        public DataTable ToDataTableRewardPointsDetailsReport<APIRewardPointsDetailsReport>(IEnumerable<APIRewardPointsDetailsReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIRewardPointsDetailsReport).Name);

            PropertyInfo[] props = typeof(APIRewardPointsDetailsReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["Point"].ColumnName = HeaderName.Point;
            tb.Columns["Category"].ColumnName = HeaderName.Category;
            tb.Columns["Description"].ColumnName = HeaderName.Description;
            tb.Columns["RewardPointDate"].ColumnName = HeaderName.RewardPointDate;

            if (OrgCode == "ent" || OrgCode.ToLower() == "lenexis")
            {
                tb.Columns.Remove("CircleZone");
                tb.Columns.Remove("Department");
                tb.Columns.Remove("Division");
                tb.Columns.Remove("ReportingManager");
            }

            return tb;
        }

        public DataTable ToDataTableRegistrationReport<APIRegistrationReport>(IEnumerable<APIRegistrationReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIRegistrationReport).Name);

            PropertyInfo[] props = typeof(APIRegistrationReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["CourseName"].ColumnName = HeaderName.CourseName;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
            tb.Columns["Availability"].ColumnName = HeaderName.Availability;
            tb.Columns["ReasonforRejection"].ColumnName = HeaderName.ReasonforRejection;

            return tb;
        }

        public DataTable ToDataTableUserLoginStatisticReport<APIUserLoginStatistic>(IEnumerable<APIUserLoginStatistic> items)
        {
            var tb = new DataTable(typeof(APIUserLoginStatistic).Name);

            PropertyInfo[] props = typeof(APIUserLoginStatistic).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["Date"].ColumnName = HeaderName.Date;
            tb.Columns["IsLoggedInWeb"].ColumnName = HeaderName.IsLoggedInWeb;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["Username"].ColumnName = HeaderName.UserName;
            tb.Columns["LoggedInTime"].ColumnName = HeaderName.LoggedInTime;
            tb.Columns["LogOutTime"].ColumnName = HeaderName.LogOutTime;

            return tb;
        }

        public DataTable ToDataTableUserwiseCourseStatusReportResult<APIUserwiseCourseStatusReportResult>(IEnumerable<APIUserwiseCourseStatusReportResult> items)
        {
            var tb = new DataTable(typeof(APIUserwiseCourseStatusReportResult).Name);

            PropertyInfo[] props = typeof(APIUserwiseCourseStatusReportResult).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["ReportsToUserId"].ColumnName = HeaderName.ReportsToUserId;
            tb.Columns["ReportsToUserName"].ColumnName = HeaderName.ReportsToUserName;
            tb.Columns["UserCategory"].ColumnName = HeaderName.UserCategory;
            tb.Columns["IsActive"].ColumnName = HeaderName.Status;
            tb.Columns["SalesOfficeName"].ColumnName = HeaderName.SalesOffName;
            tb.Columns["SalesArea"].ColumnName = HeaderName.SalesArea;
            tb.Columns["ControllingOffice"].ColumnName = HeaderName.Controllingoffice;
            tb.Columns["State"].ColumnName = HeaderName.State;
            tb.Columns["Inprogress"].ColumnName = HeaderName.Inprogress;
            tb.Columns["Completed"].ColumnName = HeaderName.Completed;
            tb.Columns["Applicable"].ColumnName = HeaderName.Applicable;
            tb.Columns["LastLoggedInDate"].ColumnName = HeaderName.LastLoginDate;

            return tb;
        }

        public DataTable ToDataTableExportTcnsRetrainingReport<ApiExportTcnsRetrainingReport>(IEnumerable<ApiExportTcnsRetrainingReport> items, string orgCode)
        {
            var tb = new DataTable(typeof(ApiExportTcnsRetrainingReport).Name);

            PropertyInfo[] props = typeof(ApiExportTcnsRetrainingReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["CourseStartDate"].ColumnName = HeaderName.CourseStartDate;
            tb.Columns["CourseCompletionDate"].ColumnName = HeaderName.CourseCompletionDate;
            tb.Columns["CourseStatus"].ColumnName = HeaderName.CourseStatus;
            tb.Columns["RetrainingDate"].ColumnName = HeaderName.RetrainingDate;
            tb.Columns["UserStatus"].ColumnName = HeaderName.UserStatus;
            if (orgCode.ToLower().Contains("tcns"))
            {
                tb.Columns["Department"].ColumnName = HeaderName.Department;
                tb.Columns["Designation"].ColumnName = HeaderName.Designation;
                tb.Columns["FunctionCode"].ColumnName = HeaderName.FunctionCode;
                tb.Columns["Group"].ColumnName = HeaderName.Group;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
            }
            else
            {
                tb.Columns.Remove("Department");
                tb.Columns.Remove("Designation");
                tb.Columns.Remove("FunctionCode");
                tb.Columns.Remove("Group");
                tb.Columns.Remove("Region");
            }
            tb.Columns["Score"].ColumnName = HeaderName.Score;

            return tb;
        }

        public DataTable ToDataTableCourseCompletionReport<APICourseCompletionReport>(IEnumerable<APICourseCompletionReport> items, string OrgCode, string SHOW_CONFCOLUMNS_INREPORT, List<UserSettings> CourseCompletionHeaderss)
        {
            var tb = new DataTable(typeof(APICourseCompletionReport).Name);

            PropertyInfo[] props = typeof(APICourseCompletionReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }
            if (SHOW_CONFCOLUMNS_INREPORT == "No")
            {
                tb.Columns.Remove("FeedbackStatus");
                tb.Columns.Remove("AssessmentStatus");
                tb.Columns.Remove("AssessmentDate");
                tb.Columns.Remove("IsAssessment");
                tb.Columns.Remove("IsFeedback");
                tb.Columns.Remove("Business");
                tb.Columns.Remove("Group");
                tb.Columns.Remove("Location");
                tb.Columns.Remove("Id");
                tb.Columns.Remove("ConfigurationColumn1");
                tb.Columns.Remove("ConfigurationColumn2");
                tb.Columns.Remove("ConfigurationColumn3");
                tb.Columns.Remove("ConfigurationColumn4");
                tb.Columns.Remove("ConfigurationColumn5");
                tb.Columns.Remove("ConfigurationColumn6");
                tb.Columns.Remove("ConfigurationColumn7");
                tb.Columns.Remove("ConfigurationColumn8");
                tb.Columns.Remove("ConfigurationColumn9");
                tb.Columns.Remove("ConfigurationColumn10");
                tb.Columns.Remove("ConfigurationColumn11");
                tb.Columns.Remove("ConfigurationColumn12");
                tb.Columns.Remove("Percentage");
                tb.Columns.Remove("CourseProgress");

                tb.Columns["Area"].ColumnName = HeaderName.Region;
            }
            else
            {
                tb.Columns.Remove("IsAssessment");
                tb.Columns.Remove("IsFeedback");
                tb.Columns.Remove("FeedbackStatus");
                tb.Columns.Remove("AssessmentStatus");
                tb.Columns.Remove("AssessmentDate");
                tb.Columns.Remove("Id");
                tb.Columns.Remove("Percentage");
                foreach (UserSettings aPICourseExportReport in CourseCompletionHeaderss)
                {
                    switch (aPICourseExportReport.ConfiguredColumnName)
                    {
                        case "Business":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["Business"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "Group":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["Group"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "Area":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["Area"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "Location":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["Location"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn1":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn1"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn2":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn2"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn3":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn3"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn4":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn4"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn5":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn5"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn6":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn6"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn7":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn7"].ColumnName = aPICourseExportReport.ChangedColumnName;

                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn8":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn8"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn9":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn9"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn10":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn10"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn11":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn11"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                        case "ConfigurationColumn12":
                            if (aPICourseExportReport.IsShowInReport == true && aPICourseExportReport.IsConfigured == true && aPICourseExportReport.IsDeleted == 0)
                            {
                                tb.Columns["ConfigurationColumn12"].ColumnName = aPICourseExportReport.ChangedColumnName;
                            }
                            else
                            {
                                tb.Columns.Remove(aPICourseExportReport.ConfiguredColumnName);
                            }
                            break;
                    }

                }
            }

            tb.Columns.Remove("UserEmailId");
            tb.Columns.Remove("MobileNumber");
            tb.Columns.Remove("DateOfJoining");
            tb.Columns.Remove("DateOfBirth");
            tb.Columns.Remove("Gender");
            if (tb.Columns["ManagerId"] != null)
            {
                tb.Columns.Remove("ManagerId");
            }
            if (tb.Columns["ManagerName"] != null)
            {
                tb.Columns.Remove("ManagerName");
            }
            if (tb.Columns["ManagerEmailId"] != null)
            {
                tb.Columns.Remove("ManagerEmailId");
            }


            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["CourseStartDate"].ColumnName = HeaderName.CourseStartDate;
            tb.Columns["CourseCompletionDate"].ColumnName = HeaderName.CourseCompletionDate;
            tb.Columns["CourseStatus"].ColumnName = HeaderName.CourseStatus;
            tb.Columns["IsRetraining"].ColumnName = HeaderName.IsRetraining;
            tb.Columns["LastActivityDate"].ColumnName = HeaderName.LastActivityDate;
            tb.Columns["UserStatus"].ColumnName = HeaderName.UserStatus;

            if (OrgCode.ToLower() != "sbil")
            {
                tb.Columns["UserDuration"].ColumnName = HeaderName.UserDuration;
            }
            else
            {
                tb.Columns.Remove("UserDuration");
            }



            return tb;
        }

        public DataTable ToDataTableCourseWiseCompletionReport<APICourseWiseCompletionReport>(IEnumerable<APICourseWiseCompletionReport> items, string OrgCode, List<APIUserSetting> userSetting)
        {
            var tb = new DataTable(typeof(APICourseWiseCompletionReport).Name);

            PropertyInfo[] props = typeof(APICourseWiseCompletionReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["CourseStartDate"].ColumnName = HeaderName.CourseStartDate;
            tb.Columns["CourseCompletionDate"].ColumnName = HeaderName.CourseCompletionDate;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["PreAssessmentResult"].ColumnName = HeaderName.PreAssessmentResult;
            tb.Columns["PreAssessmentStatus"].ColumnName = HeaderName.PreAssessmentStatus;
            tb.Columns["PreAssessmentPercentage"].ColumnName = HeaderName.PreAssessmentPercentage;
            tb.Columns["AssessmentDate"].ColumnName = HeaderName.AssessmentDate;
            tb.Columns["AssessmentPercentage"].ColumnName = HeaderName.AssessmentPercentage;
            tb.Columns["AssessmentResult"].ColumnName = HeaderName.AssessmentResult;
            tb.Columns["NoOfAttempts"].ColumnName = HeaderName.NoOfAttempts;
            tb.Columns["FeedbackDate"].ColumnName = HeaderName.FeedbackDate;
            tb.Columns["FeedbackStatus"].ColumnName = HeaderName.FeedbackStatus;
            tb.Columns["ContentCompletionDate"].ColumnName = HeaderName.ContentCompletionDate;
            tb.Columns["CourseStatus"].ColumnName = HeaderName.CourseStatus;
            tb.Columns["ModuleStatus"].ColumnName = HeaderName.ModuleStatus;

            tb.Columns["Business"].ColumnName = userSetting[0].ChangedColumnName;
            tb.Columns["Group"].ColumnName = userSetting[1].ChangedColumnName;
            tb.Columns["Area"].ColumnName = userSetting[2].ChangedColumnName;
            tb.Columns["Location"].ColumnName = userSetting[3].ChangedColumnName;
            tb.Columns["Configurationcolumn1"].ColumnName = userSetting[4].ChangedColumnName;
            tb.Columns["Configurationcolumn2"].ColumnName = userSetting[5].ChangedColumnName;
            tb.Columns["Configurationcolumn3"].ColumnName = userSetting[6].ChangedColumnName;
            tb.Columns["Configurationcolumn4"].ColumnName = userSetting[7].ChangedColumnName;
            tb.Columns["Configurationcolumn5"].ColumnName = userSetting[8].ChangedColumnName;
            tb.Columns["Configurationcolumn6"].ColumnName = userSetting[9].ChangedColumnName;
            tb.Columns["Configurationcolumn7"].ColumnName = userSetting[10].ChangedColumnName;
            tb.Columns["Configurationcolumn8"].ColumnName = userSetting[11].ChangedColumnName;
            tb.Columns["Configurationcolumn9"].ColumnName = userSetting[12].ChangedColumnName;
            tb.Columns["Configurationcolumn10"].ColumnName = userSetting[13].ChangedColumnName;
            tb.Columns["Configurationcolumn11"].ColumnName = userSetting[14].ChangedColumnName;
            tb.Columns["Configurationcolumn12"].ColumnName = userSetting[15].ChangedColumnName;
            tb.Columns["Configurationcolumn13"].ColumnName = userSetting[16].ChangedColumnName;
            tb.Columns["Configurationcolumn14"].ColumnName = userSetting[17].ChangedColumnName;
            tb.Columns["Configurationcolumn15"].ColumnName = userSetting[18].ChangedColumnName;

            if (OrgCode.ToLower() != "hdfc")
            {
                if (tb.Columns["UserDuration"] != null)
                {
                    tb.Columns["UserDuration"].ColumnName = HeaderName.UserDuration;
                }
            }
            else
            {
                tb.Columns.Remove("UserDuration");
            }
            if (OrgCode.ToLower() == "cap" || OrgCode.ToLower() == "ent")
            {
                tb.Columns["CourseSubCategory"].ColumnName = HeaderName.CourseSubCategory;
                tb.Columns["CourseCategory"].ColumnName = HeaderName.CourseCategory;
            }
            else
            {
                tb.Columns.Remove("CourseSubCategory");
                tb.Columns.Remove("CourseCategory");
            }
            if (OrgCode.ToLower() == "ent" || OrgCode.ToLower() == "singlife" || OrgCode.ToLower() == "aviva")
            {
                if (tb.Columns["ModuleDuration"] != null)
                {
                    tb.Columns["ModuleDuration"].ColumnName = "Module Duration (Minutes)";
                }
                if (tb.Columns["CourseDuration"] != null)
                {
                    tb.Columns["CourseDuration"].ColumnName = "Course Duration (Minutes)";
                }
            }

            tb.Columns.Remove("AssessmentStatus");
            tb.Columns.Remove("IsAssessment");
            tb.Columns.Remove("IsFeedback");
            tb.Columns.Remove("MarksObtained");
            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("UserStatus");

            int count = userSetting.Count;
            for (int i = 0; i < count; i++)
            {
                if (userSetting[i].IsShowInReport == false)
                    tb.Columns.Remove(userSetting[i].ChangedColumnName);
            }

            return tb;
        }

        public DataTable ToDataTableUserLearningReport<APIUserLearningReport>(IEnumerable<APIUserLearningReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APIUserLearningReport).Name);

            PropertyInfo[] props = typeof(APIUserLearningReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            tb.Columns["UserID"].ColumnName = HeaderName.UserID;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["CourseStartDate"].ColumnName = HeaderName.CourseStartDate;
            tb.Columns["CourseCompletionDate"].ColumnName = HeaderName.CourseCompletionDate;
            tb.Columns["Status"].ColumnName = HeaderName.CourseStatus;

            if (OrgCode == "ent" || OrgCode.ToLower() == "lenexis")
            {
                tb.Columns["RestaurantId"].ColumnName = HeaderName.RestaurantId;
                tb.Columns["CurrentRestaurant"].ColumnName = HeaderName.CurrentRestaurant;
                tb.Columns["Region"].ColumnName = HeaderName.Region;
                tb.Columns["State"].ColumnName = HeaderName.State;
                tb.Columns["City"].ColumnName = HeaderName.City;
                tb.Columns["ClusterManager"].ColumnName = HeaderName.ClusterManager;
                tb.Columns["AreaManager"].ColumnName = HeaderName.AreaManager;
                tb.Columns["TotalMarks"].ColumnName = HeaderName.MarksObtained;
                tb.Columns["MarksObtained"].ColumnName = HeaderName.TotalMarks;
                tb.Columns["AssessmentPercentage"].ColumnName = HeaderName.Percentage;
            }
            if (OrgCode != "ent" && OrgCode.ToLower() != "lenexis")
            {
                tb.Columns.Remove("RestaurantId");
                tb.Columns.Remove("CurrentRestaurant");
                tb.Columns.Remove("Region");
                tb.Columns.Remove("State");
                tb.Columns.Remove("City");
                tb.Columns.Remove("ClusterManager");
                tb.Columns.Remove("AreaManager");
                tb.Columns.Remove("TotalMarks");
                tb.Columns.Remove("MarksObtained");
                tb.Columns.Remove("AssessmentPercentage");
            }

            tb.Columns.Remove("TotalRecordCount");


            return tb;
        }

        public DataTable ToDataTableSchedulerReport<APISchedulerReport>(IEnumerable<APISchedulerReport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(APISchedulerReport).Name);

            PropertyInfo[] props = typeof(APISchedulerReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["CourseCategory"].ColumnName = HeaderName.CourseCategory;
            tb.Columns["CourseType"].ColumnName = HeaderName.CourseType;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["CourseStartDate"].ColumnName = HeaderName.CourseStartDate;
            tb.Columns["ContentCompletionDate"].ColumnName = HeaderName.ContentCompletionDate;
            tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
            tb.Columns["IsAssessmentAvailable"].ColumnName = HeaderName.IsAssessmentAvailable;
            tb.Columns["AssessmentStatus"].ColumnName = HeaderName.AssessmentStatus;
            tb.Columns["AssessmentDate"].ColumnName = HeaderName.AssessmentDate;
            tb.Columns["AssessmentResult"].ColumnName = HeaderName.AssessmentResult;
            tb.Columns["AssessmentPercentage"].ColumnName = HeaderName.AssessmentPercentage;
            tb.Columns["IsFeedbackAvailable"].ColumnName = HeaderName.IsFeedbackAvailable;
            tb.Columns["FeedbackStatus"].ColumnName = HeaderName.FeedbackStatus;
            tb.Columns["FeedbackDate"].ColumnName = HeaderName.FeedbackDate;
            tb.Columns["CourseCompletionDate"].ColumnName = HeaderName.CourseCompletionDate;
            tb.Columns["CourseStatus"].ColumnName = HeaderName.CourseStatus;
            tb.Columns["StartTime"].ColumnName = HeaderName.StartTime;
            tb.Columns["EndTime"].ColumnName = HeaderName.EndTime;
            tb.Columns["PlaceName"].ColumnName = HeaderName.PlaceName;
            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerName;
            if (OrgCode == "ail")
            {
                tb.Columns["CourseDuration"].ColumnName = HeaderName.ManHours;
            }
            else
            {
                tb.Columns["CourseDuration"].ColumnName = HeaderName.CourseDuration;
            }

            tb.Columns["UserRole"].ColumnName = HeaderName.UserRole;
            tb.Columns["NoOfAttempts"].ColumnName = HeaderName.NoOfAttempts;
            //tb.Columns["UserDuration"].ColumnName = HeaderName.UserDuration;
            tb.Columns["Section"].ColumnName = HeaderName.Section;

            if (OrgCode.ToLower() == "ujjivan" || OrgCode.ToLower() == "sbil")
            {
                tb.Columns.Remove("CourseSubCategory");
            }

            var removeCols = new string[] { "ModuleID","Percentage", "Department","IsAssessment","IsFeedback",
                    "UserStatus","Business","Group","Area","Location","EmailId","Function","Grade","Level","JobTitle","ReportingManager"};
            foreach (string rmcol in removeCols)
            {
                tb.Columns.Remove(rmcol);
            }

            tb.Columns.Remove("UserDuration");
            tb.Columns.Remove("ConfigurationColumn1");
            tb.Columns.Remove("ConfigurationColumn2");
            tb.Columns.Remove("ConfigurationColumn3");
            tb.Columns.Remove("ConfigurationColumn4");
            tb.Columns.Remove("ConfigurationColumn5");
            tb.Columns.Remove("ConfigurationColumn6");
            tb.Columns.Remove("ConfigurationColumn7");
            tb.Columns.Remove("ConfigurationColumn8");
            tb.Columns.Remove("ConfigurationColumn9");
            tb.Columns.Remove("ConfigurationColumn10");
            tb.Columns.Remove("ConfigurationColumn11");
            tb.Columns.Remove("ConfigurationColumn12");

            if (OrgCode.ToLower() != "sbil")
            {
                tb.Columns.Remove("Region");
            }

            return tb;
        }

        public DataTable ToDataTableAllCoursesSummaryReport<APIAllCoursesSummaryReport>(IEnumerable<APIAllCoursesSummaryReport> items)
        {
            var tb = new DataTable(typeof(APIAllCoursesSummaryReport).Name);

            PropertyInfo[] props = typeof(APIAllCoursesSummaryReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["CourseName"].ColumnName = HeaderName.CourseName;
            tb.Columns["TotalModules"].ColumnName = HeaderName.TotalModules;
            tb.Columns["UserAssigned"].ColumnName = HeaderName.UsersAssigneD;
            tb.Columns["Completed"].ColumnName = HeaderName.Completed;
            tb.Columns["InProgress"].ColumnName = HeaderName.InProgress;
            tb.Columns["NotStarted"].ColumnName = HeaderName.NotStarted;
            tb.Columns["CompletionRatio"].ColumnName = HeaderName.CompletionRatio;
            tb.Columns["AssessmentAvailable"].ColumnName = HeaderName.AssessmentAvailable;
            tb.Columns["UsersPassed"].ColumnName = HeaderName.UsersPassed;
            tb.Columns["UsersFailed"].ColumnName = HeaderName.UsersFailed;
            tb.Columns["UsersNotStarted"].ColumnName = HeaderName.UsersNotStarted;
            tb.Columns["PassRatio"].ColumnName = HeaderName.PassRatio;
            tb.Columns["CertificateAvailable"].ColumnName = HeaderName.CertificateAvailable;
            tb.Columns["CertificateEarned"].ColumnName = HeaderName.CertificateEarned;

            return tb;
        }

        public DataTable ToDataTableAllUserTimeSpentReport<APIAllUserTimeSpentReport>(IEnumerable<APIAllUserTimeSpentReport> items)
        {
            var tb = new DataTable(typeof(APIAllUserTimeSpentReport).Name);

            PropertyInfo[] props = typeof(APIAllUserTimeSpentReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["MobileNumber"].ColumnName = HeaderName.MobileNo;
            tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
            tb.Columns["UserStatus"].ColumnName = HeaderName.UserStatus;
            tb.Columns["FirstLoginDate"].ColumnName = HeaderName.FirstLoginDate;
            tb.Columns["FirstContentAccess"].ColumnName = HeaderName.FirstContentAccess;
            tb.Columns["LastContentAccess"].ColumnName = HeaderName.LastContentAccess;
            tb.Columns["CourseAssigned"].ColumnName = HeaderName.CourseAssigned;
            tb.Columns["CourseCompleted"].ColumnName = HeaderName.CourseCompleted;
            tb.Columns["Completion"].ColumnName = HeaderName.Completion;
            tb.Columns["LastActiveDate"].ColumnName = HeaderName.LastActiveDate;
            tb.Columns["DeviceMostActive"].ColumnName = HeaderName.DeviceMostActive;
            tb.Columns["TotalSignIns"].ColumnName = HeaderName.TotalSignIns;
            tb.Columns["WebSignIns"].ColumnName = HeaderName.WebSignIns;
            tb.Columns["AppSignIns"].ColumnName = HeaderName.AppSignIn;
            tb.Columns["TotalTimeSpentInMinutes"].ColumnName = HeaderName.TotalTimeSpentIn;
            tb.Columns["WebTimeSpentInMinutes"].ColumnName = HeaderName.WebTimeSpent;
            tb.Columns["AppTimeSpentInMinutes"].ColumnName = HeaderName.AppTimeSpent;
            tb.Columns["Department"].ColumnName = HeaderName.Department;
            tb.Columns["SubDepartment"].ColumnName = HeaderName.SubDepartment;
            tb.Columns["Zone"].ColumnName = HeaderName.Zone;
            tb.Columns["City"].ColumnName = HeaderName.City;
            tb.Columns["BusinessDesignation"].ColumnName = HeaderName.BusinessDesignation;
            tb.Columns["DateOfJoining"].ColumnName = HeaderName.DateOfJoining;

            return tb;
        }

        public DataTable ToDataTableWorkDiaryReport<APIWorkDiary>(IEnumerable<APIWorkDiary> items)
        {
            var tb = new DataTable(typeof(APIWorkDiary).Name);

            PropertyInfo[] props = typeof(APIWorkDiary).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["Date"].ColumnName = HeaderName.Date;
            tb.Columns["InTime"].ColumnName = HeaderName.InTime;
            tb.Columns["OutTime"].ColumnName = HeaderName.OutTime;
            tb.Columns["Accounts"].ColumnName = HeaderName.Accounts;

            return tb;
        }

        public DataTable ToDataTableILTUserDetailsReport<APIILTUserDetailsReport>(IEnumerable<APIILTUserDetailsReport> items)
        {
            var tb = new DataTable(typeof(APIILTUserDetailsReport).Name);

            PropertyInfo[] props = typeof(APIILTUserDetailsReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["CourseCategory"].ColumnName = HeaderName.CourseCategory;
            tb.Columns["CourseType"].ColumnName = HeaderName.CourseType;
            tb.Columns["TrainingCode"].ColumnName = HeaderName.TrainingCode;
            tb.Columns["TrainingName"].ColumnName = HeaderName.TrainingName;
            tb.Columns["EmpCode"].ColumnName = HeaderName.EmpCode;
            tb.Columns["EmpName"].ColumnName = HeaderName.EmpName;
            tb.Columns["MobileNumber"].ColumnName = HeaderName.MobileNo;
            tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
            tb.Columns["Mode"].ColumnName = HeaderName.Mode;
            tb.Columns["NoOfSessions"].ColumnName = HeaderName.NoOfSessions;
            tb.Columns["TotalNumberOfHours"].ColumnName = HeaderName.TotalNumberOfHours;
            tb.Columns["AttendedNumberOfHours"].ColumnName = HeaderName.AttendedNumberOfHours;
            tb.Columns["BatchCode"].ColumnName = HeaderName.BatchCode;
            tb.Columns["BatchStartDate"].ColumnName = HeaderName.BatchStartDate;
            tb.Columns["BatchEndDate"].ColumnName = HeaderName.BatchEndDate;
            tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
            tb.Columns["ScheduleStartDate"].ColumnName = HeaderName.ScheduleStartDate;
            tb.Columns["ScheduleEndDate"].ColumnName = HeaderName.ScheduleEndDate;
            tb.Columns["PassPercentage"].ColumnName = HeaderName.PassPercentage;
            tb.Columns["Result"].ColumnName = HeaderName.Result;
            tb.Columns["NoofAttempts"].ColumnName = HeaderName.NoOfAttempts;
            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerName;
            tb.Columns["Cost"].ColumnName = HeaderName.Cost;
            tb.Columns["TrainerType"].ColumnName = HeaderName.TrainerType;
            tb.Columns["TrainingPlace"].ColumnName = HeaderName.TrainingPlace;
            tb.Columns["Feedback"].ColumnName = HeaderName.Feedback;
            tb.Columns["Position"].ColumnName = HeaderName.Position;
            tb.Columns["Department"].ColumnName = HeaderName.Department;
            tb.Columns["DepartmentHirarchy"].ColumnName = HeaderName.DepartmentHirarchy;
            tb.Columns["WorkLocation"].ColumnName = HeaderName.WorkLocation;
            tb.Columns["EmployeeCity"].ColumnName = HeaderName.EmployeeCity;
            tb.Columns["EmployeeState"].ColumnName = HeaderName.EmployeeState;
            tb.Columns["DateOfJoining"].ColumnName = HeaderName.DateOfJoining;

            return tb;
        }

        public DataTable ToDataTableActivityReport<APIActivityReport>(IEnumerable<APIActivityReport> items, List<APIUserSetting> userSetting)
        {
            var tb = new DataTable(typeof(APIActivityReport).Name);

            PropertyInfo[] props = typeof(APIActivityReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["MobileNumber"].ColumnName = HeaderName.MobileNo;
            tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
            tb.Columns["UserStatus"].ColumnName = HeaderName.UserStatus;
            tb.Columns["FirstLoginDate"].ColumnName = HeaderName.FirstLoginDate;
            tb.Columns["FirstContentAccess"].ColumnName = HeaderName.FirstContentAccess;
            tb.Columns["LastContentAccess"].ColumnName = HeaderName.LastContentAccess;
            tb.Columns["CourseAssigned"].ColumnName = HeaderName.CourseAssigned;
            tb.Columns["CourseCompleted"].ColumnName = HeaderName.CourseCompleted;
            tb.Columns["CourseInprogress"].ColumnName = HeaderName.CourseInprogress;
            tb.Columns["Completion"].ColumnName = HeaderName.Completion;
            tb.Columns["LastActiveDate"].ColumnName = HeaderName.LastActiveDate;
            tb.Columns["DeviceMostActive"].ColumnName = HeaderName.DeviceMostActive;
            tb.Columns["TotalSignIns"].ColumnName = HeaderName.TotalSignIns;
            tb.Columns["WebSignIns"].ColumnName = HeaderName.WebSignIns;
            tb.Columns["AppSignIns"].ColumnName = HeaderName.AppSignIn;
            tb.Columns["TotalTimeSpentInMinutes"].ColumnName = HeaderName.TotalTimeSpentIn;
            tb.Columns["WebTimeSpentInMinutes"].ColumnName = HeaderName.WebTimeSpent;
            tb.Columns["AppTimeSpentInMinutes"].ColumnName = HeaderName.AppTimeSpent;
            tb.Columns["TotalNoofPageviews"].ColumnName = HeaderName.TotalNoofPageviews;
            tb.Columns["TotalPointEarned"].ColumnName = HeaderName.TotalPointsEarned;
            tb.Columns["CertificateEarned"].ColumnName = HeaderName.CertificateEarned;
            //tb.Columns["Position"].ColumnName = HeaderName.Position;
            //tb.Columns["Department"].ColumnName = HeaderName.Department;
            //tb.Columns["DepartmentHirarchy"].ColumnName = HeaderName.DepartmentHirarchy;
            //tb.Columns["WorkLocation"].ColumnName = HeaderName.WorkLocation;
            //tb.Columns["EmployeeCity"].ColumnName = HeaderName.EmployeeCity;
            //tb.Columns["EmployeeState"].ColumnName = HeaderName.EmployeeState;
            //tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["DateOfJoining"].ColumnName = HeaderName.DateOfJoining;

            tb.Columns["Business"].ColumnName = userSetting[0].ChangedColumnName;
            tb.Columns["Group"].ColumnName = userSetting[1].ChangedColumnName;
            tb.Columns["Area"].ColumnName = userSetting[2].ChangedColumnName;
            tb.Columns["Location"].ColumnName = userSetting[3].ChangedColumnName;
            tb.Columns["Configurationcolumn1"].ColumnName = userSetting[4].ChangedColumnName;
            tb.Columns["Configurationcolumn2"].ColumnName = userSetting[5].ChangedColumnName;
            tb.Columns["Configurationcolumn3"].ColumnName = userSetting[6].ChangedColumnName;
            tb.Columns["Configurationcolumn4"].ColumnName = userSetting[7].ChangedColumnName;
            tb.Columns["Configurationcolumn5"].ColumnName = userSetting[8].ChangedColumnName;
            tb.Columns["Configurationcolumn6"].ColumnName = userSetting[9].ChangedColumnName;
            tb.Columns["Configurationcolumn7"].ColumnName = userSetting[10].ChangedColumnName;
            tb.Columns["Configurationcolumn8"].ColumnName = userSetting[11].ChangedColumnName;
            tb.Columns["Configurationcolumn9"].ColumnName = userSetting[12].ChangedColumnName;
            tb.Columns["Configurationcolumn10"].ColumnName = userSetting[13].ChangedColumnName;
            tb.Columns["Configurationcolumn11"].ColumnName = userSetting[14].ChangedColumnName;
            tb.Columns["Configurationcolumn12"].ColumnName = userSetting[15].ChangedColumnName;
            tb.Columns["Configurationcolumn13"].ColumnName = userSetting[16].ChangedColumnName;
            tb.Columns["Configurationcolumn14"].ColumnName = userSetting[17].ChangedColumnName;
            tb.Columns["Configurationcolumn15"].ColumnName = userSetting[18].ChangedColumnName;

            int count = userSetting.Count;
            for (int i = 0; i < count; i++)
            {
                if (userSetting[i].IsShowInReport == false)
                    tb.Columns.Remove(userSetting[i].ChangedColumnName);
            }


            return tb;
        }

        public DataTable ToDataTableUserWiseCourseCompletionReport<APIUserWiseCourseCompletionReport>(IEnumerable<APIUserWiseCourseCompletionReport> items)
        {
            var tb = new DataTable(typeof(APIUserWiseCourseCompletionReport).Name);

            PropertyInfo[] props = typeof(APIUserWiseCourseCompletionReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["MobileNumber"].ColumnName = HeaderName.MobileNo;
            tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["CourseName"].ColumnName = HeaderName.CourseName;
            tb.Columns["UserStatus"].ColumnName = HeaderName.UserStatus;
            tb.Columns["CourseAssignedDate"].ColumnName = HeaderName.CourseAssignedDate;
            tb.Columns["CourseFirstAccessOn"].ColumnName = HeaderName.CourseFirstAccesson;
            tb.Columns["CourseLastAccessOn"].ColumnName = HeaderName.CourseLastAccessedOn;
            tb.Columns["CourseAccessed"].ColumnName = HeaderName.CourseAccessed;
            tb.Columns["FirstCompletionDate"].ColumnName = HeaderName.FirstCompletionDate;
            tb.Columns["CourseCompletedOn"].ColumnName = HeaderName.CourseCompletedOn;
            tb.Columns["CourseStatus"].ColumnName = HeaderName.CourseStatus;
            tb.Columns["CourseCompletionPercentage"].ColumnName = HeaderName.CourseCompletionPercentage;
            tb.Columns["IsAssessmentAvailable"].ColumnName = HeaderName.IsAssessmentAvailable;
            tb.Columns["AssessmentScore"].ColumnName = HeaderName.AssessmentScore;
            tb.Columns["AssessmentScorePercentage"].ColumnName = HeaderName.AssessmentScorePercentage;
            tb.Columns["AssessmentStatus"].ColumnName = HeaderName.AssessmentStatus;
            tb.Columns["AssessmentPassedDate"].ColumnName = HeaderName.AssessmentPassedDate;
            tb.Columns["TotalTimeSpentInMinutes"].ColumnName = HeaderName.TotalTimeSpentIn;
            tb.Columns["WebTimeSpentInMinutes"].ColumnName = HeaderName.WebTimeSpent;
            tb.Columns["AppTimeSpentInMinutes"].ColumnName = HeaderName.MobileTimeSpent;
            tb.Columns["DeviceMostActive"].ColumnName = HeaderName.DeviceMostActive;
            tb.Columns["TotalNoofPageviews"].ColumnName = HeaderName.TotalNoofPageviews;
            tb.Columns["IsFeedbackAvailable"].ColumnName = HeaderName.IsFeedbackAvailable;
            tb.Columns["FeedbackRating"].ColumnName = HeaderName.FeedbackRating;
            tb.Columns["Certificate"].ColumnName = HeaderName.Certificate;
            tb.Columns["CertificateUrl"].ColumnName = HeaderName.CertificateURL;
            tb.Columns["Position"].ColumnName = HeaderName.Position;
            tb.Columns["Department"].ColumnName = HeaderName.Department;
            tb.Columns["DepartmentHirarchy"].ColumnName = HeaderName.DepartmentHirarchy;
            tb.Columns["WorkLocation"].ColumnName = HeaderName.WorkLocation;
            tb.Columns["EmployeeCity"].ColumnName = HeaderName.EmployeeCity;
            tb.Columns["EmployeeState"].ColumnName = HeaderName.EmployeeState;
            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["DateOfJoining"].ColumnName = HeaderName.DateOfJoining;

            return tb;
        }

        public DataTable ToDataTableUserLoginHistoryReport<APIUserLoginHistoryReport>(IEnumerable<APIUserLoginHistoryReport> items)
        {
            var tb = new DataTable(typeof(APIUserLoginHistoryReport).Name);

            PropertyInfo[] props = typeof(APIUserLoginHistoryReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["MobileNumber"].ColumnName = HeaderName.MobileNo;
            tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
            tb.Columns["Status"].ColumnName = HeaderName.Status;
            tb.Columns["IsLoggedInWeb"].ColumnName = HeaderName.IsLoggedInWeb;
            tb.Columns["FirstLoginDate"].ColumnName = HeaderName.FirstLoginDate;
            tb.Columns["FirstContentAccessDate"].ColumnName = HeaderName.FirstContentAccess;
            tb.Columns["LastContentAccessDate"].ColumnName = HeaderName.LastContentAccess;
            tb.Columns["LastActiveDate"].ColumnName = HeaderName.LastActiveDate;
            tb.Columns["TotalSignIns"].ColumnName = HeaderName.TotalSignIns;
            tb.Columns["WebSignIns"].ColumnName = HeaderName.WebSignIns;
            tb.Columns["AppSignIns"].ColumnName = HeaderName.AppSignIn;
            tb.Columns["TotalTimeSpentInMinutes"].ColumnName = HeaderName.TotalTimeSpentIn;
            tb.Columns["WebTimeSpentInMinutes"].ColumnName = HeaderName.WebTimeSpent;
            tb.Columns["AppTimeSpentInMinutes"].ColumnName = HeaderName.AppTimeSpent;
            tb.Columns["NoofPageviews"].ColumnName = HeaderName.NoofPageviews;
            tb.Columns["Position"].ColumnName = HeaderName.Position;
            tb.Columns["Department"].ColumnName = HeaderName.Department;
            tb.Columns["DepartmentHirarchy"].ColumnName = HeaderName.DepartmentHirarchy;
            tb.Columns["WorkLocation"].ColumnName = HeaderName.WorkLocation;
            tb.Columns["EmployeeCity"].ColumnName = HeaderName.EmployeeCity;
            tb.Columns["EmployeeState"].ColumnName = HeaderName.EmployeeState;
            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["DateOfJoining"].ColumnName = HeaderName.DateOfJoining;

            return tb;
        }

        public DataTable ToDataTableILTDashboardReport<APIILTDashboardReport>(IEnumerable<APIILTDashboardReport> items)
        {
            var tb = new DataTable(typeof(APIILTDashboardReport).Name);

            PropertyInfo[] props = typeof(APIILTDashboardReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["Category"].ColumnName = HeaderName.Category;
            tb.Columns["Type"].ColumnName = HeaderName.Type;
            tb.Columns["TrainingCode"].ColumnName = HeaderName.TrainingCode;
            tb.Columns["TrainingName"].ColumnName = HeaderName.TrainingName;
            tb.Columns["EmpCode"].ColumnName = HeaderName.EmpCode;
            tb.Columns["EmpName"].ColumnName = HeaderName.EmpName;
            tb.Columns["MobileNumber"].ColumnName = HeaderName.MobileNo;
            tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
            tb.Columns["Mode"].ColumnName = HeaderName.Mode;
            tb.Columns["NoOfSessions"].ColumnName = HeaderName.NoOfSessions;
            tb.Columns["TotalNumberOfHours"].ColumnName = HeaderName.TotalNumberOfHours;
            tb.Columns["AttendedNumberOfHours"].ColumnName = HeaderName.AttendedNumberOfHours;
            tb.Columns["BatchCode"].ColumnName = HeaderName.BatchCode;
            tb.Columns["StartDate"].ColumnName = HeaderName.StartDate;
            tb.Columns["EndDate"].ColumnName = HeaderName.EndDate;
            tb.Columns["PassPercentage"].ColumnName = HeaderName.PassPercentage;
            tb.Columns["Result"].ColumnName = HeaderName.Result;
            tb.Columns["NoofAttempts"].ColumnName = HeaderName.NoOfAttempts;
            tb.Columns["Feedback"].ColumnName = HeaderName.Feedback;
            tb.Columns["Postion"].ColumnName = HeaderName.Position;
            tb.Columns["Department"].ColumnName = HeaderName.Department;
            tb.Columns["DepartmentHirarchy"].ColumnName = HeaderName.DepartmentHirarchy;
            tb.Columns["WorkLocation"].ColumnName = HeaderName.WorkLocation;
            tb.Columns["EmployeeCity"].ColumnName = HeaderName.EmployeeCity;
            tb.Columns["EmployeeState"].ColumnName = HeaderName.EmployeeState;
            tb.Columns["DateOfJoining"].ColumnName = HeaderName.DateOfJoining;

            return tb;
        }

        public DataTable ToDataTableCourseModuleReport<APICourseModuleReport>(IEnumerable<APICourseModuleReport> items)
        {
            var tb = new DataTable(typeof(APICourseModuleReport).Name);

            PropertyInfo[] props = typeof(APICourseModuleReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["MobileNumber"].ColumnName = HeaderName.MobileNo;
            tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
            tb.Columns["UserStatus"].ColumnName = HeaderName.UserStatus;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["CourseName"].ColumnName = HeaderName.CourseName;
            tb.Columns["CourseAssignedDate"].ColumnName = HeaderName.CourseAssignedDate;
            tb.Columns["CourseFirstAccessOn"].ColumnName = HeaderName.CourseFirstAccesson;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["ModuleFirstAccessOn"].ColumnName = HeaderName.ModuleFirstAccessOn;
            tb.Columns["ModuleLastAccessOn"].ColumnName = HeaderName.ModuleLastAccessOn;
            tb.Columns["ModuleAccessed"].ColumnName = HeaderName.ModuleAccessed;
            tb.Columns["ModuleCompletedOn"].ColumnName = HeaderName.ModuleCompletedOn;
            tb.Columns["ModuleStatus"].ColumnName = HeaderName.ModuleStatus;
            tb.Columns["CourseAccessed"].ColumnName = HeaderName.CourseAccessed;
            tb.Columns["CourseCompletedOn"].ColumnName = HeaderName.CourseCompletedOn;
            tb.Columns["CourseStatus"].ColumnName = HeaderName.CourseStatus;
            tb.Columns["CourseCompletionPercentage"].ColumnName = HeaderName.CourseCompletionPercentage;
            tb.Columns["ModuleAssessmentAvailable"].ColumnName = HeaderName.ModuleAssessmentAvailable;
            tb.Columns["ModuleAssessmentScore"].ColumnName = HeaderName.ModuleAssessmentScore;
            tb.Columns["ModuleAssessmentScorePercentage"].ColumnName = HeaderName.ModuleAssessmentScorePercentage;
            tb.Columns["ModuleAssessmentStatus"].ColumnName = HeaderName.ModuleAssessmentStatus;
            tb.Columns["ModuleAssessmentPassedDate"].ColumnName = HeaderName.ModuleAssessmentPassedDate;
            tb.Columns["TotalTimeSpent"].ColumnName = HeaderName.TotalTimeSpent;
            tb.Columns["WebTimeSpent"].ColumnName = HeaderName.WebTimeSpent;
            tb.Columns["MobileTimeSpent"].ColumnName = HeaderName.MobileTimeSpent;
            tb.Columns["DeviceMostAccessedOn"].ColumnName = HeaderName.DeviceMostAccessedOn;
            tb.Columns["NumberOfPageViews"].ColumnName = HeaderName.NoofPageViews;
            tb.Columns["IsFeedbackAvailable"].ColumnName = HeaderName.IsFeedbackAvailable;
            tb.Columns["FeedbackRating"].ColumnName = HeaderName.FeedbackRating;
            tb.Columns["Certificate"].ColumnName = HeaderName.Certificate;
            tb.Columns["CertificateURL"].ColumnName = HeaderName.CertificateURL;
            tb.Columns["Position"].ColumnName = HeaderName.Position;
            tb.Columns["Department"].ColumnName = HeaderName.Department;
            tb.Columns["DepartmentHirarchy"].ColumnName = HeaderName.DepartmentHirarchy;
            tb.Columns["WorkLocation"].ColumnName = HeaderName.WorkLocation;
            tb.Columns["EmployeeCity"].ColumnName = HeaderName.EmployeeCity;
            tb.Columns["EmployeeState"].ColumnName = HeaderName.EmployeeState;
            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["DateOfJoining"].ColumnName = HeaderName.DateOfJoining;

            return tb;
        }

        public DataTable ToDataTableCourseAssessmentReport<APICourseAssessmentReport>(IEnumerable<APICourseAssessmentReport> items)
        {
            var tb = new DataTable(typeof(APICourseAssessmentReport).Name);

            PropertyInfo[] props = typeof(APICourseAssessmentReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["MobileNumber"].ColumnName = HeaderName.MobileNo;
            tb.Columns["EmailId"].ColumnName = HeaderName.EmailId;
            tb.Columns["UserStatus"].ColumnName = HeaderName.UserStatus;
            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["CourseName"].ColumnName = HeaderName.CourseName;
            tb.Columns["ModuleName"].ColumnName = HeaderName.ModuleName;
            tb.Columns["QuestionName"].ColumnName = HeaderName.QuestionName;
            tb.Columns["QuestionWiseScore"].ColumnName = HeaderName.QuestionWiseScore;
            tb.Columns["OverallAssessmentScore"].ColumnName = HeaderName.OverallAssessmentScore;
            tb.Columns["OverallAssessmentScorePercentage"].ColumnName = HeaderName.OverallAssessmentScorePercentage;
            tb.Columns["AssessmentStatus"].ColumnName = HeaderName.AssessmentStatus;
            tb.Columns["AssessmentDate"].ColumnName = HeaderName.AssessmentDate;
            tb.Columns["AssessmentPassedDate"].ColumnName = HeaderName.AssessmentPassedDate;
            tb.Columns["TotalTimeSpent"].ColumnName = HeaderName.TotalTimeSpent;
            tb.Columns["DeviceMostAccessedOn"].ColumnName = HeaderName.DeviceMostAccessedOn;
            tb.Columns["Position"].ColumnName = HeaderName.Position;
            tb.Columns["Department"].ColumnName = HeaderName.Department;
            tb.Columns["DepartmentHirarchy"].ColumnName = HeaderName.DepartmentHirarchy;
            tb.Columns["WorkLocation"].ColumnName = HeaderName.WorkLocation;
            tb.Columns["EmployeeCity"].ColumnName = HeaderName.EmployeeCity;
            tb.Columns["EmployeeState"].ColumnName = HeaderName.EmployeeState;
            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["DateOfJoining"].ColumnName = HeaderName.DateOfJoining;

            return tb;
        }

        public DataTable ToDataTableReportUserWiseCourseDurationReport<APIReportUserWiseCourseDuration>(IEnumerable<APIReportUserWiseCourseDuration> items)
        {
            var tb = new DataTable(typeof(APIReportUserWiseCourseDuration).Name);

            PropertyInfo[] props = typeof(APIReportUserWiseCourseDuration).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["CourseDuration"].ColumnName = HeaderName.CourseDuration;

            return tb;
        }

        public DataTable ToDataTableReportProfabAccessibility<APIReportProfabAccessibility>(IEnumerable<APIReportProfabAccessibility> items)
        {
            var tb = new DataTable(typeof(APIReportProfabAccessibility).Name);

            PropertyInfo[] props = typeof(APIReportProfabAccessibility).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["ProductName"].ColumnName = HeaderName.ProductName;
            tb.Columns["DateTime"].ColumnName = HeaderName.DateTime;

            return tb;
        }

        public DataTable ToDataTableReportProfabAccessibilityCount<APIReportProfabAccessibilityCount>(IEnumerable<APIReportProfabAccessibilityCount> items)
        {
            var tb = new DataTable(typeof(APIReportProfabAccessibilityCount).Name);

            PropertyInfo[] props = typeof(APIReportProfabAccessibilityCount).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["ProductName"].ColumnName = HeaderName.ProductName;
            tb.Columns["UserCount"].ColumnName = HeaderName.UserCount;

            return tb;
        }

        public DataTable ToDataTableManagerEvaluationReport<APIManagerEvaluationReport>(IEnumerable<APIManagerEvaluationReport> items)
        {
            var tb = new DataTable(typeof(APIManagerEvaluationReport).Name);

            PropertyInfo[] props = typeof(APIManagerEvaluationReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["CourseCode"].ColumnName = HeaderName.CourseCode;
            tb.Columns["CourseTitle"].ColumnName = HeaderName.CourseTitle;
            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["LocationName"].ColumnName = HeaderName.Location;
            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["NumberofAttempts"].ColumnName = HeaderName.NumberofAttempts;
            tb.Columns["AssessmentResult"].ColumnName = HeaderName.AssessmentResult;
            tb.Columns["AssessmentPercentage"].ColumnName = HeaderName.AssessmentPercentage;
            tb.Columns["AssessmentDate"].ColumnName = HeaderName.AssessmentDate;
            tb.Columns["AssessedBy"].ColumnName = HeaderName.AssessedBy;
            tb.Columns["DateOfJoining"].ColumnName = HeaderName.DateOfJoining;
            tb.Columns["Designation"].ColumnName = HeaderName.Designation;

            return tb;
        }

        public DataTable ToDataTablePastTrainingSummeryDetailsReport<APIPastTrainingSummeryDetails>(IEnumerable<APIPastTrainingSummeryDetails> items)
        {
            var tb = new DataTable(typeof(APIPastTrainingSummeryDetails).Name);

            PropertyInfo[] props = typeof(APIPastTrainingSummeryDetails).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TrainerType");
            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("UserID");

            tb.Columns["StartDate"].ColumnName = HeaderName.StartDate;
            tb.Columns["EndDate"].ColumnName = HeaderName.EndDate;
            tb.Columns["ProgramNo"].ColumnName = HeaderName.ProgramNo;
            tb.Columns["ProgramName"].ColumnName = HeaderName.ProgramName;
            tb.Columns["SubProgramName"].ColumnName = HeaderName.SubProgramName;
            tb.Columns["ParticipantType"].ColumnName = HeaderName.ParticipantType;
            tb.Columns["TrainerID"].ColumnName = HeaderName.TrainerId;
            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerName;
            tb.Columns["TrainerGroup"].ColumnName = HeaderName.TrainerGroup;
            tb.Columns["Attended"].ColumnName = HeaderName.Attended;
            tb.Columns["ProgramScore"].ColumnName = HeaderName.ProgramScore;
            tb.Columns["TrainerScore"].ColumnName = HeaderName.TrainerScore;
            tb.Columns["CreatedByID"].ColumnName = HeaderName.CreatedByID;
            tb.Columns["CreatedByName"].ColumnName = HeaderName.CreatedByName;
            tb.Columns["ConsultantTrainerName"].ColumnName = HeaderName.ConsultantTrainerName;
            tb.Columns["ConductedBy"].ColumnName = HeaderName.ConductedBy;
            tb.Columns["DepartmentofTrainer"].ColumnName = HeaderName.DepartmentofTrainer;
            tb.Columns["SubfunctionofTrainer"].ColumnName = HeaderName.SubfunctionofTrainer;
            tb.Columns["RegionofTrainer"].ColumnName = HeaderName.RegionofTrainer;
            tb.Columns["ScheduleHours"].ColumnName = HeaderName.TotalNoHours;
            tb.Columns["FirstAttendanceUpload"].ColumnName = HeaderName.FirstAttendanceUpload;
            tb.Columns["LastAttendanceUpload"].ColumnName = HeaderName.LastAttendanceUpload;
            tb.Columns["Branch"].ColumnName = HeaderName.Branch;

            return tb;
        }

        public DataTable ToDataTableProgramAttendanceReport<ApiProgramAttendanceReportView>(IEnumerable<ApiProgramAttendanceReportView> items)
        {
            var tb = new DataTable(typeof(ApiProgramAttendanceReportView).Name);

            PropertyInfo[] props = typeof(ApiProgramAttendanceReportView).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("Region");
            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("UserIdConductedBy");

            tb.Columns["ProgramNo"].ColumnName = HeaderName.ProgramNo;
            tb.Columns["ProgramName"].ColumnName = HeaderName.ProgramName;
            tb.Columns["EmpNo"].ColumnName = HeaderName.EmpNo;
            tb.Columns["EmpName"].ColumnName = HeaderName.EmpName;
            tb.Columns["ParticipantType"].ColumnName = HeaderName.ParticipantType;
            tb.Columns["Fromdate"].ColumnName = HeaderName.FromDate;
            tb.Columns["Todate"].ColumnName = HeaderName.ToDate;
            tb.Columns["TBM_REGION"].ColumnName = HeaderName.REGION;
            tb.Columns["CreatedByID"].ColumnName = HeaderName.CreatedByID;
            tb.Columns["CreatedByName"].ColumnName = HeaderName.CreatedByName;
            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerName;
            tb.Columns["UserIdOfTrainer"].ColumnName = HeaderName.UserIdOfTrainer;
            tb.Columns["DepartmentofTrainer"].ColumnName = HeaderName.DepartmentofTrainer;
            tb.Columns["SubfunctionofTrainer"].ColumnName = HeaderName.SubfunctionofTrainer;
            tb.Columns["RegionofTrainer"].ColumnName = HeaderName.RegionofTrainer;
            tb.Columns["ConductedBy"].ColumnName = HeaderName.ConductedBy;
            tb.Columns["ConsultantTrainer"].ColumnName = HeaderName.ConsultantTrainer;
            tb.Columns["AttendanceStatus"].ColumnName = HeaderName.AttendanceStatus;
            tb.Columns["AttendanceDate"].ColumnName = HeaderName.AttendanceDate;
            tb.Columns["AttendanceTakenByUserId"].ColumnName = HeaderName.AttendanceTakenByUserId;
            tb.Columns["AttendanceTakenByUserName"].ColumnName = HeaderName.AttendanceTakenByUserName;

            return tb;
        }

        public DataTable ToDataTableTrainerProductivityReport<APITrainerProductivityReport>(IEnumerable<APITrainerProductivityReport> items)
        {
            var tb = new DataTable(typeof(APITrainerProductivityReport).Name);

            PropertyInfo[] props = typeof(APITrainerProductivityReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");

            tb.Columns["TrainerId"].ColumnName = HeaderName.TrainerId;
            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerName;
            tb.Columns["Days"].ColumnName = HeaderName.Days;
            tb.Columns["ScheduleCount"].ColumnName = HeaderName.ScheduleCount;
            tb.Columns["NoOfAttendant"].ColumnName = HeaderName.NoOfAttendant;
            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["ConductedBy"].ColumnName = HeaderName.ConductedBy;
            tb.Columns["DepartmentofTrainer"].ColumnName = HeaderName.DepartmentofTrainer;
            tb.Columns["SubfunctionofTrainer"].ColumnName = HeaderName.SubfunctionofTrainer;

            return tb;
        }

        public DataTable ToDataTableAverageProductivityReport<APIAverageProductivityReport>(IEnumerable<APIAverageProductivityReport> items)
        {
            var tb = new DataTable(typeof(APIAverageProductivityReport).Name);

            PropertyInfo[] props = typeof(APIAverageProductivityReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("AttendanceDate");
            tb.Columns.Remove("Status");
            tb.Columns.Remove("UserName");

            tb.Columns["TrainerId"].ColumnName = HeaderName.TrainerId;
            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerName;
            tb.Columns["AverageDays"].ColumnName = HeaderName.AverageDays;
            tb.Columns["AverageProgram"].ColumnName = HeaderName.AverageProgram;
            tb.Columns["ParticipantPerProgram"].ColumnName = HeaderName.ParticipantPerProgram;
            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["ConductedBy"].ColumnName = HeaderName.ConductedBy;
            tb.Columns["DepartmentofTrainer"].ColumnName = HeaderName.DepartmentofTrainer;
            tb.Columns["SubfunctionofTrainer"].ColumnName = HeaderName.SubfunctionofTrainer;

            return tb;
        }

        public DataTable ToDataTableTrainingPassportReport<APITrainingPassportReport>(IEnumerable<APITrainingPassportReport> items)
        {
            var tb = new DataTable(typeof(APITrainingPassportReport).Name);

            PropertyInfo[] props = typeof(APITrainingPassportReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns.Remove("TotalRecordCount");
            tb.Columns.Remove("Group");
            tb.Columns.Remove("Id");
            tb.Columns.Remove("TrainerType");

            tb.Columns["UserID"].ColumnName = HeaderName.UserID;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["Designation"].ColumnName = HeaderName.Designation;
            tb.Columns["GradeLevel"].ColumnName = HeaderName.GradeLevel;
            tb.Columns["Region"].ColumnName = HeaderName.Region;
            tb.Columns["ScheduleCode"].ColumnName = HeaderName.ScheduleCode;
            tb.Columns["CourseName"].ColumnName = HeaderName.CourseName;
            tb.Columns["FromDate"].ColumnName = HeaderName.FromDate;
            tb.Columns["ToDate"].ColumnName = HeaderName.ToDate;
            tb.Columns["ConductedBy"].ColumnName = HeaderName.ConductedBy;
            tb.Columns["TrainerId"].ColumnName = HeaderName.TrainerId;
            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerName;
            tb.Columns["RegionofTrainer"].ColumnName = HeaderName.RegionofTrainer;
            tb.Columns["DepartmentofTrainer"].ColumnName = HeaderName.DepartmentofTrainer;
            tb.Columns["SubfunctionofTrainer"].ColumnName = HeaderName.SubfunctionofTrainer;
            tb.Columns["AttendanceDate"].ColumnName = HeaderName.AttendanceDate;
            tb.Columns["AttendanceStatus"].ColumnName = HeaderName.AttendanceStatus;

            return tb;
        }

        public DataTable ToDataTableTNASupervisionReport<TnaSupervisionReportExport>(IEnumerable<TnaSupervisionReportExport> items, string OrgCode)
        {
            var tb = new DataTable(typeof(TnaSupervisionReportExport).Name);

            PropertyInfo[] props = typeof(TnaSupervisionReportExport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            if (OrgCode.ToLower() == "sbil" || OrgCode.ToLower() == "ent")
            {

                tb.Columns["EmpID"].ColumnName = HeaderName.EmpId;
                tb.Columns["EmpName"].ColumnName = HeaderName.EmpName;
                tb.Columns["SupervisorID"].ColumnName = HeaderName.SupervisorId;
                tb.Columns["EmpIDRegion"].ColumnName = HeaderName.EmpIdRegion;
                tb.Columns["EmpIDSubRegion"].ColumnName = HeaderName.EmpIdSubRegion;
                tb.Columns["EmpIDDepartment"].ColumnName = HeaderName.EmpIdDepartment;
                tb.Columns["EmpIDSubDepartment"].ColumnName = HeaderName.EmpIdSubDepartment;
                tb.Columns["CourseDetails"].ColumnName = HeaderName.CourseDetails;
                tb.Columns["CategoryType"].ColumnName = HeaderName.CategoryType;
                tb.Columns["RequestStatus"].ColumnName = HeaderName.RequestStatus;
                tb.Columns["RequestedByID"].ColumnName = HeaderName.RequestedById;
                tb.Columns["Requestedby"].ColumnName = HeaderName.RequestedBy2;
                tb.Columns["Approvedby"].ColumnName = HeaderName.ApprovedBy;
                tb.Columns["RequestDate"].ColumnName = HeaderName.RequestDate;
                tb.Columns["ActionDate"].ColumnName = HeaderName.ActionDate;
            }

            else
            {

                tb.Columns.Remove("SupervisorID");
                tb.Columns.Remove("EmpIDRegion");
                tb.Columns.Remove("EmpIDSubRegion");
                tb.Columns.Remove("EmpIDDepartment");
                tb.Columns.Remove("ReqestedByID");
                tb.Columns.Remove("EmpIDSubDepartment");

                tb.Columns["EmpID"].ColumnName = HeaderName.EmpId;
                tb.Columns["EmpName"].ColumnName = HeaderName.EmpName;
                /*tb.Columns["SupervisorID"].ColumnName = HeaderName.SupervisorId;
                tb.Columns["EmpIDRegion"].ColumnName = HeaderName.EmpIdRegion;
                tb.Columns["EmpIDSubRegion"].ColumnName = HeaderName.EmpIdSubRegion;
                tb.Columns["EmpIDDepartment"].ColumnName = HeaderName.EmpIdDepartment;
                tb.Columns["EmpIDSubDepartment"].ColumnName = HeaderName.EmpIdSubDepartment;*/
                tb.Columns["CourseDetails"].ColumnName = HeaderName.CourseDetails;
                tb.Columns["CategoryType"].ColumnName = HeaderName.CategoryType;
                tb.Columns["RequestStatus"].ColumnName = HeaderName.RequestStatus;
                //tb.Columns["ReqestedByID"].ColumnName = HeaderName.RequestedById;
                tb.Columns["Requestedby"].ColumnName = HeaderName.RequestedBy2;
                tb.Columns["Approvedby"].ColumnName = HeaderName.ApprovedBy;
                tb.Columns["RequestDate"].ColumnName = HeaderName.RequestDate;
                tb.Columns["ActionDate"].ColumnName = HeaderName.ActionDate;
            }


            return tb;
        }

        #endregion

        public DataTable ToDataTableLaLearningReport<APILaLearningReportData>(IEnumerable<APILaLearningReportData> items)
        {
            var tb = new DataTable(typeof(APILaLearningReportData).Name);

            PropertyInfo[] props = typeof(APILaLearningReportData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["TypeOfTraining"].ColumnName = HeaderName.TypeOfTraining;
            tb.Columns["WorkShopName"].ColumnName = HeaderName.WorkShopName;
            tb.Columns["TrainingCode"].ColumnName = HeaderName.TrainingCode;
            tb.Columns["ModuleType"].ColumnName = HeaderName.ModuleType;
            tb.Columns["StartDate"].ColumnName = HeaderName.StartDateWNS;
            tb.Columns["EndDate"].ColumnName = HeaderName.EndDateWNS;
            tb.Columns["Month"].ColumnName = HeaderName.Month;
            tb.Columns["TrainingManHours"].ColumnName = HeaderName.TrainingManHours;
            tb.Columns["TrainingManDays"].ColumnName = HeaderName.TrainingManDays;
            tb.Columns["EmployeeID"].ColumnName = HeaderName.EmployeeID;
            tb.Columns["NameofParticipant"].ColumnName = HeaderName.NameofParticipant;
            tb.Columns["Gender"].ColumnName = HeaderName.Gender;
            tb.Columns["Designation"].ColumnName = HeaderName.Designation;
            tb.Columns["Grade"].ColumnName = HeaderName.GradeWNS;
            tb.Columns["Vertical"].ColumnName = HeaderName.Vertical;
            tb.Columns["Horizontal"].ColumnName = HeaderName.Horizontal;
            tb.Columns["DepartmentLong"].ColumnName = HeaderName.DepartmentLong;
            tb.Columns["TeamGroup"].ColumnName = HeaderName.TeamGroup;
            tb.Columns["Client"].ColumnName = HeaderName.Client;
            tb.Columns["Budgetcode"].ColumnName = HeaderName.Budgetcode;
            tb.Columns["ParticipantLocation"].ColumnName = HeaderName.ParticipantLocation;
            tb.Columns["EnrollmentStatus"].ColumnName = HeaderName.EnrollmentStatus;
            tb.Columns["Attended_NotAttended"].ColumnName = HeaderName.Attended_NotAttended;
            tb.Columns["InternalRechargePrice"].ColumnName = HeaderName.InternalRechargePrice;
            tb.Columns["ExternalRechargePrice"].ColumnName = HeaderName.ExternalRechargePrice;
            tb.Columns["WorkshopLocation"].ColumnName = HeaderName.WorkshopLocation;
            tb.Columns["Estabid"].ColumnName = HeaderName.Estabid;
            tb.Columns["DateofJoining"].ColumnName = HeaderName.DateofJoiningWNS;
            tb.Columns["Quarter"].ColumnName = HeaderName.Quarter;
            tb.Columns["FY"].ColumnName = HeaderName.FY;
            tb.Columns["Geography"].ColumnName = HeaderName.Geography;
            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerNameWNS;
            tb.Columns["TrainerType"].ColumnName = HeaderName.TrainerTypeWNS;
            tb.Columns["CreaterofSchedule"].ColumnName = HeaderName.CreaterofSchedule;
            tb.Columns["Academy"].ColumnName = HeaderName.AcademyName;
            tb.Columns["SupervisorUserId"].ColumnName = HeaderName.SupervisorUserId;
            tb.Columns["SupervisorEmailId"].ColumnName = HeaderName.SupervisorEmailId;
            tb.Columns["SupervisorUserName"].ColumnName = HeaderName.SupervisorName;
            tb.Columns["Category"].ColumnName = HeaderName.Category;
            tb.Columns["SubCategory"].ColumnName = HeaderName.SubCategory;

            return tb;
        }

        public DataTable ToDataTableLearningAcademyDashboardReport<APILearningAcademyDashboardReportData>(IEnumerable<APILearningAcademyDashboardReportData> items)
        {
            var tb = new DataTable(typeof(APILearningAcademyDashboardReportData).Name);

            PropertyInfo[] props = typeof(APILearningAcademyDashboardReportData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["Data"].ColumnName = HeaderName.Data;
            tb.Columns["Coverage"].ColumnName = HeaderName.Coverage;
            tb.Columns["TrainingHours"].ColumnName = HeaderName.TrainingHours;
            tb.Columns["ManDays"].ColumnName = HeaderName.ManDays;
            tb.Columns["NoShow"].ColumnName = HeaderName.NoShow;
            tb.Columns["UniqueIds"].ColumnName = HeaderName.UniqueIds;
            tb.Columns["Schedule"].ColumnName = HeaderName.ScheduleSessions;
            tb.Columns["Completed"].ColumnName = HeaderName.CompletedSessions;

            return tb;
        }

        public DataTable ToDataTableNPSDashboardReport<APINPSDashboardReportData>(IEnumerable<APINPSDashboardReportData> items)
        {
            var tb = new DataTable(typeof(APINPSDashboardReportData).Name);

            PropertyInfo[] props = typeof(APINPSDashboardReportData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["TrainingCode"].ColumnName = HeaderName.TrainingCode;
            tb.Columns["WorkshopName"].ColumnName = HeaderName.WorkShopName;
            tb.Columns["StartDate"].ColumnName = HeaderName.StartDateWNS;
            tb.Columns["Month"].ColumnName = HeaderName.Month;
            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerNameWNS;
            tb.Columns["Promoters"].ColumnName = HeaderName.Promoters;
            tb.Columns["Detractors"].ColumnName = HeaderName.Detractors;
            tb.Columns["Total"].ColumnName = HeaderName.Total;
            tb.Columns["PromotersPercentage"].ColumnName = HeaderName.PromotersPercentage;
            tb.Columns["DetractorsPercentage"].ColumnName = HeaderName.DetractorsPercentage;
            tb.Columns["NPS"].ColumnName = HeaderName.NPS;

            return tb;
        }

        public DataTable ToDataTableTrainerUtilizationReport<APITrainerUtilizationReportData>(IEnumerable<APITrainerUtilizationReportData> items)
        {
            var tb = new DataTable(typeof(APITrainerUtilizationReportData).Name);

            PropertyInfo[] props = typeof(APITrainerUtilizationReportData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["TrainerName"].ColumnName = HeaderName.TrainerNameWNS;
            tb.Columns["WorkshopsRequired"].ColumnName = HeaderName.WorkshopsRequired;
            tb.Columns["WorkshopsConduted"].ColumnName = HeaderName.WorkshopsConduted;
            tb.Columns["Utilization"].ColumnName = HeaderName.Utilization;


            return tb;
        }

        public DataTable ToDataTableDevPlanReport<APIDevPlanCompletionReport>(IEnumerable<APIDevPlanCompletionReport> items)
        {
            var tb = new DataTable(typeof(APIDevPlanCompletionReport).Name);

            PropertyInfo[] props = typeof(APIDevPlanCompletionReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["DevelopementPlanCode"].ColumnName = HeaderName.Code;
            tb.Columns["DevelopementPlanName"].ColumnName = HeaderName.Name;
            tb.Columns["CourseStartDate"].ColumnName = HeaderName.StartDate;
            tb.Columns["CourseCompletionDate"].ColumnName = HeaderName.CompeletionDate;
            tb.Columns["Status"].ColumnName = HeaderName.Status;
            tb.Columns["UserStatus"].ColumnName = HeaderName.UserStatus;


            return tb;
        }


        public DataTable ToDataTableTrainingFeedbackSurveyReport<APITrainingFeedbackSurveyReportData>(IEnumerable<APITrainingFeedbackSurveyReportData> items)
        {
            var tb = new DataTable(typeof(APITrainingFeedbackSurveyReportData).Name);

            PropertyInfo[] props = typeof(APITrainingFeedbackSurveyReportData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["EmployeeID"].ColumnName = HeaderName.EmployeeID;
            tb.Columns["NameofParticipant"].ColumnName = HeaderName.NameofParticipant;
            tb.Columns["TrainingCode"].ColumnName = HeaderName.TrainingCode;
            tb.Columns["WorkShopName"].ColumnName = HeaderName.WorkShopName;
            tb.Columns["Designation"].ColumnName = HeaderName.Designation;
            tb.Columns["Grade"].ColumnName = HeaderName.Grade;
            tb.Columns["Vertical"].ColumnName = HeaderName.Vertical;
            tb.Columns["Horizontal"].ColumnName = HeaderName.Horizontal;
            tb.Columns["DepartmentLong"].ColumnName = HeaderName.DepartmentLong;
            tb.Columns["PrimaryInstructorID"].ColumnName = HeaderName.PrimaryInstructorID;
            tb.Columns["PrimaryInstructorName"].ColumnName = HeaderName.PrimaryInstructorName;
            tb.Columns["ActivityStartDate"].ColumnName = HeaderName.ActivityStartDate;
            tb.Columns["ActivityEndDate"].ColumnName = HeaderName.ActivityEndDate;
            tb.Columns["WorkshopLocation"].ColumnName = HeaderName.WorkshopLocation;
            tb.Columns["EmployeeLocation"].ColumnName = HeaderName.EmployeeLocation;
            tb.Columns["QuestionText"].ColumnName = HeaderName.QuestionText;
            tb.Columns["Answer"].ColumnName = HeaderName.Answer;
            tb.Columns["Month"].ColumnName = HeaderName.Month;
            tb.Columns["FY"].ColumnName = HeaderName.FY;
            tb.Columns["Academy"].ColumnName = HeaderName.AcademyName;


            return tb;
        }

        public DataTable ToDataTableLABPLearningJourneyReport<APILABPLearningJourneyReportData>(IEnumerable<APILABPLearningJourneyReportData> items)
        {
            var tb = new DataTable(typeof(APILABPLearningJourneyReportData).Name);

            PropertyInfo[] props = typeof(APILABPLearningJourneyReportData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["journey_title"].ColumnName = HeaderName.JourneyTitle;
            tb.Columns["pathway_title"].ColumnName = HeaderName.PathwayTitle;
            tb.Columns["Completed"].ColumnName = HeaderName.Completed;
            tb.Columns["InProgress"].ColumnName = HeaderName.Inprogress;
            tb.Columns["NotStarted"].ColumnName = HeaderName.NotStarted;


            return tb;
        }


        public DataTable ToDataTableLABPLearningJourneyReportMonthlyView<APILABPLearningJourneyReportMonthlyViewData>(IEnumerable<APILABPLearningJourneyReportMonthlyViewData> items)
        {
            var tb = new DataTable(typeof(APILABPLearningJourneyReportMonthlyViewData).Name);

            PropertyInfo[] props = typeof(APILABPLearningJourneyReportMonthlyViewData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["journey_title"].ColumnName = HeaderName.JourneyTitle;
            tb.Columns["pathway_title"].ColumnName = HeaderName.PathwayTitle;
            tb.Columns["Apr"].ColumnName = HeaderName.Apr;
            tb.Columns["May"].ColumnName = HeaderName.May;
            tb.Columns["Jun"].ColumnName = HeaderName.Jun;
            tb.Columns["Jul"].ColumnName = HeaderName.Jul;
            tb.Columns["Aug"].ColumnName = HeaderName.Aug;
            tb.Columns["Sep"].ColumnName = HeaderName.Sep;
            tb.Columns["Oct"].ColumnName = HeaderName.Oct;
            tb.Columns["Nov"].ColumnName = HeaderName.Nov;
            tb.Columns["Dec"].ColumnName = HeaderName.Dec;
            tb.Columns["Jan"].ColumnName = HeaderName.Jan;
            tb.Columns["Feb"].ColumnName = HeaderName.Feb;
            tb.Columns["Mar"].ColumnName = HeaderName.Mar;
            tb.Columns["Total"].ColumnName = HeaderName.Total;

            return tb;
        }


        public DataTable ToDataTableLABPLearningJourneyReportRawData<APILABPLearningJourneyReportRawData>(IEnumerable<APILABPLearningJourneyReportRawData> items)
        {
            var tb = new DataTable(typeof(APILABPLearningJourneyReportRawData).Name);

            PropertyInfo[] props = typeof(APILABPLearningJourneyReportRawData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["journey_title"].ColumnName = HeaderName.JourneyTitle;
            tb.Columns["pathway_title"].ColumnName = HeaderName.PathwayTitle;
            tb.Columns["Status"].ColumnName = HeaderName.Status;
            tb.Columns["EmployeeID"].ColumnName = HeaderName.EmployeeID;
            tb.Columns["UserName"].ColumnName = HeaderName.UserName;
            tb.Columns["Vertical"].ColumnName = HeaderName.Vertical;
            tb.Columns["Horizontal"].ColumnName = HeaderName.Horizontal;
            tb.Columns["Country"].ColumnName = HeaderName.Country;
            tb.Columns["RoleBand"].ColumnName = HeaderName.RoleBand;
            tb.Columns["Client"].ColumnName = HeaderName.Client;
            tb.Columns["Location"].ColumnName = HeaderName.Location;

            return tb;
        }

        public DataTable ToDataTableLABPCardReport<APILABPCardReportData>(IEnumerable<APILABPCardReportData> items)
        {
            var tb = new DataTable(typeof(APILABPCardReportData).Name);

            PropertyInfo[] props = typeof(APILABPCardReportData).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }

            tb.Columns["smart_title"].ColumnName = HeaderName.Smart_Title;
            tb.Columns["Completed"].ColumnName = HeaderName.Completed;
            tb.Columns["InProgress"].ColumnName = HeaderName.Inprogress;
            tb.Columns["NotStarted"].ColumnName = HeaderName.NotStarted;


            return tb;
        }

        public DataTable ToDataTableILTConsolidatedReport<ILTConsolidatedReport>(IEnumerable<ILTConsolidatedReport> items)
        {
            var tb = new DataTable(typeof(ILTConsolidatedReport).Name);

            PropertyInfo[] props = typeof(ILTConsolidatedReport).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                tb.Columns.Add(prop.Name, prop.PropertyType); //{"DataSet does not support System.Nullable<>."} Hence, the workaround
            }

            foreach (var item in items)
            {
                var values = new object[props.Length];
                for (var i = 0; i < props.Length; i++)
                {
                    values[i] = props[i].GetValue(item, null);
                }

                tb.Rows.Add(values);
            }


            //tb.Columns["Date"].ColumnName = HeaderName.Date;
            //tb.Columns["IsLoggedInWeb"].ColumnName = HeaderName.IsLoggedInWeb;
            //tb.Columns["UserId"].ColumnName = HeaderName.UserId;
            //tb.Columns["Username"].ColumnName = HeaderName.UserName;
            //tb.Columns["LoggedInTime"].ColumnName = HeaderName.LoggedInTime;
            //tb.Columns["LogOutTime"].ColumnName = HeaderName.LogOutTime;

            return tb;
        }

    }
}
