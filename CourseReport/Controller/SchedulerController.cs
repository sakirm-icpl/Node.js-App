using AspNet.Security.OAuth.Introspection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CourseReport.API.APIModel;
using CourseReport.API.Common;
using CourseReport.API.Helper;
using CourseReport.API.Helper.MetaData;
using CourseReport.API.Model;
using CourseReport.API.Repositories.Interface;
using CourseReport.API.Service;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CourseReport.API.Service;
using static CourseReport.API.Common.AuthorizePermissions;
using static CourseReport.API.Common.TokenPermissions;
using log4net;
using CourseReport.API.Helper.Log_API_Count;

namespace CourseReport.API.Controllers
{
    [ServiceFilter(typeof(APIRequestCount<ClientUserApiCount>))]
    [Route("api/v1/[controller]")]
    [Authorize(AuthenticationSchemes = OAuthIntrospectionDefaults.AuthenticationScheme)]
   [Authorize]
    //added to check expired token
    [TokenRequired()]
    public class SchedulerController : IdentityController
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(SchedulerController));
        private ISchedulerService _schedulerService;
        private readonly ITokensRepository _tokensRepository;
        public SchedulerController(
            IIdentityService identityService,
            ISchedulerService schedulerService, ITokensRepository tokensRepository) : base(identityService)
        {
            this._schedulerService = schedulerService;
            this._tokensRepository = tokensRepository;
        }

        #region AllCoursesCompletionReport

        [HttpPost("ExportAllCoursesCompletionReport")]
        [PermissionRequired(Permissions.AllCoursesCompletionReport)]
        public async Task<IActionResult> ExportCourseCompletionReport([FromBody]APISchedulerModule schedulermodule)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    schedulermodule.UserID = UserId;
                    schedulermodule.StartIndex = 1;
                    FileInfo ExcelFile;

                    ExcelFile = await this._schedulerService.ExportAllCoursesCompletionReport(schedulermodule, OrgCode);

                    FileStream Fs = ExcelFile.OpenRead();
                    byte[] fileData = null;
                    using (BinaryReader binaryReader = new BinaryReader(Fs))
                    {
                        fileData = binaryReader.ReadBytes((int)Fs.Length);
                    }

                    if (schedulermodule.ExportAs == "csv")
                    {
                        Response.ContentType = FileContentType.ExcelCSV;
                        return File(
                                fileData,
                                FileContentType.ExcelCSV,
                                ExcelFile.Name);
                    }
                    else
                    {
                        Response.ContentType = FileContentType.Excel;
                        return File(
                                fileData,
                                FileContentType.Excel,
                                ExcelFile.Name);
                    }
                }
                else
                    return this.BadRequest(ModelState);
            }
            catch(Exception ex)
            {
                _logger.Error( Utilities.GetDetailedException(ex));

                return this.BadRequest(new ResponseMessage { Message = EnumHelper.GetEnumName(MessageType.InternalServerError), Description = EnumHelper.GetEnumDescription(MessageType.InternalServerError) });
            }
        }
        #endregion


    }
}
