using MyCourse.API.Model;
//using MyCourse.API.Model.EdCastAPI;
//using MyCourse.API.Models;
using MyCourse.API.Repositories.Interfaces;
using MyCourse.API.Repositories.Interfaces;
using log4net;
using Microsoft.Extensions.Configuration;

namespace MyCourse.API.Repositories
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
