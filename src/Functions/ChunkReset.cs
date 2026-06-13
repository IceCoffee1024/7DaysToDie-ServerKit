using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HarmonyLib;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.HarmonyPatchers;
using SdtdServerKit.Managers;
using UnityEngine;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 定时区域重置功能
    /// </summary>
    public class ChunkReset : FunctionBase<ChunkResetSettings>
    {
        private readonly SubTimer _timer;
        private readonly SubTimer _buffCheckTimer;
        private readonly SubTimer _persistTimer;
        private readonly IChunkResetAreaRepository _repository;
        private List<T_ChunkResetArea> _areas = new List<T_ChunkResetArea>();
        private DateTime _lastResetTime = DateTime.Now;
        private readonly object _lock = new object();

        /// <summary>
        /// 记录当前在重置区域内的玩家（entityId），用于检测进出
        /// </summary>
        private readonly HashSet<int> _playersInResetArea = new HashSet<int>();
        private readonly object _buffLock = new object();

        /// <summary>
        /// 上次重置时间（这里指上次刷新重置基准时间的时刻）
        /// </summary>
        public DateTime LastResetTime => _lastResetTime;

        /// <summary>
        /// 下次重置时间
        /// </summary>
        public DateTime NextResetTime => _lastResetTime.AddSeconds(Settings.Interval);

        public ChunkReset(IChunkResetAreaRepository repository)
        {
            _repository = repository;
            _timer = new SubTimer(OnTimerElapsed);
            _buffCheckTimer = new SubTimer(OnBuffCheckTimerElapsed) { Interval = 1 };
            _persistTimer = new SubTimer(OnPersistTimerElapsed) { Interval = 30 };
            LoadAreas();
        }

        protected override void OnDisableFunction()
        {
            GlobalTimer.UnregisterSubTimer(_timer);
            GlobalTimer.UnregisterSubTimer(_buffCheckTimer);
            GlobalTimer.UnregisterSubTimer(_persistTimer);
            ModEventHub.PlayerDisconnected -= OnPlayerDisconnected;
            ModEventHub.GameStartDone -= OnGameStartDone;

            ChunkRegenManager.DisableAreaReset();

            ClearAllPlayerBuffs();
        }

        protected override void OnEnableFunction()
        {
            GlobalTimer.RegisterSubTimer(_timer);
            GlobalTimer.RegisterSubTimer(_buffCheckTimer);
            GlobalTimer.RegisterSubTimer(_persistTimer);
            ModEventHub.PlayerDisconnected += OnPlayerDisconnected;
            ModEventHub.GameStartDone += OnGameStartDone;

            ChunkRegenManager.EnableAreaReset(GetAreas());

            TryInitPoiProtectionZone();
        }

        /// <summary>
        /// 游戏启动完成时初始化 POI 防护区域
        /// </summary>
        private void OnGameStartDone()
        {
            if (IsRunning && !ChunkRegenManager.ThreadTakenOver)
            {
                CustomLogger.Debug("区域重置：检测到区块生成线程尚未接管，GameStartDone 时重试");
                ChunkRegenManager.EnableAreaReset(GetAreas());
            }

            TryInitPoiProtectionZone();
        }

        /// <summary>
        /// 仅当启用了 POI 禁放功能、且游戏世界已就绪时才初始化防护区域
        /// </summary>
        private void TryInitPoiProtectionZone()
        {
            try
            {
                if (Settings == null || (!Settings.IsPoiLandClaimBanEnabled && !Settings.IsPoiBedrollBanEnabled))
                {
                    return;
                }

                if (PoiProtectionZone.Initialized)
                {
                    return;
                }

                // 游戏世界尚未就绪时跳过，等待下次时机（GameStartDone 事件或下次配置变更）
                if (GameManager.Instance == null || GameManager.Instance.World == null)
                {
                    return;
                }

                PoiProtectionZone.Initialize();
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区域重置：初始化系统房防护区域失败");
            }
        }

        protected override void OnSettingsChanged()
        {
            _timer.Interval = Settings.Interval;
            _timer.IsEnabled = Settings.IsEnabled;
            _buffCheckTimer.IsEnabled = Settings.IsEnabled && Settings.IsAreaBuffEnabled && !string.IsNullOrEmpty(Settings.AreaBuffName);
            _persistTimer.IsEnabled = Settings.IsEnabled;

            ApplyBanPatch();

            if (Settings.IsEnabled)
            {
                TryInitPoiProtectionZone();
            }

            bool buffCanRun = Settings.IsEnabled
                && Settings.IsAreaBuffEnabled
                && !string.IsNullOrEmpty(Settings.AreaBuffName)
                && (Settings.IsLandClaimBanEnabled || Settings.IsBedrollBanEnabled
                    || Settings.IsPoiLandClaimBanEnabled || Settings.IsPoiBedrollBanEnabled);
            if (!buffCanRun)
            {
                ClearAllPlayerBuffs();
            }
        }

        /// <summary>
        /// 根据当前设置安装/卸载禁放补丁
        /// </summary>
        private void ApplyBanPatch()
        {
            try
            {
                var original = AccessTools.Method(typeof(GameManager), nameof(GameManager.ChangeBlocks));
                var patch = AccessTools.Method(typeof(GameManagerPatcher), nameof(GameManagerPatcher.Before_ChangeBlocks_ChunkResetBan));

                bool shouldEnable = Settings.IsEnabled && (
                    Settings.IsLandClaimBanEnabled
                    || Settings.IsBedrollBanEnabled
                    || Settings.IsPoiLandClaimBanEnabled
                    || Settings.IsPoiBedrollBanEnabled);
                if (shouldEnable)
                {
                    ModApi.Harmony.Patch(original, prefix: new HarmonyMethod(patch));
                }
                else
                {
                    ModApi.Harmony.Unpatch(original, patch);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区域重置：安装/卸载禁放补丁失败");
            }
        }

        /// <summary>
        /// 判断给定坐标是否落在任意重置区域内（仅按 X/Z 检查）
        /// </summary>
        public bool IsInResetArea(int x, int z)
        {
            lock (_lock)
            {
                foreach (var area in _areas)
                {
                    if (x >= area.MinX && x <= area.MaxX && z >= area.MinZ && z <= area.MaxZ)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void OnPlayerDisconnected(ManagedPlayer player)
        {
            lock (_buffLock)
            {
                _playersInResetArea.Remove(player.EntityId);
            }
        }

        /// <summary>
        /// 加载所有重置区域
        /// </summary>
        private void LoadAreas()
        {
            try
            {
                var areas = _repository.GetAllAsync().Result;
                lock (_lock)
                {
                    _areas = areas.ToList();
                }
                CustomLogger.Debug($"区域重置：已加载 {_areas.Count} 个重置区域");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区域重置：加载重置区域失败");
            }
        }

        /// <summary>
        /// 获取所有重置区域
        /// </summary>
        public List<T_ChunkResetArea> GetAreas()
        {
            lock (_lock)
            {
                return new List<T_ChunkResetArea>(_areas);
            }
        }

        /// <summary>
        /// 添加重置区域
        /// </summary>
        public async Task<T_ChunkResetArea> AddAreaAsync(int x1, int z1, int x2, int z2)
        {
            // Chunk对齐
            AlignToChunkBoundary(ref x1, ref z1, ref x2, ref z2);

            var entity = new T_ChunkResetArea()
            {
                CreatedAt = DateTime.Now,
                MinX = Math.Min(x1, x2),
                MinZ = Math.Min(z1, z2),
                MaxX = Math.Max(x1, x2),
                MaxZ = Math.Max(z1, z2)
            };

            await _repository.InsertAsync(entity);

            // 重新加载以获取自增Id
            LoadAreas();

            // 同步按需重生引擎的 chunk 集合
            if (IsRunning)
            {
                ChunkRegenManager.RebuildAreaStates(GetAreas());
            }

            CustomLogger.Debug($"区域重置：添加重置区域 ({entity.MinX},{entity.MinZ}) - ({entity.MaxX},{entity.MaxZ})");
            return entity;
        }

        /// <summary>
        /// 删除重置区域
        /// </summary>
        public async Task<bool> RemoveAreaAsync(int id)
        {
            int affected = await _repository.DeleteByIdAsync(id);
            if (affected > 0)
            {
                LoadAreas();

                // 同步按需重生引擎的 chunk 集合
                if (IsRunning)
                {
                    ChunkRegenManager.RebuildAreaStates(GetAreas());
                }

                CustomLogger.Debug($"区域重置：删除重置区域 Id={id}");
                return true;
            }
            return false;
        }

        /// <summary>
        /// 清空所有重置区域
        /// </summary>
        public async Task ClearAllAreasAsync()
        {
            await _repository.DeleteAllAsync();
            lock (_lock)
            {
                _areas.Clear();
            }

            // 同步按需重生引擎的 chunk 集合（清空）
            if (IsRunning)
            {
                ChunkRegenManager.RebuildAreaStates(GetAreas());
            }

            CustomLogger.Debug("区域重置：已清空所有重置区域");
        }

        /// <summary>
        /// 立即重置所有区域（把所有 chunk 标记为待重生，玩家靠近时重新生成）
        /// </summary>
        public void ResetAllNow()
        {
            ExecuteReset(immediate: true);
        }

        /// <summary>
        /// 重设下次重置时间（从现在开始计时）
        /// </summary>
        public void ResetNextTime()
        {
            _lastResetTime = DateTime.Now;
            _timer.IsEnabled = false;
            _timer.IsEnabled = Settings.IsEnabled;
            CustomLogger.Debug("区域重置：已重设下次重置时间");
        }

        /// <summary>
        /// 定时器回调
        /// </summary>
        private void OnTimerElapsed()
        {
            ExecuteReset(immediate: false);
        }

        /// <summary>
        /// 状态节流落盘定时器回调
        /// </summary>
        private void OnPersistTimerElapsed()
        {
            ChunkRegenManager.FlushIfDirty();
        }

        /// <summary>
        /// 执行区域重置
        /// </summary>
        private void ExecuteReset(bool immediate)
        {
            try
            {
                List<T_ChunkResetArea> areas;
                lock (_lock)
                {
                    areas = new List<T_ChunkResetArea>(_areas);
                }

                if (areas.Count == 0)
                {
                    CustomLogger.Debug("区域重置：没有配置重置区域，跳过");
                    return;
                }

                if (!string.IsNullOrEmpty(Settings.ResetNoticeTip))
                {
                    SendGlobalMessage(Settings.ResetNoticeTip);
                }

                if (Settings.RemoveEnemiesBeforeReset)
                {
                    foreach (var area in areas)
                    {
                        try
                        {
                            ChunkHelper.RemoveEntityInArea(area.MinX, area.MinZ, area.MaxX, area.MaxZ);
                        }
                        catch (Exception ex)
                        {
                            CustomLogger.Error(ex, $"区域重置：清除区域 ({area.MinX},{area.MinZ})-({area.MaxX},{area.MaxZ}) 内敌对实体失败");
                        }
                    }
                }

                if (immediate)
                {
                    ChunkRegenManager.ResetAllAreaChunksImmediately();
                }
                else
                {
                    ChunkRegenManager.BumpBaseline();
                }

                _lastResetTime = DateTime.Now;
                ChunkRegenManager.Save();

                CustomLogger.Debug($"区域重置：完成，{(immediate ? "已将" : "按周期刷新基准时间，")}" +
                    $"{ChunkRegenManager.TrackedChunkCount} 个 chunk 标记为待重生，" +
                    $"玩家靠近时将重新生成全新地形。");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区域重置：执行重置时发生错误");
            }
        }

        /// <summary>
        /// 将坐标对齐到chunk边界（16x16）
        /// </summary>
        public static void AlignToChunkBoundary(ref int x1, ref int z1, ref int x2, ref int z2)
        {
            int xMin = Math.Min(x1, x2);
            int xMax = Math.Max(x1, x2);
            int zMin = Math.Min(z1, z2);
            int zMax = Math.Max(z1, z2);

            xMin = (xMin >> 4) << 4;
            zMin = (zMin >> 4) << 4;
            xMax = ((xMax >> 4) << 4) + 15;
            zMax = ((zMax >> 4) << 4) + 15;

            x1 = xMin;
            z1 = zMin;
            x2 = xMax;
            z2 = zMax;
        }

        #region 区域Buff提示

        /// <summary>
        /// 定时检查玩家是否在重置区域内，添加/移除Buff
        /// </summary>
        private void OnBuffCheckTimerElapsed()
        {
            try
            {
                if (!Settings.IsAreaBuffEnabled || string.IsNullOrEmpty(Settings.AreaBuffName))
                {
                    return;
                }

                bool resetBanEnabled = Settings.IsLandClaimBanEnabled || Settings.IsBedrollBanEnabled;
                bool poiBanEnabled = Settings.IsPoiLandClaimBanEnabled || Settings.IsPoiBedrollBanEnabled;

                if (!resetBanEnabled && !poiBanEnabled)
                {
                    return;
                }

                List<T_ChunkResetArea>? areas = null;
                if (resetBanEnabled)
                {
                    lock (_lock)
                    {
                        areas = new List<T_ChunkResetArea>(_areas);
                    }
                }

                var players = LivePlayerManager.GetAll();
                var currentlyInArea = new HashSet<int>();

                foreach (var player in players)
                {
                    try
                    {
                        var pos = player.EntityPlayer.GetPosition();
                        bool inArea = false;

                        // 重置区域检测：仅在"重置区禁放"任一开关开启时生效
                        if (resetBanEnabled && areas != null && areas.Count > 0)
                        {
                            foreach (var area in areas)
                            {
                                if (pos.x >= area.MinX && pos.x <= area.MaxX
                                    && pos.z >= area.MinZ && pos.z <= area.MaxZ)
                                {
                                    inArea = true;
                                    break;
                                }
                            }
                        }

                        // 系统房 POI 区域检测：仅在"系统房禁放"任一开关开启时生效
                        if (!inArea && poiBanEnabled)
                        {
                            var blockPos = new Vector3i(
                                Mathf.FloorToInt(pos.x),
                                Mathf.FloorToInt(pos.y),
                                Mathf.FloorToInt(pos.z));

                            if (Settings.IsPoiLandClaimBanEnabled && PoiProtectionZone.IsInLandClaimZone(blockPos))
                            {
                                inArea = true;
                            }
                            else if (Settings.IsPoiBedrollBanEnabled && PoiProtectionZone.IsInBedZone(blockPos))
                            {
                                inArea = true;
                            }
                        }

                        if (inArea)
                        {
                            currentlyInArea.Add(player.EntityId);
                        }
                    }
                    catch
                    {
                        // 忽略获取位置失败
                    }
                }

                lock (_buffLock)
                {
                    // 新进入区域的玩家 -> 添加Buff
                    foreach (int entityId in currentlyInArea)
                    {
                        if (!_playersInResetArea.Contains(entityId))
                        {
                            ApplyBuffToPlayer(entityId, Settings.AreaBuffName);
                        }
                    }

                    // 离开区域的玩家 -> 移除Buff
                    foreach (int entityId in _playersInResetArea)
                    {
                        if (!currentlyInArea.Contains(entityId))
                        {
                            RemoveBuffFromPlayer(entityId, Settings.AreaBuffName);
                        }
                    }

                    _playersInResetArea.Clear();
                    foreach (int id in currentlyInArea)
                    {
                        _playersInResetArea.Add(id);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区域重置：Buff检查时发生错误");
            }
        }

        /// <summary>
        /// 给玩家添加Buff
        /// </summary>
        private void ApplyBuffToPlayer(int entityId, string buffName)
        {
            try
            {
                string command = $"buffplayer {entityId} {buffName}";
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(command, null);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, $"区域重置：给玩家 {entityId} 添加Buff失败");
            }
        }

        /// <summary>
        /// 移除玩家的Buff
        /// </summary>
        private void RemoveBuffFromPlayer(int entityId, string buffName)
        {
            try
            {
                string command = $"debuffplayer {entityId} {buffName}";
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(command, null);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, $"区域重置：移除玩家 {entityId} 的Buff失败");
            }
        }

        /// <summary>
        /// 清除所有在区域内玩家的Buff（功能关闭时调用）
        /// </summary>
        private void ClearAllPlayerBuffs()
        {
            if (string.IsNullOrEmpty(Settings.AreaBuffName))
            {
                return;
            }

            lock (_buffLock)
            {
                foreach (int entityId in _playersInResetArea)
                {
                    RemoveBuffFromPlayer(entityId, Settings.AreaBuffName);
                }
                _playersInResetArea.Clear();
            }
        }

        #endregion
    }
}
