using ILT.API.Model;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.Repositories
{
    public class UserGroupRepository : Repository<UserGroup>, IUserGroup
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(AccessibiltyRuleRepository));
        private CourseContext _db;
        private readonly IConfiguration _configuration;
        private readonly ICustomerConnectionStringRepository _customerConnectionRepository;

        public UserGroupRepository(CourseContext context,
            IConfiguration configuration,
            ICustomerConnectionStringRepository customerConnectionRepository) : base(context)
        {
            _configuration = configuration;
            _db = context;
            _customerConnectionRepository = customerConnectionRepository;
        }

        public async Task<List<UserGroup>> GetAllUsersOfGroup(int UserGroupId)
        {
            string GroupName1 = _db.UserGroup.Where(a => a.Id == UserGroupId).FirstOrDefault().GroupName;

            var Query = _db.UserGroup.Where(a => a.GroupName == GroupName1 && a.IsDeleted == false);

            return await Query.ToListAsync<UserGroup>();
        }
    }
}
