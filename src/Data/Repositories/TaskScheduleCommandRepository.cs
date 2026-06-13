using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Data.Repositories
{
    /// <summary>
    /// Repository that handles task schedule commands.
    /// </summary>
    public class TaskScheduleCommandRepository : DefaultRepository<T_TaskScheduleCommand>, ITaskScheduleCommandRepository
    {
        /// <inheritdoc/>
        public Task<int> DeleteByTaskScheduleIdAsync(int taskScheduleId)
        {
            return base.DeleteAsync("TaskScheduleId=@TaskScheduleId", param: new { TaskScheduleId = taskScheduleId });
        }
    }
}