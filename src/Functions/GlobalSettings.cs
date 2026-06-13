using HarmonyLib;
using Platform.Steam;
using SdtdServerKit.HarmonyPatchers;
using SdtdServerKit.Managers;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 全局设置
    /// </summary>
    public class GlobalSettings : FunctionBase<FunctionSettings.GlobalSettings>
    {
        private new FunctionSettings.GlobalSettings Settings => ConfigManager.GlobalSettings;

        /// <summary>
        /// 构造函数
        /// </summary>
        public GlobalSettings()
        {
            ModEventHub.EntityKilled += OnEntityKilled;
            ModEventHub.PlayerSpawnedInWorld += OnPlayerSpawnedInWorld;
            ModEventHub.EntitySpawned += OnEntitySpawned;
        }

        private void OnEntitySpawned(EntityInfo entityInfo)
        {
            if (Settings.EnableAutoZombieCleanup)
            {
                int zombies = 0;
                foreach (var entity in GameManager.Instance.World.Entities.list)
                {
                    if (entity.IsAlive())
                    {
                        if (entity is EntityEnemy)
                        {
                            zombies++;
                        }
                    }
                }
                if(zombies > Settings.AutoZombieCleanupThreshold)
                {
                    Utilities.Utils.ExecuteConsoleCommand("ty-RemoveEntity " + entityInfo.EntityId, true);
                    CustomLogger.Debug($"Auto zombie cleanup triggered, the entity: {entityInfo.EntityName} was removed.");
                }
            }
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnSettingsChanged()
        {
            {
                var original = AccessTools.Method(typeof(GameManager), nameof(GameManager.RequestToSpawnPlayer));
                var patch = AccessTools.Method(typeof(GameManagerPatcher), nameof(GameManagerPatcher.Before_RequestToSpawnPlayer));

                if (Settings.EnableXmlsSecondaryOverwrite)
                {
                    ModApi.Harmony.Patch(original, prefix: new HarmonyMethod(patch));
                }
                else
                {
                    ModApi.Harmony.Unpatch(original, patch);
                }
            }

            {
                var original = AccessTools.Method(typeof(World), nameof(World.AddFallingBlock));
                var patch = AccessTools.Method(typeof(WorldPatcher), nameof(WorldPatcher.Before_AddFallingBlock));
                if (Settings.EnableFallingBlockProtection)
                {
                    ModApi.Harmony.Patch(original, prefix: new HarmonyMethod(patch));
                }
                else
                {
                    ModApi.Harmony.Unpatch(original, patch);
                }
            }

            {
                var original = AccessTools.Method(typeof(GameManager), nameof(GameManager.RequestToSpawnPlayer));
                var patch = AccessTools.Method(typeof(GameManagerPatcher), nameof(GameManagerPatcher.Before_RequestToSpawnPlayer_SpawnPoint));

                if (Settings.SpawnPointSettings != null && Settings.SpawnPointSettings.IsEnabled)
                {
                    ModApi.Harmony.Patch(original, prefix: new HarmonyMethod(patch));
                }
                else
                {
                    ModApi.Harmony.Unpatch(original, patch);
                }
            }
        }

        private void BlockFamilySharingAccount(ClientInfo clientInfo)
        {
            if (clientInfo.PlatformId is UserIdentifierSteam userIdentifierSteam
                && userIdentifierSteam.OwnerId.Equals(userIdentifierSteam) == false)
            {
                Utilities.Utils.ExecuteConsoleCommand("kick " + clientInfo.entityId + " \"Family sharing account is not allowed to join the server!\"");
            }
        }

        private void OnPlayerSpawnedInWorld(SpawnedPlayer player)
        {
            if (Settings.BlockFamilySharingAccount)
            {
                if (player.RespawnType == Models.RespawnType.EnterMultiplayer
                    || player.RespawnType == Models.RespawnType.JoinMultiplayer)
                {
                    var clientInfo = ConnectionManager.Instance.Clients.ForEntityId(player.EntityId);
                    BlockFamilySharingAccount(clientInfo);
                }
            }

            if (Settings.DeathTrigger.IsEnabled)
            {
                if (player.RespawnType == Models.RespawnType.Died)
                {
                    var managedPlayer = LivePlayerManager.GetByEntityId(player.EntityId);
                    if (managedPlayer != null)
                    {
                        // 扣除积分（直接调用仓储，避免控制台命令刷屏日志）
                        if (Settings.DeathTrigger.DeathPenaltyPoints != 0)
                        {
                            _ = ApplyDeathPenaltyAsync(managedPlayer, Settings.DeathTrigger.DeathPenaltyPoints);
                        }
                    }
                }
            }
        }

        private void OnEntityKilled(KilledEntity entity)
        {
            if (Settings.KillZombieTrigger.IsEnabled == false)
            {
                return;
            }

            var entityType = entity.DeadEntity.EntityType;
            if (entityType != Models.EntityType.Zombie && entityType != Models.EntityType.Animal)
            {
                return;
            }

            var player = LivePlayerManager.GetByEntityId(entity.KillerEntityId);
            if (player == null)
            {
                return;
            }

            string deadEntityName = entity.DeadEntity.EntityClassName;

            FunctionSettings.ZombieKillRewardEntry matchedReward = null;

            if (Settings.KillZombieTrigger.ZombieRewards != null)
            {
                foreach (var reward in Settings.KillZombieTrigger.ZombieRewards)
                {
                    if (string.Equals(reward.EntityClassName, deadEntityName, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedReward = reward;
                        break;
                    }
                }

                if (matchedReward == null)
                {
                    foreach (var reward in Settings.KillZombieTrigger.ZombieRewards)
                    {
                        if (reward.EntityClassName == "*")
                        {
                            matchedReward = reward;
                            break;
                        }
                    }
                }
            }

            if (matchedReward == null)
            {
                foreach (var command in Settings.KillZombieTrigger.ExecuteCommands)
                {
                    if (string.IsNullOrEmpty(command) == false)
                    {
                        Utilities.Utils.ExecuteConsoleCommand(FormatCmd(command, player), true);
                    }
                }
                return;
            }

            if (matchedReward.RewardPoints != 0)
            {
                _ = ApplyKillRewardAsync(player, matchedReward.RewardPoints);
            }
        }

        /// <summary>
        /// 直接通过仓储调整积分并发送击杀通知，避免每次都走 ty-cpp 控制台命令导致日志刷屏。
        /// </summary>
        private async Task ApplyKillRewardAsync(ManagedPlayer player, int rewardPoints)
        {
            try
            {
                var repository = ModApi.ServiceContainer.Resolve<Data.IRepositories.IPointsInfoRepository>();
                await repository.ChangePointsAsync(player.PlayerId, rewardPoints, allowNegative: true);
                int totalPoints = await repository.GetPointsByIdAsync(player.PlayerId);
                Managers.PointLogger.Log(Models.PointLogCategory.ZombieKill, player.PlayerId, player.PlayerName,
                    rewardPoints, totalPoints, "击杀丧尸奖励");

                if (Settings.KillZombieTrigger.IsEnableKillNotification)
                {
                    string message = $"[FF0000]击杀获得: [00FF00]积分{rewardPoints}[FF0000],当前积分:[00FF00]{totalPoints}";
                    SendMessageToPlayer(player.PlayerId, message);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"僵尸击杀奖励：调整积分失败 {ex.Message}");
            }
        }

        /// <summary>
        /// 直接通过仓储扣减死亡惩罚积分并发送通知，避免控制台命令日志刷屏。
        /// </summary>
        private async Task ApplyDeathPenaltyAsync(ManagedPlayer player, int penaltyPoints)
        {
            try
            {
                var repository = ModApi.ServiceContainer.Resolve<Data.IRepositories.IPointsInfoRepository>();
                await repository.ChangePointsAsync(player.PlayerId, -penaltyPoints, allowNegative: true);
                int totalPoints = await repository.GetPointsByIdAsync(player.PlayerId);
                Managers.PointLogger.Log(Models.PointLogCategory.ZombieKill, player.PlayerId, player.PlayerName,
                    -penaltyPoints, totalPoints, "死亡惩罚");

                if (Settings.DeathTrigger.IsEnableDeathNotification)
                {
                    string message = $"[FF0000]死亡惩罚: 扣除[00FF00]积分{penaltyPoints}[FF0000],当前积分:[00FF00]{totalPoints}";
                    SendMessageToPlayer(player.PlayerId, message);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"死亡惩罚：扣减积分失败 {ex.Message}");
            }
        }
    }
}
