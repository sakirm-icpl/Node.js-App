using ILT.API.APIModel;
using ILT.API.Helper;
using ILT.API.Model.ILT;
using ILT.API.Models;
using ILT.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net;
namespace ILT.API.Repositories
{
    public class ModuleTopicAssociationRepository : Repository<ModuleTopicAssociation>, IModuleTopicAssociation
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ModuleTopicAssociationRepository));
        private CourseContext _db;
        public ModuleTopicAssociationRepository(CourseContext context) : base(context)
        {
            _db = context;
        }

        public async Task<List<ModuleTypeAhead>> GetModuleTypeAhead(string search = null)
        {
            try
            {
                var Query = (from Module in this._db.Module
                           where Module.IsActive == true && Module.IsDeleted == Record.NotDeleted
                                   && Module.ModuleType.ToLower() == "classroom"
                             select new ModuleTypeAhead
                             {
                                 Id = Module.Id,
                                 ModuleName = Module.Name
                             });

                Query = Query.Distinct().OrderByDescending(a => a.Id);

                if (!string.IsNullOrEmpty(search))
                {
                    Query = Query.Where(a => a.ModuleName.StartsWith(search.ToLower()));
                }

                return Query.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APIModuleDetails>> Get(int page, int pageSize, string searchText)
        {
            try
            {
                var Query = _db.ModuleTopicAssociation.Join(_db.Module, r => r.ModuleId, (p => p.Id), (r, p) => new { r, p })

                    .Where(c => (c.r.IsDeleted == false && c.r.IsActive == true))
                     .GroupBy(od => new
                     {
                         od.p.Id,
                         od.p.Name,
                         od.p.Description
                     })
                     .OrderByDescending(a => a.Key.Id)
                    .Select(m => new APIModuleDetails
                    {
                       ModuleId=m.Key.Id,
                       Name=m.Key.Name,
                       Description=m.Key.Description

                    });

                if (!string.IsNullOrEmpty(searchText))
                {
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        Query = Query.Where(r => r.Name.ToLower().Contains(searchText.ToLower()));
                    }
                }

                if (page != -1)
                    Query = Query.Skip((Convert.ToInt32(page) - 1) * Convert.ToInt32(pageSize));
                if (pageSize != -1)
                    Query = Query.Take(Convert.ToInt32(pageSize));

                return Query.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetCount(string searchText)
        {
            try
            {
                var Query = _db.ModuleTopicAssociation.Join(_db.Module, r => r.ModuleId, (p => p.Id), (r, p) => new { r, p })
                    .OrderByDescending(a=>a.p.Id)
                    .Where(c => (c.r.IsDeleted == false && c.r.IsActive == true)).Select(m => new APIModuleDetails
                    {
                        ModuleId = m.r.ModuleId,
                        Name = m.p.Name,
                        Description = m.p.Description
                    }).Distinct();

                if (!string.IsNullOrEmpty(searchText))
                {
                    if (!string.IsNullOrEmpty(searchText))
                    {
                        Query = Query.Where(r => r.Name.ToLower().Contains(searchText.ToLower()));
                    }
                }

                return Query.Count();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<APITopicDetialsByModuleId>> GetTopicDetailsByModuleId(int moduleId)
        {
            try
            {
                var Query = (from ModuleTopicAssociation in this._db.ModuleTopicAssociation
                             join Topic in this._db.TopicMaster on ModuleTopicAssociation.TopicId equals Topic.ID
                             where Topic.IsActive == true && Topic.IsDeleted == Record.NotDeleted && ModuleTopicAssociation.ModuleId == moduleId
                             select new APITopicDetialsByModuleId
                             {
                                 TopicId = ModuleTopicAssociation.TopicId,
                                 TopicName = Topic.TopicName
                             });

                return Query.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<bool> PostAssociation(APIModuleTopicAssociation aPIModuleTopicAssociation, int UserId)
        {
            try
            {
               foreach (var obj in aPIModuleTopicAssociation.TopicList)
                {
                    ModuleTopicAssociation moduleTopicAssociation = new ModuleTopicAssociation();

                    moduleTopicAssociation.ModuleId = aPIModuleTopicAssociation.ModuleId;
                    moduleTopicAssociation.TopicId = obj.TopicId;
                    moduleTopicAssociation.IsActive = true;
                    moduleTopicAssociation.IsDeleted = false;
                    moduleTopicAssociation.CreatedBy = UserId;
                    moduleTopicAssociation.CreatedDate = DateTime.UtcNow;
                    moduleTopicAssociation.ModifiedBy = UserId;
                    moduleTopicAssociation.ModifiedDate = DateTime.UtcNow;

                    await this.Add(moduleTopicAssociation);
                }
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<ModuleTopicAssociation>> IsExists(int moduleId)
        {
            try
            {
                List<ModuleTopicAssociation> moduleTopicAssociation = new List<ModuleTopicAssociation>();
                moduleTopicAssociation = this._db.ModuleTopicAssociation.Where(a => a.ModuleId == moduleId && a.IsActive == true && a.IsDeleted == Record.NotDeleted).ToList();
                return moduleTopicAssociation;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<bool> CheckInAttendance(int moduleId)
        {
            try
            {
                ILTTrainingAttendance attendance = new ILTTrainingAttendance();
                attendance = this._db.ILTTrainingAttendance.Where(a => a.ModuleID == moduleId && a.IsActive == true && a.IsDeleted == Record.NotDeleted).FirstOrDefault();
                if (attendance != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }
    }
}
