using ILT.API.APIModel;
//using ILT.API.APIModel;
using ILT.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ILT.API.Repositories.Interfaces
{
    public interface ITrainingPlaceRepository : IRepository<TrainingPlace>
    {
        Task<int> count(string search_string = null, string columnName = null);
        Task<List<TrainingPlace>> Get(int page, int pageSize, string search_string = null, string columnName = null);
        Task<bool> NameCodeExists(string q);
        Task<bool> Exists(string name);
        Task<int> GetTrainingPlaceCount();
        Task<List<TrainingPlaceTypeAhead>> GetTrainingPlaceTypeAhead(string search);
        Task<int> UpdateTrainingPlace(int id, TrainingPlace trainingPlace, int UserId);
        Task<int> DeleteTrainingPlace(int id, int UserId);
        Task<ApiResponse> CheckForValidInfo(int UserID, string MobileNumber, string ContactPerson);
        Task<ValidUserInfo> ValidUserInfo(int UserID);
    }
}
