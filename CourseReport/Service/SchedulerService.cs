using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using CourseReport.API.APIModel;
using CourseReport.API.Helper;
using CourseReport.API.Helper.MetaData;
using CourseReport.API.Repositories.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using log4net;
using System.Data;
using CourseReport.API.Helper.Interfaces;
namespace CourseReport.API.Service
{
    public class SchedulerService : ISchedulerService
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SchedulerService));
        private ISchedulerRepository _schedulerRepository;
        private IEnumerable<APIExportAllCoursesCompletionReport> apiExportAllCoursesCompletionReport;
        private readonly ITLSHelper _tlsHelper;
        private IConfiguration _configuration;
        private readonly IToDataTableConverter _toDataTableConverter;

        public SchedulerService(ISchedulerRepository schedulerRepository, ITLSHelper tlsHelper, IConfiguration configuration, IToDataTableConverter toDataTableConverter)
        {
            this._schedulerRepository = schedulerRepository;
            this._tlsHelper = tlsHelper;
            this._configuration = configuration;

            this._toDataTableConverter = toDataTableConverter;
        }

        public async Task<FileInfo> ExportAllCoursesCompletionReport(APISchedulerModule schedulermodule, string OrgCode)
        {
            try
            {
                IEnumerable<APISchedulerReport> AllCoursesCompletionList = await this._schedulerRepository.GetAllCoursesCompletionReport(schedulermodule);

                IEnumerable<APIExportAllCoursesCompletionReport> AllCoursesCompletionHeaders = await this._schedulerRepository.ExportAllCoursesCompletionReport();

                FileInfo File = GetAllCoursesCompletionExcel(AllCoursesCompletionList, AllCoursesCompletionHeaders, OrgCode, schedulermodule.ExportAs);
                return File;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));

                throw ex;
            }
        }

        private FileInfo GetAllCoursesCompletionExcel(IEnumerable<APISchedulerReport> AllCoursesCompletionList, IEnumerable<APIExportAllCoursesCompletionReport> AllCourseCompletionHeaderss, string OrgCode, string ExportAs)
        {
            if (OrgCode != null)
            {
                if (OrgCode.ToLower() == "hdfc")
                {
                    String ExcelName = FileName.AllCourseCompletionReport;
                    int RowNumber = 0;
                    Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

                    List<string> AllCourseCompletionHeaders = GetAllCourseCompletionHeaders(AllCourseCompletionHeaderss, OrgCode);
                    ExcelData.Add(RowNumber, AllCourseCompletionHeaders);

                    foreach (APISchedulerReport AllcourseCompletiondata in AllCoursesCompletionList)
                    {
                        List<string> AllCourseCompletionRow = GetAllCourseCompletionRow(AllcourseCompletiondata, AllCourseCompletionHeaderss, OrgCode, OrgCode);
                        RowNumber++;
                        ExcelData.Add(RowNumber, AllCourseCompletionRow);
                    }

                    FileInfo ExcelFile = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
                    return ExcelFile;
                }
                else if (OrgCode.ToLower() == "ghfl")
                {
                    String ExcelName = FileName.AllCourseCompletionReport;
                    int RowNumber = 0;
                    Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

                    List<string> AllCourseCompletionHeaders = GetAllCourseCompletionHeadersGhfl();
                    ExcelData.Add(RowNumber, AllCourseCompletionHeaders);

                    foreach (APISchedulerReport AllcourseCompletiondata in AllCoursesCompletionList)
                    {
                        List<string> AllCourseCompletionRow = GetAllCourseCompletionRowGhfl(AllcourseCompletiondata);
                        RowNumber++;
                        ExcelData.Add(RowNumber, AllCourseCompletionRow);
                    }

                    FileInfo ExcelFile = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
                    return ExcelFile;
                }
                else if (OrgCode.ToLower() == "cap")
                {
                    String ExcelName = FileName.AllCourseCompletionReport;
                    int RowNumber = 0;
                    Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

                    List<string> AllCourseCompletionHeaders = GetAllCourseCompletionHeadersElse(AllCourseCompletionHeaderss, OrgCode);
                    ExcelData.Add(RowNumber, AllCourseCompletionHeaders);

                    foreach (APISchedulerReport AllcourseCompletiondata in AllCoursesCompletionList)
                    {
                        List<string> AllCourseCompletionRow = GetAllCourseCompletionRow(AllcourseCompletiondata, AllCourseCompletionHeaderss, OrgCode);
                        RowNumber++;
                        ExcelData.Add(RowNumber, AllCourseCompletionRow);
                    }

                    FileInfo ExcelFile = this._tlsHelper.GenerateExcelFile(ExcelName, ExcelData);
                    return ExcelFile;
                }
                else
                {
                    String ExcelNameCSV = FileName.AllCourseCompletionReportCSV;
                    String ExcelName = FileName.AllCourseCompletionReport;
                    int RowNumber = 0;
                    Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

                    List<string> AllCourseCompletionHeaders = GetAllCourseCompletionHeadersElse(AllCourseCompletionHeaderss,OrgCode);
                    ExcelData.Add(RowNumber, AllCourseCompletionHeaders);

                    foreach (APISchedulerReport AllcourseCompletiondata in AllCoursesCompletionList)
                    {
                        List<string> AllCourseCompletionRow = GetAllCourseCompletionRow(AllcourseCompletiondata, AllCourseCompletionHeaderss, OrgCode);
                        RowNumber++;
                        ExcelData.Add(RowNumber, AllCourseCompletionRow);
                    }

                    if (ExportAs == "csv")
                    {


                        foreach (APISchedulerReport item in AllCoursesCompletionList)
                        {
                            item.UserId = Security.Decrypt(item.UserId);
                        }



                        DataTable ForToCsv = this._toDataTableConverter.ToDataTableSchedulerReport<APISchedulerReport>(AllCoursesCompletionList, OrgCode);

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
            }
            else
            {
                String ExcelName = FileName.AllCourseCompletionReport;
                String ExcelNameCSV = FileName.AllCourseCompletionReportCSV;
                int RowNumber = 0;
                Dictionary<int, List<string>> ExcelData = new Dictionary<int, List<string>>();

                List<string> AllCourseCompletionHeaders = GetAllCourseCompletionHeadersElse(AllCourseCompletionHeaderss, OrgCode);
                ExcelData.Add(RowNumber, AllCourseCompletionHeaders);

                foreach (APISchedulerReport AllcourseCompletiondata in AllCoursesCompletionList)
                {
                    List<string> AllCourseCompletionRow = GetAllCourseCompletionRow(AllcourseCompletiondata, AllCourseCompletionHeaderss, OrgCode);
                    RowNumber++;
                    ExcelData.Add(RowNumber, AllCourseCompletionRow);
                }


                if (ExportAs == "csv")
                {


                    foreach (APISchedulerReport item in AllCoursesCompletionList)
                    {
                        item.UserId = Security.Decrypt(item.UserId);
                    }



                    DataTable ForToCsv = this._toDataTableConverter.ToDataTableSchedulerReport<APISchedulerReport>(AllCoursesCompletionList, OrgCode);

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

        }

        //For HDFC Only 
        private List<string> GetAllCourseCompletionHeaders(IEnumerable<APIExportAllCoursesCompletionReport> AllCoursesCompletionHeaderss,string OrgCode )
        {
            if (OrgCode == "ent" || OrgCode =="cap")
            {
            List<string> CourseCompletionHeader = new List<string>
            {


                HeaderName.UserName,
                HeaderName.UserId,
                HeaderName.CourseTitle,
                HeaderName.CourseCode,
                HeaderName.CourseCategory,
                HeaderName.CourseSubCategory,
                HeaderName.CourseType,
                HeaderName.ModuleID,
                HeaderName.ModuleName,
                HeaderName.CourseStartDate,
                HeaderName.ContentCompletionDate,
                HeaderName.ScheduleCode,
                HeaderName.IsAssessmentAvailable,
                HeaderName.AssessmentStatus,
                HeaderName.AssessmentDate,
                HeaderName.AssessmentResult,
                HeaderName.AssessmentPercentage,
                HeaderName.IsFeedbackAvailable,
                HeaderName.FeedbackStatus,
                HeaderName.FeedbackDate,
                HeaderName.CourseCompletionDate,
                HeaderName.CourseStatus,
                //HeaderName.Department,
                HeaderName.StartTime,
                HeaderName.EndTime,
                HeaderName.PlaceName,
                HeaderName.TrainerName,
                HeaderName.CourseDuration,
                HeaderName.UserRole,
                HeaderName.NoOfAttempts,
                //HeaderName.UserDuration,
                HeaderName.Section


            };

                if (AllCoursesCompletionHeaderss != null)
                {
                    int i = 8;
                    foreach (APIExportAllCoursesCompletionReport opt in AllCoursesCompletionHeaderss)
                    {
                        string ChangedColumnName = opt.ChangedColumnName.ToString();
                        CourseCompletionHeader.Add(ChangedColumnName);
                        i++;
                    }
                }

                return CourseCompletionHeader;
            }
            else
            {
                List<string> CourseCompletionHeader = new List<string>
            {


                HeaderName.UserName,
                HeaderName.UserId,
                HeaderName.CourseTitle,
                HeaderName.CourseCode,
                HeaderName.CourseCategory,
                HeaderName.CourseType,
                HeaderName.ModuleID,
                HeaderName.ModuleName,
                HeaderName.CourseStartDate,
                HeaderName.ContentCompletionDate,
                HeaderName.ScheduleCode,
                HeaderName.IsAssessmentAvailable,
                HeaderName.AssessmentStatus,
                HeaderName.AssessmentDate,
                HeaderName.AssessmentResult,
                HeaderName.AssessmentPercentage,
                HeaderName.IsFeedbackAvailable,
                HeaderName.FeedbackStatus,
                HeaderName.FeedbackDate,
                HeaderName.CourseCompletionDate,
                HeaderName.CourseStatus,
                //HeaderName.Department,
                HeaderName.StartTime,
                HeaderName.EndTime,
                HeaderName.PlaceName,
                HeaderName.TrainerName,
                HeaderName.CourseDuration,
                HeaderName.UserRole,
                HeaderName.NoOfAttempts,
                //HeaderName.UserDuration,
                HeaderName.Section


            };

                if (AllCoursesCompletionHeaderss != null)
                {
                    int i = 8;
                    foreach (APIExportAllCoursesCompletionReport opt in AllCoursesCompletionHeaderss)
                    {
                        string ChangedColumnName = opt.ChangedColumnName.ToString();
                        CourseCompletionHeader.Add(ChangedColumnName);
                        i++;
                    }
                }

                return CourseCompletionHeader;
            }

        }
        private List<string> GetAllCourseCompletionRow(APISchedulerReport AllcourseCompletiondata, IEnumerable<APIExportAllCoursesCompletionReport> AllCourseCompletionHeaderss, string OrgCode,string o)
        {
            List<string> AllCourseCompletionRow = new List<string>();
            try
            {
                //DateTime DateValue = new DateTime();
                //if (OrgCode.ToLower() != "canh" && OrgCode.ToLower() != "canhuat")
                //{
                //    if (courseCompletiondata.CourseStatus.ToLower().Equals(CourseStatus.completed))
                //    {
                //        if (courseCompletiondata.CourseCompletionDate != "NA")
                //        {
                //            DateValue = Convert.ToDateTime(courseCompletiondata.CourseCompletionDate);
                //            courseCompletiondata.CourseCompletionDate = DateValue.ToString("MMM dd, yyyy");
                //        }
                //    }

                //    if (courseCompletiondata.CourseStartDate != "NA")
                //    {
                //        DateValue = Convert.ToDateTime(courseCompletiondata.CourseStartDate);
                //        courseCompletiondata.CourseStartDate = DateValue.ToString("MMM dd, yyyy");
                //    }
                //}


                AllCourseCompletionRow.Add(AllcourseCompletiondata.UserName);
                AllCourseCompletionRow.Add(Security.Decrypt(AllcourseCompletiondata.UserId));
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseTitle);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseCode);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseCategory);
                if (OrgCode == "ent" || OrgCode == "cap")
                {
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseSubCategory);
                }
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseType);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.ModuleID);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.ModuleName);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseStartDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.ContentCompletionDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.ScheduleCode);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.IsAssessmentAvailable);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.AssessmentStatus);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.AssessmentDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.AssessmentResult);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.AssessmentPercentage);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.IsFeedbackAvailable);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.FeedbackStatus);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.FeedbackDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseCompletionDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseStatus);
                //AllCourseCompletionRow.Add(AllcourseCompletiondata.Department);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.StartTime);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.EndTime);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.PlaceName);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.TrainerName);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseDuration);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.UserRole);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.NoOfAttempts);
               // AllCourseCompletionRow.Add(AllcourseCompletiondata.UserDuration);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.Section);

                if (AllCourseCompletionHeaderss != null)
                {
                    int i = 8;
                    foreach (APIExportAllCoursesCompletionReport opt in AllCourseCompletionHeaderss)
                    {
                        string ConfiguredColumnName = opt.ConfiguredColumnName.ToString();

                        System.Reflection.PropertyInfo pi = AllcourseCompletiondata.GetType().GetProperty(ConfiguredColumnName);
                        String name = (String)(pi.GetValue(AllcourseCompletiondata, null));

                        AllCourseCompletionRow.Add(name);
                        i++;
                    }
                }


                //int i = 7;
                //foreach (APICourseExportReport opt in CourseCompletionHeaderss)
                //{
                //    string ConfiguredColumnName = opt.ConfiguredColumnName.ToString();

                //    System.Reflection.PropertyInfo pi = courseCompletiondata.GetType().GetProperty(ConfiguredColumnName);
                //    String nameid = (String)(pi.GetValue(courseCompletiondata, null));

                //    string ConfigValue = "";
                //    if (!string.IsNullOrEmpty(nameid))
                //    {
                //        //var t = GenerateCodeService.GenerateCodeAsync();
                //        //Task.WhenAll(t);
                //        //string code = t.Result;

                //        //GetDataForUserConfiguration().ContinueWith(t => Console.WriteLine(t.Exception),TaskContinuationOptions.OnlyOnFaulted);
                //ConfigValue = GetDataForUserConfiguration(ConfiguredColumnName, nameid, OrgCode);
                //    }
                //    CourseCompletionRow.Add(ConfigValue);
                //    i++;
                //}
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return AllCourseCompletionRow;
        }


        //For Godrej Only 
        private List<string> GetAllCourseCompletionHeadersGhfl()
        {
            List<string> CourseCompletionHeader = new List<string>
            {
                HeaderName.EmployeeCode,
                HeaderName.EmployeeName,
                HeaderName.EmailId,
                HeaderName.Function,
                HeaderName.Region,
                HeaderName.Grade,
                HeaderName.Level,
                HeaderName.JobTitle,
                HeaderName.ReportingManager,
                HeaderName.CourseTitle,
                HeaderName.CompletionDate,
                HeaderName.CourseDurationInHrs,
                HeaderName.CompletionStatus
            };
            return CourseCompletionHeader;
        }

        private List<string> GetAllCourseCompletionRowGhfl(APISchedulerReport AllcourseCompletiondata)
        {
            List<string> AllCourseCompletionRow = new List<string>();
            try
            {
                AllCourseCompletionRow.Add(Security.Decrypt(AllcourseCompletiondata.UserId));
                AllCourseCompletionRow.Add(AllcourseCompletiondata.UserName);
                AllCourseCompletionRow.Add(Security.Decrypt(AllcourseCompletiondata.EmailId));
                AllCourseCompletionRow.Add(AllcourseCompletiondata.Function);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.Region);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.Grade);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.Level);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.JobTitle);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.ReportingManager);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseTitle);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseCompletionDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseDuration);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseStatus);
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return AllCourseCompletionRow;
        }


        private List<string> GetAllCourseCompletionHeadersElse(IEnumerable<APIExportAllCoursesCompletionReport> AllCoursesCompletionHeaderss, string OrgCode)
        {
            List<string> CourseCompletionHeader = new List<string>
            {
                HeaderName.UserName,
                HeaderName.UserId,
                HeaderName.EmailId,
                HeaderName.CourseTitle,
                HeaderName.CourseCode,
                HeaderName.CourseCategory,
                HeaderName.CourseSubCategory,
                HeaderName.CourseSubSubCategory,
                HeaderName.CourseType,
                //HeaderName.ModuleID,
                HeaderName.ModuleName,
                HeaderName.CourseAssignedDate,
                HeaderName.CourseStartDate,
                HeaderName.ContentCompletionDate,
                HeaderName.ScheduleCode,
                HeaderName.IsAssessmentAvailable,
                HeaderName.AssessmentStatus,
                HeaderName.AssessmentDate,
                HeaderName.AssessmentResult,
                HeaderName.AssessmentPercentage,
                HeaderName.IsFeedbackAvailable,
                HeaderName.FeedbackStatus,
                HeaderName.FeedbackDate,
                HeaderName.CourseCompletionDate,
                HeaderName.CourseStatus,
                //HeaderName.Department,
                HeaderName.StartTime,
                HeaderName.EndTime,
                HeaderName.PlaceName,
                HeaderName.TrainerName,
               // HeaderName.CourseDuration,
                HeaderName.UserRole,
                HeaderName.NoOfAttempts,
                //HeaderName.UserDuration,
                HeaderName.Section,
                HeaderName.WebTimeSpent,
                HeaderName.AppTimeSpent,
                HeaderName.DeviceMostAccessedOn


            };

            if (OrgCode == "map" || OrgCode == "digimap" || OrgCode == "mba" || OrgCode == "inditex" || OrgCode == "starbucks" || OrgCode == "active" || OrgCode == "pli" || OrgCode == "foodhall" || OrgCode == "dominos" || OrgCode == "samsonite" || OrgCode == "fashion")
            {
                CourseCompletionHeader.Remove(HeaderName.CourseStatus);
                CourseCompletionHeader.Add(HeaderName.Percentage);
            }

            //if (OrgCode != "cap" && OrgCode != "ent")
            //{

            //    CourseCompletionHeader.Remove(HeaderName.CourseSubCategory);
            //}

            if(OrgCode == "ail")
            {
                CourseCompletionHeader.Add(HeaderName.ManHours);
            }
            else
            {
                CourseCompletionHeader.Add(HeaderName.CourseDuration);
            }
            CourseCompletionHeader.Add(HeaderName.ReportsToNew);
            CourseCompletionHeader.Add(HeaderName.DateOfJoining1);
            CourseCompletionHeader.Add(HeaderName.AdaptiveLearning);

            if (AllCoursesCompletionHeaderss != null)
            {
                int i = 8;
                foreach (APIExportAllCoursesCompletionReport opt in AllCoursesCompletionHeaderss)
                {
                    string ChangedColumnName = opt.ChangedColumnName.ToString();
                    CourseCompletionHeader.Add(ChangedColumnName);
                    i++;
                }
            }

            return CourseCompletionHeader;
        }
        private List<string> GetAllCourseCompletionRow(APISchedulerReport AllcourseCompletiondata, IEnumerable<APIExportAllCoursesCompletionReport> AllCourseCompletionHeaderss, string OrgCode)
        {
            List<string> AllCourseCompletionRow = new List<string>();
            try
            {
                //DateTime DateValue = new DateTime();
                //if (OrgCode.ToLower() != "canh" && OrgCode.ToLower() != "canhuat")
                //{
                //    if (courseCompletiondata.CourseStatus.ToLower().Equals(CourseStatus.completed))
                //    {
                //        if (courseCompletiondata.CourseCompletionDate != "NA")
                //        {
                //            DateValue = Convert.ToDateTime(courseCompletiondata.CourseCompletionDate);
                //            courseCompletiondata.CourseCompletionDate = DateValue.ToString("MMM dd, yyyy");
                //        }
                //    }

                //    if (courseCompletiondata.CourseStartDate != "NA")
                //    {
                //        DateValue = Convert.ToDateTime(courseCompletiondata.CourseStartDate);
                //        courseCompletiondata.CourseStartDate = DateValue.ToString("MMM dd, yyyy");
                //    }
                //}


                AllCourseCompletionRow.Add(AllcourseCompletiondata.UserName);
                AllCourseCompletionRow.Add(Security.Decrypt(AllcourseCompletiondata.UserId));
                AllCourseCompletionRow.Add(Security.Decrypt(AllcourseCompletiondata.EmailId));
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseTitle);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseCode);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseCategory);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseSubCategory);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseSubSubCategory);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseType);
                //AllCourseCompletionRow.Add(AllcourseCompletiondata.ModuleID);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.ModuleName);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseAssignedDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseStartDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.ContentCompletionDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.ScheduleCode);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.IsAssessmentAvailable);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.AssessmentStatus);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.AssessmentDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.AssessmentResult);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.AssessmentPercentage);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.IsFeedbackAvailable);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.FeedbackStatus);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.FeedbackDate);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseCompletionDate);
                if (OrgCode == "map" || OrgCode == "digimap" || OrgCode == "mba" || OrgCode == "inditex" || OrgCode == "starbucks" || OrgCode == "active" || OrgCode == "pli" || OrgCode == "foodhall" || OrgCode == "dominos" || OrgCode == "samsonite" || OrgCode == "fashion")
                { }
                else
                {
                    AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseStatus);
                }
                //AllCourseCompletionRow.Add(AllcourseCompletiondata.Department);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.StartTime);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.EndTime);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.PlaceName);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.TrainerName);
               
                
                AllCourseCompletionRow.Add(AllcourseCompletiondata.UserRole);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.NoOfAttempts);
                //AllCourseCompletionRow.Add(AllcourseCompletiondata.UserDuration);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.Section);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.WebTimeSpentInMinutes);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.AppTimeSpentInMinutes);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.DeviceMostActive);
                if (OrgCode == "ail")
                {
                    double courseDuration = Convert.ToDouble(AllcourseCompletiondata.CourseDuration) / 60;
                    courseDuration = Math.Round(courseDuration, 2);
                    AllcourseCompletiondata.CourseDuration = courseDuration.ToString();
                    AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseDuration);
                }
                else
                {
                    AllCourseCompletionRow.Add(AllcourseCompletiondata.CourseDuration);
                }


                if (OrgCode == "map" || OrgCode == "digimap" || OrgCode == "mba" || OrgCode == "inditex" || OrgCode == "starbucks" || OrgCode == "active" || OrgCode == "pli" || OrgCode == "foodhall" || OrgCode == "dominos" || OrgCode == "samsonite" || OrgCode == "fashion")
                {
                    if (AllcourseCompletiondata.CourseStatus == "completed")
                    {
                        AllCourseCompletionRow.Add(AllcourseCompletiondata.Percentage = "100%");
                    }
                    else if (AllcourseCompletiondata.CourseStatus == "inprogress")
                    {
                        AllCourseCompletionRow.Add(AllcourseCompletiondata.Percentage = "50%");
                    }
                    else
                    {
                        AllCourseCompletionRow.Add(AllcourseCompletiondata.Percentage = "0%");
                    }
                }
                AllCourseCompletionRow.Add(Security.Decrypt(AllcourseCompletiondata.ReportsTo));
                AllCourseCompletionRow.Add(AllcourseCompletiondata.DateOfJoining);
                AllCourseCompletionRow.Add(AllcourseCompletiondata.IsAdaptiveAssessment);

                if (AllCourseCompletionHeaderss != null)
                {
                    int i = 8;
                    foreach (APIExportAllCoursesCompletionReport opt in AllCourseCompletionHeaderss)
                    {
                        string ConfiguredColumnName = opt.ConfiguredColumnName.ToString();

                        System.Reflection.PropertyInfo pi = AllcourseCompletiondata.GetType().GetProperty(ConfiguredColumnName);
                        String name = (String)(pi.GetValue(AllcourseCompletiondata, null));

                        AllCourseCompletionRow.Add(name);
                        i++;
                    }
                }


                //int i = 7;
                //foreach (APICourseExportReport opt in CourseCompletionHeaderss)
                //{
                //    string ConfiguredColumnName = opt.ConfiguredColumnName.ToString();

                //    System.Reflection.PropertyInfo pi = courseCompletiondata.GetType().GetProperty(ConfiguredColumnName);
                //    String nameid = (String)(pi.GetValue(courseCompletiondata, null));

                //    string ConfigValue = "";
                //    if (!string.IsNullOrEmpty(nameid))
                //    {
                //        //var t = GenerateCodeService.GenerateCodeAsync();
                //        //Task.WhenAll(t);
                //        //string code = t.Result;

                //        //GetDataForUserConfiguration().ContinueWith(t => Console.WriteLine(t.Exception),TaskContinuationOptions.OnlyOnFaulted);
                //ConfigValue = GetDataForUserConfiguration(ConfiguredColumnName, nameid, OrgCode);
                //    }
                //    CourseCompletionRow.Add(ConfigValue);
                //    i++;
                //}
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
            }
            return AllCourseCompletionRow;
        }


    }
}
