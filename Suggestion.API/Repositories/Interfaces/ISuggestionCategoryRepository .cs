using Suggestion.API.APIModel;
using System.Threading.Tasks;

namespace Suggestion.API.Repositories.Interfaces
{
    public interface ISuggestionCategories : IRepository<SuggestionCategory>
    {
        Task<APISuggestionCategoriesListandCount> GetSuggestionCategories(int page, int pageSize, string search);
    }
}