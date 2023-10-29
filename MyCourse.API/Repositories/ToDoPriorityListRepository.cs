using MyCourse.API.APIModel;
using MyCourse.API.Helper;
using MyCourse.API.Model;
//using MyCourse.API.Models;
using MyCourse.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using log4net;
namespace MyCourse.API.Repositories
{
    public class ToDoPriorityListRepository : Repository<ToDoPriorityList>, IToDoPriorityList
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(ToDoPriorityListRepository));
        private CourseContext _db;
        ICustomerConnectionStringRepository _customerConnection;
        public ToDoPriorityListRepository(CourseContext context, ICustomerConnectionStringRepository customerConnection) : base(context)
        {
            this._db = context;
            this._customerConnection = customerConnection;
        }
        public async Task<List<object>> GetAll(int page, int pageSize, string search = null)
        {
            var Query = _db.ToDoPriorityList.Where(r => r.IsDeleted == Record.NotDeleted);
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Type.Contains(search));
            }
            Query = Query.OrderByDescending(r => r.Id);
            if (page != -1)
                Query = Query.Skip((page - 1) * pageSize);

            if (pageSize != -1)
                Query = Query.Take(pageSize);
            List<ToDoPriorityList> ToDoList = await Query.ToListAsync();

            List<object> Result = new List<object>();
            foreach (ToDoPriorityList todo in ToDoList)
            {
                object todoobj = new
                {
                    todo.Id,
                    todo.EndDate,
                    todo.CreatedDate,
                    todo.Priority,
                    todo.RefId,
                    todo.ScheduleDate,
                    todo.Type,
                    RefName = await GetRefName(todo.Type, todo.RefId)
                };
                Result.Add(todoobj);

            }
            return Result;
        }
        public async Task<int> Count(string search = null)
        {
            var Query = _db.ToDoPriorityList.Where(r => r.IsDeleted == Record.NotDeleted);
            if (!string.IsNullOrEmpty(search))
            {
                Query = Query.Where(r => r.Type.Contains(search));
            }
            return await Query.CountAsync();
        }
        public async Task<List<ApiGetTodoList>> GetToDoList(int userId)

        {
            List<ApiGetTodoList> TodoList = new List<ApiGetTodoList>();

            List<ApiGetTodoList> CourseTodoList = await GetCourseToDoList(userId);
            List<ApiGetTodoList> SurveyToDoList = await GetSurveyToDoList(userId);
            if (CourseTodoList == null)
                return SurveyToDoList;
            if (SurveyToDoList == null)
                return CourseTodoList;

            TodoList.AddRange(SurveyToDoList);
            TodoList.AddRange(CourseTodoList);

            return TodoList;
        }

        public async Task<object> GetQuizToDoList(int userId)
        {
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetQuizToDoList";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return null;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            var result = new
                            {
                                Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                Title = row["Title"].ToString(),
                                ScheduleDate = string.IsNullOrEmpty(row["ScheduleDate"].ToString()) ? (DateTime?)null : DateTime.Parse(row["ScheduleDate"].ToString()),
                                EndDate = string.IsNullOrEmpty(row["EndDate"].ToString()) ? (DateTime?)null : DateTime.Parse(row["EndDate"].ToString()),
                                Priority = string.IsNullOrEmpty(row["Priority"].ToString()) ? false : Boolean.Parse(row["Priority"].ToString())
                            };
                            return result;

                        }
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return null;
        }
        public async Task<List<ApiGetTodoList>> GetCourseToDoList(int userId)
        {
            List<ApiGetTodoList> CourseToDoList = new List<ApiGetTodoList>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetCourseToDoList";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return null;
                        }

                        foreach (DataRow row in dt.Rows)
                        {
                            ApiGetTodoList result = new ApiGetTodoList
                            {
                                Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                Title = row["Title"].ToString(),
                                ScheduleDate = string.IsNullOrEmpty(row["ScheduleDate"].ToString()) ? null : row["ScheduleDate"].ToString(),
                                EndDate = string.IsNullOrEmpty(row["EndDate"].ToString()) ? null : row["EndDate"].ToString(),
                                Priority = string.IsNullOrEmpty(row["Priority"].ToString()) ? false : Boolean.Parse(row["Priority"].ToString()),
                                Status = row["Status"].ToString(),
                                Type = "Course",
                                ModuleType = row["ModuleType"].ToString()
                            };
                            CourseToDoList.Add(result);
                        }
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return CourseToDoList;
        }
        public async Task<List<ApiGetTodoList>> GetSurveyToDoList(int userId)
        {
            List<ApiGetTodoList> SurveyToDoList = new List<ApiGetTodoList>();
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        cmd.CommandText = "GetSurveyToDoList";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.Int) { Value = userId });
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return null;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            ApiGetTodoList result = new ApiGetTodoList
                            {
                                Id = string.IsNullOrEmpty(row["Id"].ToString()) ? 0 : int.Parse(row["Id"].ToString()),
                                Title = row["Title"].ToString(),
                                ScheduleDate = string.IsNullOrEmpty(row["ScheduleDate"].ToString()) ? null : row["ScheduleDate"].ToString(),
                                EndDate = string.IsNullOrEmpty(row["EndDate"].ToString()) ? null : row["EndDate"].ToString(),
                                Priority = string.IsNullOrEmpty(row["Priority"].ToString()) ? false : Boolean.Parse(row["Priority"].ToString()),
                                Status = row["Status"].ToString(),
                                Type = "Survey",
                                ModuleType = row["ModuleType"].ToString()
                            };
                            SurveyToDoList.Add(result);

                        }
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return SurveyToDoList;
        }

        public async Task<string> GetRefName(string type, int refId)
        {
            try
            {
                using (var dbContext = this._customerConnection.GetDbContext())
                {
                    using (var cmd = dbContext.Database.GetDbConnection().CreateCommand())
                    {
                        if (type.ToLower().Equals("survey"))
                        {
                            cmd.CommandText = "GetSurveyNameById";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@SurveyId", SqlDbType.Int) { Value = refId });
                        }
                        if (type.ToLower().Equals("quiz"))
                        {
                            cmd.CommandText = "GetQuizNameById";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@QuizId", SqlDbType.Int) { Value = refId });
                        }
                        if (type.ToLower().Equals("course"))
                        {
                            cmd.CommandText = "GetCourseNameById";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.Add(new SqlParameter("@CourseId", SqlDbType.Int) { Value = refId });
                        }
                        await dbContext.Database.OpenConnectionAsync();
                        DbDataReader reader = await cmd.ExecuteReaderAsync();
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        if (dt.Rows.Count <= 0)
                        {
                            reader.Dispose();
                            return null;
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            var Title = row["Title"].ToString();
                            return Title;
                        }
                        reader.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error(Utilities.GetDetailedException(ex));
                throw ex;
            }
            return null;
        }
        public async Task<object> GetById(int Id)
        {
            var Query = _db.ToDoPriorityList.Where(r => r.IsDeleted == Record.NotDeleted && r.Id == Id);

            ToDoPriorityList ToDoList = await Query.FirstOrDefaultAsync();
            object todoobj = new
            {
                ToDoList.Id,
                ToDoList.EndDate,
                ToDoList.CreatedDate,
                ToDoList.Priority,
                ToDoList.RefId,
                ToDoList.ScheduleDate,
                ToDoList.Type,
                RefName = await GetRefName(ToDoList.Type, ToDoList.RefId)
            };
            return todoobj;
        }
    }
}

