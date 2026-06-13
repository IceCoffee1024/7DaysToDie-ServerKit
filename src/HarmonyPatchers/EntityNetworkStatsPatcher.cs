using HarmonyLib;
using SdtdServerKit.Hooks;
using SdtdServerKit.Managers;

namespace SdtdServerKit.HarmonyPatchers
{
    /// <summary>
    /// 升级事件
    /// </remarks>
    [HarmonyPatch(typeof(EntityAlive.EntityNetworkStats), "ToEntity")]
    public class EntityNetworkStatsPatcher
    {
        [HarmonyPrefix]
        public static void ToEntity_Prefix(EntityAlive _entity, out int __state)
        {
            __state = -1;
            try
            {
                if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                {
                    return;
                }

                if (_entity is EntityPlayer player && player.Progression != null)
                {
                    __state = player.Progression.Level;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "EntityNetworkStatsPatcher.ToEntity_Prefix 执行错误");
            }
        }

        /// <summary>
        /// 触发升级事件
        /// </summary>
        [HarmonyPostfix]
        public static void ToEntity_Postfix(EntityAlive _entity, int __state)
        {
            try
            {
                if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                {
                    return;
                }

                if (__state < 0)
                {
                    return;
                }

                if (!(_entity is EntityPlayer player) || player.Progression == null)
                {
                    return;
                }

                int currentLevel = player.Progression.Level;

                if (currentLevel > __state)
                {
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
                                CustomLogger.Error(ex, "触发玩家升级事件时发生错误（网络同步）");
                            }
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "EntityNetworkStatsPatcher.ToEntity_Postfix 执行错误");
            }
        }
    }
}
