//using HarmonyLib;
//using System.Collections;
//using System.Reflection;

//namespace SdtdServerKit.HarmonyPatchers.PerformanceTuning
//{
//    [HarmonyPatch]
//    internal class NetPackagePatcher
//    {
//        //[HarmonyPrefix]
//        //[HarmonyPatch(typeof(NetPackageSetBlock), nameof(NetPackageSetBlock.ProcessPackage))]
//        //public static bool NetPackageSetBlock_ProcessPackage(
//        //    NetPackageSetBlock __instance,
//        //    PlatformUserIdentifierAbs ___persistentPlayerId,
//        //    int ___localPlayerThatChanged,
//        //    List<BlockChangeInfo> ___blockChanges,
//        //    World _world,
//        //    GameManager _callbacks)
//        //{
//        //    Task.Run(() =>
//        //    {
//        //        try
//        //        {
//        //            if (PlatformUserIdentifierAbs.Equals(___persistentPlayerId, __instance.Sender.PlatformId) || PlatformUserIdentifierAbs.Equals(___persistentPlayerId, __instance.Sender.CrossplatformId))
//        //            {
//        //                GameManager.Instance.SetBlocksOnClients(___localPlayerThatChanged, __instance);

//        //                if (GameManager.Instance.World.ChunkClusters[0] == null)
//        //                {
//        //                    return;
//        //                }

//        //                if (DynamicMeshManager.CONTENT_ENABLED)
//        //                {
//        //                    Parallel.ForEach(___blockChanges, (item) =>
//        //                    {
//        //                        DynamicMeshManager.ChunkChanged(item.pos, -1, item.blockValue.type);
//        //                    });
//        //                }

//        //                ModApi.MainThreadSyncContext.Post((state) =>
//        //                {
//        //                    GameManager.Instance.ChangeBlocks(___persistentPlayerId, ___blockChanges);
//        //                }, null);
//        //            }
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            CustomLogger.Error(ex, "Error in NetPackagePatcher.NetPackageSetBlock_ProcessPackage");
//        //        }
//        //    });

//        //    return false;
//        //}

//        //[HarmonyPrefix]
//        //[HarmonyPatch(typeof(NetPackageSetBlockTexture), nameof(NetPackageSetBlockTexture.ProcessPackage))]
//        //public static bool NetPackageSetBlockTexture_ProcessPackage(
//        //    Vector3i ___blockPos,
//        //    BlockFace ___blockFace,
//        //    byte ___idx,
//        //    int ___playerIdThatChanged,
//        //    World _world,
//        //    GameManager _callbacks)
//        //{
//        //    Task.Run(() =>
//        //    {
//        //        try
//        //        {
//        //            if (GameManager.Instance.World.ChunkClusters[0] != null)
//        //            {
//        //                GameManager.Instance.SetBlockTextureClient(___blockPos, ___blockFace, ___idx);
//        //            }
//        //            var netPackageSetBlockTexture = NetPackageManager.GetPackage<NetPackageSetBlockTexture>().Setup(___blockPos, ___blockFace, ___idx, ___playerIdThatChanged);
//        //            ConnectionManager.Instance.SendPackage(netPackageSetBlockTexture, false, -1, ___playerIdThatChanged, -1, -1);
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            CustomLogger.Error(ex, "Error in NetPackagePatcher.NetPackageSetBlockTexture_ProcessPackage");
//        //        }
//        //    });

//        //    return false;
//        //}

//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(NetPackageBlockTrigger), nameof(NetPackageBlockTrigger.ProcessPackage))]
//        public static bool NetPackageBlockTrigger_ProcessPackage(
//            NetPackageBlockTrigger __instance,
//            int ___clrIdx,
//            Vector3i ___blockPos,
//            BlockValue ___blockValue,
//            World _world,
//            GameManager _callbacks)
//        {
//            Task.Run(() =>
//            {
//                try
//                {
//                    if (__instance.Sender.bAttachedToEntity)
//                    {
//                        if (_world.GetEntity(__instance.Sender.entityId) is EntityPlayer entityPlayer)
//                        {
//                            ___blockValue.Block.HandleTrigger(entityPlayer, _world, ___clrIdx, ___blockPos, ___blockValue);
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    CustomLogger.Error(ex, "Error in NetPackagePatcher.NetPackageBlockTrigger_ProcessPackage");
//                }
//            });

//            return false;
//        }
//    }
//}