//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SdtdServerKit.HarmonyPatchers.PerformanceTuning
//{
//    [HarmonyPatch(typeof(ConnectionManager))]
//    internal class ConnectionManagerPatcher
//    {
//        //[HarmonyPrefix]
//        //[HarmonyPatch(nameof(ConnectionManager.SendPackage), new Type[] { typeof(NetPackage), typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int) })]
//        //public static bool SendPackage(NetPackage _package, bool _onlyClientsAttachedToAnEntity = false, int _attachedToEntityId = -1, int _allButAttachedToEntityId = -1, int _entitiesInRangeOfEntity = -1, int _range = -1)
//        //{
//        //    _package.RegisterSendQueue();

//        //    Parallel.ForEach<ClientInfo>(ConnectionManager.Instance.Clients.List, (clientInfo) =>
//        //    {
//        //        try
//        //        {
//        //            if (!(!clientInfo.loginDone | (_onlyClientsAttachedToAnEntity && !clientInfo.bAttachedToEntity) | (_attachedToEntityId != -1 && (!clientInfo.bAttachedToEntity || clientInfo.entityId != _attachedToEntityId)) | (_allButAttachedToEntityId != -1 && (!clientInfo.bAttachedToEntity || clientInfo.entityId == _allButAttachedToEntityId)) | (_entitiesInRangeOfEntity != -1 && !GameManager.Instance.World.IsEntityInRange(_entitiesInRangeOfEntity, clientInfo.entityId, _range))))
//        //            {
//        //                clientInfo.netConnection[_package.Channel].AddToSendQueue(_package);
//        //                if (_package.FlushQueue)
//        //                {
//        //                    clientInfo.netConnection[_package.Channel].FlushSendQueue();
//        //                }
//        //            }
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            CustomLogger.Error(ex, "Error in ConnectionManagerPatcher.SendPackage~NetPackage");
//        //        }
//        //    });

//        //    _package.SendQueueHandled();

//        //    return false;
//        //}

//        //[HarmonyPrefix]
//        //[HarmonyPatch(nameof(ConnectionManager.SendPackage), new Type[] { typeof(List<NetPackage>), typeof(bool), typeof(int), typeof(int), typeof(int), typeof(int) })]
//        //public static bool SendPackage(List<NetPackage> _packages, bool _onlyClientsAttachedToAnEntity = false, int _attachedToEntityId = -1, int _allButAttachedToEntityId = -1, int _entitiesInRangeOfEntity = -1, int _range = -1)
//        //{
//        //    int packagesCount = _packages.Count;
//        //    for (int i = 0; i < packagesCount; i++)
//        //    {
//        //        _packages[i].RegisterSendQueue();
//        //    }

//        //    Parallel.ForEach<ClientInfo>(ConnectionManager.Instance.Clients.List, (clientInfo) =>
//        //    {
//        //        try
//        //        {
//        //            if (!clientInfo.loginDone | (_onlyClientsAttachedToAnEntity && !clientInfo.bAttachedToEntity) | (_attachedToEntityId != -1 && (!clientInfo.bAttachedToEntity || clientInfo.entityId != _attachedToEntityId)) | (_allButAttachedToEntityId != -1 && (!clientInfo.bAttachedToEntity || clientInfo.entityId == _allButAttachedToEntityId)) | (_entitiesInRangeOfEntity != -1 && !GameManager.Instance.World.IsEntityInRange(_entitiesInRangeOfEntity, clientInfo.entityId, _range)))
//        //            {
//        //                return;
//        //            }

//        //            bool flag = false;
//        //            bool flag2 = false;
//        //            bool flag3 = false;
//        //            for (int k = 0; k < packagesCount; k++)
//        //            {
//        //                NetPackage netPackage = _packages[k];
//        //                clientInfo.netConnection[netPackage.Channel].AddToSendQueue(netPackage);
//        //                if (netPackage.Channel == 1)
//        //                {
//        //                    flag2 = true;
//        //                }
//        //                else
//        //                {
//        //                    flag = true;
//        //                }

//        //                flag3 |= netPackage.FlushQueue;
//        //            }

//        //            if (flag3)
//        //            {
//        //                if (flag)
//        //                {
//        //                    clientInfo.netConnection[0].FlushSendQueue();
//        //                }

//        //                if (flag2)
//        //                {
//        //                    clientInfo.netConnection[1].FlushSendQueue();
//        //                }
//        //            }
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            CustomLogger.Error(ex, "Error in ConnectionManagerPatcher.SendPackage~List<NetPackage>");
//        //        }
//        //    });

//        //    for (int l = 0; l < packagesCount; l++)
//        //    {
//        //        _packages[l].SendQueueHandled();
//        //    }

//        //    return false;
//        //}

//        //[HarmonyPrefix]
//        //[HarmonyPatch(nameof(ConnectionManager.FlushClientSendQueues))]
//        //public static bool FlushClientSendQueues()
//        //{
//        //    Parallel.ForEach<ClientInfo>(ConnectionManager.Instance.Clients.List, (clientInfo) =>
//        //    {
//        //        try
//        //        {
//        //            clientInfo.netConnection[0].FlushSendQueue();
//        //            clientInfo.netConnection[1].FlushSendQueue();
//        //        }
//        //        catch (Exception ex)
//        //        {
//        //            CustomLogger.Error(ex, "Error in ConnectionManagerPatcher.FlushClientSendQueues");
//        //        }
//        //    });

//        //    return false;
//        //}

//        [HarmonyPrefix]
//        [HarmonyPatch("UpdatePings")]
//        public static bool UpdatePings()
//        {
//            Parallel.ForEach<ClientInfo>(ConnectionManager.Instance.Clients.List, (clientInfo) =>
//            {
//                try
//                {
//                    clientInfo.UpdatePing();
//                }
//                catch (Exception ex)
//                {
//                    CustomLogger.Error(ex, "Error in ConnectionManagerPatcher.UpdatePing");
//                }
//            });

//            return false;
//        }
//    }
//}
