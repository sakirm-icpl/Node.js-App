using MyCourse.API.APIModel;
using MyCourse.API.Helper;
using MyCourse.API.Model;
using MyCourse.API.Repositories.Interfaces;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using MyCourse.API.Repositories;

namespace MyCourse.API.Common
{
    public class AssignmentImport
    {

        public static class ProcessFile
        {
            private static readonly ILog _logger = LogManager.GetLogger(typeof(ProcessFile));
            static StringBuilder sb = new StringBuilder();
            static string[] header = { };
            static string[] headerStar = { };
            static string[] headerWithoutStar = { };
            static List<string> assignmentRecords = new List<string>();
            static AssignmentDetails assignmentDetails = new AssignmentDetails();
            static MyCourse.API.Model.Course courseInfo = new MyCourse.API.Model.Course();
            static APIModel.ApiAssignmentDetails apiAssignmentDetails = new APIModel.ApiAssignmentDetails();
            static AssignmentDetailsRejected assignmentDetailsRejected = new AssignmentDetailsRejected();
            static StringBuilder sbError = new StringBuilder();
            static int totalRecordInsert = 0;
            static int totalRecordRejected = 0;

            public static void Reset()
            {
                sb.Clear();
                header = new string[0];
                headerStar = new string[0];
                headerWithoutStar = new string[0];
                assignmentRecords.Clear();
                assignmentDetails = new AssignmentDetails();
                apiAssignmentDetails = new APIModel.ApiAssignmentDetails();
                assignmentDetailsRejected = new AssignmentDetailsRejected();
                courseInfo = new MyCourse.API.Model.Course();
                sbError.Clear();
                totalRecordInsert = 0;
                totalRecordRejected = 0;
            }

