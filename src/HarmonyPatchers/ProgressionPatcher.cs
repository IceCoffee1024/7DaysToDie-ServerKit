using HarmonyLib;
using SdtdServerKit.Hooks;
using SdtdServerKit.Managers;

namespace SdtdServerKit.HarmonyPatchers
{
    /// <summary>
    /// 监听玩家升级事件
    /// </summary>
    [HarmonyPatch(typeof(Progression))]
    public class ProgressionPatcher
    {
        private static readonly Dictionary<int, int> _previousLevels = new Dictionary<int, int>();


        [HarmonyPatch("AddLevelExp")]
        [HarmonyPrefix]
        public static void AddLevelExp_Prefix(Progression __instance)
        {
            try
            {
                if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                {
                    return;
                }

                EntityAlive entityAlive = __instance.parent;
                if (entityAlive == null || !(entityAlive is EntityPlayer player))
                {
                    return;
                }

                _previousLevels[player.entityId] = __instance.Level;
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "ProgressionPatcher.AddLevelExp_Prefix 执行错误");
            }
        }

        /// <summary>
        /// 玩家升级后触发事件
        /// </summary>
        [HarmonyPatch("AddLevelExp")]
        [HarmonyPostfix]
        public static void AddLevelExp_Postfix(Progression __instance)
        {
            try
            {
                if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                {
                    return;
                }

                EntityAlive entityAlive = __instance.parent;
                if (entityAlive == null || !(entityAlive is EntityPlayer player))
                {
                    return;
                }

                // 获取当前等级
                int currentLevel = __instance.Level;

                if (_previousLevels.TryGetValue(player.entityId, out int previousLevel) && currentLevel > previousLevel)
                {
                    _previousLevels.Remove(player.entityId);

                    if (LivePlayerManager.TryGetByEntityId(player.entityId, out var managedPlayer) && managedPlayer != null)
                    {
                        Task.Run(async () =>
                        {
                            try
                            {
                                await PlayerLevelUpHook.OnPlayerLevelUp(managedPlayer, currentLevel);
                            }
                            catch (Exception ex)
                            {
                                CustomLogger.Error(ex, "触发玩家升级事件时发生错误");
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "ProgressionPatcher.AddLevelExp_Postfix 执行错误");
            }
        }
    }
}
