using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using SdtdServerKit.Models;
using SdtdServerKit.Variables;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 等级礼包
    /// </summary>
    public class LevelGift : FunctionBase<LevelGiftSettings>
    {
        private readonly ILevelGiftRepository _levelGiftRepository;
        private readonly ILevelGiftClaimRecordRepository _claimRecordRepository;
        private readonly IItemListRepository _itemListRepository;
        private readonly ICommandListRepository _commandListRepository;
        private readonly IPointsInfoRepository _pointsInfoRepository;

        // 按玩家加锁，防止升级事件与进入世界兜底并发导致重复发放
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, SemaphoreSlim> _grantLocks = new();

        /// <inheritdoc/>
        public LevelGift(
            ILevelGiftRepository levelGiftRepository,
            ILevelGiftClaimRecordRepository claimRecordRepository,
            IItemListRepository itemListRepository,
            ICommandListRepository commandListRepository,
            IPointsInfoRepository pointsInfoRepository)
        {
            _levelGiftRepository = levelGiftRepository;
            _claimRecordRepository = claimRecordRepository;
            _itemListRepository = itemListRepository;
            _commandListRepository = commandListRepository;
            _pointsInfoRepository = pointsInfoRepository;
        }

        /// <inheritdoc/>
        protected override async Task<bool> OnChatCmd(string message, ManagedPlayer managedPlayer)
        {
            if (string.Equals(message, Settings.ClaimCmd, StringComparison.OrdinalIgnoreCase))
            {
                await HandlePlayerGiftClaim(managedPlayer);
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        protected override void OnEnableFunction()
        {
            base.OnEnableFunction();
            // 玩家进入世界时兜底补发（处理升级事件遗漏的情况，例如达标后才新增礼包）
            ModEventHub.PlayerSpawnedInWorld += OnPlayerSpawnedInWorld;
        }

        /// <inheritdoc/>
        protected override void OnDisableFunction()
        {
            base.OnDisableFunction();
            ModEventHub.PlayerSpawnedInWorld -= OnPlayerSpawnedInWorld;
        }

        /// <inheritdoc/>
        protected override async Task OnPlayerLevelUp(ManagedPlayer managedPlayer, int newLevel)
        {
            // 玩家升级时自动检查并发放通用礼包
            await CheckAndGrantUniversalGifts(managedPlayer, newLevel);
        }

        /// <summary>
        /// 玩家进入世界时，按当前等级兜底补发未领取的通用礼包
        /// </summary>
        private void OnPlayerSpawnedInWorld(SpawnedPlayer spawnedPlayer)
        {
            try
            {
                // 只处理玩家加入/进入多人游戏及读取存档的情况，避免传送、死亡重生时重复检查
                var respawnType = spawnedPlayer.RespawnType;
                if (respawnType != Models.RespawnType.EnterMultiplayer
                    && respawnType != Models.RespawnType.JoinMultiplayer
                    && respawnType != Models.RespawnType.LoadedGame
                    && respawnType != Models.RespawnType.NewGame)
                {
                    return;
                }

                if (LivePlayerManager.TryGetByEntityId(spawnedPlayer.EntityId, out var managedPlayer) && managedPlayer != null)
                {
                    int currentLevel = managedPlayer.EntityPlayer?.Progression?.Level ?? 0;
                    if (currentLevel <= 0)
                    {
                        return;
                    }

                    // 异步执行，避免阻塞生成流程
                    Task.Run(async () =>
                    {
                        try
                        {
                            await CheckAndGrantUniversalGifts(managedPlayer, currentLevel);
                        }
                        catch (Exception ex)
                        {
                            CustomLogger.Error(ex, "玩家进入世界时补发通用等级礼包失败");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Error in LevelGift.PlayerSpawnedInWorld");
            }
        }

        /// <summary>
        /// 处理玩家礼包领取（玩家输入命令）
        /// </summary>
        private async Task HandlePlayerGiftClaim(ManagedPlayer managedPlayer)
        {
            string playerId = managedPlayer.PlayerId;
            var levelGift = await _levelGiftRepository.GetByIdAsync(playerId);
            
            if (levelGift == null || levelGift.GiftType != 0)
            {
                SendMessageToPlayer(playerId, Settings.NoGiftTip);
                return;
            }

            // 检查是否已领取
            if (levelGift.ClaimState)
            {
                SendMessageToPlayer(playerId, Settings.HasClaimedTip);
                return;
            }

            // 检查等级是否满足要求
            if (managedPlayer.EntityPlayer.Progression.Level < levelGift.RequiredLevel)
            {
                SendMessageToPlayer(playerId, FormatCmd(Settings.LevelNotEnoughTip, managedPlayer, levelGift));
                return;
            }

            // 发放礼包
            await GrantGift(managedPlayer, levelGift);

            // 更新领取状态
            levelGift.ClaimState = true;
            levelGift.TotalClaimCount++;
            levelGift.LastClaimAt = DateTime.Now;
            await _levelGiftRepository.UpdateAsync(levelGift);

            SendMessageToPlayer(playerId, FormatCmd(Settings.ClaimSuccessTip, managedPlayer, levelGift));
            CustomLogger.Debug("玩家ID: {0}, 玩家名称: {1}, 领取了玩家等级礼包: {2}.", playerId, managedPlayer.PlayerName, levelGift.Name);
        }

        /// <summary>
        /// 检查并发放通用礼包（玩家升级或进入世界时触发）
        /// </summary>
        private async Task CheckAndGrantUniversalGifts(ManagedPlayer managedPlayer, int newLevel)
        {
            string playerId = managedPlayer.PlayerId;

            // 按玩家加锁，避免升级事件与进入世界兜底两条路径并发导致同一礼包重复发放
            var gate = _grantLocks.GetOrAdd(playerId, _ => new SemaphoreSlim(1, 1));
            await gate.WaitAsync();
            try
            {
                // 获取所有通用礼包
                var allGifts = await _levelGiftRepository.GetAllAsync();
                var universalGifts = allGifts.Where(g => g.GiftType == 1 && g.RequiredLevel <= newLevel).ToList();

                if (!universalGifts.Any())
                {
                    return;
                }

                // 获取玩家已领取的礼包ID列表
                var claimedGiftIds = await _claimRecordRepository.GetClaimedGiftIdsAsync(playerId);

                // 筛选出未领取的礼包
                var unclaimedGifts = universalGifts.Where(g => !claimedGiftIds.Contains(g.Id)).ToList();

                foreach (var gift in unclaimedGifts)
                {
                    // 发放礼包
                    await GrantGift(managedPlayer, gift);

                    // 记录领取
                    var record = new T_LevelGiftClaimRecord
                    {
                        PlayerId = playerId,
                        GiftId = gift.Id,
                        ClaimAt = DateTime.Now
                    };
                    await _claimRecordRepository.InsertAsync(record);

                    // 更新礼包统计
                    gift.TotalClaimCount++;
                    gift.LastClaimAt = DateTime.Now;
                    await _levelGiftRepository.UpdateAsync(gift);

                    SendMessageToPlayer(playerId, FormatCmd(Settings.ClaimSuccessTip, managedPlayer, gift));
                    CustomLogger.Debug("玩家ID: {0}, 玩家名称: {1}, 自动领取了通用等级礼包: {2}.", playerId, managedPlayer.PlayerName, gift.Name);
                }
            }
            finally
            {
                gate.Release();
            }
        }

        /// <summary>
        /// 发放礼包（物品和命令）
        /// </summary>
        private async Task GrantGift(ManagedPlayer managedPlayer, T_LevelGift levelGift)
        {
            string playerId = managedPlayer.PlayerId;

            // 发放物品 / 积分
            var itemList = await _itemListRepository.GetListByLevelGiftIdAsync(levelGift.Id);
            foreach (var item in itemList)
            {
                if (item.RewardType == 2)
                {
                    await _pointsInfoRepository.ChangePointsAsync(playerId, item.Count, allowNegative: true);
                }
                else
                {
                    Utilities.Utils.GiveItem(playerId, item.ItemName, item.Count, item.Quality, item.Durability);
                }
                await Task.Delay(20);
            }

            // 执行命令
            var commandList = await _commandListRepository.GetListByLevelGiftIdAsync(levelGift.Id);
            foreach (var item in commandList)
            {
                foreach (var cmd in item.Command.Split('\n'))
                {
                    Utilities.Utils.ExecuteConsoleCommand(FormatCmd(cmd, managedPlayer, levelGift), item.InMainThread);
                }
                await Task.Delay(20);
            }
        }

        private static string FormatCmd(string message, ManagedPlayer player, T_LevelGift levelGift)
        {
            string result = StringTemplate.Render(message, new LevelGiftVariables()
            {
                EntityId = player.EntityId,
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                GiftName = levelGift.Name,
                RequiredLevel = levelGift.RequiredLevel,
                TotalClaimCount = levelGift.TotalClaimCount,
                GiftDescription = levelGift.Description,
            });
            return result;
        }
    }
}
