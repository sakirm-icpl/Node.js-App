using Competency.API.APIModel.Competency;
using Competency.API.Helper;
using Competency.API.Model.Competency;
using Competency.API.Models;
using Competency.API.Repositories.Interfaces.Competency;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;

namespace Competency.API.Repositories.Competency
{
    public class RolewiseCompetenciesMappingRepository : Repository<RolewiseCompetenciesMapping>, IRolewiseCompetenciesMappingRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(RolewiseCompetenciesMappingRepository));
        private CourseContext db;
        public RolewiseCompetenciesMappingRepository(CourseContext context) : base(context)
        {
            this.db = context;
        }
        public async Task<IEnumerable<APIRolewiseCompetenciesMapping>> GetAllCompetenciesMapping(int page, int pageSize, string search = null)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from rolewiseCompetenciesMapping in context.RolewiseCompetenciesMapping
                                  join competencyCategory in context.CompetencyCategory on rolewiseCompetenciesMapping.CompetencyCategoryId equals competencyCategory.Id
                                  join competenciesMaster in context.CompetenciesMaster on rolewiseCompetenciesMapping.CompetencyId equals competenciesMaster.Id

                                  where rolewiseCompetenciesMapping.IsDeleted == Record.NotDeleted
                                  select new APIRolewiseCompetenciesMapping
                                  {
                                      Id = rolewiseCompetenciesMapping.Id,
                                      CompetencyId = rolewiseCompetenciesMapping.CompetencyId,
                                      CompetencyCategoryId = rolewiseCompetenciesMapping.CompetencyCategoryId,
                                      RoleId = rolewiseCompetenciesMapping.RoleId,
                                      RoleName = rolewiseCompetenciesMapping.RoleName,
                                      Competency = competenciesMaster.CompetencyDescription,
                                      CompetencyCategory = competencyCategory.Category


                                  });
                    if (!string.IsNullOrEmpty(search))
                    {
                        result = result.Where(a => ((Convert.ToString(a.CompetencyCategoryId).StartsWith(search) || Convert.ToString(a.RoleId).StartsWith(search) || Convert.ToString(a.CompetencyId).StartsWith(search) || Convert.ToString(a.CompetencyCategoryId).StartsWith(search)) && (a.IsDeleted == Record.NotDeleted)));
                    }
                    if (page != -1)
                        result = result.Skip((page - 1) * pageSize);

                    if (pageSize != -1)
                        result = result.Take(pageSize);

                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<IEnumerable<APIRolewiseCompetenciesMapping>> GetAllCompetenciesMapping()
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from rolewiseCompetenciesMapping in context.RolewiseCompetenciesMapping
                                  join competencyCategory in context.CompetencyCategory on rolewiseCompetenciesMapping.CompetencyCategoryId equals competencyCategory.Id
                                  join competenciesMaster in context.CompetenciesMaster on rolewiseCompetenciesMapping.CompetencyId equals competenciesMaster.Id

                                  where rolewiseCompetenciesMapping.IsDeleted == Record.NotDeleted
                                  select new APIRolewiseCompetenciesMapping
                                  {
                                      Id = rolewiseCompetenciesMapping.Id,
                                      CompetencyId = rolewiseCompetenciesMapping.CompetencyId,
                                      CompetencyCategoryId = rolewiseCompetenciesMapping.CompetencyCategoryId,
                                      RoleId = rolewiseCompetenciesMapping.RoleId,
                                      RoleName = rolewiseCompetenciesMapping.RoleName,
                                      Competency = competenciesMaster.CompetencyDescription,
                                      CompetencyCategory = competencyCategory.Category


                                  });

                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }


        public async Task<IEnumerable<APIRolewiseCompetenciesMapping>> GetAllCompetenciesMapping(int id)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from rolewiseCompetenciesMapping in context.RolewiseCompetenciesMapping
                                  join competencyCategory in context.CompetencyCategory on rolewiseCompetenciesMapping.CompetencyCategoryId equals competencyCategory.Id
                                  join competenciesMaster in context.CompetenciesMaster on rolewiseCompetenciesMapping.CompetencyId equals competenciesMaster.Id

                                  where (rolewiseCompetenciesMapping.IsDeleted == Record.NotDeleted && rolewiseCompetenciesMapping.Id == id)
                                  select new APIRolewiseCompetenciesMapping
                                  {
                                      Id = rolewiseCompetenciesMapping.Id,
                                      CompetencyId = rolewiseCompetenciesMapping.CompetencyId,
                                      CompetencyCategoryId = rolewiseCompetenciesMapping.CompetencyCategoryId,
                                      RoleId = rolewiseCompetenciesMapping.RoleId,
                                      RoleName = rolewiseCompetenciesMapping.RoleName,
                                      Competency = competenciesMaster.CompetencyDescription,
                                      CompetencyCategory = competencyCategory.Category


                                  });

                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }

        public async Task<IEnumerable<APIRolewiseCompetenciesMapping>> GetAllByRoleCompetenciesMapping(int roleid)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from rolewiseCompetenciesMapping in context.RolewiseCompetenciesMapping
                                  join competencyCategory in context.CompetencyCategory on rolewiseCompetenciesMapping.CompetencyCategoryId equals competencyCategory.Id
                                  join competenciesMaster in context.CompetenciesMaster on rolewiseCompetenciesMapping.CompetencyId equals competenciesMaster.Id

                                  where (rolewiseCompetenciesMapping.IsDeleted == Record.NotDeleted && rolewiseCompetenciesMapping.RoleId == roleid)
                                  select new APIRolewiseCompetenciesMapping
                                  {
                                      Id = rolewiseCompetenciesMapping.Id,
                                      CompetencyId = rolewiseCompetenciesMapping.CompetencyId,
                                      CompetencyCategoryId = rolewiseCompetenciesMapping.CompetencyCategoryId,
                                      RoleId = rolewiseCompetenciesMapping.RoleId,
                                      RoleName = rolewiseCompetenciesMapping.RoleName,
                                      Competency = competenciesMaster.CompetencyDescription,
                                      CompetencyCategory = competencyCategory.Category

                                  });

                    return await result.ToListAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
        public async Task<int> Count(string search = null)
        {
            if (!string.IsNullOrWhiteSpace(search))
                return await this.db.RolewiseCompetenciesMapping.Where(r => ((Convert.ToString(r.CompetencyCategoryId).StartsWith(search) || Convert.ToString(r.CompetencyId).StartsWith(search)) && (r.IsDeleted == Record.NotDeleted))).CountAsync();
            return await this.db.RolewiseCompetenciesMapping.Where(r => r.IsDeleted == Record.NotDeleted).CountAsync();
        }

        public async Task<bool> Exists(int roleId, int ComCatId, int comId)
        {
            var count = await this.db.RolewiseCompetenciesMapping.Where(p => ((p.RoleId == roleId) && (p.CompetencyCategoryId == ComCatId) && (p.CompetencyId == comId) && (p.IsDeleted == Record.NotDeleted))).CountAsync();

            if (count > 0)
                return true;
            return false;
        }

        public async Task<int> CountRole(int roleid)
        {

            return await this.db.RolewiseCompetenciesMapping.Where(r => ((r.RoleId == roleid) && (r.IsDeleted == Record.NotDeleted))).CountAsync();

        }

        public async Task<RolewiseCompetenciesMapping> GetRecordRole(int roleId)
        {
            try
            {
                using (var context = this.db)
                {
                    var result = (from rolewiseCompetenciesMapping in context.RolewiseCompetenciesMapping
                                  orderby rolewiseCompetenciesMapping.Id descending
                                  where (rolewiseCompetenciesMapping.IsDeleted == Record.NotDeleted && rolewiseCompetenciesMapping.RoleId == roleId)
                                  select new RolewiseCompetenciesMapping
                                  {
                                      Id = rolewiseCompetenciesMapping.Id,
                                      RoleId = rolewiseCompetenciesMapping.RoleId,
                                      CompetencyId = rolewiseCompetenciesMapping.CompetencyId,
                                      CompetencyCategoryId = rolewiseCompetenciesMapping.CompetencyCategoryId,
                                      RoleName = rolewiseCompetenciesMapping.RoleName

                                  });
                    return await result.FirstOrDefaultAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                string exception = ex.Message;
            }
            return null;
        }
    }
}
