//using GamePath;
//using HarmonyLib;
//using UAI;

//namespace SdtdServerKit.HarmonyPatchers.PerformanceTuning
//{
//    [HarmonyPatch]
//    internal class EntityAlivePatcher
//    {
//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(EntityAlive), nameof(EntityAlive.ProcessDamageResponse))]
//        public static bool ProcessDamageResponse(EntityAlive __instance, DamageResponse _dmResponse)
//        {
//            __instance.ProcessDamageResponseLocal(_dmResponse);

//            Task.Run(() =>
//            {
//                try
//                {
//                    Entity entity = __instance.world.GetEntity(_dmResponse.Source.getEntityId());
//                    if (entity && !entity.isEntityRemote && __instance.isEntityRemote && __instance is EntityPlayer)
//                    {
//                        __instance.world.entityDistributer.SendPacketToTrackedPlayers(__instance.entityId, __instance.entityId, NetPackageManager.GetPackage<NetPackageDamageEntity>().Setup(__instance.entityId, _dmResponse));
//                        return;
//                    }
//                    __instance.world.entityDistributer.SendPacketToTrackedPlayersAndTrackedEntity(__instance.entityId, _dmResponse.Source.getEntityId(), NetPackageManager.GetPackage<NetPackageDamageEntity>().Setup(__instance.entityId, _dmResponse));
//                }
//                catch (Exception ex)
//                {
//                    CustomLogger.Error(ex, "Error in EntityAlivePatcher.ProcessDamageResponse");
//                }
//            });

//            return false;
//        }
//    }
//}
