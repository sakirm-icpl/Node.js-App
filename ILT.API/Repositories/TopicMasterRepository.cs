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
    public class TopicMasterRepository : Repository<TopicMaster>, ITopicMaster
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(TopicMasterRepository));
        private CourseContext _db;
        public TopicMasterRepository(CourseContext context) : base(context)
        {
            _db = context;
        }

        public async Task<List<TopicMaster>> GetTopics(string searchText = null)
        {
            try
            {
                var Query = (from TopicMaster in this._db.TopicMaster
                             where TopicMaster.IsActive == true && TopicMaster.IsDeleted == Record.NotDeleted
                             select new TopicMaster
                             {
                                 ID = TopicMaster.ID,
                                 TopicName = TopicMaster.TopicName
                             });

                Query = Query.Distinct().OrderByDescending(a => a.ID);

                if (!string.IsNullOrEmpty(searchText))
                    Query = Query.Where(a => a.TopicName.Contains(searchText));

                return Query.ToList();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<int> GetTopicsCount(string searchText = null)
        {
            try
            {
                var Query = (from TopicMaster in this._db.TopicMaster
                             where TopicMaster.IsActive == true && TopicMaster.IsDeleted == Record.NotDeleted
                             select new TopicMaster
                             {
                                 ID = TopicMaster.ID,
                                 TopicName = TopicMaster.TopicName
                             });
                if (!string.IsNullOrEmpty(searchText))
                    Query = Query.Where(a => a.TopicName.Contains(searchText));

                return Query.Count();
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<List<TopicMaster>> GetAllTopics(int page, int pageSize, string searchText)
        {
            try
            {
                var Query = (from TopicMaster in this._db.TopicMaster
                             where TopicMaster.IsActive == true && TopicMaster.IsDeleted == Record.NotDeleted
                             select new TopicMaster
                             {
                                 ID = TopicMaster.ID,
                                 TopicName = TopicMaster.TopicName
                             });

                Query = Query.Distinct().OrderByDescending(a => a.ID);

                if (!string.IsNullOrEmpty(searchText))
                    Query = Query.Where(a => a.TopicName.Contains(searchText));

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

        public async Task<bool> Exists(string name)
        {
            if (_db.TopicMaster.Count(y => y.TopicName == name) > 0)
                return true;
            return false;
        }

        public async Task<int> PostTopics(APITopicMaster obj, int UserId)
        {
            try
            {
                TopicMaster topicMaster = new TopicMaster();
                topicMaster = this._db.TopicMaster.Where(a => a.TopicName == obj.TopicName && a.IsActive == true && a.IsDeleted == Record.NotDeleted).FirstOrDefault();
                if (topicMaster != null)
                {
                    return 0;
                }

                topicMaster.TopicName = obj.TopicName;
                topicMaster.IsActive = true;
                topicMaster.IsDeleted = false;
                topicMaster.CreatedBy = UserId;
                topicMaster.CreatedDate = DateTime.UtcNow;
                topicMaster.ModifiedBy = UserId;
                topicMaster.ModifiedDate = DateTime.UtcNow;

                await this.Add(topicMaster);
                return 1;
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
        }

        public async Task<bool> CheckForExistance(int Id)
        {
            try
            {
                ModuleTopicAssociation existsInAssosciation = this._db.ModuleTopicAssociation.Where(a => a.TopicId == Id && a.IsActive == true && a.IsDeleted == Record.NotDeleted).FirstOrDefault();
                if (existsInAssosciation != null)
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
