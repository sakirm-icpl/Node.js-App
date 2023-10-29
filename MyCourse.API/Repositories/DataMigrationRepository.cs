using MyCourse.API.APIModel;
//using Assessment.API.Models;
using MyCourse.API.Repositories;
using MyCourse.API.APIModel;
using MyCourse.API.Helper;
//using MyCourse.API.Model.ILT;
using MyCourse.API.Model;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.Repositories.Interfaces;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using log4net;
using MyCourse.API.Helper.Metadata;
using System.Data.Common;
using MyCourse.API.Common;
using MyCourse.API.Repositories.Interface;

namespace MyCourse.API.Repositories
{
    public class DataMigrationRepository : IDataMigration
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DataMigrationRepository));
        private CourseContext _db;
        private IConfiguration _configuration;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private IAssessmentQuestion _assessmentQuestion;
        private IAsessmentQuestionOption _asessmentQuestionOption;
        private ICourseRepository _courseRepository;
        StringBuilder sb = new StringBuilder();

        string[] header = { };
        string[] headerStar = { };
        string[] headerWithoutStar = { };
        List<string> CourseCompletionRecord = new List<string>();
        APIDataMigration dataMigrationRecords = new APIDataMigration();

        APICourseModuleDataMigration CourseModuleDataMigration = new APICourseModuleDataMigration();
        List<AssessmentOptions> AssOptions = new List<AssessmentOptions>();
        APIAssDataMigration assDataMigration = new APIAssDataMigration();

        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;

        public DataMigrationRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionStringRepository, IAssessmentQuestion assessmentQuestion, IAsessmentQuestionOption asessmentQuestionOption,
                ICourseRepository courseRepository)
        {
            _db = context;
            this._configuration = configuration;
            _customerConnectionStringRepository = customerConnectionStringRepository;
            _assessmentQuestion = assessmentQuestion;
            _asessmentQuestionOption = asessmentQuestionOption;
            _courseRepository = courseRepository;
        }

        #region Coursecompletion
        public async Task<ApiResponse> ProcessImportFile(FileInfo file, IDataMigration _dataMigration, int UserId, string OrgCode)
        {
            ApiResponse Response = new ApiResponse();
            try
            {

                Reset();
                bool resultMessage = await InitilizeAsync(file);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(_dataMigration, UserId, OrgCode);
                    Reset();
                    return Response;
                }
                else
                {

                    Response.StatusCode = 204;
                    Response.ResponseObject = Record.FileInvalid;
                    Reset();
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;
        }

        public void Reset()
        {
            sb.Clear();

            header = new string[0];
            headerStar = new string[0];
            headerWithoutStar = new string[0];
            CourseCompletionRecord.Clear();

            sbError.Clear();
            totalRecordInsert = 0;
            totalRecordRejected = 0;
        }


        public async Task<bool> InitilizeAsync(FileInfo file)

        {
            bool result = false;
            try
            {
                using (ExcelPackage package = new ExcelPackage(file))
                {

                    ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

                    int rowCount = worksheet.Dimension.Rows;
                    int ColCount = worksheet.Dimension.Columns;
                    for (int row = 1; row <= rowCount; row++)
                    {
                        for (int col = 1; col <= ColCount; col++)
                        {
                            string append = "";
                            if (worksheet.Cells[row, col].Value == null)
                            {

                            }
                            else
                            {
                                append = Convert.ToString(worksheet.Cells[row, col].Value.ToString());
                            }
                            string finalAppend = append + "\t";
                            sb.Append(finalAppend);

                        }
                        sb.Append(Environment.NewLine);
                    }

                    string fileInfo = sb.ToString();
                    CourseCompletionRecord = new List<string>(fileInfo.Split('\n'));
                    foreach (string record in CourseCompletionRecord)
                    {
                        string[] mainsp = record.Split('\r');
                        string[] mainsp2 = mainsp[0].Split('\"');
                        header = mainsp2[0].Split('\t');
                        headerStar = mainsp2[0].Split('\t');
                        break;
                    }
                    CourseCompletionRecord.RemoveAt(0);
                    result = true;

                }
                /////Remove Star from Header
                for (int i = 0; i < header.Count(); i++)
                {
                    header[i] = header[i].Replace("*", "");

                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                result = false;
            }


            return result;
        }

        public async Task<ApiResponse> ProcessRecordsAsync(IDataMigration _dataMigration, int userid, string OrgCode)
        {
            List<APIDataMigrationRejected> aPIDataMigrationRejectedList = new List<APIDataMigrationRejected>();
            if (CourseCompletionRecord != null && CourseCompletionRecord.Count > 0)
            {

                List<APIDataMigration> aPIDataMigration = new List<APIDataMigration>();
                var applicationDateFormat = await _courseRepository.GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);
                foreach (string record in CourseCompletionRecord)
                {

                    int countLenght = record.Length;
                    if (record != null && countLenght > 1)
                    {
                        string[] textpart = record.Split('\t');
                        string[][] mainRecord = { header, textpart };

                        string lblEmployeeCode = string.Empty;
                        string lblCourseCode = string.Empty;
                        string lblScheduleCode = string.Empty;
                        string lblModifiedDate = string.Empty;
                        string lblStartDate = string.Empty;

                        string lblAssessmentPercentage = string.Empty;
                        string lblAssessmentResult = string.Empty;
                        string lblMarksObtained = string.Empty;
                        string lblNoOfAttempts = string.Empty;

                        string lblScormScore = string.Empty;
                        string lblScormResult = string.Empty;


                        string headerText = "";

                        int arrasize = header.Count();

                        for (int j = 0; j < arrasize - 1; j++)
                        {
                            headerText = header[j].Trim();
                            string[] mainspilt = headerText.Split('\t');

                            headerText = mainspilt[0];
                            if (headerText == "UserId")
                            {
                                try
                                {

                                    string[] EmployeeCodeSplit = mainRecord[1][j].Split('\t');
                                    lblEmployeeCode = EmployeeCodeSplit[0].Trim().ToLower().Encrypt();
                                    bool valid = ValidateEmployeeCode(headerText, lblEmployeeCode);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                            else
                            if (headerText == "CourseCode")
                            {
                                try
                                {
                                    string[] CourseCodesplit = mainRecord[1][j].Split('\t');
                                    lblCourseCode = CourseCodesplit[0].Trim();
                                    bool valid = ValidateCourseCode(headerText, lblCourseCode);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                            else
                            if (headerText == "LastActivityDate")
                            {
                                try
                                {
                                    string[] LastActivityDatesplit = mainRecord[1][j].Split('\t');
                                    lblModifiedDate = LastActivityDatesplit[0].Trim();

                                    bool valid = ValidateLastActivityDate(lblModifiedDate, applicationDateFormat);

                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }

                            else
                            if (headerText == "StartDate")
                            {
                                try
                                {
                                    string[] LastActivityDatesplit = mainRecord[1][j].Split('\t');
                                    lblStartDate = LastActivityDatesplit[0].Trim();

                                    bool valid = ValidateStartDate(lblStartDate, applicationDateFormat);

                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }


                            else if (headerText == "ScheduleCode")
                            {
                                string[] ScheduleCodesplit = mainRecord[1][j].Split('\t');
                                lblScheduleCode = ScheduleCodesplit[0].Trim();

                                if (lblScheduleCode != null && !string.IsNullOrEmpty(lblScheduleCode))
                                {
                                    dataMigrationRecords.ScheduleCode = lblScheduleCode;

                                }
                                else
                                {
                                    dataMigrationRecords.ScheduleCode = null;
                                }

                            }


                            else if (headerText == "External_AssessmentMarksObtained")
                            {
                                string[] ScheduleCodesplit = mainRecord[1][j].Split('\t');
                                lblMarksObtained = ScheduleCodesplit[0].Trim();

                                if (lblMarksObtained != null && !string.IsNullOrEmpty(lblMarksObtained))
                                {
                                    dataMigrationRecords.MarksObtained = lblMarksObtained;

                                }
                                else
                                {
                                    dataMigrationRecords.MarksObtained = null;

                                }


                            }
                            else if (headerText == "External_NoOfAttempts")
                            {
                                string[] NoOfAttemptssplit = mainRecord[1][j].Split('\t');
                                lblNoOfAttempts = NoOfAttemptssplit[0].Trim();

                                if (lblNoOfAttempts != null && !string.IsNullOrEmpty(lblNoOfAttempts))
                                {
                                    dataMigrationRecords.NoOfAttempts = Convert.ToInt32(lblNoOfAttempts);

                                }
                                else
                                {
                                    dataMigrationRecords.NoOfAttempts = null;

                                }
                            }

                            else if (headerText == "External_AssessmentResult")
                            {
                                string[] AssessmentResultsplit = mainRecord[1][j].Split('\t');
                                lblAssessmentResult = AssessmentResultsplit[0].Trim();

                                if (lblAssessmentResult != null && !string.IsNullOrEmpty(lblAssessmentResult))
                                {
                                    dataMigrationRecords.AssessmentResult = lblAssessmentResult;

                                }
                                else
                                {
                                    dataMigrationRecords.AssessmentResult = null;
                                }

                            }

                            else if (headerText == "Inbuilt_AssessmentMarks")
                            {
                                string[] Inbuilt_AssessmentMarkssplit = mainRecord[1][j].Split('\t');
                                lblScormScore = Inbuilt_AssessmentMarkssplit[0].Trim();

                                if (lblScormScore != null && !string.IsNullOrEmpty(lblScormScore))
                                {
                                    dataMigrationRecords.ScormScore = Convert.ToInt32(lblScormScore);

                                }
                                else
                                {
                                    dataMigrationRecords.ScormScore = null;
                                }

                            }
                            else if (headerText == "Inbuilt_Result")
                            {
                                string[] Inbuilt_Resultsplit = mainRecord[1][j].Split('\t');
                                lblScormScore = Inbuilt_Resultsplit[0].Trim();

                                if (lblScormScore != null && !string.IsNullOrEmpty(lblScormScore))
                                {
                                    dataMigrationRecords.ScormResult = lblScormScore;

                                }
                                else
                                {
                                    dataMigrationRecords.ScormResult = null;
                                }

                            }
                        }


                        try
                        {
                            if (string.IsNullOrEmpty(sbError.ToString()))
                            {
                                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                                {

                                    var connection = dbContext.Database.GetDbConnection();
                                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                        connection.Open();

                                    DynamicParameters parameters = new DynamicParameters();
                                    parameters.Add("@EmployeeCode", dataMigrationRecords.EmployeeCode);
                                    parameters.Add("@CourseCode", dataMigrationRecords.CourseCode);
                                    parameters.Add("@ScheduleCode", dataMigrationRecords.ScheduleCode);
                                    parameters.Add("@LastActivityDate", dataMigrationRecords.ModifiedDate);
                                    parameters.Add("@External_AssessmentResult", dataMigrationRecords.AssessmentResult);
                                    parameters.Add("@External_AssessmentMarksObtained", dataMigrationRecords.MarksObtained);
                                    parameters.Add("@External_NoOfAttempts", dataMigrationRecords.NoOfAttempts);
                                    parameters.Add("@Inbuilt_AssessmentMarks", dataMigrationRecords.ScormScore);
                                    parameters.Add("@Inbuilt_Result", dataMigrationRecords.ScormResult);
                                    parameters.Add("@StartDate", dataMigrationRecords.StartDate);

                                    var Result = await SqlMapper.QueryAsync<APIDataMigration>((SqlConnection)connection, "dbo.SMS_CourseCompletion_BulkUpload", parameters, null, null, CommandType.StoredProcedure);
                                    APIDataMigration resAPIDataMigration = Result.FirstOrDefault();
                                    dataMigrationRecords.ErrMessage = resAPIDataMigration.ErrMessage;
                                    dataMigrationRecords.IsInserted = resAPIDataMigration.IsInserted;
                                    dataMigrationRecords.IsUpdated = resAPIDataMigration.IsUpdated;
                                    dataMigrationRecords.InsertedID = resAPIDataMigration.InsertedID;
                                    dataMigrationRecords.InsertedCode = resAPIDataMigration.InsertedCode;
                                    dataMigrationRecords.notInsertedCode = resAPIDataMigration.notInsertedCode;

                                    aPIDataMigration.Add(dataMigrationRecords);

                                    connection.Close();

                                }
                            }
                            else
                            {
                                dataMigrationRecords.ErrMessage = sbError.ToString();
                                dataMigrationRecords.IsInserted = "false";
                                dataMigrationRecords.IsUpdated = "false";
                                dataMigrationRecords.InsertedID = null;
                                dataMigrationRecords.InsertedCode = null;
                                dataMigrationRecords.notInsertedCode = dataMigrationRecords.EmployeeCode;
                                aPIDataMigration.Add(dataMigrationRecords);
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        dataMigrationRecords = new APIDataMigration();
                        sbError.Clear();
                    }
                    else
                    {
                    }
                }

                foreach (var data in aPIDataMigration)
                {
                    if (!string.IsNullOrEmpty(data.CourseCode))
                    {
                        if (data.ErrMessage != null)
                        {

                            APIDataMigrationRejected aPIDataMigrationRejected = new APIDataMigrationRejected();

                            totalRecordRejected++;
                            aPIDataMigrationRejected.EmployeeCode = data.EmployeeCode.Decrypt();
                            aPIDataMigrationRejected.CourseCode = data.CourseCode;
                            aPIDataMigrationRejected.ScheduleCode = data.ScheduleCode;
                            aPIDataMigrationRejected.ErrMessage = data.ErrMessage;

                            aPIDataMigrationRejectedList.Add(aPIDataMigrationRejected);
                        }
                        else
                        {
                            totalRecordInsert++;

                        }
                    }
                }
            }
            string resultstring = "Total number of record inserted :" + totalRecordInsert + ",  Total number of record record rejected : " + totalRecordRejected;
            ApiResponse response = new ApiResponse();
            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, aPIDataMigrationRejectedList };
            return response;
        }

        public bool ValidateEmployeeCode(string headerText, string EmployeeCode)
        {
            bool valid = false;

            //UserId
            try
            {
                if (EmployeeCode != null && !string.IsNullOrEmpty(EmployeeCode))
                {
                    dataMigrationRecords.EmployeeCode = EmployeeCode;

                }
                else
                {
                    dataMigrationRecords.EmployeeCode = EmployeeCode;
                    if (string.IsNullOrEmpty(sbError.ToString()))
                        sbError.Append("EmployeeCode required");
                    else
                        sbError.Append(", EmployeeCode required");

                    valid = true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }

        public bool ValidateCourseCode(string headerText, string CourseCode)
        {
            bool valid = false;

            //UserId
            try
            {

                if (CourseCode != null && !string.IsNullOrEmpty(CourseCode))
                {
                    dataMigrationRecords.CourseCode = CourseCode;

                }
                else
                {
                    dataMigrationRecords.CourseCode = CourseCode;
                    if (string.IsNullOrEmpty(sbError.ToString()))
                        sbError.Append("CourseCode required");
                    else
                        sbError.Append(", CourseCode required");
                    valid = true;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        public bool ValidateCourseType(string headerText, string CourseType)
        {
            bool valid = false;

            //CourseType
            try
            {

                if (CourseType != null && !string.IsNullOrEmpty(CourseType))
                {
                    CourseModuleDataMigration.CourseType = CourseType;

                }
                else
                {
                    CourseModuleDataMigration.CourseType = CourseType;

                    sbError.Append("CourseType is null");
                    valid = true;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        public bool ValidateContentType(string headerText, string ContentType)
        {
            bool valid = false;

            //CourseType
            try
            {
                if (ContentType != null && !string.IsNullOrEmpty(ContentType))
                {
                    CourseModuleDataMigration.ContentType = ContentType;
                }
                else
                {
                    CourseModuleDataMigration.ContentType = ContentType;
                    sbError.Append("ContentType is null");
                    valid = true;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        public bool ValidateMetaData(string headerText, string MetaData)
        {
            bool valid = false;

            //CourseType
            try
            {
                if (MetaData != null && !string.IsNullOrEmpty(MetaData))
                {
                    CourseModuleDataMigration.MetaData = MetaData;
                }
                else
                {
                    CourseModuleDataMigration.MetaData = MetaData;
                    sbError.Append("MetaData is null");
                    valid = true;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        public bool ValidateIsApplicableToAll(string headerText, string IsApplicableToAll)
        {
            bool valid = false;

            //CourseType
            try
            {
                if (IsApplicableToAll != null && !string.IsNullOrEmpty(IsApplicableToAll))
                {
                    CourseModuleDataMigration.IsApplicableToAll = IsApplicableToAll;
                }
                else
                {
                    CourseModuleDataMigration.IsApplicableToAll = IsApplicableToAll;
                    sbError.Append("IsApplicableToAll is null");
                    valid = true;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        public bool ValidateModuleType(string headerText, string ModuleType)
        {
            bool valid = false;

            //CourseType
            try
            {
                if (ModuleType != null && !string.IsNullOrEmpty(ModuleType))
                {
                    CourseModuleDataMigration.ModuleType = ModuleType;
                }
                else
                {
                    CourseModuleDataMigration.ModuleType = ModuleType;
                    sbError.Append("ModuleType is null");
                    valid = true;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        public bool ValidateCourseName(string headerText, string CourseName)
        {
            bool valid = false;

            //CourseName
            try
            {

                if (CourseName != null && !string.IsNullOrEmpty(CourseName))
                {
                    CourseModuleDataMigration.CourseName = CourseName;
                }
                else
                {
                    CourseModuleDataMigration.CourseName = CourseName;

                    sbError.Append("CourseName is null");
                    valid = true;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        public bool ValidateLastActivityDate(string ModifiedDate, string validDateFormat)
        {
            bool valid = false;

            //UserId
            try
            {

                if (ModifiedDate != null && !string.IsNullOrEmpty(ModifiedDate))
                {
                    string attDate = ValidateDateFormat(ModifiedDate, validDateFormat);
                    if (attDate == null)
                    {
                        dataMigrationRecords.ModifiedDate = null;
                        if (string.IsNullOrEmpty(sbError.ToString()))
                            sbError.Append("Last activity date is not in " + validDateFormat + " format");
                        else
                            sbError.Append(", Last activity date is not in " + validDateFormat + " format");
                        valid = true;
                    }
                    else
                    {
                        dataMigrationRecords.ModifiedDate = attDate;
                    }
                }
                else
                {
                    dataMigrationRecords.ModifiedDate = null;
                    if (string.IsNullOrEmpty(sbError.ToString()))
                        sbError.Append("LastActivityDate required");
                    else
                        sbError.Append(", LastActivityDate required");
                    valid = true;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        public string ValidateDateFormat(string date, string validDateFormat)
        {
            string resultDate = null;
            try
            {
                DateTime result;
                DateTime date1 = DateTime.Now;
                if (date.Length == 5)
                {
                    date1 = DateTime.FromOADate(Convert.ToInt32(date));

                    string inputstring = date1.ToString("dd/MM/yyyy");
                    inputstring = inputstring.Replace("-", "/");
                    inputstring = inputstring.Replace(".", "/");
                    string[] dateParts = inputstring.Split('/');
                    string day = dateParts[0];
                    string month = dateParts[1];
                    string year = dateParts[2];
                    resultDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).ToString("yyyy-MM-dd HH:mm:ss");
                    string resultdate1 = resultDate.Substring(0, 13);
                    string minute = resultDate.Substring(14, 2);
                    string second = resultDate.Substring(17, 2);
                    resultDate = resultdate1 + ":" + minute + ":" + second;
                }
                else
                {
                    switch (validDateFormat)
                    {
                        case "dd MMM yyyy":
                            {
                                date = date.Substring(0, 11);
                                break;
                            }
                        case "MMM dd yyyy":
                            {
                                date = date.Substring(0, 11);
                                break;
                            }
                        case "yyyy/MM/dd":
                            {
                                date = date.Substring(0, 10);
                                break;
                            }
                        case "yyyy-MM-dd":
                            {
                                date = date.Substring(0, 10);
                                break;
                            }
                        case "dd-MM-yyyy":
                            {
                                date = date.Substring(0, 10);
                                break;
                            }
                        case "dd/MM/yyyy":
                            {
                                date = date.Substring(0, 10);
                                break;
                            }
                        case "dd MM yyyy":
                            {
                                date = date.Substring(0, 10);
                                break;
                            }
                    }
                    string[] dateParts = new string[3];
                    if (validDateFormat != "dd-MM-yyyy" && validDateFormat != "dd/MM/yyyy" && validDateFormat != "dd MM yyyy")
                    {
                        result = DateTime.ParseExact(date, validDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                        string inputstring = result.ToString("dd/MM/yyyy");
                        inputstring = inputstring.Replace("-", "/");
                        inputstring = inputstring.Replace(".", "/");
                        dateParts = inputstring.Split('/');
                        
                    }
                    else
                    {
                        if(validDateFormat == "dd-MM-yyyy")
                        {
                            string inputstring = date;
                            inputstring = inputstring.Replace("-", "-");
                            inputstring = inputstring.Replace(".", "-");
                            dateParts = inputstring.Split('-');
                        }
                        else if (validDateFormat == "dd/MM/yyyy")
                        {
                            string inputstring = date;
                            inputstring = inputstring.Replace("-", "/");
                            inputstring = inputstring.Replace(".", "/");
                            dateParts = inputstring.Split('/');
                        }
                        else if(validDateFormat == "dd MM yyyy")
                        {
                            string inputstring = date;
                            inputstring = inputstring.Replace("-", " ");
                            inputstring = inputstring.Replace(".", " ");
                            dateParts = inputstring.Split(' ');
                        }
                    }
                    string day = dateParts[0];
                    string month = dateParts[1];
                    string year = dateParts[2];
                    resultDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).ToString("yyyy-MM-dd HH:mm:ss");
                    string resultdate1 = resultDate.Substring(0, 13);
                    string minute = resultDate.Substring(14, 2);
                    string second = resultDate.Substring(17, 2);
                    resultDate = resultdate1 + ":" + minute + ":" + second;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return resultDate;
        }
        public bool ValidateCourseCo(string headerText, string CourseCode)
        {
            bool valid = false;

            //CourseType
            try
            {
                if (CourseCode != null && !string.IsNullOrEmpty(CourseCode))
                {
                    CourseModuleDataMigration.CourseCode = CourseCode;
                }
                else
                {
                    CourseModuleDataMigration.CourseCode = CourseCode;
                    sbError.Append("CourseCode is null");
                    valid = true;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }

        public bool ValidateStartDate(string StartDate, string validDateFormat)
        {
            bool valid = false;

            //UserId
            try
            {

                if (StartDate != null && !string.IsNullOrEmpty(StartDate))
                {
                    string attDate = ValidateDateFormat(StartDate, validDateFormat);
                    if (attDate == null)
                    {
                        dataMigrationRecords.ModifiedDate = null;
                        if (string.IsNullOrEmpty(sbError.ToString()))
                            sbError.Append("Start date is not in " + validDateFormat + " format");
                        else
                            sbError.Append(", Start date is not in " + validDateFormat + " format");
                        valid = true;
                    }
                    else
                    {
                        dataMigrationRecords.StartDate = attDate;
                    }
                }
                else
                {
                    dataMigrationRecords.StartDate = null;
                    if (string.IsNullOrEmpty(sbError.ToString()))
                        sbError.Append("StartDate required");
                    else
                        sbError.Append(", StartDate required");
                    valid = true;
                }

            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        #endregion





        #region CompetencyImport


        public async Task<ApiResponse> ProcessImportFile_Competency(FileInfo file, IDataMigration _dataMigration, int UserId)
        {
            ApiResponse Response = new ApiResponse();
            try
            {

                Reset();
                bool resultMessage = await InitilizeAsync(file);
                if (resultMessage == true)
                {
                    Response = await ProcessCompetenctRecordRecordsAsync(_dataMigration, UserId);
                    Reset();
                    return Response;
                }
                else
                {
                    Response.StatusCode = 204;
                    Response.ResponseObject = Record.FileInvalid;
                    Reset();
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;
        }

        public async Task<ApiResponse> ProcessCompetenctRecordRecordsAsync(IDataMigration _dataMigration, int userid)
        {
            List<APIDataMigrationCompetencyRejected> aPIDataMigrationRejectedList = new List<APIDataMigrationCompetencyRejected>();
            if (CourseCompletionRecord != null && CourseCompletionRecord.Count > 0)
            {
                List<APICompetencyImport> aPIDataMigration = new List<APICompetencyImport>();
                foreach (string record in CourseCompletionRecord)
                {
                    StringBuilder sbErrorCompetency = new StringBuilder();
                    APICompetencyImport dataMigrationRecords_Competency = new APICompetencyImport();

                    int countLenght = record.Length;
                    if (record != null && countLenght > 1)
                    {
                        string[] textpart = record.Split('\t');
                        string[][] mainRecord = { header, textpart };

                        string lblCategoryCode = string.Empty;
                        string lblCategoryName = string.Empty;
                        string lblCompetencyName = string.Empty;
                        string lblCompetencyDescription = string.Empty;
                        string lblCompetencyLevel = string.Empty;
                        string lblLevelDescription = string.Empty;
                        string lblCourseCode = string.Empty;
                        string headerText = "";
                        int arrasize = header.Count();

                        for (int j = 0; j < arrasize - 1; j++)
                        {
                            headerText = header[j].Trim();
                            string[] mainspilt = headerText.Split('\t');

                            headerText = mainspilt[0];
                            if (headerText == "Category")
                            {
                                try
                                {
                                    string[] CategoryCodeSplit = mainRecord[1][j].Split('\t');
                                    lblCategoryCode = CategoryCodeSplit[0].Trim();

                                    if (lblCategoryCode != null && !string.IsNullOrEmpty(lblCategoryCode))
                                    {
                                        dataMigrationRecords_Competency.CategoryCode = lblCategoryCode;
                                    }
                                    else
                                    {
                                        dataMigrationRecords_Competency.CategoryCode = null;
                                        sbErrorCompetency.Append("Category is null");
                                        break;
                                    }

                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                            else
                            if (headerText == "CategoryName")
                            {
                                try
                                {
                                    string[] CategoryNamesplit = mainRecord[1][j].Split('\t');
                                    lblCategoryName = CategoryNamesplit[0].Trim();
                                    if (lblCategoryName != null && !string.IsNullOrEmpty(lblCategoryName))
                                    {
                                        dataMigrationRecords_Competency.CategoryName = lblCategoryName;
                                    }
                                    else
                                    {
                                        dataMigrationRecords_Competency.CategoryName = null;
                                        sbErrorCompetency.Append("CategoryName is null");
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                            else
                            if (headerText == "CompetencyName")
                            {
                                try
                                {
                                    string[] CompetencyNamesplit = mainRecord[1][j].Split('\t');
                                    lblCompetencyName = CompetencyNamesplit[0].Trim();

                                    if (lblCompetencyName != null && !string.IsNullOrEmpty(lblCompetencyName))
                                    {
                                        dataMigrationRecords_Competency.CompetencyName = lblCompetencyName;
                                    }
                                    else
                                    {
                                        dataMigrationRecords_Competency.CompetencyName = null;
                                        sbErrorCompetency.Append("CompetencyName is null");
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }

                            else
                            if (headerText == "CompetencyDescription")
                            {
                                try
                                {
                                    string[] CompetencyDescriptionsplit = mainRecord[1][j].Split('\t');
                                    lblCompetencyDescription = CompetencyDescriptionsplit[0].Trim();

                                    if (lblCompetencyDescription != null && !string.IsNullOrEmpty(lblCompetencyDescription))
                                    {
                                        dataMigrationRecords_Competency.CompetencyDescription = lblCompetencyDescription;
                                    }
                                    else
                                    {
                                        dataMigrationRecords_Competency.CompetencyDescription = null;
                                        sbErrorCompetency.Append("CompetencyDescription is null");
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }


                            else if (headerText == "CompetencyLevel")
                            {
                                string[] CompetencyLevelsplit = mainRecord[1][j].Split('\t');
                                lblCompetencyLevel = CompetencyLevelsplit[0].Trim();

                                if (lblCompetencyLevel != null && !string.IsNullOrEmpty(lblCompetencyLevel))
                                {
                                    dataMigrationRecords_Competency.CompetencyLevel = lblCompetencyLevel;
                                }
                                else
                                {
                                    dataMigrationRecords_Competency.CompetencyLevel = null;
                                    sbErrorCompetency.Append("CompetencyLevel is null");
                                }

                            }
                            else if (headerText == "LevelDescription")
                            {
                                string[] LevelDescriptionsplit = mainRecord[1][j].Split('\t');
                                lblLevelDescription = LevelDescriptionsplit[0].Trim();

                                if (lblLevelDescription != null && !string.IsNullOrEmpty(lblLevelDescription))
                                {
                                    dataMigrationRecords_Competency.LevelDescription = lblLevelDescription;
                                }
                                else
                                {
                                    dataMigrationRecords_Competency.LevelDescription = null;
                                    sbErrorCompetency.Append("Competency Level Description is null");
                                }

                            }
                            else if (headerText == "CourseCode")
                            {
                                string[] CourseCodesplit = mainRecord[1][j].Split('\t');
                                lblCourseCode = CourseCodesplit[0].Trim();

                                if (lblCourseCode != null && !string.IsNullOrEmpty(lblCourseCode))
                                {
                                    dataMigrationRecords_Competency.CourseCode = lblCourseCode;
                                }
                                else
                                {
                                    dataMigrationRecords_Competency.CourseCode = null;
                                    sbErrorCompetency.Append("CourseCode is null");
                                }

                            }
                        }

                        try
                        {
                            if (string.IsNullOrEmpty(sbErrorCompetency.ToString()))
                            {

                                using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                                {


                                    var connection = dbContext.Database.GetDbConnection();
                                    if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                        connection.Open();


                                    DynamicParameters parameters = new DynamicParameters();
                                    parameters.Add("@CategoryCode", dataMigrationRecords_Competency.CategoryCode);
                                    parameters.Add("@CategoryName", dataMigrationRecords_Competency.CategoryName);
                                    parameters.Add("@CompetencyName", dataMigrationRecords_Competency.CompetencyName);
                                    parameters.Add("@CompetencyDescription", dataMigrationRecords_Competency.CompetencyDescription);
                                    parameters.Add("@CompetencyLevel", dataMigrationRecords_Competency.CompetencyLevel);
                                    parameters.Add("@LevelDescription", dataMigrationRecords_Competency.LevelDescription);
                                    parameters.Add("@CourseCode", dataMigrationRecords_Competency.CourseCode);

                                    var Result = await SqlMapper.QueryAsync<APICompetencyImport>((SqlConnection)connection, "dbo.CompetencyMasterDataMigration_BulkUpload", parameters, null, null, CommandType.StoredProcedure);

                                    APICompetencyImport resAPIDataMigration = Result.FirstOrDefault();
                                    dataMigrationRecords_Competency.ErrMessage = resAPIDataMigration.ErrMessage;
                                    dataMigrationRecords_Competency.IsInserted = resAPIDataMigration.IsInserted;
                                    dataMigrationRecords_Competency.IsUpdated = resAPIDataMigration.IsUpdated;
                                    dataMigrationRecords_Competency.InsertedID = resAPIDataMigration.InsertedID;
                                    dataMigrationRecords_Competency.InsertedCode = resAPIDataMigration.InsertedCode;
                                    dataMigrationRecords_Competency.notInsertedCode = resAPIDataMigration.notInsertedCode;

                                    aPIDataMigration.Add(dataMigrationRecords_Competency);
                                    connection.Close();
                                }
                            }
                            else
                            {
                                dataMigrationRecords_Competency.ErrMessage = sbErrorCompetency.ToString();
                                dataMigrationRecords_Competency.IsInserted = null;
                                dataMigrationRecords_Competency.IsUpdated = null;
                                dataMigrationRecords_Competency.InsertedID = null;
                                dataMigrationRecords_Competency.InsertedCode = null;
                                dataMigrationRecords_Competency.notInsertedCode = null;
                                aPIDataMigration.Add(dataMigrationRecords_Competency);

                            }

                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));
                        }

                        dataMigrationRecords = new APIDataMigration();
                        sbErrorCompetency.Clear();
                    }
                    else
                    {
                    }
                }

                foreach (var data in aPIDataMigration)
                {
                    if (!string.IsNullOrEmpty(data.CategoryCode))
                    {
                        if (data.ErrMessage != null)
                        {
                            APIDataMigrationCompetencyRejected aPIDataMigrationRejected = new APIDataMigrationCompetencyRejected();
                            totalRecordRejected++;
                            aPIDataMigrationRejected.CategoryCode = data.CategoryCode;
                            aPIDataMigrationRejected.CompetencyName = data.CompetencyName;
                            aPIDataMigrationRejected.CompetencyLevel = data.CompetencyLevel;
                            aPIDataMigrationRejected.ErrMessage = data.ErrMessage;
                            aPIDataMigrationRejectedList.Add(aPIDataMigrationRejected);
                        }
                        else
                        {
                            totalRecordInsert++;

                        }
                    }
                }
            }
            string resultstring = "Total number of record inserted :" + totalRecordInsert + ",  Total number of record record rejected : " + totalRecordRejected;
            ApiResponse response = new ApiResponse();
            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, aPIDataMigrationRejectedList };
            return response;
        }


        #endregion






        #region AssessmentData
        public async Task<ApiResponse> ProcessAssessmentImportFile(FileInfo file, IDataMigration _dataMigration, int UserId)
        {
            ApiResponse Response = new ApiResponse();
            try
            {

                ResetAssessement();
                bool resultMessage = await InitilizeAsync(file);
                if (resultMessage != true)
                {
                    Response = await ProcessRecordsAsync_Assessment(_dataMigration, UserId);
                    ResetAssessement();
                    return Response;
                }
                else
                {

                    Response.StatusCode = 200;
                    Response.ResponseObject = Record.FileInvalid;
                    Reset();
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;
        }
        public async Task<byte[]> ExportImportFormat(string OrgCode)
        {
            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string sFileName = FileName.ILTScheduleImportFormat;
            string DomainName = this._configuration["ApiGatewayUrl"];
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ILTSchedule");
                worksheet.Cells[1, 1].Value = "CourseCode*";
                worksheet.Cells[1, 2].Value = "ModuleName*";
                worksheet.Cells[1, 3].Value = "ScheduleCode*";
                worksheet.Cells[1, 4].Value = "StartDate*";
                worksheet.Cells[1, 5].Value = "EndDate*";
                worksheet.Cells[1, 6].Value = "StartTime*";
                worksheet.Cells[1, 7].Value = "EndTime*";
                worksheet.Cells[1, 8].Value = "RegistrationEndDate*";
                worksheet.Cells[1, 9].Value = "TrainerType*";
                worksheet.Cells[1, 10].Value = "TrainerName*";
                worksheet.Cells[1, 11].Value = "TrainingPlaceType*";
                worksheet.Cells[1, 12].Value = "AcademyAgencyName*";
                worksheet.Cells[1, 13].Value = "PlaceName*";
                worksheet.Cells[1, 14].Value = "SeatCapacity*";
                worksheet.Cells[1, 15].Value = "City*";
                worksheet.Cells[1, 16].Value = "PostalAddress*";
                worksheet.Cells[1, 17].Value = "CoordinatorName";
                worksheet.Cells[1, 18].Value = "ContactNumber";
                worksheet.Cells[1, 19].Value = "Currency";
                worksheet.Cells[1, 20].Value = "Cost";
                using (var rngitems = worksheet.Cells["A1:P1"])//Applying Css for header
                {
                    rngitems.Style.Font.Bold = true;
                }
                package.Save();
            }
            var Fs = file.OpenRead();
            byte[] fileData = null;
            using (BinaryReader binaryReader = new BinaryReader(Fs))
            {
                fileData = binaryReader.ReadBytes((int)Fs.Length);
            }
            return fileData;
        }
        public async Task<ApiResponse> ILTScheduleImportFile(APIDataMigrationFilePath aPIILTScheduleImport, int UserId, string OrgCode)
        {
            ApiResponse Response = new ApiResponse();
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string DomainName = this._configuration["ApiGatewayUrl"];
                string filepath = sWebRootFolder + aPIILTScheduleImport.Path;

                DataTable scheduledt = ReadFile(filepath);
                if (scheduledt == null || scheduledt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(scheduledt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessScheduleRecordsAsync(scheduledt, UserId, OrgCode);
                    return Response;
                }
                else
                {
                    Response.StatusCode = 204;
                    string resultstring = Record.FileInvalid;
                    Response.ResponseObject = new { resultstring };
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;
        }
        public async Task<bool> ValidateFileColumnHeaders(DataTable scheduledt, List<string> importColumns)
        {
            if (scheduledt.Columns.Count != importColumns.Count)
                return false;

            for (int i = 0; i < scheduledt.Columns.Count; i++)
            {
                string col = scheduledt.Columns[i].ColumnName.Replace("*", "").Replace(" ", "");
                scheduledt.Columns[i].ColumnName = col;

                if (!importColumns.Contains(scheduledt.Columns[i].ColumnName))
                    return false;
            }
            return true;
        }
        private List<KeyValuePair<string, int>> GetImportColumns(bool allcolumns = false)
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.CourseCode, 100));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.ModuleName, 600));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.ScheduleCode, 100));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.StartDate, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.EndDate, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.StartTime, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.EndTime, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.RegistrationEndDate, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.TrainerType, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.TrainerName, 600));
            if (allcolumns)
                columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.TrainerNameEncrypted, 2000));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.TrainingPlaceType, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.AcademyAgencyName, 500));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.PlaceName, 500));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.SeatCapacity, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.City, 500));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.PostalAddress, 500));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.CoordinatorName, 500));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.ContactNumber, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.Currency, 50));
            columns.Add(new KeyValuePair<string, int>(APIILTScheduleMigrationImportColumns.Cost, 50));
            return columns;
        }
        private List<KeyValuePair<string, int>> GetTrainingNeedImportColumns()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(APITrainingNeedImportColumns.JobRole, 500));
            columns.Add(new KeyValuePair<string, int>(APITrainingNeedImportColumns.Department, 500));
            columns.Add(new KeyValuePair<string, int>(APITrainingNeedImportColumns.Section, 500));
            columns.Add(new KeyValuePair<string, int>(APITrainingNeedImportColumns.Level, 50));
            columns.Add(new KeyValuePair<string, int>(APITrainingNeedImportColumns.Status, 50));
            columns.Add(new KeyValuePair<string, int>(APITrainingNeedImportColumns.TraniningProgram, 1000));
            columns.Add(new KeyValuePair<string, int>(APITrainingNeedImportColumns.Category, 500));
            return columns;
        }
        public DataTable ReadFile(string filepath)
        {
            DataTable dt = new DataTable();

            using (var pck = new ExcelPackage())
            {
                using (var stream = File.OpenRead(filepath))
                    pck.Load(stream);
                var ws = pck.Workbook.Worksheets.First();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    if (!dt.Columns.Contains(firstRowCell.Text))
                        dt.Columns.Add(firstRowCell.Text.Trim());
                }
                var startRow = 2;
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                    if (!string.IsNullOrEmpty(Convert.ToString(wsRow.ElementAtOrDefault(0))))
                    {
                        DataRow row = dt.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            if (!string.IsNullOrEmpty(cell.Text))
                                row[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                    else
                        break;
                }
            }
            //check for empty rows
            DataTable validDt = new DataTable();
            validDt = dt.Clone();
            foreach (DataRow dataRow in dt.Rows)
            {
                bool IsEmpty = true;
                foreach (DataColumn dataColumn in dt.Columns)
                {
                    if (!string.IsNullOrEmpty(Convert.ToString(dataRow[dataColumn])))
                    {
                        IsEmpty = false;
                        break;
                    }
                }
                if (!IsEmpty)
                    validDt.ImportRow(dataRow);
            }

            return validDt;
        }
        public async Task<ApiResponse> ProcessScheduleRecordsAsync(DataTable scheduledt, int userId, string OrgCode)
        {
            ApiResponse response = new ApiResponse();
            var applicationDateFormat = await _courseRepository.GetConfigurationValueAsync("APPLICATION_DATE_FORMAT", OrgCode);

            scheduledt.Columns.Add("TrainerNameEncrypted", typeof(string));
            scheduledt.Columns.Add("ErrorMessage", typeof(string));

            int columnIndex = 0;
            DataColumnCollection columns = scheduledt.Columns;
            List<string> importcolumns = GetImportColumns(true).Select(c => c.Key).ToList(); ;
            foreach (string column in importcolumns)
            {
                scheduledt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }
            List<APIILTScheduleMigrationRejected> aPIILTScheduleRejectedlist = new List<APIILTScheduleMigrationRejected>();
            List<KeyValuePair<string, int>> columnlengths = GetImportColumns(true);
            DataTable finalDt = scheduledt.Clone();
            if (scheduledt != null && scheduledt.Rows.Count > 0)
            {
                foreach (DataRow dataRow in scheduledt.Rows)
                {
                    APIILTScheduleMigrationRejected aPIILTScheduleRejected = new APIILTScheduleMigrationRejected();

                    bool IsError = false;
                    string errorMsg = "";
                    foreach (string column in importcolumns)
                    {
                        if (string.Compare(column, "StartDate", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string StartDate = Convert.ToString(dataRow[column]);
                                string outStartDate = ValidateDate(StartDate, applicationDateFormat);
                                if (outStartDate == null)
                                {
                                    IsError = true;
                                    errorMsg = "Start Date is not in " + applicationDateFormat + " format.";
                                }
                                else
                                    dataRow[column] = outStartDate;
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Start Date required.";
                            }
                        }
                        else if (string.Compare(column, "EndDate", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string EndDate = Convert.ToString(dataRow[column]);
                                string outEndDate = ValidateDate(EndDate, applicationDateFormat);
                                if (outEndDate == null)
                                {
                                    IsError = true;
                                    errorMsg = "End Date is not in " + applicationDateFormat + " format.";
                                }
                                else
                                    dataRow[column] = outEndDate;
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "End Date required.";
                            }
                        }
                        else if (string.Compare(column, "RegistrationEndDate", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string EndDate = Convert.ToString(dataRow[column]);
                                string outEndDate = ValidateDate(EndDate, applicationDateFormat);
                                if (outEndDate == null)
                                {
                                    IsError = true;
                                    errorMsg = "Registration End Date is not in " + applicationDateFormat + " format.";
                                }
                                else
                                    dataRow[column] = outEndDate;
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Registration End Date required.";
                            }
                        }
                        else if (string.Compare(column, "StartTime", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string StartTime = Convert.ToString(dataRow[column]);
                                string outStartTime = ValidateTime(StartTime);
                                if (outStartTime == null)
                                {
                                    IsError = true;
                                    errorMsg = "Start Time is not in hh:mm format.";
                                }
                                else
                                    dataRow[column] = outStartTime;
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Start Time required.";
                            }
                        }
                        else if (string.Compare(column, "EndTime", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                string EndTime = Convert.ToString(dataRow[column]);
                                string outEndTime = ValidateTime(EndTime);
                                if (outEndTime == null)
                                {
                                    IsError = true;
                                    errorMsg = "End Time is not in hh:mm format.";
                                }
                                else
                                    dataRow[column] = outEndTime;
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "End Time required.";
                            }
                        }
                        else if (string.Compare(column, "SeatCapacity", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                int seatcapacity;
                                bool flag = int.TryParse(Convert.ToString(dataRow[column]), out seatcapacity);
                                if (flag)
                                    dataRow[column] = seatcapacity;
                                else
                                {
                                    IsError = true;
                                    errorMsg = "Invalid Seat Capacity.";
                                }
                            }
                        }
                        else if (string.Compare(column, "Cost", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                int cost;
                                bool flag = int.TryParse(Convert.ToString(dataRow[column]), out cost);
                                if (flag)
                                    dataRow[column] = cost;
                                else
                                {
                                    IsError = true;
                                    errorMsg = "Invalid Cost.";
                                }
                            }
                        }
                        else if (string.Compare(column, "TrainerName", true) == 0)
                            dataRow["TrainerNameEncrypted"] = Security.Encrypt(Convert.ToString(dataRow[column]));

                        if (!DBNull.Value.Equals(dataRow[column]))
                        {
                            int columnlength = columnlengths.Where(c => c.Key == column).Select(len => len.Value).FirstOrDefault();
                            if (columnlength < Convert.ToString(dataRow[column]).Length)
                            {
                                IsError = true;
                                errorMsg = "Invalid data in " + column + ". Must be less than equal to " + Convert.ToString(columnlength) + " characters.";
                                break;
                            }
                        }
                        if (IsError)
                            break;
                    }
                    if (IsError)
                    {
                        aPIILTScheduleRejected.CourseCode = dataRow["CourseCode"] != null ? Convert.ToString(dataRow["CourseCode"]) : null;
                        aPIILTScheduleRejected.ModuleCode = dataRow["ModuleName"] != null ? Convert.ToString(dataRow["ModuleName"]) : null;
                        aPIILTScheduleRejected.ScheduleCode = dataRow["ScheduleCode"] != null ? Convert.ToString(dataRow["ScheduleCode"]) : null;
                        aPIILTScheduleRejected.StartDate = dataRow["StartDate"] != null ? Convert.ToString(dataRow["StartDate"]) : null;
                        aPIILTScheduleRejected.EndDate = dataRow["EndDate"] != null ? Convert.ToString(dataRow["EndDate"]) : null;
                        aPIILTScheduleRejected.StartTime = dataRow["StartTime"] != null ? Convert.ToString(dataRow["StartTime"]) : null;
                        aPIILTScheduleRejected.EndTime = dataRow["EndTime"] != null ? Convert.ToString(dataRow["EndTime"]) : null;
                        aPIILTScheduleRejected.RegistrationEndDate = dataRow["RegistrationEndDate"] != null ? Convert.ToString(dataRow["RegistrationEndDate"]) : null;
                        aPIILTScheduleRejected.TrainerType = dataRow["TrainerType"] != null ? Convert.ToString(dataRow["TrainerType"]) : null;
                        aPIILTScheduleRejected.AgencyTrainerName = dataRow["TrainerName"] != null ? Convert.ToString(dataRow["TrainerName"]) : null;
                        aPIILTScheduleRejected.TrainingPlaceType = dataRow["TrainingPlaceType"] != null ? Convert.ToString(dataRow["TrainingPlaceType"]) : null;
                        aPIILTScheduleRejected.AcademyAgencyName = dataRow["AcademyAgencyName"] != null ? Convert.ToString(dataRow["AcademyAgencyName"]) : null;
                        aPIILTScheduleRejected.PlaceName = dataRow["PlaceName"] != null ? Convert.ToString(dataRow["PlaceName"]) : null;
                        aPIILTScheduleRejected.SeatCapacity = dataRow["SeatCapacity"] != null ? Convert.ToString(dataRow["SeatCapacity"]) : null;
                        aPIILTScheduleRejected.City = dataRow["City"] != null ? Convert.ToString(dataRow["City"]) : null;
                        aPIILTScheduleRejected.PostalAddress = dataRow["PostalAddress"] != null ? Convert.ToString(dataRow["PostalAddress"]) : null;
                        aPIILTScheduleRejected.CoordinatorName = dataRow["CoordinatorName"] != null ? Convert.ToString(dataRow["CoordinatorName"]) : null;
                        aPIILTScheduleRejected.ContactNumber = dataRow["ContactNumber"] != null ? Convert.ToString(dataRow["ContactNumber"]) : null;
                        aPIILTScheduleRejected.Currency = dataRow["Currency"] != null ? Convert.ToString(dataRow["Currency"]) : null;
                        aPIILTScheduleRejected.Cost = dataRow["Cost"] != null ? Convert.ToString(dataRow["Cost"]) : null;
                        aPIILTScheduleRejected.Status = Record.Rejected;
                        aPIILTScheduleRejected.ErrMessage = errorMsg;
                        dataRow["ErrorMessage"] = aPIILTScheduleRejected.ErrMessage;
                        aPIILTScheduleRejectedlist.Add(aPIILTScheduleRejected);
                    }
                    else
                    {
                        finalDt.ImportRow(dataRow);
                    }
                }

                try
                {
                    DataTable dtResult = new DataTable();
                    using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "[dbo].[ILTSchedule_DataMigration]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@ILTScheduleDataMigrationType", SqlDbType.Structured) { Value = finalDt });
                            cmd.CommandTimeout = 0;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }
                        connection.Close();
                    }
                    aPIILTScheduleRejectedlist.AddRange(dtResult.ConvertToList<APIILTScheduleMigrationRejected>());
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            int totalRecordInsert = aPIILTScheduleRejectedlist.Where(x => x.ErrMessage == null).Count();
            int totalRecordRejected = aPIILTScheduleRejectedlist.Where(x => x.ErrMessage != null).Count();
            string resultstring = "Total records inserted: " + totalRecordInsert + ", rejected: " + totalRecordRejected;

            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, aPIScheduleDataMigrationRejected = aPIILTScheduleRejectedlist.Where(x => x.ErrMessage != null).ToList() };
            return response;
        }

        public async Task<ApiResponse> ProcessTrainingNeedRecordsAsync(DataTable scheduledt, int userId, string OrgCode)
        {
            ApiResponse response = new ApiResponse();
            scheduledt.Columns.Add("ErrorMessage", typeof(string));
            int columnIndex = 0;
            DataColumnCollection columns = scheduledt.Columns;
            List<string> importcolumns = GetTrainingNeedImportColumns().Select(c => c.Key).ToList(); ;
            foreach (string column in importcolumns)
            {
                scheduledt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }
            List<RejectedTrainingReommendationNeeds> rejectedTrainingReommendationList = new List<RejectedTrainingReommendationNeeds>();
            List<KeyValuePair<string, int>> columnlengths = GetTrainingNeedImportColumns();
            DataTable finalDt = scheduledt.Clone();
            if (scheduledt != null && scheduledt.Rows.Count > 0)
            {
                foreach (DataRow dataRow in scheduledt.Rows)
                {
                    RejectedTrainingReommendationNeeds rejectedTrainingReommendation = new RejectedTrainingReommendationNeeds();

                    bool IsError = false;
                    string errorMsg = "";
                    foreach (string column in importcolumns)
                    {
                        if (string.Compare(column, "JobRole", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                dataRow[column] = Convert.ToString(dataRow[column]);
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Job Role required.";
                            }
                        }
                        else if (string.Compare(column, "Department", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                dataRow[column] = Convert.ToString(dataRow[column]);
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Department required.";
                            }
                        }
                        else if (string.Compare(column, "Section", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                dataRow[column] = Convert.ToString(dataRow[column]);
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Section required.";
                            }
                        }
                        else if (string.Compare(column, "Level", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                dataRow[column] = Convert.ToString(dataRow[column]);
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Level required.";
                            }
                        }
                        else if (string.Compare(column, "Status", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                dataRow[column] = Convert.ToString(dataRow[column]);
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Status required.";
                            }
                        }
                        else if (string.Compare(column, "TraniningProgram ", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                dataRow[column] = Convert.ToString(dataRow[column]);
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "TraniningProgram required.";
                            }
                        }
                        else if (string.Compare(column, "Category ", true) == 0)
                        {
                            if (!string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                dataRow[column] = Convert.ToString(dataRow[column]);
                            }
                            else
                            {
                                IsError = true;
                                errorMsg = "Category  required.";
                            }
                        }
                       
                        if (IsError)
                            break;
                    }
                    if (IsError)
                    {
                        rejectedTrainingReommendation.JobRole = dataRow["JobRole"] != null ? Convert.ToString(dataRow["JobRole"]) : null;
                        rejectedTrainingReommendation.Department = dataRow["Department"] != null ? Convert.ToString(dataRow["Department"]) : null;
                        rejectedTrainingReommendation.Section = dataRow["Section"] != null ? Convert.ToString(dataRow["Section"]) : null;
                        rejectedTrainingReommendation.Level = dataRow["Level"] != null ? Convert.ToString(dataRow["Level"]) : null;
                        rejectedTrainingReommendation.Status = dataRow["Status"] != null ? Convert.ToString(dataRow["Status"]) : null;
                        rejectedTrainingReommendation.TrainingProgram = dataRow["TrainingProgram"] != null ? Convert.ToString(dataRow["TrainingProgram"]) : null;
                        rejectedTrainingReommendation.Category = dataRow["Category"] != null ? Convert.ToString(dataRow["Category"]) : null;                        
                        rejectedTrainingReommendation.RecordStatus = Record.Rejected;
                        rejectedTrainingReommendation.ErrorMessage = errorMsg;
                        dataRow["ErrorMessage"] = rejectedTrainingReommendation.ErrorMessage;
                        rejectedTrainingReommendationList.Add(rejectedTrainingReommendation);
                    }
                    else
                    {
                        finalDt.ImportRow(dataRow);
                    }
                }

                try
                {
                    DataTable dtResult = new DataTable();
                    using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                    {
                        var connection = dbContext.Database.GetDbConnection();

                        if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                            connection.Open();

                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "[dbo].[TrainingRecommondationBulkImport]";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = userId });
                            cmd.Parameters.Add(new SqlParameter("@TrainingRecommondationType", SqlDbType.Structured) { Value = finalDt });
                            cmd.CommandTimeout = 0;
                            DbDataReader reader = await cmd.ExecuteReaderAsync();
                            dtResult.Load(reader);
                        }
                        connection.Close();
                    }
                    rejectedTrainingReommendationList.AddRange(dtResult.ConvertToList<RejectedTrainingReommendationNeeds>());
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
            }
            int totalRecordInsert = rejectedTrainingReommendationList.Where(x => x.ErrorMessage == null).Count();
            int totalRecordRejected = rejectedTrainingReommendationList.Where(x => x.ErrorMessage != null).Count();
            string resultstring = "Total records inserted: " + totalRecordInsert + ", rejected: " + totalRecordRejected;

            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, aPIScheduleDataMigrationRejected = rejectedTrainingReommendationList.Where(x => x.ErrorMessage != null).ToList() };
            return response;
        }
        public string ValidateDate(string InputDate, string validDateFormat)
        {
            string outputDate = null;
            try
            {
                DateTime result;
                result = DateTime.ParseExact(InputDate, validDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None);
                string inputstring = result.ToString("dd/MM/yyyy");
                inputstring = inputstring.Replace("-", "/");
                inputstring = inputstring.Replace(".", "/");
                string[] dateParts = inputstring.Split('/');
                string day = dateParts[0];
                string month = dateParts[1];
                string year = dateParts[2];
                outputDate = new DateTime(Convert.ToInt32(year), Convert.ToInt32(month), Convert.ToInt32(day)).ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return outputDate;
        }
        public string ValidateTime(string InputTime)
        {
            string outputTime = null;
            try
            {
                DateTime dt;
                if (DateTime.TryParseExact(InputTime, "HH:mm", CultureInfo.InvariantCulture,
                                                              DateTimeStyles.None, out dt))
                {
                    outputTime = dt.TimeOfDay.ToString(@"hh\:mm");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return outputTime;
        }
        public async Task<ApiResponse> ProcessCourseModuleImportFile(FileInfo file, IDataMigration _dataMigration, int UserId)
        {
            ApiResponse Response = new ApiResponse();
            try
            {

                ResetAssessement();
                bool resultMessage = await InitilizeAsync(file);
                if (resultMessage == true)
                {
                    Response = await ProcessCourseModuleRecordsAsync(_dataMigration, UserId);
                    ResetAssessement();
                    return Response;
                }
                else
                {

                    Response.StatusCode = 200;
                    Response.ResponseObject = Record.FileInvalid;
                    Reset();
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;


        }
        public void ResetAssessement()
        {
            sb.Clear();
            header = new string[0];
            headerStar = new string[0];
            headerWithoutStar = new string[0];
            CourseCompletionRecord.Clear();

            sbError.Clear();
            totalRecordInsert = 0;
            totalRecordRejected = 0;
        }
        public async Task<ApiResponse> ProcessRecordsAsync_Assessment(IDataMigration _dataMigration, int userid)
        {
            List<APIAssDataMigration> aPIDataMigrationRejected = new List<APIAssDataMigration>();
            List<AssessmentQuestion> AssessmentQuestionList = new List<AssessmentQuestion>();
            List<AssessmentQuestion> errorAssessment = new List<AssessmentQuestion>();

            if (CourseCompletionRecord != null && CourseCompletionRecord.Count > 0)
            {
                List<APIAssDataMigration> aPIDataMigration = new List<APIAssDataMigration>();
                List<AssessmentOptions> assessmentOptions = new List<AssessmentOptions>();

                string lblQuestionText = string.Empty;
                foreach (string record in CourseCompletionRecord)
                {
                    int countLenght = record.Length;

                    if (record != null && countLenght > 1)
                    {
                        string[] textpart = record.Split('\t');
                        string[][] mainRecord = { header, textpart };

                        string lblOptionType = string.Empty;
                        string lblMarks = string.Empty;
                        string lblIsCorrectAnswer = string.Empty;
                        string lblOptionText = string.Empty;
                        string lblUserId = string.Empty;
                        string lblDifficultyLevel = string.Empty;
                        string lblQuestionType = string.Empty;
                        string lblSection = string.Empty;
                        string headerText = "";

                        int arrasize = header.Count();

                        AssessmentOptions assessmentOption = new AssessmentOptions();

                        for (int j = 0; j < arrasize - 1; j++)
                        {
                            headerText = header[j].Trim();
                            string[] mainspilt = headerText.Split('\t');
                            headerText = mainspilt[0];


                            if (headerText == "QuestionText")
                            {
                                string[] ScheduleCodesplit = mainRecord[1][j].Split('\t');
                                lblQuestionText = ScheduleCodesplit[0].Trim();
                                if (lblQuestionText != null && !string.IsNullOrEmpty(lblQuestionText))
                                {
                                    assDataMigration.QuestionText = lblQuestionText;
                                }
                                else
                                {
                                    assDataMigration.QuestionText = null;
                                }
                            }
                            else if (headerText == "OptionType")
                            {
                                string[] OptionTypesplit = mainRecord[1][j].Split('\t');
                                lblOptionType = OptionTypesplit[0].Trim();

                                if (lblOptionType != null && !string.IsNullOrEmpty(lblOptionType))
                                {
                                    assDataMigration.OptionType = lblOptionType;
                                }
                                else
                                {
                                    assDataMigration.OptionType = null;
                                }
                            }
                            else if (headerText == "Marks")
                            {
                                string[] Markssplit = mainRecord[1][j].Split('\t');
                                lblMarks = Markssplit[0].Trim();

                                if (lblMarks != null && !string.IsNullOrEmpty(lblMarks))
                                {
                                    assDataMigration.Marks = Convert.ToInt32(lblMarks);
                                }
                                else
                                {
                                    assDataMigration.Marks = 0;
                                }
                            }
                            else if (headerText == "DifficultyLevel")
                            {
                                string[] DifficultyLevelsplit = mainRecord[1][j].Split('\t');
                                lblDifficultyLevel = DifficultyLevelsplit[0].Trim();

                                if (lblDifficultyLevel != null && !string.IsNullOrEmpty(lblDifficultyLevel))
                                {
                                    assDataMigration.DifficultyLevel = lblDifficultyLevel;
                                }
                                else
                                {
                                    assDataMigration.DifficultyLevel = null;
                                }
                            }
                            else if (headerText == "OptionType")
                            {
                                string[] OptionTextsplit = mainRecord[1][j].Split('\t');
                                lblOptionText = OptionTextsplit[0].Trim();

                                if (lblOptionText != null && !string.IsNullOrEmpty(lblOptionText))
                                {
                                    assDataMigration.DifficultyLevel = lblOptionText;
                                }
                                else
                                {
                                    assDataMigration.DifficultyLevel = null;
                                }
                            }

                            else if (headerText == "Section")
                            {
                                string[] SectionTextsplit = mainRecord[1][j].Split('\t');
                                lblSection = SectionTextsplit[0].Trim();

                                if (lblSection != null && !string.IsNullOrEmpty(lblSection))
                                {
                                    assDataMigration.Section = lblSection;
                                }
                                else
                                {
                                    assDataMigration.Section = null;
                                }
                            }

                            else if (headerText == "QuestionType")
                            {
                                string[] QuestionTypeTextsplit = mainRecord[1][j].Split('\t');
                                lblQuestionType = QuestionTypeTextsplit[0].Trim();

                                if (lblQuestionType != null && !string.IsNullOrEmpty(lblQuestionType))
                                {
                                    assDataMigration.QuestionType = lblQuestionType;
                                }
                                else
                                {
                                    assDataMigration.QuestionType = null;
                                }
                            }


                            else if (headerText == "IsCorrectAnswer")
                            {
                                string[] IsCorrectAnswersplit = mainRecord[1][j].Split('\t');
                                lblIsCorrectAnswer = IsCorrectAnswersplit[0].Trim();

                                if (lblIsCorrectAnswer != null && !string.IsNullOrEmpty(lblIsCorrectAnswer))
                                {
                                    assDataMigration.IsCorrectAnswer = lblIsCorrectAnswer.Equals("0") ? false : true;
                                }
                                else
                                {
                                    assDataMigration.IsCorrectAnswer = Convert.ToBoolean(0);
                                }
                            }
                            else if (headerText == "OptionText")
                            {
                                string[] OptionTextsplit = mainRecord[1][j].Split('\t');
                                lblOptionText = OptionTextsplit[0].Trim();

                                if (lblOptionText != null && !string.IsNullOrEmpty(lblOptionText))
                                {
                                    assDataMigration.OptionText = lblOptionText;
                                }
                                else
                                {
                                    assDataMigration.OptionText = null;
                                }
                            }

                        }
                        aPIDataMigration.Add(assDataMigration);
                    }
                    else
                    {
                    }
                }

                var QuestionLists = aPIDataMigration.Select(g => new { g.QuestionText, g.DifficultyLevel, g.Marks, g.QuestionType, g.Section, g.OptionType }).Distinct().ToList();
                var OptionsList = aPIDataMigration.Select(g => new { g.QuestionText, g.OptionText, g.IsCorrectAnswer }).Distinct().ToList();

                foreach (var assDataMigration in QuestionLists)
                {
                    AssessmentQuestion questionAssessments = new AssessmentQuestion();
                    questionAssessments.LearnerInstruction = "";
                    questionAssessments.DifficultyLevel = assDataMigration.DifficultyLevel;
                    questionAssessments.Marks = assDataMigration.Marks;
                    questionAssessments.ModelAnswer = null;
                    questionAssessments.OptionType = assDataMigration.OptionType;
                    questionAssessments.QuestionText = assDataMigration.QuestionText;
                    questionAssessments.QuestionStyle = null;
                    questionAssessments.AnswerAsImages = null;
                    questionAssessments.QuestionType = assDataMigration.QuestionType;
                    questionAssessments.Metadata = null;
                    questionAssessments.Section = assDataMigration.Section.ToLower();
                    questionAssessments.Status = true;
                    questionAssessments.CreatedBy = userid;
                    questionAssessments.CreatedDate = DateTime.UtcNow;
                    questionAssessments.ModifiedBy = userid;
                    questionAssessments.ModifiedDate = DateTime.UtcNow;
                    questionAssessments.IsMemoQuestion = false;


                    List<APIAssessmentOptions> aPIassessmentOptions = new List<APIAssessmentOptions>();

                    aPIassessmentOptions = OptionsList.Where(s => s.QuestionText == assDataMigration.QuestionText).Select(s => new APIAssessmentOptions { OptionText = s.OptionText, IsCorrectAnswer = s.IsCorrectAnswer }).Distinct().ToList();


                    if (assDataMigration.Section.ToLower().Equals(FileType.Objective))
                    {
                        if (aPIassessmentOptions.Count < 2)
                        {
                            errorAssessment.Add(questionAssessments);
                        }
                        else if (!ValidateDifficultyLevel(assDataMigration.DifficultyLevel))
                        {
                            errorAssessment.Add(questionAssessments);
                        }
                        else if (!ValidateMarks(Convert.ToString(assDataMigration.Marks)))
                        {
                            errorAssessment.Add(questionAssessments);
                        }
                        else if (!ValidateSection(Convert.ToString(assDataMigration.Section)))
                        {
                            errorAssessment.Add(questionAssessments);
                        }
                        else if (!ValidateQuestionText(Convert.ToString(assDataMigration.QuestionText)))
                        {
                            errorAssessment.Add(questionAssessments);
                        }
                        else
                        {
                            if (!await this._assessmentQuestion.Exist(assDataMigration.QuestionText))
                            {

                                AssessmentQuestionList.Add(questionAssessments);

                                List<AssessmentQuestionOption> AssessmentQues = new List<AssessmentQuestionOption>();
                                foreach (APIAssessmentOptions opt in aPIassessmentOptions)
                                {
                                    AssessmentQuestionOption assessmentOptiones = new AssessmentQuestionOption();
                                    assessmentOptiones.OptionText = opt.OptionText;
                                    assessmentOptiones.IsCorrectAnswer = opt.IsCorrectAnswer;
                                    assessmentOptiones.QuestionID = questionAssessments.Id;
                                    assessmentOptiones.CreatedBy = userid;
                                    assessmentOptiones.CreatedDate = DateTime.UtcNow;
                                    assessmentOptiones.ModifiedDate = DateTime.UtcNow;
                                    assessmentOptiones.ModifiedBy = userid;
                                    if (!string.IsNullOrEmpty(opt.OptionText))
                                    {
                                        AssessmentQues.Add(assessmentOptiones);
                                    }
                                }
                                await _asessmentQuestionOption.AddRange(AssessmentQues);

                            }
                            {
                                errorAssessment.Add(questionAssessments);
                            }
                        }
                    }
                    else if (assDataMigration.Section.ToLower().Equals(FileType.Subjective))
                    {
                        if (aPIassessmentOptions.Count < 2)
                        {
                            errorAssessment.Add(questionAssessments);
                        }
                        else if (!ValidateDifficultyLevel(assDataMigration.DifficultyLevel))
                        {
                            errorAssessment.Add(questionAssessments);
                        }
                        else if (!ValidateMarks(Convert.ToString(assDataMigration.Marks)))
                        {
                            errorAssessment.Add(questionAssessments);
                        }
                        else if (!ValidateSection(Convert.ToString(assDataMigration.Section)))
                        {
                            errorAssessment.Add(questionAssessments);
                        }
                        else if (!ValidateQuestionText(Convert.ToString(assDataMigration.QuestionText)))
                        {
                            errorAssessment.Add(questionAssessments);
                        }
                        else
                        {
                            if (!await this._assessmentQuestion.Exist(assDataMigration.QuestionText))
                            {
                                await this._assessmentQuestion.Add(questionAssessments);

                                AssessmentQuestionList.Add(questionAssessments);

                                List<AssessmentQuestionOption> AssessmentQues = new List<AssessmentQuestionOption>();
                                foreach (APIAssessmentOptions opt in aPIassessmentOptions)
                                {
                                    AssessmentQuestionOption assessmentOptiones = new AssessmentQuestionOption();
                                    assessmentOptiones.OptionText = opt.OptionText;
                                    assessmentOptiones.IsCorrectAnswer = opt.IsCorrectAnswer;
                                    assessmentOptiones.QuestionID = questionAssessments.Id;
                                    assessmentOptiones.CreatedBy = userid;
                                    assessmentOptiones.CreatedDate = DateTime.UtcNow;
                                    assessmentOptiones.ModifiedDate = DateTime.UtcNow;
                                    assessmentOptiones.ModifiedBy = userid;
                                    if (!string.IsNullOrEmpty(opt.OptionText))
                                    {
                                        AssessmentQues.Add(assessmentOptiones);
                                    }
                                }
                                await _asessmentQuestionOption.AddRange(AssessmentQues);
                            }
                            {
                                errorAssessment.Add(questionAssessments);
                            }
                        }
                    }
                }
                totalRecordInsert = AssessmentQuestionList.Count();
                totalRecordRejected = errorAssessment.Count();
            }
            string resultstring = "Total number of record inserted :" + totalRecordInsert + ",  Total number of record record rejected : " + totalRecordRejected;
            ApiResponse response = new ApiResponse();
            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, errorAssessment };
            return response;

        }
        public async Task<ApiResponse> ProcessRecordsAsync_ILTSchedule(IDataMigration _dataMigration, int userid, IILTSchedule _IILTSchedule)
        {
            List<APIILTScheduleDataMigration> aPIILTScheduleDataMigrationRejected = new List<APIILTScheduleDataMigration>();
            List<ILTSchedule> ILTScheduleList = new List<ILTSchedule>();
            List<ILTScheduleTrainerBindings> ILTScheduleTrainerBindingsList = new List<ILTScheduleTrainerBindings>();
            List<APIILTScheduleDataMigration> aPIScheduleDataMigrationRejected = new List<APIILTScheduleDataMigration>();


            if (CourseCompletionRecord != null && CourseCompletionRecord.Count > 0)
            {
                List<APIILTScheduleDataMigration> aPIILTSchedule = new List<APIILTScheduleDataMigration>();

                foreach (string record in CourseCompletionRecord)
                {
                    APIILTScheduleDataMigration aPIILTScheduleDataMigration = new APIILTScheduleDataMigration();

                    int countLenght = record.Length;

                    if (record != null && countLenght > 1)
                    {
                        string[] textpart = record.Split('\t');
                        string[][] mainRecord = { header, textpart };

                        string lblModuleCode = string.Empty;
                        string lblScheduleCode = string.Empty;
                        string lblPlaceName = string.Empty;
                        string lblTrainerType = string.Empty;
                        string lblAcademyTrainerUserId = string.Empty;
                        string lblAcademyAgencyCode = string.Empty;
                        string lblAgencyTrainerName = string.Empty;
                        string lblStartDate = string.Empty;
                        string lblStartTime = string.Empty;
                        string lblEndDate = string.Empty;
                        string lblEndTime = string.Empty;
                        string lblTrainerName = string.Empty;
                        string headerText = "";

                        int arrasize = header.Count();

                        for (int j = 0; j < arrasize - 1; j++)
                        {
                            headerText = header[j].Trim();
                            string[] mainspilt = headerText.Split('\t');
                            headerText = mainspilt[0];

                            if (headerText == "ModuleCode")
                            {
                                string[] ModuleCodesplit = mainRecord[1][j].Split('\t');
                                lblModuleCode = ModuleCodesplit[0].Trim();
                                if (lblModuleCode != null && !string.IsNullOrEmpty(lblModuleCode))
                                {
                                    aPIILTScheduleDataMigration.ModuleCode = lblModuleCode;
                                }
                                else
                                {
                                    aPIILTScheduleDataMigration.ModuleCode = null;
                                }
                            }
                            if (headerText == "ScheduleCode")
                            {
                                string[] ScheduleCodesplit = mainRecord[1][j].Split('\t');
                                lblScheduleCode = ScheduleCodesplit[0].Trim();
                                if (lblScheduleCode != null && !string.IsNullOrEmpty(lblScheduleCode))
                                {
                                    aPIILTScheduleDataMigration.ScheduleCode = lblScheduleCode;
                                }
                                else
                                {
                                    aPIILTScheduleDataMigration.ModuleCode = null;
                                }
                            }
                            if (headerText == "PlaceName")
                            {
                                string[] PlaceNamesplit = mainRecord[1][j].Split('\t');
                                lblPlaceName = PlaceNamesplit[0].Trim();

                                if (lblPlaceName != null && !string.IsNullOrEmpty(lblPlaceName))
                                {
                                    aPIILTScheduleDataMigration.PlaceName = lblPlaceName;
                                }
                                else
                                {
                                    aPIILTScheduleDataMigration.PlaceName = null;
                                }
                            }
                            if (headerText == "TrainerType")
                            {
                                string[] TrainerTypesplit = mainRecord[1][j].Split('\t');
                                lblTrainerType = TrainerTypesplit[0].Trim();

                                if (lblTrainerType != null && !string.IsNullOrEmpty(lblTrainerType))
                                {
                                    aPIILTScheduleDataMigration.TrainerType = lblTrainerType;
                                }
                                else
                                {
                                    aPIILTScheduleDataMigration.TrainerType = null;
                                }
                            }
                            if (headerText == "AcademyTrainerUserID")
                            {
                                string[] AcademyTrainerUserIDsplit = mainRecord[1][j].Split('\t');
                                lblAcademyTrainerUserId = AcademyTrainerUserIDsplit[0].Trim();
                                if (lblAcademyTrainerUserId != null && !string.IsNullOrEmpty(lblAcademyTrainerUserId))
                                {
                                    aPIILTScheduleDataMigration.AcademyTrainerUserId = Convert.ToInt16(lblAcademyTrainerUserId);
                                }
                                else
                                {
                                    aPIILTScheduleDataMigration.AcademyTrainerUserId = null;
                                }
                            }
                            if (headerText == "AcademyAgencyCode")
                            {
                                string[] AcademyAgencyCodesplit = mainRecord[1][j].Split('\t');
                                lblAcademyAgencyCode = AcademyAgencyCodesplit[0].Trim();

                                if (lblAcademyAgencyCode != null && !string.IsNullOrEmpty(lblAcademyAgencyCode))
                                {
                                    aPIILTScheduleDataMigration.AcademyAgencyCode = lblAcademyAgencyCode;
                                }
                                else
                                {
                                    aPIILTScheduleDataMigration.AcademyAgencyCode = null;
                                }
                            }
                            if (headerText == "AgencyTrainerName")
                            {
                                string[] AgencyTrainerNamesplit = mainRecord[1][j].Split('\t');
                                lblAgencyTrainerName = AgencyTrainerNamesplit[0].Trim();
                                if (lblAgencyTrainerName != null && !string.IsNullOrEmpty(lblAgencyTrainerName))
                                {
                                    aPIILTScheduleDataMigration.AgencyTrainerName = Convert.ToString(lblAgencyTrainerName);
                                }
                                else
                                {
                                    aPIILTScheduleDataMigration.AgencyTrainerName = null;
                                }
                            }
                            if (headerText == "StartDate")
                            {
                                string[] StartDatesplit = mainRecord[1][j].Split('\t');
                                lblStartDate = StartDatesplit[0].Trim();

                                if (lblStartDate != null && !string.IsNullOrEmpty(lblStartDate))
                                {
                                    aPIILTScheduleDataMigration.StartDate = Convert.ToDateTime(lblStartDate);
                                }
                                else
                                {
                                }
                            }
                            if (headerText == "StartTime")
                            {
                                string[] StartTimesplit = mainRecord[1][j].Split('\t');
                                lblStartTime = StartTimesplit[0].Trim();

                                if (lblStartTime != null && !string.IsNullOrEmpty(lblStartTime))
                                {
                                    DateTime dt = Convert.ToDateTime(lblStartTime);
                                    string time = dt.ToString("hh:mm:ss");
                                    aPIILTScheduleDataMigration.StartTime = TimeSpan.Parse(time);
                                }
                                else
                                {
                                }
                            }
                            if (headerText == "EndDate")
                            {
                                string[] EndDatesplit = mainRecord[1][j].Split('\t');
                                lblEndDate = EndDatesplit[0].Trim();

                                if (lblEndDate != null && !string.IsNullOrEmpty(lblEndDate))
                                {
                                    aPIILTScheduleDataMigration.EndDate = Convert.ToDateTime(lblEndDate);
                                }
                                else
                                {
                                }
                            }
                            if (headerText == "EndTime")
                            {
                                string[] EndTimesplit = mainRecord[1][j].Split('\t');
                                lblEndTime = EndTimesplit[0].Trim();

                                if (lblEndTime != null && !string.IsNullOrEmpty(lblEndTime))
                                {
                                    DateTime dt = Convert.ToDateTime(lblEndTime);
                                    string time = dt.ToString("hh:mm:ss");
                                    aPIILTScheduleDataMigration.EndTime = TimeSpan.Parse(time);
                                }
                                else
                                {
                                }

                            }
                        }
                        try
                        {
                            aPIILTSchedule.Add(aPIILTScheduleDataMigration);

                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));

                        }

                    }
                    else
                    {

                    }
                    if (await this._db.AcademyAgencyMaster.Where(a => a.AcademyAgencyName == aPIILTScheduleDataMigration.AcademyAgencyCode && (a.IsDeleted == false)).CountAsync() == 0)
                    {
                        AcademyAgencyMaster AcademyAgencyMaster = new AcademyAgencyMaster();
                        AcademyAgencyMaster.AcademyAgencyName = aPIILTScheduleDataMigration.AcademyAgencyCode;
                        AcademyAgencyMaster.CreatedBy = 1;
                        AcademyAgencyMaster.CreatedDate = DateTime.UtcNow;
                        AcademyAgencyMaster.ModifiedBy = userid;
                        AcademyAgencyMaster.ModifiedDate = DateTime.UtcNow;
                        AcademyAgencyMaster.IsActive = true;

                        await this._db.AcademyAgencyMaster.AddAsync(AcademyAgencyMaster);
                        await this._db.SaveChangesAsync();
                    }
                }

                var ScheduleLists = aPIILTSchedule.Select(g => new { g.ModuleCode, g.PlaceName, g.TrainerType, g.AcademyTrainerUserId, g.AcademyAgencyCode, g.AgencyTrainerName, g.StartDate, g.StartTime, g.EndDate, g.EndTime, g.AcademyTrainerName, g.RegistrationEndDate, g.ScheduleCode }).ToList();

                foreach (var ScheduleDataMigration in ScheduleLists)
                {
                    List<ILTSchedule> iltSchedsule = new List<ILTSchedule>();
                    ILTSchedule Schedule = new ILTSchedule();
                    List<ILTScheduleTrainerBindings> ScheduleTrainer = new List<ILTScheduleTrainerBindings>();

                    APIILTScheduleDataMigration objrejected = new APIILTScheduleDataMigration();
                    APIILTScheduleDataMigration iltscheduledata = new APIILTScheduleDataMigration();


                    if (ScheduleDataMigration.ModuleCode == null || ScheduleDataMigration.PlaceName == null || ScheduleDataMigration.TrainerType == null || ScheduleDataMigration.AcademyAgencyCode == null
                        || ScheduleDataMigration.AgencyTrainerName == null)
                    {
                        objrejected.ModuleCode = ScheduleDataMigration.ModuleCode;
                        objrejected.PlaceName = ScheduleDataMigration.PlaceName;
                        objrejected.AgencyTrainerName = ScheduleDataMigration.AgencyTrainerName;
                        objrejected.AcademyAgencyCode = ScheduleDataMigration.AcademyAgencyCode;
                        objrejected.TrainerType = ScheduleDataMigration.TrainerType;

                        if (ScheduleDataMigration.ModuleCode == null)
                            objrejected.ErrMessage = "Module Code Required";
                        if (ScheduleDataMigration.PlaceName == null)
                            objrejected.ErrMessage = "PlaceName is Required";

                        if (ScheduleDataMigration.AcademyAgencyCode == null)
                            objrejected.ErrMessage = "AcademyAgencyCode is Required";
                        if (ScheduleDataMigration.AcademyTrainerName == null)
                            objrejected.ErrMessage = "AcademyTrainerName is Required";
                        if (ScheduleDataMigration.StartDate == null)
                            objrejected.ErrMessage = "StartDate is Required";
                        if (ScheduleDataMigration.EndDate == null)
                            objrejected.ErrMessage = "EndDate is Required";
                        if (ScheduleDataMigration.StartTime == null)
                            objrejected.ErrMessage = "StartTime is Required";
                        if (ScheduleDataMigration.EndTime == null)
                            objrejected.ErrMessage = "EndTime is Required";
                        if (ScheduleDataMigration.TrainerType == null)
                            objrejected.ErrMessage = "TrainerType Is Required";
                        else
                        if (ScheduleDataMigration.TrainerType.ToLower() == "internal")
                            if (ScheduleDataMigration.AcademyAgencyCode == null)
                                objrejected.ErrMessage = "Academy/Agency is required";
                            else
                        if (ScheduleDataMigration.TrainerType.ToLower() == "external")
                                if (ScheduleDataMigration.AcademyTrainerName == null)
                                    objrejected.ErrMessage = "AcademyTrainerName is required";

                        aPIScheduleDataMigrationRejected.Add(objrejected);
                        aPIILTScheduleDataMigrationRejected.Add(objrejected);
                        continue;
                    }

                    try
                    {

                        var ModuleId = _db.Module.Where(e => e.Name.Trim() == ScheduleDataMigration.ModuleCode.Trim()).Select(e => new ILTSchedule { ModuleId = e.Id }).FirstOrDefault();
                        var PlaceId = _db.TrainingPlace.Where(e => e.PlaceName == ScheduleDataMigration.PlaceName).Select(e => new ILTSchedule { PlaceID = e.Id }).Distinct().FirstOrDefault();
                        var AcademyAgencyID = _db.AcademyAgencyMaster.Where(e => e.AcademyAgencyName == ScheduleDataMigration.AcademyAgencyCode).Select(e => new ILTSchedule { AcademyAgencyID = e.Id }).Distinct().FirstOrDefault();

                        if (ModuleId != null && PlaceId != null && AcademyAgencyID != null && ScheduleDataMigration.TrainerType != null)
                        {
                            List<ILTSchedule> DuplicateSchedule = await this._db.ILTSchedule.Where(a => a.ModuleId == ModuleId.ModuleId
                                                                                         && (a.AcademyTrainerID == ScheduleDataMigration.AcademyTrainerUserId || a.AgencyTrainerName == ScheduleDataMigration.AgencyTrainerName
                                                                                         || a.AcademyTrainerName == ScheduleDataMigration.AcademyTrainerName)
                                                                                         && a.IsActive == Record.Active && a.IsDeleted == false
                                                                                         && a.StartDate.Date == ScheduleDataMigration.StartDate.Date
                                                                                         && a.EndDate.Date == ScheduleDataMigration.EndDate.Date
                                                                                         && a.StartTime == ScheduleDataMigration.StartTime
                                                                                         && a.EndTime == ScheduleDataMigration.EndTime
                                                                                         && a.PlaceID == PlaceId.PlaceID && a.ScheduleCode == ScheduleDataMigration.ScheduleCode).ToListAsync();
                            if (DuplicateSchedule != null && DuplicateSchedule.Count > 0)
                            {
                                objrejected.ModuleCode = ScheduleDataMigration.ModuleCode;
                                objrejected.PlaceName = ScheduleDataMigration.PlaceName;
                                objrejected.ScheduleCode = ScheduleDataMigration.ScheduleCode;
                                objrejected.TrainerType = ScheduleDataMigration.TrainerType;

                                objrejected.ErrMessage = "Duplicate Record";
                                aPIScheduleDataMigrationRejected.Add(objrejected);
                                aPIILTScheduleDataMigrationRejected.Add(objrejected);
                                continue;
                            }


                            if (ScheduleDataMigration.TrainerType.ToLower().ToString() != "internal" && ScheduleDataMigration.TrainerType.ToLower().ToString() != "external")
                            {
                                objrejected.ModuleCode = ScheduleDataMigration.ModuleCode;
                                objrejected.PlaceName = ScheduleDataMigration.PlaceName;
                                objrejected.AgencyTrainerName = ScheduleDataMigration.AgencyTrainerName;
                                objrejected.AcademyAgencyCode = ScheduleDataMigration.AcademyAgencyCode;
                                objrejected.TrainerType = ScheduleDataMigration.TrainerType;

                                objrejected.ErrMessage = "Trainer Type should be Internal/External";
                                aPIScheduleDataMigrationRejected.Add(objrejected);
                                aPIILTScheduleDataMigrationRejected.Add(objrejected);
                                continue;
                            }


                            Schedule.ModuleId = Convert.ToInt32(ModuleId.ModuleId);
                            Schedule.PlaceID = Convert.ToInt32(PlaceId.PlaceID);

                            Schedule.AcademyTrainerID = null;
                            Schedule.AcademyAgencyID = Convert.ToInt32(AcademyAgencyID.AcademyAgencyID);
                            Schedule.TrainerType = ScheduleDataMigration.TrainerType;
                            Schedule.StartDate = ScheduleDataMigration.StartDate;
                            Schedule.StartTime = ScheduleDataMigration.StartTime;
                            Schedule.EndDate = ScheduleDataMigration.EndDate;
                            Schedule.EndTime = ScheduleDataMigration.EndTime;
                            Schedule.RegistrationEndDate = ScheduleDataMigration.StartDate;
                            Schedule.CreatedBy = userid;
                            Schedule.CreatedDate = DateTime.UtcNow;
                            Schedule.ModifiedDate = DateTime.UtcNow;
                            Schedule.ModifiedBy = userid;
                            ScheduleCode ScheduleCode = await _IILTSchedule.GetScheduleCode(userid);
                            Schedule.ScheduleCode = ScheduleDataMigration.ScheduleCode;
                            Schedule.IsActive = true;
                            Schedule.ScheduleType = "Scheduled";

                            iltSchedsule.Add(Schedule);

                            _db.ILTSchedule.AddRange(iltSchedsule);

                            await _db.SaveChangesAsync();

                            ILTScheduleList.Add(Schedule);


                            // Save Tabel [ILTScheduleTrainerBindings]

                            int ScheduleID = Schedule.ID;
                            foreach (var obj in iltSchedsule)
                            {
                                ILTScheduleTrainerBindings objILTScheduleTrainerBindings = new ILTScheduleTrainerBindings();
                                objILTScheduleTrainerBindings.ID = 0;
                                objILTScheduleTrainerBindings.ScheduleID = ScheduleID;
                                objILTScheduleTrainerBindings.TrainerID = ScheduleDataMigration.AcademyTrainerUserId;
                                objILTScheduleTrainerBindings.TrainerName = ScheduleDataMigration.AgencyTrainerName;
                                objILTScheduleTrainerBindings.TrainerType = obj.TrainerType;
                                objILTScheduleTrainerBindings.AcademyTrainerName = null;
                                objILTScheduleTrainerBindings.ScheduleType = null;
                                objILTScheduleTrainerBindings.CreatedBy = userid;
                                objILTScheduleTrainerBindings.CreatedDate = DateTime.UtcNow;
                                objILTScheduleTrainerBindings.ModifiedDate = DateTime.UtcNow;
                                objILTScheduleTrainerBindings.ModifiedBy = userid;
                                objILTScheduleTrainerBindings.IsActive = true;
                                objILTScheduleTrainerBindings.IsDeleted = false;

                                ScheduleTrainer.Add(objILTScheduleTrainerBindings);

                                _db.ILTScheduleTrainerBindings.AddRange(ScheduleTrainer);
                                await _db.SaveChangesAsync();
                                ILTScheduleTrainerBindingsList.Add(objILTScheduleTrainerBindings);
                            }
                        }

                        else
                        {
                            APIILTScheduleDataMigration rejected = new APIILTScheduleDataMigration();

                            rejected.ModuleCode = ScheduleDataMigration.ModuleCode;
                            rejected.PlaceName = ScheduleDataMigration.PlaceName;
                            rejected.AgencyTrainerName = ScheduleDataMigration.AgencyTrainerName;
                            rejected.AcademyAgencyCode = ScheduleDataMigration.AcademyAgencyCode;
                            rejected.TrainerType = ScheduleDataMigration.TrainerType;
                            if (ModuleId == null)
                                rejected.ErrMessage = "Module Code not Found";
                            if (PlaceId == null)
                                rejected.ErrMessage = "Place Code not Found";
                            if (AcademyAgencyID == null)
                                rejected.ErrMessage = "Academy Agency Code not Found";
                            if (ScheduleDataMigration.TrainerType == null)
                                rejected.ErrMessage = "TrainerType Code not Found";
                            if (ScheduleDataMigration.AgencyTrainerName == null)
                                rejected.ErrMessage = "AgencyTrainerName  not Found";
                            if (ScheduleDataMigration.AcademyAgencyCode == null)
                                rejected.ErrMessage = "AgencyTrainerCode not found";

                            aPIScheduleDataMigrationRejected.Add(rejected);
                            aPIILTScheduleDataMigrationRejected.Add(rejected);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(Utilities.GetDetailedException(ex));
                    }
                }
                totalRecordInsert = ILTScheduleList.Count();
                totalRecordRejected = aPIILTScheduleDataMigrationRejected.Count();
            }
            string resultstring = "Total number of record inserted :" + totalRecordInsert + ",  Total number of record rejected : " + totalRecordRejected;
            ApiResponse response = new ApiResponse();
            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, aPIScheduleDataMigrationRejected };
            return response;
        }

        public static bool ValidateDifficultyLevel(string difficultyLevel)
        {
            bool valid = false;

            //UserId
            try
            {
                if (difficultyLevel != null && !string.IsNullOrEmpty(difficultyLevel))
                {
                    if ((difficultyLevel.ToLower()) == Record.Simple || (difficultyLevel.ToLower()) == Record.Difficult || (difficultyLevel.ToLower()) == Record.Tough)
                    {
                        valid = true;

                    }
                    else
                    {
                        valid = false;

                    }
                }
                else
                {
                    valid = false;

                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }

        public static bool ValidateMarks(string marks)
        {

            bool valid = false;
            //UserId
            try
            {
                if (marks != null && !string.IsNullOrEmpty(marks))
                {
                    int mark = Convert.ToInt32(marks);
                    if (mark > 10 || mark < 0)
                    {
                        sbError.Append("Enter Valid Marks. ");
                        valid = false;
                    }
                    else
                    {
                        valid = true;
                    }
                }
                else
                {
                    sbError.Append("Marks is null ");
                    valid = true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                valid = false;
            }
            return valid;
        }

        public static bool ValidateQuestionText(string questionText)
        {
            bool valid = false;
            //UserId
            try
            {
                if (questionText != null && !string.IsNullOrEmpty(questionText))
                {
                    string str = "^[a-zA-Z0-9-!@#$%&*" + "(-_,.?;:'/+)][a-zA-Z0-9-!@#$%&\n*(-_,.?;:' ]*$";
                    var regexQuestionText = new Regex(str);
                    if (regexQuestionText.IsMatch(questionText))
                    {
                        valid = true;
                    }
                    else
                    {
                        sbError.Append("QuestionText is Wrong");
                        valid = false;
                    }
                }
                else
                {
                    sbError.Append("QuestionText is null");
                    valid = false;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }
        public bool ValidateSection(string Section)
        {
            bool valid = false;
            try
            {

                if (Section != null && !string.IsNullOrEmpty(Section))
                {
                    if ((Section.ToLower()) == Record.Objective || (Section.ToLower()) == Record.Subjective)
                    {
                        valid = true;

                    }
                    else
                    {
                        valid = false;

                    }
                    dataMigrationRecords.CourseCode = Section;
                }
                else
                {
                    dataMigrationRecords.CourseCode = Section;
                    sbError.Append("Section is null");
                    valid = true;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return valid;
        }

        #endregion
        #region Course Module Data Migration
        public async Task<ApiResponse> ProcessCourseModuleRecordsAsync(IDataMigration _dataMigration, int userid)
        {
            List<APICourseModuleDataMigration> aPIDataMigrationRejected = new List<APICourseModuleDataMigration>();
            if (CourseCompletionRecord != null && CourseCompletionRecord.Count > 0)
            {
                List<APICourseModuleDataMigration> CourseModuleData = new List<APICourseModuleDataMigration>();
                foreach (string record in CourseCompletionRecord)
                {
                    int countLenght = record.Length;
                    if (record != null && countLenght > 1)
                    {
                        string[] textpart = record.Split('\t');
                        string[][] mainRecord = { header, textpart };

                        string lblCourseName = string.Empty;
                        string lblCourseType = string.Empty;
                        string lblContentType = string.Empty;
                        string lblModifiedDate = string.Empty;
                        string lblMetaData = string.Empty;
                        string lblModuleType = string.Empty;
                        string lblCourseCode = string.Empty;
                        string lblIsApplicableToAll = string.Empty;
                        string lblCourseCatagoryName = string.Empty;
                        string lblCourseSubCatagoryName = string.Empty;
                        string lblCourseDesciption = string.Empty;
                        string lblModuleName = string.Empty;
                        string lblIsActive = string.Empty;

                        string headerText = "";

                        int arrasize = header.Count();

                        for (int j = 0; j < arrasize - 1; j++)
                        {
                            headerText = header[j].Trim();
                            string[] mainspilt = headerText.Split('\t');

                            headerText = mainspilt[0];
                            if (headerText == "CourseName")
                            {
                                try
                                {

                                    string[] EmployeeCodeSplit = mainRecord[1][j].Split('\t');
                                    lblCourseName = EmployeeCodeSplit[0].Trim();
                                    bool valid = ValidateCourseName(headerText, lblCourseName);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }

                            if (headerText == "CourseType")
                            {
                                try
                                {
                                    string[] CourseCodesplit = mainRecord[1][j].Split('\t');
                                    lblCourseType = CourseCodesplit[0].Trim();
                                    bool valid = ValidateCourseType(headerText, lblCourseType);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }

                            if (headerText == "ContentType")
                            {
                                try
                                {
                                    string[] LastActivityDatesplit = mainRecord[1][j].Split('\t');
                                    lblContentType = LastActivityDatesplit[0].Trim();

                                    bool valid = ValidateContentType(headerText, lblContentType);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }

                            if (headerText == "MetaData")
                            {
                                try
                                {
                                    string[] LastActivityDatesplit = mainRecord[1][j].Split('\t');
                                    lblMetaData = LastActivityDatesplit[0].Trim();

                                    bool valid = ValidateMetaData(headerText, lblMetaData);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }

                            if (headerText == "IsApplicableToAll")
                            {
                                try
                                {
                                    string[] LastActivityDatesplit = mainRecord[1][j].Split('\t');
                                    lblIsApplicableToAll = LastActivityDatesplit[0].Trim();

                                    bool valid = ValidateIsApplicableToAll(headerText, lblIsApplicableToAll);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }

                            if (headerText == "ModuleType")
                            {
                                try
                                {
                                    string[] LastActivityDatesplit = mainRecord[1][j].Split('\t');
                                    lblModuleType = LastActivityDatesplit[0].Trim();

                                    bool valid = ValidateModuleType(headerText, lblModuleType);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                            if (headerText == "CourseCode")
                            {
                                try
                                {
                                    string[] LastActivityDatesplit = mainRecord[1][j].Split('\t');
                                    lblCourseCode = LastActivityDatesplit[0].Trim();

                                    bool valid = ValidateCourseCo(headerText, lblCourseCode);
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                            if (headerText == "CourseCatagoryName")
                            {
                                try
                                {
                                    string[] LastActivityDatesplit = mainRecord[1][j].Split('\t');
                                    lblCourseCatagoryName = LastActivityDatesplit[0].Trim();
                                    CourseModuleDataMigration.CourseCatagoryName = lblCourseCatagoryName;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                            if (headerText == "CourseSubCatagoryName")
                            {
                                try
                                {
                                    string[] LastActivityDatesplit = mainRecord[1][j].Split('\t');
                                    lblCourseSubCatagoryName = LastActivityDatesplit[0].Trim();
                                    CourseModuleDataMigration.CourseSubCatagoryName = lblCourseSubCatagoryName;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                            if (headerText == "CourseDesciption")
                            {
                                try
                                {
                                    string[] CourseDesciptionsplit = mainRecord[1][j].Split('\t');
                                    lblCourseDesciption = CourseDesciptionsplit[0].Trim();
                                    CourseModuleDataMigration.CourseDescription = lblCourseDesciption;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                            if (headerText == "ModuleName")
                            {
                                try
                                {
                                    string[] ModuleNamesplit = mainRecord[1][j].Split('\t');
                                    lblModuleName = ModuleNamesplit[0].Trim();
                                    CourseModuleDataMigration.ModuleName = lblModuleName;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }
                            if (headerText == "IsActive")
                            {
                                try
                                {
                                    string[] IsActivesplit = mainRecord[1][j].Split('\t');
                                    lblIsActive = IsActivesplit[0].Trim();
                                    CourseModuleDataMigration.IsActive = lblIsActive;
                                }
                                catch (Exception ex)
                                {
                                    _logger.Error(Utilities.GetDetailedException(ex));
                                }
                            }

                        }
                        try
                        {
                            using (var dbContext = this._customerConnectionStringRepository.GetDbContext())
                            {
                                var connection = dbContext.Database.GetDbConnection();
                                if (connection.State == ConnectionState.Broken || connection.State == ConnectionState.Closed)
                                    connection.Open();
                                DynamicParameters parameters = new DynamicParameters();

                                parameters.Add("@CourseName", CourseModuleDataMigration.CourseName);
                                parameters.Add("@CourseCode", CourseModuleDataMigration.CourseCode);
                                parameters.Add("@CourseType", CourseModuleDataMigration.CourseType);
                                parameters.Add("@ContentType", CourseModuleDataMigration.ContentType);
                                parameters.Add("@MetaData", CourseModuleDataMigration.MetaData);
                                parameters.Add("@ModuleType", CourseModuleDataMigration.ModuleType);
                                parameters.Add("@IsApplicableToAll", CourseModuleDataMigration.IsApplicableToAll);
                                parameters.Add("@LoginUserID", userid);
                                parameters.Add("@CourseCatagoryName", CourseModuleDataMigration.CourseCatagoryName);
                                parameters.Add("@CourseSubCatagoryName", CourseModuleDataMigration.CourseSubCatagoryName);
                                parameters.Add("@CourseDescription", CourseModuleDataMigration.CourseDescription);
                                parameters.Add("@ModuleName", CourseModuleDataMigration.ModuleName);
                                parameters.Add("@IsActive", CourseModuleDataMigration.IsActive);

                                var Result = await SqlMapper.QueryAsync<APICourseModuleDataMigration>((SqlConnection)connection, "dbo.CourseModuleDataMigration_BulkUpload", parameters, null, null, CommandType.StoredProcedure);

                                APICourseModuleDataMigration resAPIDataMigration = Result.FirstOrDefault();

                                dataMigrationRecords.ErrMessage = resAPIDataMigration.ErrMessage;
                                dataMigrationRecords.IsInserted = resAPIDataMigration.IsInserted;
                                dataMigrationRecords.IsUpdated = resAPIDataMigration.IsUpdated;
                                dataMigrationRecords.InsertedID = resAPIDataMigration.InsertedID;
                                dataMigrationRecords.InsertedCode = resAPIDataMigration.InsertedCode;
                                dataMigrationRecords.notInsertedCode = resAPIDataMigration.notInsertedCode;

                                CourseModuleData.Add(resAPIDataMigration);

                                connection.Close();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(Utilities.GetDetailedException(ex));

                        }
                        dataMigrationRecords = new APIDataMigration();
                        sbError.Clear();

                    }
                    else
                    {
                    }
                }
                foreach (var data in CourseModuleData)
                {
                    if (data.ErrMessage != null)
                    {
                        totalRecordRejected++;
                        aPIDataMigrationRejected.Add(data);
                    }
                    else
                    {
                        totalRecordInsert++;

                    }
                }
            }

            string resultstring = "Total number of record inserted :" + totalRecordInsert + ",  Total number of record rejected : " + totalRecordRejected;
            ApiResponse response = new ApiResponse();
            if (totalRecordRejected == 0)
            {
                response.StatusCode = 200;
                response.ResponseObject = new { resultstring, aPIDataMigrationRejected };
            }
            else
            {
                response.StatusCode = 400;
                response.ResponseObject = new { resultstring, aPIDataMigrationRejected };

            }
            return response;
        }
        #endregion

        public async Task<ApiResponse> TrainingRecommondationImportFile(APIDataMigrationFilePath aPIILTScheduleImport, int UserId, string OrgCode)
        {
            ApiResponse Response = new ApiResponse();
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string DomainName = this._configuration["ApiGatewayUrl"];
                string filepath = sWebRootFolder + aPIILTScheduleImport.Path;

                DataTable scheduledt = ReadFile(filepath);
                if (scheduledt == null || scheduledt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new ApiResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                List<string> importcolumns = GetTrainingNeedImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(scheduledt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessTrainingNeedRecordsAsync(scheduledt, UserId, OrgCode);
                    return Response;
                }
                else
                {
                    Response.StatusCode = 204;
                    string resultstring = Record.FileInvalid;
                    Response.ResponseObject = new { resultstring };
                    return Response;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return Response;
        }
       

        public string ValidateJobRole(string InputJobRole)
        {
            string JobRole = null;
            try
            {
                if (JobRole != null && !string.IsNullOrEmpty(JobRole))
                {
                    JobRole = InputJobRole;
                }
               
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return JobRole;
        }
        public async Task<byte[]> TrainingReommendationFormat(string OrgCode)
        {
            string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
            sWebRootFolder = Path.Combine(sWebRootFolder, OrgCode);
            string sFileName = FileName.TrainingReommendationNeeds;
            string DomainName = this._configuration["ApiGatewayUrl"];
            string URL = string.Format("{0}{1}/{2}", DomainName, OrgCode, sFileName);
            FileInfo file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            if (file.Exists)
            {
                file.Delete();
                file = new FileInfo(Path.Combine(sWebRootFolder, sFileName));
            }
            using (ExcelPackage package = new ExcelPackage(file))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("ILTSchedule");
                worksheet.Cells[1, 1].Value = "JobRole*";
                worksheet.Cells[1, 2].Value = "Department*";
                worksheet.Cells[1, 3].Value = "Section*";
                worksheet.Cells[1, 4].Value = "Level*";
                worksheet.Cells[1, 5].Value = "Status*";
                worksheet.Cells[1, 6].Value = "TraniningProgram *";
                worksheet.Cells[1, 7].Value = "Category*";
                using (var rngitems = worksheet.Cells["A1:G1"])//Applying Css for header
                {
                    rngitems.Style.Font.Bold = true;
                }
                package.Save();
            }
            var Fs = file.OpenRead();
            byte[] fileData = null;
            using (BinaryReader binaryReader = new BinaryReader(Fs))
            {
                fileData = binaryReader.ReadBytes((int)Fs.Length);
            }
            return fileData;
        }
    }
}
