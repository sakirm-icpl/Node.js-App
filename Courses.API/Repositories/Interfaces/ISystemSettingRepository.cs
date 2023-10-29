using System.Threading.Tasks;

namespace Courses.API.Repositories.Interfaces
{
    public interface ISystemSettingRepository
    {
        Task<int> GetScormFileMaxSizeInMb();
    }
}
