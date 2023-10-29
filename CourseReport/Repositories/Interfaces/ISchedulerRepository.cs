using CourseReport.API.APIModel;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CourseReport.API.Repositories.Interface
{
    public interface ISchedulerRepository
    {
        Task<IEnumerable<APISchedulerReport>> GetAllCoursesCompletionReport(APISchedulerModule schedulermodule);
        Task<IEnumerable<APIExportAllCoursesCompletionReport>> ExportAllCoursesCompletionReport();
     
       

    }
}
