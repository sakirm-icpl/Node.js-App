using ILT.API.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface IUserGroup : IRepository<UserGroup>
    {
        Task<List<UserGroup>> GetAllUsersOfGroup(int UserGroupId);
    }
}
