using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// Represents a task schedule repository.
    /// </summary>
    public class TaskScheduleRepository : DefaultRepository<T_TaskSchedule>, ITaskScheduleRepository
    {
    }
}