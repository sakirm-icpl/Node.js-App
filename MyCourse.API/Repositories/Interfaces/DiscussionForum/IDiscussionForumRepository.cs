using MyCourse.API.APIModel.DiscussionForum;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces.DiscussionForum
{
    public interface IDiscussionForumRepository : IRepository<Model.DiscussionForum>
    {
        Task<Action> SavePost(int? id, int? PostId, int CourseId, string SubjectText, string User, int ModifiedBy, string FilePath, string FileType, string organisationCode);
        bool Exists(int courseID);
        Task<IEnumerable<APIDiscussionForum>> GetDiscussionForumByCourseId(int CourseId, int userId, bool IsShowActiveRecords, int? page = null, int? pageSize = null);
        Task<int> GetCountDiscussionForum(bool? IsReview, int CourseId);
        Task<IEnumerable<APIDiscussionForum>> GetAllDiscussion(int CourseId, int userId, bool IsShowActiveRecords, int? page = null, int? pageSize = null);
        Task<IEnumerable<APIDiscussionForum>> GetAllDiscussionCommentsByParentId(int CourseId, int userId, bool IsShowActiveRecords, int? page = null, int? pageSize = null);
        Task<bool> CheckInprogessValidation(int CourseId, int ModifiedBy, string RoleCode);
        Task<string> GetParameterValue(string OrgCode, string configValue);
        
    }
}