            public static async Task<int> InitilizeAsync(FileInfo file)
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
                                append = Convert.ToString(worksheet.Cells[row, col].Value.ToString().Trim());
                            }
                            string finalAppend = append + "\t";
                            sb.Append(finalAppend);

                        }
                        sb.Append(Environment.NewLine);
                    }

                    string fileInfo = sb.ToString();
                    assignmentRecords = new List<string>(fileInfo.Split('\n'));

                    if (rowCount != assignmentRecords.Count - 1)
                    {
                        sbError.Append("Cannot contain new line characters");
                        {
                            return 2;
                        }
                    }
                    else
                    {
                        foreach (string record in assignmentRecords)
                        {
                            string[] mainsp = record.Split('\r');
                            string[] mainsp2 = mainsp[0].Split('\"');
                            header = mainsp2[0].Split('\t');
                            headerStar = mainsp2[0].Split('\t');
                            break;
                        }
                        assignmentRecords.RemoveAt(0);

                    }
                }
                /////Remove Star from Header
                for (int i = 0; i < header.Count(); i++)
                {
                    header[i] = header[i].Replace("*", "");

                }
                // invalid file
                int count = 0;
                for (int i = 0; i < header.Count() - 1; i++)
                {
                    string headerColumn = header[i].ToString().Trim();
                    if (!string.IsNullOrEmpty(headerColumn))
                    {
                        count++;
                    }
                }
                if (count == 4)
                {
                    return 1;

                }
                else
                {
                    return 0;
                }
            }

            public static async Task<string> ProcessRecordsAsync(
                IAssignmentDetailsRepository _assignmentRepository,
                ICourseRepository _courseRepository,
                int userid, string OrganisationCode)
            {
                List<MyCourse.API.Model.Course> CoursesList = new List<MyCourse.API.Model.Course>();
                if (assignmentRecords != null && assignmentRecords.Count > 0)
                {
                    foreach (string record in assignmentRecords)
                    {
                        bool validvalue = false;
                        int countLenght = record.Length;
                        if (record != null && countLenght > 1)
                        {
                            string[] textpart = record.Split('\t');
                            string[][] mainRecord = { header, textpart };
                            string txtCourseCode = string.Empty;
                            string txtUserId = string.Empty;
                            string txtStatus = string.Empty;
                            string txtRemarks = string.Empty;

                            string headerText = "";

                            int arrasize = header.Count();

                            for (int j = 0; j < arrasize - 1; j++)
                            {
                                headerText = header[j];
                                string[] mainspilt = headerText.Split('\t');

                                headerText = mainspilt[0];

                                if (headerText == "CourseCode")
                                {
                                    //CourseCode
                                    try
                                    {
                                        string CourseCode = mainRecord[1][j];
                                        string[] textCourseCodeSplit = mainRecord[1][j].Split('\t');
                                        txtCourseCode = textCourseCodeSplit[0];
                                        txtCourseCode = txtCourseCode.Trim();

                                        bool valid = ValidateCourseCode(headerText, txtCourseCode);
                                        if (valid == true)
                                        {
                                            validvalue = true;
                                            break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                                else
                                if (headerText == "UserId")
                                {
                                    //UserId
                                    try
                                    {
                                        string userId = mainRecord[1][j];
                                        string[] textUserIdSplit = mainRecord[1][j].Split('\t');
                                        string textUserId = textUserIdSplit[0];
                                        txtUserId = textUserId.Trim();
                                        bool valid = ValidateUserId(headerText, txtUserId);
                                        if (valid == true)
                                        {
                                            validvalue = true;
                                            break;
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                                else
                                if (headerText == "Status")
                                {
                                    //Status
                                    try
                                    {
                                        string Status = mainRecord[1][j];
                                        string[] textStatusSplit = mainRecord[1][j].Split('\t');
                                        string textStatus = textStatusSplit[0];
                                        txtStatus = textStatus.Trim();

                                        bool valid = ValidateStatus(headerText, txtStatus);
                                        if (valid == true)
                                        {
                                            validvalue = true;
                                            break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                                else
                                if (headerText == "Remark")
                                {
                                    //Remark
                                    try
                                    {

                                        string remarks = mainRecord[1][j];
                                        string[] textRemarksSplit = mainRecord[1][j].Split('\t');
                                        string textRemarkText = textRemarksSplit[0];
                                        txtRemarks = textRemarkText.Trim();

                                        bool valid = ValidateRemark(headerText, txtRemarks);
                                        if (valid == true)
                                        {
                                            validvalue = true;
                                            break;
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        _logger.Error(Utilities.GetDetailedException(ex));
                                    }
                                }
                            }

                            {
                                for (int j = 0; j < 1; j++)
                                {
                                    if (sbError.ToString() == "" || string.IsNullOrEmpty(sbError.ToString()))
                                    {
                                        if (CoursesList != null) // Check Course List first
                                        {
                                            foreach (var courses in CoursesList)
                                            {
                                                if (courses.Code == txtCourseCode)
                                                {
                                                    courseInfo = courses;
                                                    break;
                                                }
                                            }
                                        }

                                        if (courseInfo.Id == 0) // if Course Not Found in List get from DB
                                        {
                                            //Get CourseInfo by Course Code 
                                            courseInfo = await _courseRepository.GetCourseInfoByCourseCode(txtCourseCode);
                                            if (courseInfo != null)
                                                CoursesList.Add(courseInfo);
                                            else
                                            {
                                                validvalue = true;
                                                sbError.Append("Course not Found.Please enter valid data.");
                                            }

                                        }

                                        if (courseInfo != null)
                                        {
                                            if (courseInfo.Id == 0)
                                            {

                                            }
                                            else
                                            {
                                                if (courseInfo.AssignmentId.HasValue) //Check Course Has Assignment 
                                                    apiAssignmentDetails.AssignmentId = courseInfo.AssignmentId.Value;
                                                else
                                                {
                                                    validvalue = true;
                                                    sbError.Append("Course do not have any Assignment.");
                                                    //throw Error assignment 
                                                }
                                            }
                                        }


                                        //Get User Info by UserId
                                        GetUserInfo getUserInfo = await _assignmentRepository.GetUserMasterInfo(txtUserId);

                                        if (getUserInfo.Id == 0)
                                        {
                                            validvalue = true;
                                            sbError.Append("UserId not found. Please enter valid data.");
                                        }

                                        if (validvalue == false)
                                        {
                                            apiAssignmentDetails.CourseId = courseInfo.Id;
                                            apiAssignmentDetails.UserId = getUserInfo.Id;//usermaster id 
                                            apiAssignmentDetails.Status = txtStatus;
                                            apiAssignmentDetails.Remark = txtRemarks;

                                            //update assignment
                                            var result = await _assignmentRepository.UpdateAdssignmentDetail(apiAssignmentDetails, OrganisationCode);

                                            if (result != "true")
                                            {
                                                validvalue = true;
                                                sbError.Append(result);
                                            }
                                            else
                                            {
                                                totalRecordInsert++;
                                                assignmentDetailsRejected = new AssignmentDetailsRejected();
                                                assignmentDetails = new AssignmentDetails();
                                                courseInfo = new MyCourse.API.Model.Course();
                                                sbError.Clear();
                                            }
                                        }

                                    }
                                }
                            }

                            if (validvalue == true)
                            {
                                assignmentDetailsRejected.ErrorMessage = sbError.ToString();
                                assignmentDetailsRejected.CreatedDate = DateTime.UtcNow;
                                assignmentDetailsRejected.CreatedBy = userid;
                                assignmentDetailsRejected.ModifiedDate = DateTime.UtcNow;
                                assignmentDetailsRejected.ModifiedBy = userid;
                                assignmentDetailsRejected.CourseCode = txtCourseCode;
                                assignmentDetailsRejected.UserId = txtUserId;
                                await _assignmentRepository.AddAssignmentDetailsRejected(assignmentDetailsRejected);
                                totalRecordRejected++;
                                assignmentDetailsRejected = new AssignmentDetailsRejected();
                                assignmentDetails = new AssignmentDetails();
                                courseInfo = new MyCourse.API.Model.Course();
                                sbError.Clear();

                            }
                            else
                            {
                                assignmentDetailsRejected.ErrorMessage = sbError.ToString();

                                if (sbError.ToString() != "")
                                {
                                    assignmentDetailsRejected.CreatedDate = DateTime.UtcNow;
                                    assignmentDetailsRejected.CreatedBy = userid;
                                    assignmentDetailsRejected.ModifiedDate = DateTime.UtcNow;
                                    assignmentDetailsRejected.ModifiedBy = userid;
                                    assignmentDetailsRejected.ErrorMessage = sbError.ToString();
                                    assignmentDetailsRejected.CourseCode = txtCourseCode;
                                    assignmentDetailsRejected.UserId = txtUserId;
                                    await _assignmentRepository.AddAssignmentDetailsRejected(assignmentDetailsRejected);
                                    totalRecordRejected++;
                                    assignmentDetailsRejected = new AssignmentDetailsRejected();
                                    assignmentDetails = new AssignmentDetails();
                                    sbError.Clear();
                                }

                            }
                        }
                        else
                        {
                        }

                    }
                }
                return "Total number of record updated :" + totalRecordInsert + ",  Total number of record record rejected : " + totalRecordRejected;

            }

            public static bool ValidateCourseCode(string headerText, string courseCode)
            {
                bool valid = false;
                //CourseName
                try
                {
                    if (courseCode != null && !string.IsNullOrEmpty(courseCode))
                    {
                        if (courseCode.Trim().Length >= 3)
                        {
                            assignmentDetailsRejected.CourseCode = courseCode;
                        }
                        else
                        {
                            assignmentDetailsRejected.CourseCode = courseCode;
                            sbError.Append("Please enter valid CourseCode");
                            valid = true;
                        }
                    }
                    else
                    {
                        assignmentDetailsRejected.CourseCode = courseCode;
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

            public static bool ValidateUserId(string headerText, string userId)
            {
                bool valid = false;
                //UserId
                try
                {
                    if (userId != null && !string.IsNullOrEmpty(userId))
                    {
                        if (userId.Trim().Length >= 2)
                        {
                            assignmentDetailsRejected.UserId = userId;
                        }
                        else
                        {
                            assignmentDetailsRejected.UserId = userId;
                            sbError.Append("Please enter valid UserId");
                            valid = true;
                        }
                    }
                    else
                    {
                        assignmentDetailsRejected.UserId = userId;
                        sbError.Append("UserId is null");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateStatus(string headerText, string status)
            {
                bool valid = false;

                //AssignmentName
                try
                {
                    if (status != null && !string.IsNullOrEmpty(status))
                    {
                        if ((status) == Record.Approved || (status) == Record.Rejected)
                        {
                            assignmentDetailsRejected.Status = status;
                        }
                        else
                        {
                            assignmentDetailsRejected.Status = status;
                            sbError.Append("Please enter valid Status");
                            valid = true;
                        }
                    }
                    else
                    {
                        assignmentDetailsRejected.Status = status;
                        sbError.Append("Status is null");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }

            public static bool ValidateRemark(string headerText, string remark)
            {
                bool valid = false;
                //AssignmentName
                try
                {
                    if (remark != null && !string.IsNullOrWhiteSpace(remark))
                    {
                        if (remark.Trim().Length >= 3)
                        {
                            assignmentDetailsRejected.Remark = remark;
                        }
                        else
                        {
                            assignmentDetailsRejected.Remark = remark;
                            sbError.Append("Please enter valid Remark");
                            valid = true;
                        }
                    }
                    else
                    {
                        assignmentDetailsRejected.Remark = remark;
                        sbError.Append("Remark is null");
                        valid = true;
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }
                return valid;
            }
        }
    }
}
