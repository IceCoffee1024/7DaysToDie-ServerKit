using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using SdtdServerKit.Variables;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 抽奖功能
    /// </summary>
    public class Lottery : FunctionBase<LotterySettings>
    {
        private readonly ILotteryRepository _lotteryRepository;
        private readonly ILotteryItemRepository _lotteryItemRepository;
        private readonly ILotteryCommandRepository _lotteryCommandRepository;
        private readonly ILotteryRecordRepository _lotteryRecordRepository;
        private readonly IPointsInfoRepository _pointsInfoRepository;
        private readonly IItemListRepository _itemListRepository;
        private readonly ICommandListRepository _commandListRepository;

        public Lottery(
            ILotteryRepository lotteryRepository,
            ILotteryItemRepository lotteryItemRepository,
            ILotteryCommandRepository lotteryCommandRepository,
            ILotteryRecordRepository lotteryRecordRepository,
            IPointsInfoRepository pointsInfoRepository,
            IItemListRepository itemListRepository,
            ICommandListRepository commandListRepository)
        {
            _lotteryRepository = lotteryRepository;
            _lotteryItemRepository = lotteryItemRepository;
            _lotteryCommandRepository = lotteryCommandRepository;
            _lotteryRecordRepository = lotteryRecordRepository;
            _pointsInfoRepository = pointsInfoRepository;
            _itemListRepository = itemListRepository;
            _commandListRepository = commandListRepository;
        }

        protected override async Task<bool> OnChatCmd(string message, ManagedPlayer managedPlayer)
        {
            try
            {
                // 查询抽奖池列表
                if (string.Equals(message, Settings.QueryListCmd, StringComparison.OrdinalIgnoreCase))
                {
                    await HandleQueryList(managedPlayer);
                    return true;
                }

                // 获取所有启用的抽奖池
                var lotteryList = await _lotteryRepository.GetAllOrderByIdAsync();
                var enabledLotteries = lotteryList.Where(l => l.IsEnabled).ToList();

                // 检查是否匹配任何抽奖池的自定义命令
                foreach (var lottery in enabledLotteries)
                {
                    if (!string.IsNullOrEmpty(lottery.DrawCommand) && 
                        string.Equals(message, lottery.DrawCommand, StringComparison.OrdinalIgnoreCase))
                    {
                        await HandleDraw(managedPlayer, lottery.Id);
                        return true;
                    }
                }

                // 兼容旧的命令格式: cj-1 (使用全局命令前缀)
                if (message.StartsWith(Settings.DrawCommand + ConfigManager.GlobalSettings.ChatCommandSeparator, StringComparison.OrdinalIgnoreCase))
                {
                    int lotteryId = message.Substring(Settings.DrawCommand.Length + ConfigManager.GlobalSettings.ChatCommandSeparator.Length).ToInt(-1);
                    if (lotteryId > 0)
                    {
                        await HandleDraw(managedPlayer, lotteryId);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "抽奖功能处理命令时发生错误");
                return false;
            }
        }

        /// <summary>
        /// 处理查询抽奖池列表
        /// </summary>
        private async Task HandleQueryList(ManagedPlayer managedPlayer)
        {
            try
            {
                string playerId = managedPlayer.PlayerId;
                var lotteryList = await _lotteryRepository.GetAllOrderByIdAsync();
                var enabledLotteries = lotteryList.Where(l => l.IsEnabled).ToList();

                if (enabledLotteries.Any() == false)
                {
                    SendMessageToPlayer(playerId, Settings.NoLottery);
                }
                else
                {
                    foreach (var lottery in enabledLotteries)
                    {
                        SendMessageToPlayer(playerId, FormatLotteryCmd(Settings.LotteryItemTip, managedPlayer, lottery));
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "查询抽奖池列表时发生错误");
            }
        }

        /// <summary>
        /// 处理抽奖
        /// </summary>
        private async Task HandleDraw(ManagedPlayer managedPlayer, int lotteryId)
        {
            try
            {
                string playerId = managedPlayer.PlayerId;

                // 1. 检查抽奖池是否存在
                var lottery = await _lotteryRepository.GetByIdAsync(lotteryId);
                if (lottery == null)
                {
                    SendMessageToPlayer(playerId, Settings.LotteryNotFoundTip);
                    return;
                }

                // 2. 检查抽奖池是否启用
                if (!lottery.IsEnabled)
                {
                    SendMessageToPlayer(playerId, Settings.LotteryDisabledTip);
                    return;
                }

                // 3. 检查冷却时间
                var record = await _lotteryRecordRepository.GetByPlayerIdAndLotteryIdAsync(playerId, lotteryId);
                if (record != null && record.LastDrawAt.HasValue)
                {
                    var cooldownEnd = record.LastDrawAt.Value.AddSeconds(lottery.DrawInterval);
                    if (DateTime.Now < cooldownEnd)
                    {
                        int remainingSeconds = (int)(cooldownEnd - DateTime.Now).TotalSeconds;
                        SendMessageToPlayer(playerId, FormatLotteryCmd(Settings.CoolingTip, managedPlayer, lottery, remainingSeconds, true));
                        return;
                    }
                }

                // 4. 检查积分是否足够
                int points = await _pointsInfoRepository.GetPointsByIdAsync(playerId);
                if (points < lottery.DrawCost)
                {
                    SendMessageToPlayer(playerId, FormatLotteryCmd(Settings.PointsNotEnoughTip, managedPlayer, lottery, points));
                    return;
                }

                // 5. 获取抽奖池中的奖品（物品+命令两类）
                var lotteryItems = (await _lotteryItemRepository.GetListByLotteryIdAsync(lotteryId)).ToList();
                var lotteryCommands = (await _lotteryCommandRepository.GetListByLotteryIdAsync(lotteryId)).ToList();
                if (lotteryItems.Count == 0 && lotteryCommands.Count == 0)
                {
                    SendMessageToPlayer(playerId, Settings.LotteryEmptyTip);
                    return;
                }

                // 6. 根据权重统一加权抽奖（物品 + 命令奖品共同参与）
                var pool = new List<(string kind, int id, int weight)>();
                foreach (var li in lotteryItems) pool.Add(("item", li.ItemId, li.Weight));
                foreach (var lc in lotteryCommands) pool.Add(("cmd", lc.CommandId, lc.Weight));

                int totalWeight = pool.Sum(x => x.weight);
                if (totalWeight <= 0)
                {
                    SendMessageToPlayer(playerId, Settings.LotteryEmptyTip);
                    return;
                }

                int randomValue = new Random().Next(1, totalWeight + 1);
                int cur = 0;
                (string kind, int id, int weight) drawn = pool[0];
                foreach (var entry in pool)
                {
                    cur += entry.weight;
                    if (randomValue <= cur) { drawn = entry; break; }
                }

                // 7. 扣除积分
                await _pointsInfoRepository.ChangePointsAsync(playerId, -lottery.DrawCost);
                int balance = await _pointsInfoRepository.GetPointsByIdAsync(playerId);
                Managers.PointLogger.Log(Models.PointLogCategory.Lottery, playerId, managedPlayer.PlayerName,
                    -lottery.DrawCost, balance, $"抽奖: {lottery.Name}");

                // 8. 按奖品类型发放
                string drawnDisplayName = string.Empty;
                int drawnCount = 0;
                if (drawn.kind == "item")
                {
                    var itemList = await _itemListRepository.GetByIdAsync(drawn.id);
                    if (itemList == null)
                    {
                        CustomLogger.Warn($"抽奖池 {lotteryId} 中的奖品 {drawn.id} 不存在");
                        return;
                    }
                    drawnCount = itemList.Count;
                    drawnDisplayName = !string.IsNullOrEmpty(itemList.DisplayName)
                        ? itemList.DisplayName!
                        : GetLocalizedItemName(itemList.ItemName);
                    if (itemList.RewardType == 2)
                    {
                        // 积分奖品
                        await _pointsInfoRepository.ChangePointsAsync(playerId, itemList.Count, allowNegative: true);
                        balance += itemList.Count;
                        Managers.PointLogger.Log(Models.PointLogCategory.Lottery, playerId, managedPlayer.PlayerName,
                            itemList.Count, balance, $"抽奖 {lottery.Name} 获得积分奖品");
                    }
                    else
                    {
                        Utilities.Utils.GiveItem(playerId, itemList.ItemName, itemList.Count, itemList.Quality, itemList.Durability);
                    }
                }
                else
                {
                    var cmd = await _commandListRepository.GetByIdAsync(drawn.id);
                    if (cmd == null)
                    {
                        CustomLogger.Warn($"抽奖池 {lotteryId} 中的命令奖品 {drawn.id} 不存在");
                        return;
                    }
                    drawnCount = 1;
                    drawnDisplayName = !string.IsNullOrEmpty(cmd.DisplayName) ? cmd.DisplayName! : cmd.Command;

                    foreach (var line in (cmd.Command ?? string.Empty).Split('\n'))
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        // 渲染命令模板，支持 {PlayerId} {PlayerName} {EntityId} {LotteryId} 等占位符
                        string rendered = FormatLotteryRewardCmd(line, managedPlayer, lottery, drawnDisplayName, drawnCount);
                        Utilities.Utils.ExecuteConsoleCommand(rendered, cmd.InMainThread);
                    }
                }

                // 9. 更新抽奖记录
                if (record == null)
                {
                    record = new T_LotteryRecord
                    {
                        LotteryId = lotteryId,
                        PlayerId = playerId,
                        PlayerName = managedPlayer.PlayerName,
                        CreatedAt = DateTime.Now,
                        LastDrawAt = DateTime.Now
                    };
                    await _lotteryRecordRepository.InsertAsync(record);
                }
                else
                {
                    record.LastDrawAt = DateTime.Now;
                    record.PlayerName = managedPlayer.PlayerName;
                    await _lotteryRecordRepository.UpdateAsync(record);
                }

                // 10. 发送成功消息
                SendMessageToPlayer(playerId, FormatLotteryRewardCmd(Settings.DrawSuccessTip, managedPlayer, lottery, drawnDisplayName, drawnCount));

                CustomLogger.Debug($"玩家 {managedPlayer.PlayerName}({playerId}) 在抽奖池 {lottery.Name}({lotteryId}) 中抽中了 {drawnDisplayName} x{drawnCount}");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, $"玩家 {managedPlayer.PlayerName} 抽奖时发生错误");
            }
        }

        /// <summary>
        /// 格式化命令（带抽奖池信息）
        /// </summary>
        private string FormatLotteryCmd(string message, ManagedPlayer player, T_Lottery lottery)
        {
            string result = StringTemplate.Render(message, new LotteryVariables()
            {
                EntityId = player.EntityId,
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                LotteryId = lottery.Id,
                LotteryName = lottery.Name,
                DrawCost = lottery.DrawCost,
                DrawInterval = lottery.DrawInterval,
            });
            return result;
        }

        /// <summary>
        /// 格式化命令（带抽奖池和积分信息）
        /// </summary>
        private string FormatLotteryCmd(string message, ManagedPlayer player, T_Lottery lottery, int points)
        {
            string result = StringTemplate.Render(message, new LotteryVariables()
            {
                EntityId = player.EntityId,
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                LotteryId = lottery.Id,
                LotteryName = lottery.Name,
                DrawCost = lottery.DrawCost,
                DrawInterval = lottery.DrawInterval,
                Points = points,
            });
            return result;
        }

        /// <summary>
        /// 格式化命令（带抽奖池和冷却时间信息）
        /// </summary>
        private string FormatLotteryCmd(string message, ManagedPlayer player, T_Lottery lottery, int remainingSeconds, bool isCooldown)
        {
            string result = StringTemplate.Render(message, new LotteryVariables()
            {
                EntityId = player.EntityId,
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                LotteryId = lottery.Id,
                LotteryName = lottery.Name,
                DrawCost = lottery.DrawCost,
                DrawInterval = lottery.DrawInterval,
                Seconds = remainingSeconds,
            });
            return result;
        }

        /// <summary>
        /// 格式化命令（带抽奖池和奖品信息：通用，奖品名按 displayName 已计算好）
        /// </summary>
        private string FormatLotteryRewardCmd(string message, ManagedPlayer player, T_Lottery lottery, string rewardName, int count)
        {
            string result = StringTemplate.Render(message, new LotteryVariables()
            {
                EntityId = player.EntityId,
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                LotteryId = lottery.Id,
                LotteryName = lottery.Name,
                DrawCost = lottery.DrawCost,
                DrawInterval = lottery.DrawInterval,
                ItemName = rewardName,
                Count = count,
                Quality = 0,
                Durability = 0,
            });
            return result;
        }

        /// <summary>
        /// 获取物品的本地化名称，如果没有则返回原始名称
        /// </summary>
        private static string GetLocalizedItemName(string itemName)
        {
            if (string.IsNullOrEmpty(itemName))
            {
                return string.Empty;
            }

            try
            {
                string languageStr = GamePrefs.GetString(EnumGamePrefs.Language);
                if (Enum.TryParse<Language>(languageStr, true, out var language))
                {
                    string localized = Utilities.Utils.GetLocalization(itemName, language);
                    if (!string.IsNullOrEmpty(localized))
                    {
                        return localized;
                    }
                }
            }
            catch
            {
                // 本地化失败，返回原始名称
            }

            return itemName;
        }
    }
}
