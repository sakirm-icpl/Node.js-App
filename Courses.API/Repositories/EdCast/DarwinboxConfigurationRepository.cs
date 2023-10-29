using Courses.API.APIModel;
using Courses.API.Model;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Courses.API.Repositories.Interfaces.EdCast;
using log4net;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Courses.API.Repositories.EdCast
{
    public class DarwinboxConfigurationRepository : Repository<DarwinboxConfiguration>, IDarwinboxConfiguration
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DarwinboxConfigurationRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;       
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;



        public DarwinboxConfigurationRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _configuration = configuration;
            _db = context;
            _customerConnectionRepository = customerConnectionRepository;
        }
        public async Task<APIDarwinboxConfiguration> GetDarwinboxConfiguration()
        {
            APIDarwinboxConfiguration data = new APIDarwinboxConfiguration();
            var Query = (from darwinboxConfiguration in _db.DarwinboxConfiguration
                         select new APIDarwinboxConfiguration
                         {
                             Create_LA = darwinboxConfiguration.Create_LA,
                             Update_LA = darwinboxConfiguration.Update_LA,
                             DarwinboxCourseUrl = darwinboxConfiguration.DarwinboxCourseUrl,
                             DarwinboxHost = darwinboxConfiguration.DarwinboxHost,
                             Username = darwinboxConfiguration.Username,
                             Password = darwinboxConfiguration.Password,
                             Program = darwinboxConfiguration.Program,                           
                             Id = darwinboxConfiguration.Id,

                         }
                       );
            var course_Data = await Query.FirstOrDefaultAsync();

            return data;

        }
    }

}
