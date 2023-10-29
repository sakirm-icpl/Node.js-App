using ILT.API.Model;
using ILT.API.Model.EdCastAPI;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
//using Courses.API.Repositories.Interfaces.EdCast;
using log4net;
using Microsoft.Extensions.Configuration;

namespace ILT.API.Repositories
{
    public class EdCastTransactionDetailsRepository : Repository<EdCastTransactionDetails>, IEdCastTransactionDetails
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccessibiltyRuleRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;       
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;
      

        public EdCastTransactionDetailsRepository(CourseContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _configuration = configuration;
            _db = context;            
            _customerConnectionRepository = customerConnectionRepository;
           
        }

      
    }

}
