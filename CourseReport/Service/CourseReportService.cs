using CourseReport.API.APIModel;
using CourseReport.API.Helper;
using CourseReport.API.Helper.MetaData;
using CourseReport.API.Repositories.Interface;
using CourseReport.API.Service.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using log4net;
using CourseReport.API.APIModel;
using CourseReport.API.Model;
using CourseReport.API.Helper.Interfaces;

namespace CourseReport.API.Service
{
    public class CourseReportService : ICourseReportService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(CourseReportService));

        private readonly ICourseReportRepository _coursereportRepository;
        private readonly IReportRepository _reportRepository;
        private readonly ITLSHelper _tlsHelper;
        private readonly IToDataTableConverter _toDataTableConverter;

        public CourseReportService(ICourseReportRepository coursereportRepository, ITLSHelper tlsHelper, IToDataTableConverter toDataTableConverter)
        {
            this._coursereportRepository = coursereportRepository;
            this._tlsHelper = tlsHelper;

            this._toDataTableConverter = toDataTableConverter;
        }

        public async Task<int> PostCourseWiseCompletionReport(APIPostCourseWiseCompletionReport data, int UserId)
        {
            int Id = await this._coursereportRepository.PostCourseWiseCompletionReport(data, UserId);

            return Id;
        }
        public async Task<string> UpdateCourseWiseCompletionReport(APIUpdateCourseWiseCompletionReport data, int UserId)
        {
            string path = await this._coursereportRepository.UpdateCourseWiseCompletionReport(data, UserId);
            return path;
        }

        public async Task<ExportCourseCompletionDetailReport> UpdateonDownloadReport(int Id)
        {
            ExportCourseCompletionDetailReport data = await this._coursereportRepository.UpdateonDownloadReport(Id);
            return data;
        }
        
        public async Task<CourseWiseCompletionReports> GetCourseWiseCompletionReports(int page, int pageSize, int UserId)
        {
            CourseWiseCompletionReports ListandCount = await this._coursereportRepository.GetCourseWiseCompletionReports(page,pageSize,UserId);
            
            return ListandCount; 
        }


        public async Task<IEnumerable<APICourseWiseCompletionReport>> GetCourseWiseCompletionReport(APICourseWiseCompletionModule courseWiseCompletionModule, string OrgCode)
        {
            IEnumerable<APICourseWiseCompletionReport> ReportData = await this._coursereportRepository.GetCourseWiseCompletionReport(courseWiseCompletionModule, OrgCode);
            string dateformat = await this._coursereportRepository.GetConfigurableParameterValue("APPLICATION_DATE_FORMAT");
            int length = dateformat.Length;
            foreach (APICourseWiseCompletionReport report in ReportData)
            {
                report.UserId = Security.Decrypt(report.UserId);

                if (!string.IsNullOrEmpty(report.CourseStartDate))
                {
                    report.CourseStartDate = report.CourseStartDate.Substring(0, length);
                }

                if (!string.IsNullOrEmpty(report.ContentCompletionDate) && report.ContentCompletionDate != "NA")
                {
                    report.ContentCompletionDate = report.ContentCompletionDate.Substring(0, length);
                }

                if (!string.IsNullOrEmpty(report.AssessmentDate))
                {
                    report.AssessmentDate = report.AssessmentDate.Substring(0, length);
                }

                if (!string.IsNullOrEmpty(report.FeedbackDate))
                {
                    report.FeedbackDate = report.FeedbackDate.Substring(0, length);
                }

                if (!string.IsNullOrEmpty(report.CourseCompletionDate) && report.CourseCompletionDate != "NA")
                {
                    report.CourseCompletionDate = report.CourseCompletionDate.Substring(0, length);
                }
            }
            return ReportData;
        }
        public async Task<IEnumerable<ApiModeratorwiseSubjectSummaryReport>> GetModeratorwiseSubjectSummaryReport(APIModeratorwiseSubjectSummaryModule subjectSummaryModule)
        {
            IEnumerable<ApiModeratorwiseSubjectSummaryReport> SubjectSummaryReport = await this._coursereportRepository.GetModeratorSubjectSummaryReport(subjectSummaryModule);

            return SubjectSummaryReport;

        }
        public async Task<IEnumerable<ApiModeratorwiseSubjectDeailsReport>> GetModeratorwiseSubjectDetailsReport(APIModeratorwiseSubjectDetailsModule subjectDetailsModule)
        {
            IEnumerable<ApiModeratorwiseSubjectDeailsReport> SubjectDetailsReport = await this._coursereportRepository.GetModeratorSubjectDetailsReport(subjectDetailsModule);

            return SubjectDetailsReport;

        }

        public async Task<FileInfo> ExportCourseWiseCompletionReport(APICourseWiseCompletionModule courseWiseCompletionModule, string OrgCode, List<APIUserSetting> userSetting)
        {
            IEnumerable<APICourseWiseCompletionReport> CourseWiseCompletionList = await this._coursereportRepository.GetCourseWiseCompletionReportExport(courseWiseCompletionModule, OrgCode);

            FileInfo File = GetCourseWiseCompletionReportExcel(CourseWiseCompletionList, OrgCode, courseWiseCompletionModule.ExportAs, userSetting);
            return File;
        }

        public async Task<FileInfo> ExportModeratorwiseSubjectSummaryReport(APIModeratorwiseSubjectSummaryModule subjectSummaryModule)
        {
            DataTable SubjectSummaryList = await this._coursereportRepository.GetSubjectSummaryReport(subjectSummaryModule);

            List<string> HeaderList = new List<string>();
            for (int i = 0; i < SubjectSummaryList.Columns.Count; i++)
            {
                HeaderList.Add(SubjectSummaryList.Columns[i].ColumnName);
            }

            FileInfo File = GetSubjectSummaryReportExcel(SubjectSummaryList, HeaderList);

            return File;
        }
        public async Task<FileInfo> ExportModeratorwiseSubjectDetailsReport(APIModeratorwiseSubjectDetailsModule subjectDetailsModule)
        {
            DataTable SubjectDetailsList = await this._coursereportRepository.GetSubjectDetailsReport(subjectDetailsModule);

            List<string> HeaderList = new List<string>();
            for (int i = 0; i < SubjectDetailsList.Columns.Count; i++)
            {
                HeaderList.Add(SubjectDetailsList.Columns[i].ColumnName);
            }

            FileInfo File = GetSubjectDetailsReportExcel(SubjectDetailsList, HeaderList);

            return File;
        }
        private FileInfo GetSubjectSummaryReportExcel(DataTable objList, List<string> HeaderList)
        {
            String ExcelName = FileName.ModeratorSubjectwiseContentSummaryReport;
            int RowNumber = 0;
            Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

            //Adding Headers for excel file
            ExcelData.Add(RowNumber, HeaderList);

            //Adding data row wise for excel file
            while (RowNumber < objList.Rows.Count)
            {
                List<string> ProcessEvaluationReportRow = ProcessEvaluationReport(objList, RowNumber);
                RowNumber++;
                ExcelData.Add(RowNumber, ProcessEvaluationReportRow);
            }

            FileInfo ExcelFile = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
            return ExcelFile;
        }
        private FileInfo GetSubjectDetailsReportExcel(DataTable objList, List<string> HeaderList)
        {
            String ExcelName = FileName.ModeratorSubjectwiseContentDetailsReport;
            int RowNumber = 0;
            Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

            //Adding Headers for excel file
            ExcelData.Add(RowNumber, HeaderList);

            //Adding data row wise for excel file
            while (RowNumber < objList.Rows.Count)
            {
                List<string> ProcessEvaluationReportRow = ProcessEvaluationReport(objList, RowNumber);
                RowNumber++;
                ExcelData.Add(RowNumber, ProcessEvaluationReportRow);
            }

            FileInfo ExcelFile = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
            return ExcelFile;
        }
        private List<string> ProcessEvaluationReport(DataTable dt, int RowNumber)
        {
            List<string> ProcessEvaluationRow = new List<string>();
            try
            {

                // ----- j=0 as dynamic columns are coming with index 192 from SP -------- //
                for (int j = 0; j <= dt.Columns.Count; j++)
                {
                    ProcessEvaluationRow.Add(dt.Rows[RowNumber][j].ToString());
                }
            }
            catch (Exception ex)
            { }
            return ProcessEvaluationRow;
        }


        private FileInfo GetCourseWiseCompletionReportExcel(IEnumerable<APICourseWiseCompletionReport> courseWiseCompletionList, string OrgCode, string ExportAs, List<APIUserSetting> userSetting)
        {
            String ExcelName = FileName.CourseWiseCompletionReport;
            String ExcelNameCSV = FileName.CourseWiseCompletionReportCSV;
            int RowNumber = 0;
            Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

            //Adding Headers for excel file
            List<string> CourseWiseCompletionHeaders = GetCourseWiseCompletionHeaders(OrgCode, userSetting);
            ExcelData.Add(RowNumber, CourseWiseCompletionHeaders);

            //Adding data row wise for excel file
            foreach (APICourseWiseCompletionReport courseWiseCompletiondata in courseWiseCompletionList)
            {
                List<string> courseWiseCompletionRow = CourseWiseCompletionRow(courseWiseCompletiondata, OrgCode, userSetting);
                RowNumber++;
                ExcelData.Add(RowNumber, courseWiseCompletionRow);
            }
            if (ExportAs == "csv")
            {


                foreach (APICourseWiseCompletionReport item in courseWiseCompletionList)
                {
                    item.UserId = Security.Decrypt(item.UserId);

/*                    if (item.CourseStatus == "completed")
                    {

                        item.CourseStatus = Convert.ToString("Completed");
                    }
                    else
                    {
                        item.CourseStatus = Convert.ToString("Inprogress");
                    }*/

/*                    if (item.ModuleStatus == "completed")
                    {

                        item.ModuleStatus = Convert.ToString("Completed");
                    }
                    else if (item.ModuleStatus == "inprogress")
                    {

                        item.ModuleStatus = Convert.ToString("Inprogress");
                    }*/
                }



                DataTable ForToCsv = this._toDataTableConverter.ToDataTableCourseWiseCompletionReport<APICourseWiseCompletionReport>(courseWiseCompletionList, OrgCode, userSetting);

                //file info here which takes the datatable and filename as input
                FileInfo csvFile = this._tlsHelper.ToCSV(ForToCsv, ExcelNameCSV);

                return csvFile;
            }
            else
            {
                FileInfo ExcelFile = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
                return ExcelFile;
            }
            
        }

        private List<string> GetCourseWiseCompletionHeaders(string OrgCode, List<APIUserSetting> userSetting)
        {
            if (OrgCode == "hdfc" || OrgCode == "HDFC")
            {
                List<string> CourseWiseCompletionHeader = new List<string>
                {
                    HeaderName.UserId,
                    HeaderName.UserName,
                    HeaderName.EmailId,
                    HeaderName.CourseCode,
                    HeaderName.CourseTitle,
                    HeaderName.ModuleName,
                    HeaderName.CourseAssignedDate,
                    HeaderName.CourseStartDate,
                    HeaderName.ContentCompletionDate,
                    HeaderName.PreAssessmentResult,
                    HeaderName.PreAssessmentStatus,
                    HeaderName.PreAssessmentPercentage,
                    HeaderName.AssessmentDate,
                    HeaderName.AssessmentPercentage,
                    HeaderName.AssessmentResult,
                    HeaderName.NoOfAttempts,
                    HeaderName.FeedbackDate,
                    HeaderName.FeedbackStatus,
                    HeaderName.CourseCompletionDate,
                    //HeaderName.MarksObtained,
                    HeaderName.CourseStatus,
                    HeaderName.ModuleStatus,
                    HeaderName.DateOfJoining1,
                    HeaderName.AdaptiveLearning,
              //      HeaderName.UserDuration


                };

                int count = userSetting.Count;
                for (int i = 0; i < count; i++)
                {
                    if (userSetting[i].IsShowInReport == true)
                    {
                        CourseWiseCompletionHeader.Add(userSetting[i].ChangedColumnName);
                    }
                }

                return CourseWiseCompletionHeader;
            }
            else if (OrgCode == "aviva" || OrgCode == "singlife" || OrgCode == "ent" || OrgCode == "thermax" || OrgCode == "evalueserve")
            {
                List<string> CourseWiseCompletionHeader = new List<string>
                {
                    HeaderName.UserId,
                    HeaderName.UserName,
                    HeaderName.EmailId,
                    HeaderName.CourseCode,
                    HeaderName.CourseTitle,
                    HeaderName.ModuleName,
                    HeaderName.CourseAssignedDate,
                    HeaderName.CourseStartDate,
                    HeaderName.ContentCompletionDate,
                    HeaderName.PreAssessmentResult,
                    HeaderName.PreAssessmentStatus,
                    HeaderName.PreAssessmentPercentage,
                    HeaderName.AssessmentDate,
                    HeaderName.AssessmentPercentage,
                    HeaderName.AssessmentResult,
                    HeaderName.NoOfAttempts,
                    HeaderName.FeedbackDate,
                    HeaderName.FeedbackStatus,
                    HeaderName.CourseCompletionDate,
                    //HeaderName.MarksObtained,
                    HeaderName.CourseStatus,
                    HeaderName.ModuleStatus,
                 //   HeaderName.UserDuration,
                    HeaderName.ModuleDurationInMinutes,
                    HeaderName.CourseDurationInMinutes,
                    HeaderName.DateOfJoining1,
                    HeaderName.AdaptiveLearning,

                };

                int count = userSetting.Count;
                for (int i = 0; i < count; i++)
                {
                    if (userSetting[i].IsShowInReport == true)
                    {
                        CourseWiseCompletionHeader.Add(userSetting[i].ChangedColumnName);
                    }
                }

                return CourseWiseCompletionHeader;
            }
            else if (OrgCode == "cap" )
            {
                List<string> CourseWiseCompletionHeader = new List<string>
                {
                    HeaderName.UserId,
                    HeaderName.UserName,
                    HeaderName.EmailId,
                    HeaderName.CourseCode,
                    HeaderName.CourseTitle,
                    HeaderName.CourseCategory,
                    HeaderName.CourseSubCategory,
                    HeaderName.ModuleName,
                    HeaderName.CourseAssignedDate,
                    HeaderName.CourseStartDate,
                    HeaderName.ContentCompletionDate,
                    HeaderName.PreAssessmentResult,
                    HeaderName.PreAssessmentStatus,
                    HeaderName.PreAssessmentPercentage,
                    HeaderName.AssessmentDate,
                    HeaderName.AssessmentPercentage,
                    HeaderName.AssessmentResult,
                    HeaderName.NoOfAttempts,
                    HeaderName.FeedbackDate,
                    HeaderName.FeedbackStatus,
                    HeaderName.CourseCompletionDate,
                    //HeaderName.MarksObtained,
                    HeaderName.CourseStatus,
                    HeaderName.ModuleStatus,
              //      HeaderName.UserDuration,
                    HeaderName.CourseDurationInMinutes,
                    HeaderName.DateOfJoining1,
                    HeaderName.AdaptiveLearning,


                };

                int count = userSetting.Count;
                for (int i = 0; i < count; i++)
                {
                    if (userSetting[i].IsShowInReport == true)
                    {
                        CourseWiseCompletionHeader.Add(userSetting[i].ChangedColumnName);
                    }
                }

                return CourseWiseCompletionHeader;
            }
            else
            {
                List<string> CourseWiseCompletionHeader = new List<string>
                {
                    HeaderName.UserId,
                    HeaderName.UserName,
                    HeaderName.EmailId,
                    HeaderName.CourseCode,
                    HeaderName.CourseTitle,
                    HeaderName.ModuleName,
                    HeaderName.CourseAssignedDate,
                    HeaderName.CourseStartDate,
                    HeaderName.ContentCompletionDate,
                    HeaderName.PreAssessmentResult,
                    HeaderName.PreAssessmentStatus,
                    HeaderName.PreAssessmentPercentage,
                    HeaderName.AssessmentDate,
                    HeaderName.AssessmentPercentage,
                    HeaderName.AssessmentResult,
                    HeaderName.NoOfAttempts,
                    HeaderName.FeedbackDate,
                    HeaderName.FeedbackStatus,
                    HeaderName.CourseCompletionDate,
                    //HeaderName.MarksObtained,
                    HeaderName.CourseStatus,
                    HeaderName.ModuleStatus,
                    HeaderName.DateOfJoining1,
                    HeaderName.AdaptiveLearning,
                 //   HeaderName.UserDuration
                };

                int count = userSetting.Count;
                for (int i = 0; i < count; i++)
                {
                    if (userSetting[i].IsShowInReport == true)
                    {
                        CourseWiseCompletionHeader.Add(userSetting[i].ChangedColumnName);
                    }
                }

                return CourseWiseCompletionHeader;
            }
        }

        private List<string> CourseWiseCompletionRow(APICourseWiseCompletionReport courseWiseCompletiondata, string OrgCode, List<APIUserSetting> userSetting)
        {
            List<string> CourseWiseCompletionRow = new List<string>();

            try
            {
                //  DateTime DateValue = new DateTime();

                if (OrgCode.ToLower() != "canh" && OrgCode.ToLower() != "canhuat")
                {

                    //if (!string.IsNullOrEmpty(courseWiseCompletiondata.CourseStartDate))
                    //{
                    //    DateValue = Convert.ToDateTime(courseWiseCompletiondata.CourseStartDate);
                    //    courseWiseCompletiondata.CourseStartDate = DateValue.ToString("MMM dd, yyyy");
                    //}


                    //if (courseWiseCompletiondata.ContentCompletionDate != "NA")
                    //{
                    //    DateValue = Convert.ToDateTime(courseWiseCompletiondata.ContentCompletionDate);
                    //    courseWiseCompletiondata.ContentCompletionDate = DateValue.ToString("MMM dd, yyyy");
                    //}

                    //if (!string.IsNullOrEmpty(courseWiseCompletiondata.AssessmentDate))
                    //{
                    //    DateValue = Convert.ToDateTime(courseWiseCompletiondata.AssessmentDate);
                    //    courseWiseCompletiondata.AssessmentDate = DateValue.ToString("MMM dd, yyyy");
                    //}

                    //if (!string.IsNullOrEmpty(courseWiseCompletiondata.FeedbackDate))
                    //{
                    //    DateValue = Convert.ToDateTime(courseWiseCompletiondata.FeedbackDate);
                    //    courseWiseCompletiondata.FeedbackDate = DateValue.ToString("MMM dd, yyyy");
                    //}

                    //if (courseWiseCompletiondata.CourseCompletionDate != "NA")
                    //{
                    //    DateValue = Convert.ToDateTime(courseWiseCompletiondata.CourseCompletionDate);
                    //    courseWiseCompletiondata.CourseCompletionDate = DateValue.ToString("MMM dd, yyyy");
                    //}
                }

                CourseWiseCompletionRow.Add(Security.Decrypt(courseWiseCompletiondata.UserId));
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.UserName);
                CourseWiseCompletionRow.Add(Security.Decrypt(courseWiseCompletiondata.EmailId));
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseCode);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseTitle);
                if (OrgCode == "cap")
                {
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseCategory);
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseSubCategory);
                }
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.ModuleName);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseAssignedDate);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseStartDate);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.ContentCompletionDate);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.PreAssessmentResult);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.PreAssessmentStatus);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.PreAssessmentPercentage);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.AssessmentDate);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.AssessmentPercentage);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.AssessmentResult);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.NoOfAttempts);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.FeedbackDate);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.FeedbackStatus);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseCompletionDate);
                //CourseWiseCompletionRow.Add(courseWiseCompletiondata.MarksObtained);
                //CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseStatus);
                //CourseWiseCompletionRow.Add(courseWiseCompletiondata.ModuleStatus);
                if (courseWiseCompletiondata.CourseStatus == "completed")

                {

                    courseWiseCompletiondata.CourseStatus = Convert.ToString("Completed");
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseStatus);
                }
                else
                {
                    courseWiseCompletiondata.CourseStatus = Convert.ToString("Inprogress");
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseStatus);
                }
                if (courseWiseCompletiondata.ModuleStatus == "completed")
                {

                    courseWiseCompletiondata.ModuleStatus = Convert.ToString("Completed");
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.ModuleStatus);
                }
                else if (courseWiseCompletiondata.ModuleStatus == "inprogress")
                {

                    courseWiseCompletiondata.ModuleStatus = Convert.ToString("Inprogress");
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.ModuleStatus);
                }
                else
                {
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.ModuleStatus);
                }
                //if (OrgCode.ToLower() != "hdfc")
                //{
                //    CourseWiseCompletionRow.Add(courseWiseCompletiondata.UserDuration);
                //}
                if (OrgCode.ToLower() == "aviva" || OrgCode.ToLower() == "singlife"  || OrgCode == "ent" || OrgCode == "thermax" || OrgCode == "evalueserve")
                {
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.ModuleDuration);
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.CourseDuration);
                }

                CourseWiseCompletionRow.Add(courseWiseCompletiondata.DateOfJoining);
                CourseWiseCompletionRow.Add(courseWiseCompletiondata.IsAdaptiveAssessment);

                if (userSetting[0].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Business);

                if (userSetting[1].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Group);

                if (userSetting[2].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Area);

                if (userSetting[3].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Location);

                if (userSetting[4].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn1);

                if (userSetting[5].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn2);

                if (userSetting[6].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn3);

                if (userSetting[7].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn4);

                if (userSetting[8].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn5);

                if (userSetting[9].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn6);

                if (userSetting[10].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn7);

                if (userSetting[11].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn8);

                if (userSetting[12].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn9);

                if (userSetting[13].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn10);

                if (userSetting[14].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn11);

                if (userSetting[15].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn12);

                if (userSetting[16].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn13);

                if (userSetting[17].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn14);

                if (userSetting[18].IsShowInReport == true)
                    CourseWiseCompletionRow.Add(courseWiseCompletiondata.Configurationcolumn15);
            }


            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return CourseWiseCompletionRow;
        }

        #region User Learning Report
        public async Task<IEnumerable<APIUserLearningReport>> GetUserLearningReport(APIUserLearningReportModule UserLearningReportModule, int UserID, string OrgCode)
        {
            IEnumerable<APIUserLearningReport> ReportData = await this._coursereportRepository.GetUserLearningReport(UserLearningReportModule, UserID, OrgCode);

            return ReportData;
        }


        public async Task<FileInfo> ExportUserLearningReport(APIUserLearningReportModule UserLearningReportModule, int UserID, string OrgCode)
        {
            IEnumerable<APIUserLearningReport> UserLearningReportList = await this._coursereportRepository.GetUserLearningReport(UserLearningReportModule, UserID, OrgCode);
            FileInfo File = GetUserLearningReportExcel(UserLearningReportList, UserLearningReportModule.ExportAs, OrgCode);
            return File;
        }

        private FileInfo GetUserLearningReportExcel(IEnumerable<APIUserLearningReport> UserLearningReportList, string ExportAs,string OrgCode)
        {
            String ExcelName = FileName.UserLearningReport;
            String ExcelNameCSV = FileName.UserLearningReportCSV;
            int RowNumber = 0;
            Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

            //Adding Headers for excel file
            List<string> UserLearningReportHeaders = GetUserLearningReportHeaders(OrgCode);
            ExcelData.Add(RowNumber, UserLearningReportHeaders);

            //Adding data row wise for excel file
            foreach (APIUserLearningReport UserLearningdata in UserLearningReportList)
            {
                List<string> UserLearningRow = UserLearningReportRow(UserLearningdata, OrgCode);
                RowNumber++;
                ExcelData.Add(RowNumber, UserLearningRow);
            }

            if (ExportAs == "csv")
            {


                //foreach (APIUserLearningReport item in UserLearningReportList)
                //{
                //    item.UserID = Security.Decrypt(item.UserID);
                //}



                DataTable ForToCsv = this._toDataTableConverter.ToDataTableUserLearningReport<APIUserLearningReport>(UserLearningReportList,OrgCode);

                //file info here which takes the datatable and filename as input
                FileInfo csvFile = this._tlsHelper.ToCSV(ForToCsv, ExcelNameCSV);

                return csvFile;
            }
            else
            {
                FileInfo fileInfo = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
                return fileInfo;
            }
        }

        private List<string> GetUserLearningReportHeaders(string OrgCode)
        {
            List<string> UserLearningHeader = new List<string>();
            if (OrgCode == "ent" || OrgCode.ToLower() == "lenexis")
            {
                UserLearningHeader = new List<string>
            {
                HeaderName.UserId,
                HeaderName.UserName,
                HeaderName.CourseTitle,
                HeaderName.CourseCode,
                HeaderName.CourseStartDate,
                HeaderName.CourseCompletionDate,
                HeaderName.CourseStatus,
                HeaderName.CourseDuration,
                HeaderName.RestaurantId,
                HeaderName.CurrentRestaurant,
                HeaderName.Region,
                HeaderName.State,
                HeaderName.City,
                HeaderName.ClusterManager,
                HeaderName.AreaManager,
                HeaderName.MarksObtained,
                HeaderName.TotalMarks,
                HeaderName.Percentage
                };
            }
            else
            {
                UserLearningHeader = new List<string>
            {
                HeaderName.UserId,
                HeaderName.UserName,
                HeaderName.CourseTitle,
                HeaderName.CourseCode,
                HeaderName.CourseStartDate,
                HeaderName.CourseCompletionDate,
                HeaderName.CourseStatus,
                HeaderName.CourseDuration
                };
            }
               
            return UserLearningHeader;
        }

        private List<string> UserLearningReportRow(APIUserLearningReport UserLearningdata,string OrgCode)
        {

            List<string> UserLearningRow = new List<string>();
            if (OrgCode == "ent" || OrgCode.ToLower() == "lenexis")
            {
                UserLearningRow = new List<string>
            {
                UserLearningdata.UserID,
                UserLearningdata.UserName,
                UserLearningdata.CourseTitle,
                UserLearningdata.CourseCode,
                UserLearningdata.CourseStartDate,
                UserLearningdata.CourseCompletionDate,
                UserLearningdata.Status,
                UserLearningdata.CourseDuration,
                
                UserLearningdata.RestaurantId == null ? "-" : UserLearningdata.RestaurantId,
                UserLearningdata.CurrentRestaurant == null ? "-" : UserLearningdata.CurrentRestaurant,
                UserLearningdata.Region == null ? "-" :UserLearningdata.Region,
                UserLearningdata.State == null ? "-" :UserLearningdata.State,
                UserLearningdata.City == null ? "-" :UserLearningdata.City,
                UserLearningdata.ClusterManager == null ? "-" :UserLearningdata.ClusterManager,
                UserLearningdata.AreaManager == null ? "-" : UserLearningdata.AreaManager,

                UserLearningdata.MarksObtained == null ? "-" :UserLearningdata.MarksObtained,
                UserLearningdata.TotalMarks == null ? "-" :UserLearningdata.TotalMarks,
                UserLearningdata.AssessmentPercentage == null ? "-" : UserLearningdata.AssessmentPercentage
            };
            }
            else
            {
                UserLearningRow = new List<string>
            {
                UserLearningdata.UserID,
                UserLearningdata.UserName,
                UserLearningdata.CourseTitle,
                UserLearningdata.CourseCode,
                UserLearningdata.CourseStartDate,
                UserLearningdata.CourseCompletionDate,
                UserLearningdata.Status,
                UserLearningdata.CourseDuration,
            };
            }
               

            return UserLearningRow;

            #endregion

        }

        public async Task<IEnumerable<APICourseRatingReport>> CourseRatingReport(APICourseRatingReport aPICourseRatingReport)
        {
            IEnumerable<APICourseRatingReport> ReportData = await this._coursereportRepository.GetCourseRatingReport(aPICourseRatingReport);
            foreach (APICourseRatingReport report in ReportData)
            {
                report.UserId = Security.Decrypt(report.UserId);
            }
            return ReportData;
        }

        public async Task<int> GetCourseRatingReportCount(APICourseRatingReport aPICourseRatingReport)
        {
            int Count = await this._coursereportRepository.GetCourseRatingReportCount(aPICourseRatingReport);
            return Count;
        }
        public async Task<FileInfo> ExportCourseRatingReport(APICourseRatingReport aPICourseRatingReport, string OrgCode)
        {
            IEnumerable<APICourseRatingReport> CourseRatingReportList = await this._coursereportRepository.GetCourseRatingReport(aPICourseRatingReport);
            FileInfo File = GetCourseRatingReportExcel(CourseRatingReportList, OrgCode, aPICourseRatingReport.ExportAs);
            return File;
        }

        private FileInfo GetCourseRatingReportExcel(IEnumerable<APICourseRatingReport> CourseRatingReportList, string OrgCode, string ExportAs)
        {
            String ExcelName = FileName.CourseRatingReport;
            String ExcelNameCSV = FileName.CourseRatingReportCSV;
            int RowNumber = 0;
            Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

            //Adding Headers for excel file
            List<string> CourseRatingReportHeaders = GetCourseRatingReportHeaders();
            ExcelData.Add(RowNumber, CourseRatingReportHeaders);

            List<int> datecolumns = new List<int>();
            int i = 1;
            foreach (string obj in CourseRatingReportHeaders)
            {
                if (obj.Contains("Date"))
                {
                    datecolumns.Add(i);
                }
                i++;
            }

            //Adding data row wise for excel file
            foreach (APICourseRatingReport CourseRatingdata in CourseRatingReportList)
            {
                List<string> CourseRatingRow = CourseRatingReportRow(CourseRatingdata);
                RowNumber++;
                ExcelData.Add(RowNumber, CourseRatingRow);
            }

            if (ExportAs == "csv")
            {


                foreach (APICourseRatingReport item in CourseRatingReportList)
                {
                    item.UserId = Security.Decrypt(item.UserId);
                }



                DataTable ForToCsv = this._toDataTableConverter.ToDataTableCourseRatingReport<APICourseRatingReport>(CourseRatingReportList, OrgCode);

                //file info here which takes the datatable and filename as input
                FileInfo csvFile = this._tlsHelper.ToCSV(ForToCsv, ExcelNameCSV);

                return csvFile;
            }
            else
            {
                FileInfo ExcelFile = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
                return ExcelFile;
            }
        }

        private List<string> GetCourseRatingReportHeaders()
        {
            List<string> CourseRatingHeader = new List<string>
            {
                HeaderName.CourseName,
                 HeaderName.UserId,
                HeaderName.UserName,
                HeaderName.CourseRating,
                HeaderName.ReviewText,
                HeaderName.RatingDate
            };
            return CourseRatingHeader;
        }

        private List<string> CourseRatingReportRow(APICourseRatingReport CourseRatingdata)
        {

            List<string> CourseRatingRow = new List<string>
            {
                CourseRatingdata.CourseName,
                 CourseRatingdata.UserId.Decrypt(),
                CourseRatingdata.UseName,
                CourseRatingdata.ReviewRating,
                CourseRatingdata.ReviewText,
                (CourseRatingdata.ModifiedDate).ToString()
            };
            return CourseRatingRow;
        }

        #region UserwiseCourseStatusReport

        public async Task<FileInfo> ExportUserwiseCourseStatusReport(APIUserwiseCourseStatusReport aPIUserwiseCourseStatusReport)
        {
            IEnumerable<APIUserwiseCourseStatusReportResult> aPIUserwiseCourseStatusReportResults = await _coursereportRepository.GetUserwiseCourseStatusReport(aPIUserwiseCourseStatusReport);
            FileInfo fileInfo = GetUserwiseCourseStatusReport(aPIUserwiseCourseStatusReportResults, aPIUserwiseCourseStatusReport.ExportAs);
            return fileInfo;
        }

        private FileInfo GetUserwiseCourseStatusReport(IEnumerable<APIUserwiseCourseStatusReportResult> aPIUserwiseCourseStatusReportResults, string ExportAs)
        {
            String ExcelNameCSV = FileName.UserwiseCourseStatusReportCSV;
            int RowNumber = 0;
            Dictionary<int, List<string>> ExportData = new Dictionary<int, List<string>>();

            List<string> ExportHeader = GetUserwiseCourseStatusReportHeader();
            ExportData.Add(RowNumber, ExportHeader);

            foreach (APIUserwiseCourseStatusReportResult aPIUserwiseCourseStatusReportResult in aPIUserwiseCourseStatusReportResults)
            {
                List<string> DataRow = new List<string>();
                DataRow = GetUserwiseCourseStatusReportRowData(aPIUserwiseCourseStatusReportResult);
                RowNumber++;
                ExportData.Add(RowNumber, DataRow);
            }

            if (ExportAs == "csv")
            {


                foreach (APIUserwiseCourseStatusReportResult item in aPIUserwiseCourseStatusReportResults)
                {
                    item.UserId = Security.Decrypt(item.UserId);
                    item.ReportsToUserId = Security.Decrypt(item.ReportsToUserId);
                }



                DataTable ForToCsv = this._toDataTableConverter.ToDataTableUserwiseCourseStatusReportResult<APIUserwiseCourseStatusReportResult>(aPIUserwiseCourseStatusReportResults);

                //file info here which takes the datatable and filename as input
                FileInfo csvFile = this._tlsHelper.ToCSV(ForToCsv, ExcelNameCSV);

                return csvFile;
            }
            else
            {
                FileInfo fileInfo = this._tlsHelper.GenerateExcelFile(FileName.UserwiseCourseStatusReport, ExportData);
                return fileInfo;
            }
            
        }

        private List<string> GetUserwiseCourseStatusReportHeader()
        {
            List<string> ExportHeader = new List<string>()
            {
                HeaderName.UserId,
                HeaderName.UserName,
                HeaderName.ReportsToUserId,
                HeaderName.ReportsToUserName,
                HeaderName.UserCategory,
                HeaderName.Status,
                HeaderName.SalesOffName,
                HeaderName.SalesArea,
                HeaderName.Controllingoffice,
                HeaderName.State,
                HeaderName.InProgress,
                HeaderName.Completed,
                HeaderName.Applicable,
                HeaderName.LastLoginDate
            };
            return ExportHeader;
        }

        private List<string> GetUserwiseCourseStatusReportRowData(APIUserwiseCourseStatusReportResult aPIUserwiseCourseStatusReportResult)
        {
            List<string> ExportData = new List<string>()
            {
                aPIUserwiseCourseStatusReportResult.UserId.Decrypt(),
                aPIUserwiseCourseStatusReportResult.UserName,
                aPIUserwiseCourseStatusReportResult.ReportsToUserId.Decrypt(),
                aPIUserwiseCourseStatusReportResult.ReportsToUserName,
                aPIUserwiseCourseStatusReportResult.UserCategory,
                aPIUserwiseCourseStatusReportResult.IsActive,
                aPIUserwiseCourseStatusReportResult.SalesOfficeName,
                aPIUserwiseCourseStatusReportResult.SalesArea,
                aPIUserwiseCourseStatusReportResult.ControllingOffice,
                aPIUserwiseCourseStatusReportResult.State,
                aPIUserwiseCourseStatusReportResult.Inprogress.ToString(),
                aPIUserwiseCourseStatusReportResult.Completed.ToString(),
                aPIUserwiseCourseStatusReportResult.Applicable.ToString(),
                aPIUserwiseCourseStatusReportResult.LastLoggedInDate
            };
            return ExportData;
        }

        #endregion

        #region Tcns Retraining Report
        public async Task<FileInfo> GetTcnsRetrainingReport(APITcnsRetrainingReport tcnsRetrainingReport, string orgCode)
        {
            try
            {
                IEnumerable<ApiExportTcnsRetrainingReport> TcnsRetrainingReport = await this._coursereportRepository.GetTcnsRetrainingReport(tcnsRetrainingReport);

                FileInfo File = GetTcnsRetrainingReportExcel(TcnsRetrainingReport, tcnsRetrainingReport.ExportAs, orgCode);

                return File;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        private FileInfo GetTcnsRetrainingReportExcel(IEnumerable<ApiExportTcnsRetrainingReport> TcnsRetrainingReport, string ExportAs,string orgCode)
        {
            String ExcelName = FileName.TcnsRetrainingReport;
            String ExcelNameCSV = FileName.TcnsRetrainingReportCSV;
            int RowNumber = 0;
            Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

            List<string> UserWiseCourseCompletionHeaders = GetTcnsRetrainingReportHeaders(orgCode);
            ExcelData.Add(RowNumber, UserWiseCourseCompletionHeaders);

            foreach (ApiExportTcnsRetrainingReport TcnsRetrainingReportdata in TcnsRetrainingReport)
            {
                List<string> TcnsRetrainingReportRow = GetTcnsRetrainingReportRow(TcnsRetrainingReportdata,orgCode);
                RowNumber++;
                ExcelData.Add(RowNumber, TcnsRetrainingReportRow);
            }

            if (ExportAs == "csv")
            {


                foreach (ApiExportTcnsRetrainingReport item in TcnsRetrainingReport)
                {
                    item.UserId = Security.Decrypt(item.UserId);
                }



                DataTable ForToCsv = this._toDataTableConverter.ToDataTableExportTcnsRetrainingReport<ApiExportTcnsRetrainingReport>(TcnsRetrainingReport, orgCode);

                //file info here which takes the datatable and filename as input
                FileInfo csvFile = this._tlsHelper.ToCSV(ForToCsv, ExcelNameCSV);

                return csvFile;
            }
            else
            {
                FileInfo fileInfo = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
                return fileInfo;
            }
        }

        private List<string> GetTcnsRetrainingReportHeaders(string orgCode)
        {
            List<string> UserWiseCourseCompletionHeader = new List<string>();
            if (orgCode.ToLower().Contains("tcns"))
            {
                UserWiseCourseCompletionHeader = new List<string>
                {
                HeaderName.UserId,
                HeaderName.CourseCode,
                HeaderName.UserName,
                HeaderName.CourseTitle,
                HeaderName.CourseStartDate,
                HeaderName.CourseCompletionDate,
                HeaderName.CourseStatus,
                HeaderName.RetrainingDate,
                HeaderName.UserStatus,
                HeaderName.Department,
                HeaderName.Designation,
                HeaderName.FunctionCode,
                HeaderName.Group,
                HeaderName.Region,
                HeaderName.Score
                };
            }
            else
            {
                UserWiseCourseCompletionHeader = new List<string>
                {
                HeaderName.UserId,
                HeaderName.CourseCode,
                HeaderName.UserName,
                HeaderName.CourseTitle,
                HeaderName.CourseStartDate,
                HeaderName.CourseCompletionDate,
                HeaderName.CourseStatus,
                HeaderName.RetrainingDate,
                HeaderName.UserStatus,
                HeaderName.Score
               };
            }
            return UserWiseCourseCompletionHeader;
        }

        private List<string> GetTcnsRetrainingReportRow(ApiExportTcnsRetrainingReport TcnsRetrainingReportdata, string orgCode)
        {
            List<string> TcnsRetrainingReportdataRow = new List<string>();
            try
            {
                if (orgCode.ToLower().Contains("tcns"))
                {

                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.UserId);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.CourseCode);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.UserName);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.CourseTitle);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.CourseStartDate);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.CourseCompletionDate);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.CourseStatus);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.RetrainingDate);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.UserStatus);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.Department);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.Designation);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.FunctionCode);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.Group);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.Region);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.Score);
                }
                else
                {
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.UserId);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.CourseCode);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.UserName);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.CourseTitle);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.CourseStartDate);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.CourseCompletionDate);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.CourseStatus);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.RetrainingDate);
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.UserStatus);                   
                    TcnsRetrainingReportdataRow.Add(TcnsRetrainingReportdata.Score);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return TcnsRetrainingReportdataRow;
        }
        #endregion
    }
}
