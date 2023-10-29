using Courses.API.Model;
using Courses.API.Model.EdCastAPI;
using Courses.API.Models;
using Courses.API.Repositories.Interfaces;
using Courses.API.Repositories.Interfaces.EdCast;
using log4net;
using Microsoft.Extensions.Configuration;

namespace Courses.API.Repositories.EdCast
{
    public class DarwinboxTransactionDetailsRepository : Repository<DarwinboxTransactionDetails>, IDarwinboxTransactionDetails
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DarwinboxTransactionDetailsRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;       
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;
      

        public DarwinboxTransactionDetailsRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _configuration = configuration;
            _db = context;            
            _customerConnectionRepository = customerConnectionRepository;
           
        }

      
    }

}
