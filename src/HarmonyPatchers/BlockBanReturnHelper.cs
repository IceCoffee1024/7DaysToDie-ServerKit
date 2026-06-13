using System;
using System.Collections;
using System.Collections.Generic;
using SdtdServerKit;
using UnityEngine;

namespace SdtdServerKit.HarmonyPatchers
{
    /// <summary>
    /// 禁放命中后的物品返还/标记清理
    /// </summary>
    internal static class BlockBanReturnHelper
    {
        private static readonly WaitForSeconds _waitBriefly = new WaitForSeconds(0.1f);

        /// <summary>
        /// 处理被禁止的领地石/睡袋方块：
        /// - 删除地图上的领地石/睡袋标记
        /// - 给玩家把对应物品返还到背包
        /// - 睡袋还会恢复玩家原有睡袋的标记
        /// </summary>
        /// <param name="clientInfo">玩家客户端</param>
        /// <param name="persistentPlayerId">玩家持久化 ID</param>
        /// <param name="landClaimsToReturn">被拦截的领地石放置信息</param>
        /// <param name="bedsToReturn">被拦截的睡袋放置信息</param>
        public static void ReturnBlockedItems(
            ClientInfo clientInfo,
            PlatformUserIdentifierAbs persistentPlayerId,
            List<BlockChangeInfo> landClaimsToReturn,
            List<BlockChangeInfo> bedsToReturn)
        {
            if (clientInfo == null)
            {
                return;
            }

            try
            {
                var gameManager = GameManager.Instance;
                if (gameManager == null || gameManager.World == null)
                {
                    return;
                }

                PersistentPlayerData? persistentPlayerData = null;
                if (persistentPlayerId != null
                    && gameManager.persistentPlayers != null
                    && gameManager.persistentPlayers.Players.TryGetValue(persistentPlayerId, out var pdata))
                {
                    persistentPlayerData = pdata;
                }

                EntityPlayer? player = null;
                if (gameManager.World.Players?.dict != null)
                {
                    gameManager.World.Players.dict.TryGetValue(clientInfo.entityId, out player);
                }

                if (landClaimsToReturn != null && landClaimsToReturn.Count > 0)
                {
                    ThreadManager.StartCoroutine(ReturnLandClaimItems(clientInfo, player, landClaimsToReturn));
                }

                if (bedsToReturn != null && bedsToReturn.Count > 0)
                {
                    ThreadManager.StartCoroutine(ReturnBedItems(clientInfo, persistentPlayerData, player, bedsToReturn));
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "禁放返还：调度物品返还协程失败");
            }
        }

        private static IEnumerator ReturnLandClaimItems(
            ClientInfo clientInfo,
            EntityPlayer? player,
            List<BlockChangeInfo> landClaims)
        {
            yield return _waitBriefly;

            for (int i = 0; i < landClaims.Count; i++)
            {
                var change = landClaims[i];
                try
                {
                    RemoveMapMarkerByPosition(clientInfo, change.pos, "land_claim", EnumMapObjectType.LandClaim);
                    GiveItemToPlayer(clientInfo, player, change.pos, change.blockValue);
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex, $"禁放返还：返还领地石失败 - 位置 {change.pos}");
                }
            }
        }

        private static IEnumerator ReturnBedItems(
            ClientInfo clientInfo,
            PersistentPlayerData? persistentPlayerData,
            EntityPlayer? player,
            List<BlockChangeInfo> beds)
        {
            yield return _waitBriefly;

            for (int i = 0; i < beds.Count; i++)
            {
                var change = beds[i];
                try
                {
                    RemoveMapMarkerByOwner(clientInfo, player, change.pos, "sleeping_bag", EnumMapObjectType.SleepingBag);
                    GiveItemToPlayer(clientInfo, player, change.pos, change.blockValue);
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex, $"禁放返还：返还睡袋失败 - 位置 {change.pos}");
                }
            }

            try
            {
                if (persistentPlayerData != null
                    && persistentPlayerData.HasBedrollPos
                    && GameManager.Instance?.World != null)
                {
                    BlockValue serverBedroll = GameManager.Instance.World.GetBlock(persistentPlayerData.BedrollPos);
                    clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(
                        persistentPlayerData,
                        new List<BlockChangeInfo>
                        {
                            new BlockChangeInfo(persistentPlayerData.BedrollPos, serverBedroll, false, false)
                        },
                        clientInfo.entityId));
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "禁放返还：恢复玩家原有睡袋标记失败");
            }
        }

        private static void RemoveMapMarkerByPosition(ClientInfo clientInfo, Vector3i pos,
            string navObjectName, EnumMapObjectType type)
        {
            try
            {
                NavObjectManager.Instance.UnRegisterNavObjectByPosition(pos.ToVector3(), navObjectName);
            }
            catch (Exception ex)
            {
                CustomLogger.Debug(ex, "禁放返还：取消导航对象失败（按位置）");
            }
            clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>()
                .Setup(type, pos.ToVector3()));
        }

        private static void RemoveMapMarkerByOwner(ClientInfo clientInfo, EntityPlayer? player, Vector3i pos,
            string navObjectName, EnumMapObjectType type)
        {
            try
            {
                if (player != null)
                {
                    NavObjectManager.Instance.UnRegisterNavObjectByOwnerEntity(player, navObjectName);
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Debug(ex, "禁放返还：取消导航对象失败（按所有者）");
            }
            clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityMapMarkerRemove>()
                .Setup(type, pos.ToVector3()));
        }

        private static void GiveItemToPlayer(ClientInfo clientInfo, EntityPlayer? player, Vector3i pos, BlockValue blockValue)
        {
            string? itemName = GetItemNameFromBlock(blockValue);
            if (string.IsNullOrEmpty(itemName))
            {
                CustomLogger.Debug($"禁放返还：方块 {blockValue.Block?.GetBlockName()} 无法解析物品名，跳过返还");
                return;
            }

            var itemClass = ItemClass.GetItem(itemName, true);
            if (itemClass == null || itemClass.type == 0)
            {
                CustomLogger.Debug($"禁放返还：物品 {itemName} 在 XML 中未定义，跳过返还");
                return;
            }

            ItemStack itemStack = new ItemStack(itemClass, 1);

            Vector3 dropPos = player != null
                ? player.position + new Vector3(0f, 1f, 0f)
                : pos.ToVector3() + new Vector3(0.5f, 1.0f, 0.5f);

            int entityId = CreateItemEntity(dropPos, itemStack, clientInfo.entityId);
            if (entityId != -1)
            {
                GameManager.Instance.CollectEntityServer(entityId, clientInfo.entityId);
            }
        }

        private static string? GetItemNameFromBlock(BlockValue blockValue)
        {
            var block = blockValue.Block;
            if (block == null)
            {
                return null;
            }

            if (block.CanPickup && !string.IsNullOrEmpty(block.PickedUpItemValue))
            {
                return block.PickedUpItemValue;
            }

            return block.GetBlockName();
        }

        private static int CreateItemEntity(Vector3 dropPos, ItemStack itemStack, int belongsPlayerId)
        {
            try
            {
                int entityId = EntityFactory.nextEntityID++;
                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData
                {
                    entityClass = EntityClass.FromString("item"),
                    id = entityId,
                    itemStack = itemStack.Clone(),
                    pos = dropPos,
                    rot = new Vector3(20f, 0f, 20f),
                    lifetime = 60f,
                    belongsPlayerId = belongsPlayerId
                });

                GameManager.Instance.World.SpawnEntityInWorld(entityItem);
                return entityId;
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "禁放返还：创建物品实体失败");
                return -1;
            }
        }
    }
}
