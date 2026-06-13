using IceCoffee.Cron;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using System.Threading.Tasks;

namespace SdtdServerKit.Functions
{
    public class TaskSchedule : FunctionBase<TaskScheduleSettings>
    {
        private readonly CronDaemon _cronDaemon = new CronDaemon();
        private readonly ITaskScheduleRepository _taskScheduleRepository;
        private readonly ICommandListRepository _commandListRepository;

        /// <summary>
        /// Cron daemon
        /// </summary>
        public CronDaemon CronDaemon => _cronDaemon;

        public TaskSchedule(ITaskScheduleRepository taskScheduleRepository, ICommandListRepository commandListRepository)
        {
            _taskScheduleRepository = taskScheduleRepository;
            _commandListRepository = commandListRepository;
        }

        protected override void OnEnableFunction()
        {
            var taskSchedules = _taskScheduleRepository.GetAll();

            foreach (var item in taskSchedules)
            {
                if (item.IsEnabled)
                {
                    AddTaskSchedule(item);
                }
            }

            _cronDaemon.Start();
        }

        protected override void OnDisableFunction()
        {
            _cronDaemon.Stop();
        }

        public void AddTaskSchedule(T_TaskSchedule taskSchedule)
        {
            var ronJob = new CronJob(taskSchedule.Id.ToString(), taskSchedule.CronExpression, async () =>
            {
                var commandList = await _commandListRepository.GetListByTaskScheduleIdAsync(taskSchedule.Id);
                foreach (var item in commandList)
                {
                    foreach (var cmd in item.Command.Split('\n'))
                    {
                        Utilities.Utils.ExecuteConsoleCommand(cmd, item.InMainThread);
                    }
                    await Task.Delay(20);
                }

                taskSchedule.LastRunAt = DateTime.Now;
                await _taskScheduleRepository.UpdateAsync(taskSchedule);
            });
            _cronDaemon.AddJob(ronJob);
        }

        public void RemoveTaskSchedule(int id)
        {
            _cronDaemon.RemoveJob(id.ToString());
        }

        public bool IsTaskScheduleRunning(int id)
        {
            if (_cronDaemon.CronJobs.TryGetValue(id.ToString(), out var job) && job.IsRunning)
            {
                return true;
            }

            return false;
        }
    }
}
