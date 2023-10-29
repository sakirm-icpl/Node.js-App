using Gadget.API.Data;
using Gadget.API.Repositories.Interfaces;
using log4net;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gadget.API.APIModel;
using Gadget.API.Models;
using System.Data;
using Gadget.API.Helper;
using System;
using OfficeOpenXml;
using System.IO;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;

namespace Gadget.API.Repositories
{
    public class DigitalAdoptionReviewListRepository : Repository<DigitalAdoptionReview>, IDigitalAdoptionReviewList
    {
        StringBuilder sb = new StringBuilder();
        string[] header = { };
        string[] headerStar = { };
        string[] headerWithoutStar = { };
        List<string> ReviewList = new List<string>();
        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;
        private GadgetDbContext _db;
        private readonly IConfiguration _configuration;
        DigitalAdoptionReviewData competencyCategoryImport = new DigitalAdoptionReviewData();
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DigitalAdoptionReviewListRepository));
        public DigitalAdoptionReviewListRepository(GadgetDbContext context, IConfiguration configuration) : base(context)
        {
            _db = context;
            _configuration = configuration;
        }

        public async Task<APIDigitalAdoptionReviewsListandCount> GetDigitalAdoptionReview(int page, int pageSize, string search)
        {
            var Query = (from x in _db.DigitalAdoptionReview
                         join um in _db.UserMaster on x.EmployeeId equals um.Id
                         join um1 in _db.UserMaster on x.ReviewerId equals um1.Id
                         join um2 in _db.UseCase on x.DescriptionId equals um2.Id
                         join um3 in _db.DigitalRole on x.RoleId equals um3.Id
                         orderby x.CreatedDate descending
                         select new APIDigitalAdoptionReviewGet
                         {
                             Id = x.Id,
                             EmployeeName = um.UserName,
                             ReviewerName = um1.UserName,
                             UseCaseDescription = um2.Description,
                             CreatedDate = x.CreatedDate,
                             InvolvementLevel = x.InvolvementLevel,
                             DigitalAwareness = x.DigitalAwareness,
                             UseCaseKnowledge = x.UseCaseKnowledge,
                             UserRoleDescription = um3.Description,
                             Remarks = x.Remarks,
                             EmployeeId = x.EmployeeId,
                             ReviewerId = x.ReviewerId,
                             RoleId = x.RoleId,
                             DescriptionId = x.DescriptionId,
                             code = x.code
                         }).AsNoTracking();

            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => (r.UseCaseDescription.Contains(search) || r.EmployeeName.Contains(search) || r.ReviewerName.Contains(search)));
            }
            APIDigitalAdoptionReviewsListandCount ListandCount = new APIDigitalAdoptionReviewsListandCount();
            ListandCount.Count = Query.Distinct().Count();
            ListandCount.DigitalAdoptionReviewListandCount = await Query.Skip((page - 1) * pageSize).Take(pageSize).Distinct().ToListAsync();
            return ListandCount;
        }

        public async Task<APIDigitalAdoptionReviewDashBoard> DigitalAdoptionReviewDashboard(APIFilter filterData)
        {
            var QueryGroupBy = _db.DigitalAdoptionReview.Where(a=>a.IsDeleted == false).GroupBy(a => new { a.EmployeeId }).Select(c => new EmployeeGroupDigitalAdoptionReview { EmployeeId = c.Key.EmployeeId, Total = c.Count() });
            List<EmployeeGroupDigitalAdoptionReview> DigitalAdoptionGroupBy = QueryGroupBy.ToList();

            foreach (EmployeeGroupDigitalAdoptionReview item in DigitalAdoptionGroupBy.ToList())
            {
                UserMaster userMater = _db.UserMaster.Where(a=>a.Id == item.EmployeeId && a.IsDeleted == false && a.IsActive == true).FirstOrDefault();
                if(userMater == null)
                {
                    DigitalAdoptionGroupBy.Remove(item);
                }
            }

            if (filterData.UseCase != null)
            {
                List<DigitalAdoptionReview> digitalAdoptionReview = _db.DigitalAdoptionReview.Where(a => a.DescriptionId == filterData.UseCase).ToList();
                DigitalAdoptionGroupBy = digitalAdoptionReview.GroupBy(a => new { a.EmployeeId }).Select(c => new EmployeeGroupDigitalAdoptionReview { EmployeeId = c.Key.EmployeeId, Total = c.Count() }).ToList();
            }

            if (filterData.Month != null)
            {
                List<DigitalAdoptionReview> digitalAdoptionReview = _db.DigitalAdoptionReview.Where(a => a.CreatedDate.Month == filterData.Month).ToList();
                if (filterData.UseCase != null)
                {
                    digitalAdoptionReview = digitalAdoptionReview.Where(a => a.DescriptionId == filterData.Month).ToList();
                }
                DigitalAdoptionGroupBy = digitalAdoptionReview.GroupBy(a => new { a.EmployeeId }).Select(c => new EmployeeGroupDigitalAdoptionReview { EmployeeId = c.Key.EmployeeId, Total = c.Count() }).ToList();
            }

            if (filterData.Year != null)
            {
                List<DigitalAdoptionReview> digitalAdoptionReview = _db.DigitalAdoptionReview.Where(a => a.CreatedDate.Year == filterData.Year).ToList();
                if (filterData.UseCase != null)
                {
                    digitalAdoptionReview = digitalAdoptionReview.Where(a => a.DescriptionId == filterData.UseCase).ToList();
                }
                if (filterData.Month != null)
                {
                    digitalAdoptionReview = digitalAdoptionReview.Where(a => a.CreatedDate.Month == filterData.Month).ToList();
                }
                DigitalAdoptionGroupBy = digitalAdoptionReview.GroupBy(a => new { a.EmployeeId }).Select(c => new EmployeeGroupDigitalAdoptionReview { EmployeeId = c.Key.EmployeeId, Total = c.Count() }).ToList();
            }

            if (filterData.Role.Length != 0)
            {
                List<DigitalAdoptionReview> digitalAdoptionReview = new List<DigitalAdoptionReview>();
                for (int i = 0; i < filterData.Role.Length; i++)
                {
                    List<DigitalAdoptionReview> reviewByRoleId = _db.DigitalAdoptionReview.Where(a => a.RoleId == filterData.Role[i]).ToList();
                    digitalAdoptionReview.AddRange(reviewByRoleId);
                }
                if (filterData.UseCase != null)
                {
                    digitalAdoptionReview = digitalAdoptionReview.Where(a => a.DescriptionId == filterData.UseCase).ToList();
                }
                if (filterData.Month != null)
                {
                    digitalAdoptionReview = digitalAdoptionReview.Where(a => a.CreatedDate.Month == filterData.Month).ToList();
                }
                if (filterData.Year != null)
                {
                    digitalAdoptionReview = digitalAdoptionReview.Where(a => a.CreatedDate.Year == filterData.Year).ToList();
                }
                DigitalAdoptionGroupBy = digitalAdoptionReview.GroupBy(a => new { a.EmployeeId }).Select(c => new EmployeeGroupDigitalAdoptionReview { EmployeeId = c.Key.EmployeeId, Total = c.Count() }).ToList();
            }

            if (filterData.Cluster != null)
            {

                List<EmployeeGroupDigitalAdoptionReview> businessFilter = new List<EmployeeGroupDigitalAdoptionReview>();
                foreach (EmployeeGroupDigitalAdoptionReview item in DigitalAdoptionGroupBy)
                {
                    UserMasterDetails userMasterDetails = _db.UserMasterDetails.Where(a => a.UserMasterId == item.EmployeeId).FirstOrDefault();
                    if (userMasterDetails.BusinessId == filterData.Cluster)
                    {
                        EmployeeGroupDigitalAdoptionReview employee = new EmployeeGroupDigitalAdoptionReview();
                        employee.EmployeeId = item.EmployeeId;
                        employee.Total = item.Total;
                        businessFilter.Add(employee);
                    }
                }
                DigitalAdoptionGroupBy = businessFilter;
            }

            if (filterData.Project != null)
            {
                List<EmployeeGroupDigitalAdoptionReview> projectFilter = new List<EmployeeGroupDigitalAdoptionReview>();
                foreach (EmployeeGroupDigitalAdoptionReview item in DigitalAdoptionGroupBy)
                {
                    UserMasterDetails userMasterDetails = _db.UserMasterDetails.Where(a => a.UserMasterId == item.EmployeeId).FirstOrDefault();
                    if (userMasterDetails.AreaId == filterData.Project)
                    {
                        EmployeeGroupDigitalAdoptionReview employee = new EmployeeGroupDigitalAdoptionReview();
                        employee.EmployeeId = item.EmployeeId;
                        employee.Total = item.Total;
                        projectFilter.Add(employee);
                    }
                }
                DigitalAdoptionGroupBy = projectFilter;
            }

            foreach (EmployeeGroupDigitalAdoptionReview item in DigitalAdoptionGroupBy)
            {
                List<DigitalAdoptionReview> data1 = _db.DigitalAdoptionReview.Where(a => a.EmployeeId == item.EmployeeId).Select(a => new DigitalAdoptionReview { InvolvementLevel = a.InvolvementLevel, UseCaseKnowledge = a.UseCaseKnowledge, DigitalAwareness = a.DigitalAwareness, DescriptionId = a.DescriptionId, CreatedDate = a.CreatedDate }).ToList();
                int involvementLevel = 0, digitalAwareness = 0, useCaseKnowledge = 0;

                if (filterData.Role.Length != 0)
                {
                    data1.Clear();
                    for (int i = 0; i < filterData.Role.Length; i++)
                    {
                        List<DigitalAdoptionReview> data2 = _db.DigitalAdoptionReview.Where(a => a.RoleId == filterData.Role[i] && a.EmployeeId == item.EmployeeId).ToList();
                        data1.AddRange(data2);
                    }
                }
                if (filterData.UseCase != null)
                {
                    data1 = data1.Where(a => a.DescriptionId == filterData.UseCase).ToList();
                }
                if (filterData.Month != null)
                {
                    data1 = data1.Where(a => a.CreatedDate.Month == filterData.Month).ToList();
                }
                if (filterData.Year != null)
                {
                    data1 = data1.Where(a => a.CreatedDate.Year == filterData.Year).ToList();
                }

                foreach (DigitalAdoptionReview item1 in data1)
                {
                    involvementLevel = involvementLevel + item1.InvolvementLevel;
                    digitalAwareness = digitalAwareness + item1.DigitalAwareness;
                    useCaseKnowledge = useCaseKnowledge + item1.UseCaseKnowledge;
                }

                int IL = involvementLevel / item.Total;
                int DA = digitalAwareness / item.Total;
                int UC = useCaseKnowledge / item.Total;

                item.IlAverage = IL;
                item.DaAverage = DA;
                item.UcAverage = UC;

                UserMaster userMaster = _db.UserMaster.Where(a => a.Id == item.EmployeeId).FirstOrDefault();
                UserMasterDetails userMasterDetails = _db.UserMasterDetails.Where(a => a.UserMasterId == item.EmployeeId).FirstOrDefault();
                item.EmployeeName = userMaster.UserName;
                item.ProfilePicture = userMasterDetails.ProfilePicture;
                item.Gender = userMasterDetails.Gender;
            }

            List<EmployeeGroupDigitalAdoptionReview> IlTop5List = DigitalAdoptionGroupBy.OrderByDescending(a => a.IlAverage).Take(5).ToList();
            List<EmployeeGroupDigitalAdoptionReview> DaTop5List = DigitalAdoptionGroupBy.OrderByDescending(a => a.DaAverage).Take(5).ToList();
            List<EmployeeGroupDigitalAdoptionReview> UcTop5List = DigitalAdoptionGroupBy.OrderByDescending(a => a.UcAverage).Take(5).ToList();

            APIDigitalAdoptionReviewDashBoard data = new APIDigitalAdoptionReviewDashBoard();

            data.IlTop5List = IlTop5List;
            data.DaTop5List = DaTop5List;
            data.UcTop5List = UcTop5List;

            return data;
        }

        public async Task<UserDigitalAdoptionReview> UserDigitalAdoption(int UserId)
        {
            List<DigitalAdoptionReview> digitalAdoptionReviews = await _db.DigitalAdoptionReview.Where(a => a.EmployeeId == UserId).ToListAsync();
            UserDigitalAdoptionReview totalAverage = new UserDigitalAdoptionReview();
            if (digitalAdoptionReviews.Count == 0)
            {
                totalAverage.IlAverage = 0;
                totalAverage.DaAverage = 0;
                totalAverage.UcAverage = 0;
                return totalAverage;
            }
            
            int involvementLevel = 0, digitalAwareness = 0, useCaseKnowledge = 0;

            foreach (DigitalAdoptionReview item in digitalAdoptionReviews)
            {
                involvementLevel = involvementLevel + item.InvolvementLevel;
                digitalAwareness = digitalAwareness + item.DigitalAwareness;
                useCaseKnowledge = useCaseKnowledge + item.UseCaseKnowledge;
            }
            int IL = involvementLevel / digitalAdoptionReviews.Count();
            int DA = digitalAwareness / digitalAdoptionReviews.Count();
            int UC = useCaseKnowledge / digitalAdoptionReviews.Count();

            
            totalAverage.IlAverage = IL;
            totalAverage.DaAverage = DA;
            totalAverage.UcAverage = UC;

            return totalAverage;

        }

        public async Task<APIResponse> ProcessImportFile(APIDARImport aPIDataMigration, int UserId, string OrgCode, string UserName)
        {
            APIResponse Response = new APIResponse();
            try
            {
                string sWebRootFolder = this._configuration["ApiGatewayWwwroot"];
                string DomainName = this._configuration["ApiGatewayUrl"];
                string filepath = sWebRootFolder + aPIDataMigration.Path;


                DataTable competencycategoryImportdt = ReadFile(filepath);
                if (competencycategoryImportdt == null || competencycategoryImportdt.Rows.Count == 0)
                {
                    string resultstring = Record.FileDoesNotContainsData;
                    return new APIResponse { StatusCode = 400, ResponseObject = new { resultstring } };
                }
                Reset();
                List<string> importcolumns = GetImportColumns().Select(c => c.Key).ToList();
                bool resultMessage = await ValidateFileColumnHeaders(competencycategoryImportdt, importcolumns);
                if (resultMessage == true)
                {
                    Response = await ProcessRecordsAsync(competencycategoryImportdt, UserId, OrgCode, UserName, importcolumns);
                    Reset();
                    return Response;
                }
                else
                {
                    Response.StatusCode = 204;
                    string resultstring = Record.FileInvalid;
                    Response.ResponseObject = new { resultstring };
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

        public DataTable ReadFile(string filepath)
        {
            DataTable dt = new DataTable();

            using (var pck = new ExcelPackage())
            {
                using (var stream = File.OpenRead(filepath))
                {
                    pck.Load(stream);
                }

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

        public void Reset()
        {
            sb.Clear();
            header = new string[0];
            headerStar = new string[0];
            headerWithoutStar = new string[0];
            ReviewList.Clear();

            sbError.Clear();
            totalRecordInsert = 0;
            totalRecordRejected = 0;
        }

        private List<KeyValuePair<string, int>> GetImportColumns()
        {
            List<KeyValuePair<string, int>> columns = new List<KeyValuePair<string, int>>();
            columns.Add(new KeyValuePair<string, int>(ImportHeaders.EmployeeCode, 250));
            columns.Add(new KeyValuePair<string, int>(ImportHeaders.UseCase, 250));
            columns.Add(new KeyValuePair<string, int>(ImportHeaders.Role, 250));
            columns.Add(new KeyValuePair<string, int>(ImportHeaders.InvolvementLevel, 250));
            columns.Add(new KeyValuePair<string, int>(ImportHeaders.DigitalAwareness, 250));
            columns.Add(new KeyValuePair<string, int>(ImportHeaders.UseCaseKnowledge, 250));
            columns.Add(new KeyValuePair<string, int>(ImportHeaders.Remark, 250));
            return columns;
        }

        public async Task<bool> ValidateFileColumnHeaders(DataTable userImportdt, List<string> importColumns)
        {
            if (userImportdt.Columns.Count != importColumns.Count)
                return false;

            for (int i = 0; i < userImportdt.Columns.Count; i++)
            {
                string col = userImportdt.Columns[i].ColumnName.Replace("*", "").Replace(" ", "");
                userImportdt.Columns[i].ColumnName = col;

                if (!importColumns.Contains(userImportdt.Columns[i].ColumnName))
                    return false;
            }
            return true;
        }


        public async Task<APIResponse> ProcessRecordsAsync(DataTable competencycategoryImportdt, int userId, string OrgCode, string UserName, List<string> importcolumns)
        {
            APIResponse response = new APIResponse();
            List<DigitalAdoptionReviewData> apiCompetencyCategoryImportRejected = new List<DigitalAdoptionReviewData>();

            competencycategoryImportdt.Columns.Add("ErrorMessage", typeof(string));

            int columnIndex = 0;
            DataColumnCollection columns = competencycategoryImportdt.Columns;
            foreach (string column in importcolumns)
            {
                competencycategoryImportdt.Columns[column].SetOrdinal(columnIndex);
                columnIndex++;
            }
            List<KeyValuePair<string, int>> columnlengths = GetImportColumns();
            DataTable finalDt = competencycategoryImportdt.Clone();
            DataTable newfinalDt = competencycategoryImportdt.Clone();


            if (competencycategoryImportdt != null && competencycategoryImportdt.Rows.Count > 0)
            {
                List<DigitalAdoptionReviewData> apiCompetencyCategoryImportList = new List<DigitalAdoptionReviewData>();

                foreach (DataRow dataRow in competencycategoryImportdt.Rows)
                {
                    bool isError = false;
                    string errorMsg = "";
                    foreach (string column in importcolumns)
                    {

                        if (string.Compare(column, "EmployeeCode", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Employee Code required";
                            }

                        }
                        if (string.Compare(column, "UseCase", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Use Case required";
                            }
                        }
                        if (string.Compare(column, "Role", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Role required";
                            }
                        }
                        if (string.Compare(column, "InvolvementLevel", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Involvement Level required";
                            }
                        }
                        if (string.Compare(column, "DigitalAwareness", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Digital Awareness required";
                            }
                        }
                        if (string.Compare(column, "UseCaseKnowledge", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Use Case Knowledge required"; 
                            }
                        }
                        if (string.Compare(column, "Remark", true) == 0)
                        {
                            if (string.IsNullOrEmpty(Convert.ToString(dataRow[column])))
                            {
                                isError = true;
                                errorMsg = "Remark required"; 
                            }
                        }

                    }
                    int? useCaseId = _db.UseCase.Where(a => a.Description == Convert.ToString(dataRow[1]) && a.IsDeleted == false).Select(a => a.Id).FirstOrDefault();
                    int? roleId = _db.DigitalRole.Where(a => a.Description == Convert.ToString(dataRow[2]) && a.IsDeleted == false).Select(a => a.Id).FirstOrDefault();
                    int? employeeId = _db.UserMaster.Where(a => a.UserId == Security.Encrypt(Convert.ToString(dataRow[0]))).Select(a => a.Id).FirstOrDefault();

                    string errMassage =  await DataValidation(employeeId, useCaseId, roleId, Convert.ToString(dataRow[3]), Convert.ToString(dataRow[4]), Convert.ToString(dataRow[5]), userId);
                    
                    if (errMassage!="")
                    {
                        isError = true;
                        errorMsg = errMassage;
                    }

                    if (isError)
                    {
                        competencyCategoryImport.EmployeeCode = dataRow[0] != null ? Convert.ToString(dataRow[0]) : null;
                        competencyCategoryImport.UseCase = dataRow[1] != null ? Convert.ToString(dataRow[1]) : null;
                        competencyCategoryImport.Role = dataRow[2] != null ? Convert.ToString(dataRow[2]) : null;
                        competencyCategoryImport.InvolvementLevel = dataRow[3] != null ? Convert.ToString(dataRow[3]) : null;
                        competencyCategoryImport.DigitalAwareness = dataRow[4] != null ? Convert.ToString(dataRow[4]) : null;
                        competencyCategoryImport.UseCaseKnowledge = dataRow[5] != null ? Convert.ToString(dataRow[5]) : null;
                        competencyCategoryImport.Remark = dataRow[6] != null ? Convert.ToString(dataRow[6]) : null;
                    
                        competencyCategoryImport.ErrMessage = errorMsg;
                        competencyCategoryImport.IsInserted = "false";
                        competencyCategoryImport.IsUpdated = "false";
                        competencyCategoryImport.notInsertedCode = "";
                        dataRow[7] = competencyCategoryImport.ErrMessage;
                        apiCompetencyCategoryImportList.Add(competencyCategoryImport);
                    }
                    else
                    {
                        finalDt.ImportRow(dataRow);
                    }

                    competencyCategoryImport = new DigitalAdoptionReviewData();
                    sb.Clear();

                }

                //Remove duplicates from the Excel file DataTable
                newfinalDt = removeDuplicatesRows(finalDt);
                

                try
                {
                    foreach(DataRow dataRow in competencycategoryImportdt.Rows)
                    {
                        int? useCaseId = _db.UseCase.Where(a => a.Description == Convert.ToString(dataRow[1]) && a.IsDeleted == false).Select(a => a.Id).FirstOrDefault();
                        int? roleId = _db.DigitalRole.Where(a => a.Description == Convert.ToString(dataRow[2]) && a.IsDeleted == false).Select(a => a.Id).FirstOrDefault();
                        int? employeeId = _db.UserMaster.Where(a => a.UserId == Security.Encrypt(Convert.ToString(dataRow[0]))).Select(a => a.Id).FirstOrDefault();
                        string errMassage = await DataValidation(employeeId, useCaseId, roleId, Convert.ToString(dataRow[3]), Convert.ToString(dataRow[4]), Convert.ToString(dataRow[5]), userId);
                        if (errMassage == "")
                        {
                            DigitalAdoptionReview digitalAdoptionReview = new DigitalAdoptionReview();
                            digitalAdoptionReview.EmployeeId = (int)employeeId;
                            digitalAdoptionReview.DescriptionId = (int)useCaseId;
                            digitalAdoptionReview.RoleId = (int)roleId;
                            digitalAdoptionReview.ReviewerId = userId;
                            digitalAdoptionReview.InvolvementLevel = Convert.ToInt32(dataRow[3]);
                            digitalAdoptionReview.DigitalAwareness = Convert.ToInt32(dataRow[4]);
                            digitalAdoptionReview.UseCaseKnowledge = Convert.ToInt32(dataRow[5]);
                            digitalAdoptionReview.Remarks= Convert.ToString(dataRow[6]);
                            digitalAdoptionReview.code = Convert.ToString(dataRow[0]);
                            digitalAdoptionReview.ModifiedBy = userId;
                            digitalAdoptionReview.CreatedBy = userId;
                            digitalAdoptionReview.IsActive = true;
                            digitalAdoptionReview.IsDeleted = false;
                            digitalAdoptionReview.CreatedDate = DateTime.UtcNow;
                            digitalAdoptionReview.ModifiedDate = DateTime.UtcNow;
                            await _db.DigitalAdoptionReview.AddAsync(digitalAdoptionReview);
                            this._db.SaveChanges();

                            DigitalAdoptionReviewData insertedData = new DigitalAdoptionReviewData();
                            insertedData.EmployeeCode = dataRow[0] != null ? Convert.ToString(dataRow[0]) : null;
                            insertedData.UseCase = dataRow[1] != null ? Convert.ToString(dataRow[1]) : null;
                            insertedData.Role = dataRow[2] != null ? Convert.ToString(dataRow[2]) : null;
                            insertedData.InvolvementLevel = dataRow[3] != null ? Convert.ToString(dataRow[3]) : null;
                            insertedData.DigitalAwareness = dataRow[4] != null ? Convert.ToString(dataRow[4]) : null;
                            insertedData.UseCaseKnowledge = dataRow[5] != null ? Convert.ToString(dataRow[5]) : null;
                            insertedData.Remark = dataRow[6] != null ? Convert.ToString(dataRow[6]) : null;

                            insertedData.ErrMessage = null;
                            insertedData.IsInserted = "true";
                            insertedData.IsUpdated = "true";
                            insertedData.notInsertedCode = "";
                            dataRow[7] = null;

                            apiCompetencyCategoryImportList.Add(insertedData);
                        }
                       

                    }
                    
                }
                catch (Exception ex)
                {
                    _logger.Error(Utilities.GetDetailedException(ex));
                }

                foreach (var data in apiCompetencyCategoryImportList)
                {
                    if (!string.IsNullOrEmpty(data.EmployeeCode) || !string.IsNullOrEmpty(data.UseCase) || !string.IsNullOrEmpty(data.Role) || !string.IsNullOrEmpty(data.InvolvementLevel) || !string.IsNullOrEmpty(data.DigitalAwareness) || !string.IsNullOrEmpty(data.UseCaseKnowledge) || !string.IsNullOrEmpty(data.Remark))
                    {
                        if (data.ErrMessage != null)
                        {
                            totalRecordRejected++;
                            apiCompetencyCategoryImportRejected.Add(data);
                        }
                        else
                        {
                            totalRecordInsert++;
                        }
                    }
                }

            }
            string resultstring = "Total number of records inserted : " + totalRecordInsert + ",  Total number of records rejected : " + totalRecordRejected + ". Duplicate entries were removed from the file";

            response.StatusCode = 200;
            response.ResponseObject = new { resultstring, apiCompetencyCategoryImportRejected };
            return response;

        }

        public async Task<string> DataValidation(int? Code, int? UseCase, int? Role, string IL, string DA,string UC,int UserId)
        {
            string errMsg = "";
            if(Code == UserId)
            {
               errMsg =  errMsg+ " You cannot review yourself!!";
            }
            if(Code == null ||Code == 0 )
            {
                errMsg =  errMsg + " Please Provide Valid Employee Code";
            }

            if (UseCase == null || UseCase == 0)
            {
                errMsg = errMsg + " Please Provide Valid Use Case";
            }

            if (Role == null || Role == 0)
            {
                errMsg =  errMsg + " Please Provide Valid Role" ;
            }
            if(Convert.ToInt32(IL) > 10 || Convert.ToInt32(DA) > 10 || Convert.ToInt32(UC) > 10)
            {
                errMsg = errMsg + " Score should be in between 1 and 10";
            }

            List<DigitalAdoptionReview> darList =(from dar in _db.DigitalAdoptionReview
                                                  where dar.EmployeeId == Code &&  dar.DescriptionId == UseCase && dar.RoleId == Role && dar.UseCaseKnowledge == Convert.ToInt32(UC)
                                                  && dar.InvolvementLevel == Convert.ToInt32(IL) && dar.DigitalAwareness == Convert.ToInt32(DA) && dar.IsDeleted == false
                                                  select dar).ToList();
            if (darList.Count() > 0)
                 errMsg = errMsg + " You have already given review for this Employee!!";

            if(errMsg!="")
                return errMsg;
            else
                return "";

        }

        public DataTable removeDuplicatesRows(DataTable dt)
        {
            DataTable uniqueCols = dt.DefaultView.ToTable(true, "EmployeeCode", "UseCase","Role", "InvolvementLevel", "DigitalAwareness", "UseCaseKnowledge", "Remark", "ErrorMessage");
            return uniqueCols;
        }



    }
}
