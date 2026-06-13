using HarmonyLib;
using Noemax.GZip;
using SdtdServerKit.Functions;
using SdtdServerKit.Managers;
using UnityEngine;

namespace SdtdServerKit.HarmonyPatchers
{
    [HarmonyPatch(typeof(GameManager))]
    internal class GameManagerPatcher
    {
        public static bool Before_RequestToSpawnPlayer(ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile)
        {
            var xmlsToLoad = WorldStaticData.xmlsToLoad;

            foreach (var item in xmlsToLoad)
            {
                if (item.SendToClients && item.CompressedXmlData != null)
                {
                    var xmlName = item.XmlName;
                    using var compressedMemoryStream = new MemoryStream();
                    using (var deflateOutputStream = new DeflateOutputStream(compressedMemoryStream, 1))
                    {
                        deflateOutputStream.WriteByte(0);
                    }

                    _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageConfigFile>().Setup(xmlName, compressedMemoryStream.ToArray()));
                }
            }

            return true;
        }

        /// <summary>
        /// 重置区域禁放补丁：拦截在定时重置区域内放置领地石/睡袋
        /// </summary>
        public static bool Before_ChangeBlocks_ChunkResetBan(GameManager __instance, PlatformUserIdentifierAbs persistentPlayerId, List<BlockChangeInfo> _blocksToChange)
        {
            try
            {
                if (persistentPlayerId == null || _blocksToChange == null || _blocksToChange.Count == 0)
                {
                    return true;
                }

                if (!FunctionManager.TryGetFunction<Functions.ChunkReset>(out var chunkReset) || chunkReset == null)
                {
                    return true;
                }

                var settings = chunkReset.Settings;
                if (settings == null || !settings.IsEnabled)
                {
                    return true;
                }

                bool banLandClaim = settings.IsLandClaimBanEnabled;
                bool banBedroll = settings.IsBedrollBanEnabled;
                bool banPoiLandClaim = settings.IsPoiLandClaimBanEnabled;
                bool banPoiBedroll = settings.IsPoiBedrollBanEnabled;
                if (!banLandClaim && !banBedroll && !banPoiLandClaim && !banPoiBedroll)
                {
                    return true;
                }

                var clientInfo = ConnectionManager.Instance.Clients.ForUserId(persistentPlayerId);
                if (clientInfo == null)
                {
                    return true;
                }

                World world = __instance.World;
                List<BlockChangeInfo>? returned = null;
                List<BlockChangeInfo>? landClaimsToReturn = null;
                List<BlockChangeInfo>? bedsToReturn = null;
                bool blockedLandClaim = false;
                bool blockedBedroll = false;
                bool blockedPoiLandClaim = false;
                bool blockedPoiBedroll = false;

                foreach (var info in _blocksToChange.ToList())
                {
                    if (!info.bChangeBlockValue)
                    {
                        continue;
                    }

                    var block = info.blockValue.Block;
                    if (block == null)
                    {
                        continue;
                    }

                    bool isLandClaim = block is BlockLandClaim;
                    bool isBedroll = block is BlockSleepingBag;

                    if (!isLandClaim && !isBedroll)
                    {
                        continue;
                    }

                    var existingBlock = world.GetBlock(info.pos).Block;
                    if (isLandClaim && existingBlock is BlockLandClaim)
                    {
                        continue;
                    }
                    if (isBedroll && existingBlock is BlockSleepingBag)
                    {
                        continue;
                    }

                    bool hitChunkResetLand = isLandClaim && banLandClaim
                        && chunkReset.IsInResetArea(info.pos.x, info.pos.z);
                    bool hitChunkResetBed = isBedroll && banBedroll
                        && chunkReset.IsInResetArea(info.pos.x, info.pos.z);
                    bool hitPoiLand = isLandClaim && banPoiLandClaim
                        && Functions.PoiProtectionZone.IsInLandClaimZone(info.pos);
                    bool hitPoiBed = isBedroll && banPoiBedroll
                        && Functions.PoiProtectionZone.IsInBedZone(info.pos);

                    if (!hitChunkResetLand && !hitChunkResetBed && !hitPoiLand && !hitPoiBed)
                    {
                        continue;
                    }

                    returned ??= new List<BlockChangeInfo>();
                    returned.Add(new BlockChangeInfo(info.clrIdx, info.pos, world.GetBlock(info.pos)));

                    if (isLandClaim)
                    {
                        landClaimsToReturn ??= new List<BlockChangeInfo>();
                        landClaimsToReturn.Add(info);
                    }
                    else
                    {
                        bedsToReturn ??= new List<BlockChangeInfo>();
                        bedsToReturn.Add(info);
                    }

                    _blocksToChange.Remove(info);

                    if (hitChunkResetLand)
                    {
                        blockedLandClaim = true;
                    }
                    if (hitChunkResetBed)
                    {
                        blockedBedroll = true;
                    }
                    if (hitPoiLand)
                    {
                        blockedPoiLandClaim = true;
                    }
                    if (hitPoiBed)
                    {
                        blockedPoiBedroll = true;
                    }

                    string reason;
                    if (hitChunkResetLand) reason = "重置区领地石";
                    else if (hitChunkResetBed) reason = "重置区睡袋";
                    else if (hitPoiLand) reason = "系统房领地石";
                    else reason = "系统房睡袋";

                    CustomLogger.Debug($"区域禁放：玩家 {clientInfo.entityId} 在 ({info.pos.x},{info.pos.z}) 放置 {reason} 已被拦截");
                }

                if (returned != null && returned.Count > 0)
                {
                    NetPackageSetBlock package = NetPackageManager.GetPackage<NetPackageSetBlock>().Setup(null, returned, -1);
                    clientInfo.SendPackage(package);
                }

                if ((landClaimsToReturn != null && landClaimsToReturn.Count > 0)
                    || (bedsToReturn != null && bedsToReturn.Count > 0))
                {
                    BlockBanReturnHelper.ReturnBlockedItems(clientInfo, persistentPlayerId,
                        landClaimsToReturn ?? new List<BlockChangeInfo>(),
                        bedsToReturn ?? new List<BlockChangeInfo>());
                }

                string? landClaimMessage = null;
                if (blockedLandClaim && !string.IsNullOrEmpty(settings.LandClaimBanTip))
                {
                    landClaimMessage = settings.LandClaimBanTip;
                }
                else if (blockedPoiLandClaim && !string.IsNullOrEmpty(settings.PoiLandClaimBanTip))
                {
                    landClaimMessage = settings.PoiLandClaimBanTip;
                }

                string? bedrollMessage = null;
                if (blockedBedroll && !string.IsNullOrEmpty(settings.BedrollBanTip))
                {
                    bedrollMessage = settings.BedrollBanTip;
                }
                else if (blockedPoiBedroll && !string.IsNullOrEmpty(settings.PoiBedrollBanTip))
                {
                    bedrollMessage = settings.PoiBedrollBanTip;
                }

                if (landClaimMessage != null)
                {
                    clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>()
                        .Setup(landClaimMessage, "ui_denied"));
                }

                if (bedrollMessage != null)
                {
                    clientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageShowToolbeltMessage>()
                        .Setup(bedrollMessage, "ui_denied"));
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "重置区域禁放补丁执行失败");
            }

            return true;
        }

        //public static bool Before_ChatMessageServer(ClientInfo _cInfo, EChatType _chatType, int _senderEntityId, ref string _msg, List<int> _recipientEntityIds, ref EMessageSender _msgSender)
        //{
        //    if(_msgSender == EMessageSender.SenderIdAsPlayer && LivePlayerManager.TryGetByEntityId(_senderEntityId, out var managedPlayer))
        //    {
        //        var repository = ModApi.ServiceContainer.Resolve<IColoredChatRepository>();
        //        var coloredChat = repository.GetById(managedPlayer!.PlayerId);
        //        if (coloredChat != null)
        //        {
        //            string playerName = managedPlayer.PlayerName;

        //            _msg = $"[{coloredChat.NameColor}]{playerName}[{GetDefaultColor(_chatType)}]: [{coloredChat.TextColor}]{_msg}";
        //            _msgSender = EMessageSender.None;
        //        }
        //    }

        //    return true;
        //}

        //private static string GetDefaultColor(EChatType eChatType)
        //{
        //    switch (eChatType)
        //    {
        //        case EChatType.Global:
        //            return "FFFFFF";
        //        case EChatType.Friends:
        //            return "00BB00";
        //        case EChatType.Party:
        //            return "FFCC00";
        //        case EChatType.Whisper:
        //            return "D00000";
        //        default:
        //            throw new ArgumentOutOfRangeException(nameof(eChatType), eChatType, null);
        //    }
        //}

        /// <summary>
        /// 出生点配置
        /// </summary>
        public static void Before_RequestToSpawnPlayer_SpawnPoint(GameManager __instance, ClientInfo _cInfo, int _chunkViewDim, PlayerProfile _playerProfile, int _nearEntityId)
        {
            try
            {
                var settings = ConfigManager.GlobalSettings.SpawnPointSettings;
                if (settings == null || !settings.IsEnabled)
                {
                    return;
                }

                if (settings.SpawnPoints == null || settings.SpawnPoints.Length == 0)
                {
                    CustomLogger.Debug("出生点配置：出生点列表为空，跳过设置");
                    return;
                }

                PlatformUserIdentifierAbs persistentPlayerId = __instance.getPersistentPlayerID(_cInfo);
                if (persistentPlayerId == null)
                {
                    CustomLogger.Debug("出生点配置：无法获取玩家持久化ID");
                    return;
                }

                // 检查玩家是否是首次进入游戏
                PlayerDataFile playerDataFile = new PlayerDataFile();
                playerDataFile.Load(GameIO.GetPlayerDataDir(), persistentPlayerId.CombinedString);
                
                if (playerDataFile.bLoaded)
                {
                    return;
                }

                // 创建自定义出生点列表
                SpawnPointList customSpawnPoints = new SpawnPointList();
                
                foreach (var spawnPointEntry in settings.SpawnPoints)
                {
                    if (string.IsNullOrEmpty(spawnPointEntry.Position))
                    {
                        continue;
                    }

                    string[] coords = spawnPointEntry.Position.Split(',');
                    if (coords.Length != 3)
                    {
                        CustomLogger.Debug($"出生点配置：坐标格式无效 {spawnPointEntry.Position}");
                        continue;
                    }

                    if (!int.TryParse(coords[0].Trim(), out int x) ||
                        !int.TryParse(coords[1].Trim(), out int y) ||
                        !int.TryParse(coords[2].Trim(), out int z))
                    {
                        CustomLogger.Debug($"出生点配置：坐标解析失败 {spawnPointEntry.Position}");
                        continue;
                    }

                    Vector3i blockPos = new Vector3i(x, y, z);
                    SpawnPoint spawnPoint = new SpawnPoint(blockPos);
                    customSpawnPoints.Add(spawnPoint);
                }

                if (customSpawnPoints.Count == 0)
                {
                    CustomLogger.Debug("出生点配置：没有有效的出生点坐标");
                    return;
                }

                __instance.World.ChunkClusters[0].ChunkProvider.SetSpawnPointList(customSpawnPoints);
                
                _cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageWorldSpawnPoints>().Setup(__instance.GetSpawnPointList()));

                CustomLogger.Debug($"出生点配置：已为玩家 {_cInfo.playerName} 设置 {customSpawnPoints.Count} 个自定义出生点");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "出生点配置补丁错误");
            }
        }
    }
}