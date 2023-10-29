using log4net;
using System.Text;
using Assessment.API.Model.Competency;
using Assessment.API.Repositories.Interfaces;
using Assessment.API.Repositories;
using Assessment.API.Repositories.Interfaces.Competency;
using Assessment.API.Models;

namespace Assessment.API.Repositories.Competency
{
    public class CompetenciesMasterRepository : Repository<CompetenciesMaster>, ICompetenciesMasterRepository
    {
        StringBuilder sb = new StringBuilder();
        string[] header = { };
        string[] headerStar = { };
        string[] headerWithoutStar = { };
        List<string> CompetencyMasterList = new List<string>();

        static StringBuilder sbError = new StringBuilder();
        static int totalRecordInsert = 0;
        static int totalRecordRejected = 0;


        private static readonly ILog _logger = LogManager.GetLogger(typeof(CompetenciesMasterRepository));
        private AssessmentContext db;
        private ICustomerConnectionStringRepository _customerConnectionStringRepository;
        private ICourseRepository _courseRepository;
        private readonly IConfiguration _configuration;

        public CompetenciesMasterRepository(IConfiguration configuration, AssessmentContext context, ICustomerConnectionStringRepository customerConnectionStringRepository, ICourseRepository courseRepository) : base(context)
        {
            this.db = context;
            this._customerConnectionStringRepository = customerConnectionStringRepository;
            _configuration = configuration;
            _courseRepository = courseRepository;
        }
    }
}