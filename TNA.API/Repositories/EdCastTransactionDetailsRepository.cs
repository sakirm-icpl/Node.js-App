using TNA.API.Model;
using TNA.API.Model.EdCastAPI;
//using TNA.API.Models;
using TNA.API.Repositories.Interfaces;
//using TNA.API.Repositories.Interfaces.EdCast;
using log4net;
using Microsoft.Extensions.Configuration;

namespace TNA.API.Repositories
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
