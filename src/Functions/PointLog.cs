using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using System;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 积分日志功能
    /// </summary>
    public class PointLog : FunctionBase<PointLogSettings>
    {
        private const int CleanupIntervalSeconds = 3600;

        private readonly IPointLogRepository _repository;
        private readonly SubTimer _cleanupTimer;


        public IPointLogRepository Repository => _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PointLog(IPointLogRepository repository)
        {
            _repository = repository;
            _cleanupTimer = new SubTimer(OnCleanupTimerElapsed) { Interval = CleanupIntervalSeconds };
        }

        protected override void OnEnableFunction()
        {
            PointLogger.Start(_repository, Settings);
            GlobalTimer.RegisterSubTimer(_cleanupTimer);
            CustomLogger.Debug("积分日志：功能已启用");
        }

        protected override void OnDisableFunction()
        {
            GlobalTimer.UnregisterSubTimer(_cleanupTimer);
            PointLogger.Stop();
            CustomLogger.Debug("积分日志：功能已禁用");
        }

        protected override void OnSettingsChanged()
        {
            if (IsRunning)
            {
                PointLogger.UpdateSettings(Settings);
            }

            // 仅在保留天数 > 0 时启用清理定时器
            _cleanupTimer.IsEnabled = IsRunning && Settings.RetentionDays > 0;
        }

        /// <summary>
        /// 定时器：删除早于 RetentionDays 之前的日志
        /// </summary>
        private void OnCleanupTimerElapsed()
        {
            if (!IsRunning) return;
            int days = Settings.RetentionDays;
            if (days <= 0) return;

            try
            {
                var threshold = DateTime.Now.AddDays(-days);
                var task = _repository.DeleteOlderThanAsync(threshold);
                task.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        CustomLogger.Warn(t.Exception, "积分日志：自动清理失败");
                    }
                    else if (t.Result > 0)
                    {
                        CustomLogger.Debug("积分日志：自动清理 {0} 天前数据，删除 {1} 条", days, t.Result);
                    }
                });
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "积分日志：自动清理触发失败");
            }
        }
    }
}
