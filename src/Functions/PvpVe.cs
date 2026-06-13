using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using SdtdServerKit.PvpVeAreas;
using System;
using System.Collections.Generic;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// PVP/PVE 混合区域功能
    /// </summary>
    public class PvpVe : FunctionBase<PvpVeSettings>
    {
        private readonly IPvpVeAreaRepository _repository;
        private readonly SubTimer _checkTimer;


        private readonly Dictionary<int, AppliedState> _playerApplied = new Dictionary<int, AppliedState>();

        private readonly object _stateLock = new object();

        /// <summary>
        /// 玩家当前已下发状态
        /// </summary>
        private readonly struct AppliedState
        {
            public readonly int AreaId;
            public readonly string BuffName;
            public AppliedState(int areaId, string buffName)
            {
                AreaId = areaId;
                BuffName = buffName ?? string.Empty;
            }
        }

        /// <summary>
        /// 仓储
        /// </summary>
        public IPvpVeAreaRepository Repository => _repository;

        /// <summary>
        /// 构造函数
        /// </summary>
        public PvpVe(IPvpVeAreaRepository repository)
        {
            _repository = repository;
            _checkTimer = new SubTimer(OnCheckTimerElapsed) { Interval = 1 };
        }

        protected override void OnEnableFunction()
        {
            if (!PvpVeManager.IsInitialized)
            {
                PvpVeManager.Initialize(_repository);
            }

            GlobalTimer.RegisterSubTimer(_checkTimer);
            ModEventHub.PlayerDisconnected += OnPlayerDisconnected;
            ModEventHub.PlayerSpawnedInWorld += OnPlayerSpawnedInWorld;
            CustomLogger.Debug($"PVP/PVE 混合区域：功能已启用（当前 {PvpVeManager.Count} 个自定义区域）");
        }

        protected override void OnDisableFunction()
        {
            GlobalTimer.UnregisterSubTimer(_checkTimer);
            ModEventHub.PlayerDisconnected -= OnPlayerDisconnected;
            ModEventHub.PlayerSpawnedInWorld -= OnPlayerSpawnedInWorld;

            ResetAllPlayersToServerDefaults();

            if (PvpVeManager.IsInitialized)
            {
                PvpVeManager.Shutdown();
            }
            CustomLogger.Debug("PVP/PVE 混合区域：功能已禁用");
        }

        protected override void OnSettingsChanged()
        {
            _checkTimer.IsEnabled = IsRunning;
            if (IsRunning)
            {
                ForceRefresh();
            }
        }

        private void OnPlayerDisconnected(ManagedPlayer player)
        {
            lock (_stateLock)
            {
                _playerApplied.Remove(player.EntityId);
            }
        }


        private void OnPlayerSpawnedInWorld(SpawnedPlayer player)
        {
            lock (_stateLock)
            {
                _playerApplied.Remove(player.EntityId);
            }
        }

        private void OnCheckTimerElapsed()
        {
            try
            {
                var players = LivePlayerManager.GetAll();
                foreach (var player in players)
                {
                    try
                    {
                        ApplyAreaRulesToPlayer(player);
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Error(ex, $"PVP/PVE 混合区域：处理玩家 {player.PlayerName}({player.EntityId}) 时发生错误");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "PVP/PVE 混合区域：定时检查时发生错误");
            }
        }

        private void ApplyAreaRulesToPlayer(ManagedPlayer player)
        {
            var entityPlayer = player.EntityPlayer;
            if (entityPlayer == null) return;

            var pos = entityPlayer.GetPosition();
            if (float.IsNaN(pos.x) || float.IsNaN(pos.z)
                || float.IsInfinity(pos.x) || float.IsInfinity(pos.z))
            {
                return;
            }

            int x = (int)Math.Floor(pos.x);
            int z = (int)Math.Floor(pos.z);

            // 1. 找出玩家当前所在的自定义区域；找不到则使用"默认区域"（Id=0）
            var customArea = PvpVeManager.FindArea(x, z);
            int targetAreaId = customArea?.Id ?? 0;

            // 2. 决定本次规则
            int killMode, dropOnDeath, landClaimOnline, landClaimOffline;
            string buffName;
            if (customArea != null)
            {
                killMode = customArea.KillMode;
                dropOnDeath = customArea.DropOnDeath;
                landClaimOnline = customArea.LandClaimOnline;
                landClaimOffline = customArea.LandClaimOffline;
                buffName = customArea.BuffName ?? string.Empty;
            }
            else
            {
                var s = Settings;
                killMode = s.DefaultKillMode;
                dropOnDeath = s.DefaultDropOnDeath;
                landClaimOnline = s.DefaultLandClaimOnline;
                landClaimOffline = s.DefaultLandClaimOffline;
                buffName = s.DefaultBuffName ?? string.Empty;
            }

            string previousBuff;
            bool needSend;
            lock (_stateLock)
            {
                if (_playerApplied.TryGetValue(player.EntityId, out var prev))
                {
                    needSend = prev.AreaId != targetAreaId;
                    previousBuff = prev.BuffName;
                }
                else
                {
                    needSend = true;
                    previousBuff = string.Empty;
                }

                if (needSend)
                {
                    _playerApplied[player.EntityId] = new AppliedState(targetAreaId, buffName);
                }
            }

            if (!needSend) return;

            SendAreaRulesToClient(player.EntityId, killMode, dropOnDeath, landClaimOnline, landClaimOffline, buffName, previousBuff);
        }

        /// <summary>
        /// 向指定客户端下发命令
        /// </summary>
        private static void SendAreaRulesToClient(int entityId, int killMode, int dropOnDeath,
            int landClaimOnline, int landClaimOffline, string buffName, string previousBuff)
        {
            try
            {
                var clientInfo = ConnectionManager.Instance?.Clients?.ForEntityId(entityId);
                if (clientInfo == null) return;

                clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                    .Setup($"sgs PlayerKillingMode {killMode}", true));
                clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                    .Setup($"sgs DropOnDeath {dropOnDeath}", true));
                clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                    .Setup($"sgs LandClaimOnlineDurabilityModifier {landClaimOnline}", true));
                clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                    .Setup($"sgs LandClaimOfflineDurabilityModifier {landClaimOffline}", true));

                if (!string.IsNullOrEmpty(previousBuff)
                    && !string.Equals(previousBuff, buffName, StringComparison.Ordinal))
                {
                    clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                        .Setup($"debuff {previousBuff}", true));
                }

                if (!string.IsNullOrEmpty(buffName))
                {
                    clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                        .Setup($"buff {buffName}", true));
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, $"PVP/PVE 混合区域：向客户端 {entityId} 下发命令失败");
            }
        }

        /// <summary>
        /// 关闭功能 / 区域为空时：向所有在线玩家下发服务器原始 GameStats，并移除区域提示 Buff
        /// </summary>
        private void ResetAllPlayersToServerDefaults()
        {
            try
            {
                int killMode = GameStats.GetInt(EnumGameStats.PlayerKillingMode);
                int dropOnDeath = GameStats.GetInt(EnumGameStats.DropOnDeath);
                int onlineMod = GameStats.GetInt(EnumGameStats.LandClaimOnlineDurabilityModifier);
                int offlineMod = GameStats.GetInt(EnumGameStats.LandClaimOfflineDurabilityModifier);

                var buffsToClear = new HashSet<string>();
                if (!string.IsNullOrEmpty(Settings.DefaultBuffName))
                {
                    buffsToClear.Add(Settings.DefaultBuffName);
                }
                foreach (var area in PvpVeManager.GetAll())
                {
                    if (!string.IsNullOrEmpty(area.BuffName))
                    {
                        buffsToClear.Add(area.BuffName);
                    }
                }

                var players = LivePlayerManager.GetAll();
                foreach (var player in players)
                {
                    try
                    {
                        var clientInfo = ConnectionManager.Instance?.Clients?.ForEntityId(player.EntityId);
                        if (clientInfo == null) continue;

                        clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                            .Setup($"sgs PlayerKillingMode {killMode}", true));
                        clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                            .Setup($"sgs DropOnDeath {dropOnDeath}", true));
                        clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                            .Setup($"sgs LandClaimOnlineDurabilityModifier {onlineMod}", true));
                        clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                            .Setup($"sgs LandClaimOfflineDurabilityModifier {offlineMod}", true));

                        foreach (var buff in buffsToClear)
                        {
                            clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConsoleCmdClient>()
                                .Setup($"debuff {buff}", true));
                        }
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Error(ex, $"PVP/PVE 混合区域：恢复玩家 {player.PlayerName}({player.EntityId}) 默认规则失败");
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "PVP/PVE 混合区域：恢复所有玩家默认规则失败");
            }

            lock (_stateLock)
            {
                _playerApplied.Clear();
            }
        }

        /// <summary>
        /// 区域配置变更（增删改）后调用，强制重新检测每个玩家所在区域并重新下发
        /// </summary>
        public void ForceRefresh()
        {
            lock (_stateLock)
            {
                _playerApplied.Clear();
            }
        }
    }
}
