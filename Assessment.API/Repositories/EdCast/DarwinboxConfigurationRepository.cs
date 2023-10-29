using log4net;
using Assessment.API.Model;
using Assessment.API.Repositories.Interfaces.EdCast;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Models;

namespace Assessment.API.Repositories.EdCast
{
    public class DarwinboxConfigurationRepository : Repository<DarwinboxConfiguration>, IDarwinboxConfiguration
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(DarwinboxConfigurationRepository));
        private AssessmentContext _db;
        private readonly IConfiguration _configuration;
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;



        public DarwinboxConfigurationRepository(AssessmentContext context, IConfiguration configuration, ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _configuration = configuration;
            _db = context;
            _customerConnectionRepository = customerConnectionRepository;
        }
    }
}