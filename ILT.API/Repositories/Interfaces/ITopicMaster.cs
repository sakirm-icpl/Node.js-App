using ILT.API.APIModel;
using ILT.API.Model.ILT;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface ITopicMaster : IRepository<TopicMaster>
    {
        Task<bool> Exists(string name);
        Task<int> PostTopics(APITopicMaster obj, int UserId);
        Task<List<TopicMaster>> GetTopics(string searchText = null);
        Task<int> GetTopicsCount(string searchText = null);
        Task<List<TopicMaster>> GetAllTopics(int page, int pageSize, string searchText);
        Task<bool> CheckForExistance(int Id);
    }
}
