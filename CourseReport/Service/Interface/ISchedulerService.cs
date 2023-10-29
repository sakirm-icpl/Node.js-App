using CourseReport.API.APIModel;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace CourseReport.API.Service
{
    public interface ISchedulerService
    {

        Task<FileInfo> ExportAllCoursesCompletionReport(APISchedulerModule schedulermodule, string OrgCode);

    }
}
