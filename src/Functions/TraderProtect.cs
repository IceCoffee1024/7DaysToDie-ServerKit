using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using SdtdServerKit.TraderProtectAreas;
using System;
using System.Collections.Generic;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 自定义商人保护区域功能
    /// </summary>
    public class TraderProtect : FunctionBase<TraderProtectSettings>
    {
        private readonly ITraderProtectAreaRepository _repository;
        private readonly SubTimer _buffCheckTimer;

        /// <summary>
        /// 当前在保护区内的玩家（entityId）
        /// </summary>
        private readonly HashSet<int> _playersInArea = new HashSet<int>();
        private readonly object _buffLock = new object();

        /// <summary>
        /// 仓储
        /// </summary>
        public ITraderProtectAreaRepository Repository => _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public TraderProtect(ITraderProtectAreaRepository repository)
        {
            _repository = repository;
            _buffCheckTimer = new SubTimer(OnBuffCheckTimerElapsed) { Interval = 1 };
        }

        protected override void OnEnableFunction()
        {
            if (!TraderProtectManager.IsInitialized)
            {
                TraderProtectManager.Initialize(_repository);
            }

            // 注册 Buff 检查定时器
            GlobalTimer.RegisterSubTimer(_buffCheckTimer);
            ModEventHub.PlayerDisconnected += OnPlayerDisconnected;

            CustomLogger.Debug($"商人保护区域：功能已启用（当前 {TraderProtectManager.Count} 个保护区域）");
        }

        protected override void OnDisableFunction()
        {
            GlobalTimer.UnregisterSubTimer(_buffCheckTimer);
            ModEventHub.PlayerDisconnected -= OnPlayerDisconnected;

            ClearAllPlayerBuffs();

            if (TraderProtectManager.IsInitialized)
            {
                TraderProtectManager.Shutdown();
            }
            CustomLogger.Debug("商人保护区域：功能已禁用");
        }

        protected override void OnSettingsChanged()
        {
            _buffCheckTimer.IsEnabled = IsRunning
                && Settings.IsAreaBuffEnabled
                && !string.IsNullOrEmpty(Settings.AreaBuffName);

            if (!_buffCheckTimer.IsEnabled)
            {
                ClearAllPlayerBuffs();
            }
        }

        private void OnPlayerDisconnected(ManagedPlayer player)
        {
            lock (_buffLock)
            {
                _playersInArea.Remove(player.EntityId);
            }
        }

        /// <summary>
        /// 定时检查玩家是否在保护区内，添加/移除 Buff
        /// </summary>
        private void OnBuffCheckTimerElapsed()
        {
            try
            {
                if (!Settings.IsAreaBuffEnabled || string.IsNullOrEmpty(Settings.AreaBuffName))
                {
                    return;
                }

                if (TraderProtectManager.Count == 0)
                {
                    ClearAllPlayerBuffs();
                    return;
                }

                var players = LivePlayerManager.GetAll();
                var currentlyInArea = new HashSet<int>();

                foreach (var player in players)
                {
                    try
                    {
                        var pos = player.EntityPlayer.GetPosition();
                        int x = (int)Math.Floor(pos.x);
                        int z = (int)Math.Floor(pos.z);

                        if (TraderProtectManager.IsWithinProtectArea(x, z))
                        {
                            currentlyInArea.Add(player.EntityId);
                        }
                    }
                    catch
                    {
                    }
                }

                lock (_buffLock)
                {
                    string buffName = Settings.AreaBuffName;

                    foreach (int entityId in currentlyInArea)
                    {
                        if (!_playersInArea.Contains(entityId))
                        {
                            ApplyBuffToPlayer(entityId, buffName);
                        }
                    }

                    foreach (int entityId in _playersInArea)
                    {
                        if (!currentlyInArea.Contains(entityId))
                        {
                            RemoveBuffFromPlayer(entityId, buffName);
                        }
                    }

                    _playersInArea.Clear();
                    foreach (int id in currentlyInArea)
                    {
                        _playersInArea.Add(id);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "商人保护区域：Buff 检查时发生错误");
            }
        }

        /// <summary>
        /// 给玩家添加 Buff
        /// </summary>
        private static void ApplyBuffToPlayer(int entityId, string buffName)
        {
            try
            {
                string command = $"buffplayer {entityId} {buffName}";
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(command, null);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, $"商人保护区域：给玩家 {entityId} 添加 Buff 失败");
            }
        }

        /// <summary>
        /// 移除玩家的 Buff
        /// </summary>
        private static void RemoveBuffFromPlayer(int entityId, string buffName)
        {
            try
            {
                string command = $"debuffplayer {entityId} {buffName}";
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(command, null);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, $"商人保护区域：移除玩家 {entityId} 的 Buff 失败");
            }
        }

        /// <summary>
        /// 清除所有在区域内玩家的 Buff
        /// </summary>
        private void ClearAllPlayerBuffs()
        {
            string buffName = Settings.AreaBuffName;
            lock (_buffLock)
            {
                if (!string.IsNullOrEmpty(buffName))
                {
                    foreach (int entityId in _playersInArea)
                    {
                        RemoveBuffFromPlayer(entityId, buffName);
                    }
                }
                _playersInArea.Clear();
            }
        }
    }
}
