using ILT.API.Model;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using log4net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.Repositories
{
    public class AccebilityRuleUserGroupRepository : Repository<AccebilityRuleUserGroup>, IAccebilityRuleUserGroup
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccessibiltyRuleRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;

        public AccebilityRuleUserGroupRepository(CourseContext context, 
            IConfiguration configuration,
            ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _configuration = configuration;
            _db = context;
            _customerConnectionRepository = customerConnectionRepository;
        }
    }
}
