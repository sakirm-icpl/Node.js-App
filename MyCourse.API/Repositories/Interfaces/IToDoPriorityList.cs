using MyCourse.API.APIModel;
using MyCourse.API.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MyCourse.API.Repositories.Interfaces
{
    public interface IToDoPriorityList : IRepository<ToDoPriorityList>
    {
        Task<int> Count(string search = null);
        Task<List<object>> GetAll(int page, int pageSize, string search = null);
        Task<object> GetQuizToDoList(int userId);
        Task<List<ApiGetTodoList>> GetSurveyToDoList(int userId);
        Task<object> GetById(int Id);
        Task<List<ApiGetTodoList>> GetCourseToDoList(int userId);
        Task<List<ApiGetTodoList>> GetToDoList(int userId);
    }
}
