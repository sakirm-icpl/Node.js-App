using Suggestion.API.APIModel;
using Suggestion.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suggestion.API.Repositories.Interfaces
{
    public interface IEmployeeSuggestions : IRepository<EmployeeSuggestions>
    {
        Task<APIEmployeeSuggestionsListandCount> GetEmployeeSuggestions(int page, int pageSize, int userId, string searchBy, string search);
        Task<List<LikesList>> GetLikeList(int suggestionId, string like);
        Task<AttachedFilesListandCount> GetAttachedFiles(int Id);
        void savefile(EmployeeSuggestionFile employeeSuggestionFile);
        Task<EmployeeSuggestionLike> PostEmployeeSuggestionLike(int suggestionId, EmployeeSuggestionLike data, int UserId);
        Task<string> UpdateEmployeeSuggestionLike(int suggestionId, EmployeeSuggestionLike RemarkData, EmployeeSuggestionLike data, int UserId);
        void UpdateDeleteInFileTable(EmployeeSuggestionFile file);
        void UpdateDeleteInLikeTable(EmployeeSuggestionLike like);
        List<EmployeeSuggestionLike> GetLikeData(int suggestionId);
        List<EmployeeSuggestionFile> GetFileData(int suggestionId);
        Task<List<getfilecount>> GetFileCount();
        Task<APIEmployeeSuggestionAndDigitalAdoptionReview> GetEmployeeSuggestionsTop5(APIFilter filterData);
        Task<EmployeeSuggestionLike> GetRemarkData(int suggestionId, int userId);
        Task<APIEmployeeSuggestionsListandCount> GetEmployeeSuggestionsForUser(int page, int pageSize, int UserId);
        //Task<object> GetAttachedFiles(object id);
        Task<GetAreaListandCount> GetArea();
        Task<List<Business>> GetCluster();

    }
}