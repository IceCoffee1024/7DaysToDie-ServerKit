using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using SdtdServerKit.Variables;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// GameStore
    /// </summary>
    public class GameStore : FunctionBase<GameStoreSettings>
    {
        private readonly IGoodsRepository _goodsRepository;
        private readonly IPointsInfoRepository _pointsInfoRepository;
        private readonly IItemListRepository _itemListRepository;
        private readonly ICommandListRepository _commandListRepository;

        /// <inheritdoc/>
        public GameStore(IPointsInfoRepository pointsInfoRepository, IGoodsRepository goodsRepository, IItemListRepository itemListRepository, ICommandListRepository commandListRepository)
        {
            _pointsInfoRepository = pointsInfoRepository;
            _goodsRepository = goodsRepository;
            _itemListRepository = itemListRepository;
            _commandListRepository = commandListRepository;
        }
        /// <inheritdoc/>
        protected override async Task<bool> OnChatCmd(string message, ManagedPlayer managedPlayer)
        {
            if (string.Equals(message, Settings.QueryListCmd, StringComparison.OrdinalIgnoreCase))
            {
                string playerId = managedPlayer.PlayerId;
                var goodsList = await _goodsRepository.GetAllOrderByIdAsync();
                if (goodsList.Any() == false)
                {
                    SendMessageToPlayer(playerId, Settings.NoGoods);
                }
                else
                {
                    foreach (var item in goodsList)
                    {
                        SendMessageToPlayer(playerId, FormatCmd(Settings.GoodsItemTip, managedPlayer, item));
                    }
                }

                return true;
            }
            else if (message.StartsWith(Settings.BuyCmdPrefix + ConfigManager.GlobalSettings.ChatCommandSeparator, StringComparison.OrdinalIgnoreCase))
            {
                int goodsId = message.Substring(Settings.BuyCmdPrefix.Length + ConfigManager.GlobalSettings.ChatCommandSeparator.Length).ToInt(-1);
                var goods = await _goodsRepository.GetByIdAsync(goodsId);
                if (goods == null)
                {
                    return false;
                }
                else
                {
                    string playerId = managedPlayer.PlayerId;

                    int points = await _pointsInfoRepository.GetPointsByIdAsync(playerId);
                    if (points < goods.Price)
                    {
                        SendMessageToPlayer(playerId, FormatCmd(Settings.PointsNotEnoughTip, managedPlayer, goods));
                    }
                    else
                    {
                        await _pointsInfoRepository.ChangePointsAsync(playerId, -goods.Price);
                        int balance = points - goods.Price;
                        Managers.PointLogger.Log(Models.PointLogCategory.Shop, playerId, managedPlayer.PlayerName,
                            -goods.Price, balance, $"购买商品: {goods.Name}");

                        var itemList = await _itemListRepository.GetListByGoodsIdAsync(goods.Id);
                        foreach (var item in itemList)
                        {
                            if (item.RewardType == 2)
                            {
                                // 积分奖品：直接增加积分
                                await _pointsInfoRepository.ChangePointsAsync(playerId, item.Count, allowNegative: true);
                                balance += item.Count;
                                Managers.PointLogger.Log(Models.PointLogCategory.Shop, playerId, managedPlayer.PlayerName,
                                    item.Count, balance, $"购买商品 {goods.Name} 获得积分奖品");
                            }
                            else
                            {
                                Utilities.Utils.GiveItem(playerId, item.ItemName, item.Count, item.Quality, item.Durability);
                            }
                            await Task.Delay(20);
                        }

                        var commandList = await _commandListRepository.GetListByGoodsIdAsync(goods.Id);
                        foreach (var item in commandList)
                        {
                            foreach (var cmd in item.Command.Split('\n'))
                            {
                                Utilities.Utils.ExecuteConsoleCommand(FormatCmd(cmd, managedPlayer, goods), item.InMainThread);
                            }
                            await Task.Delay(20);
                        }

                        SendMessageToPlayer(playerId, FormatCmd(Settings.BuySuccessTip, managedPlayer, goods));

                        CustomLogger.Info("PlayerId: {0}, playerName: {1}, {1} bought: {2}.", playerId, managedPlayer.PlayerName, goods.Name);
                    }

                    return true;
                }
            }

            return false;
        }

        private static string FormatCmd(string message, ManagedPlayer player, T_Goods goods)
        {
            string result = StringTemplate.Render(message, new GameStoreVariables()
            {
                EntityId = player.EntityId,
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                GoodsId = goods.Id,
                GoodsName = goods.Name,
                Price = goods.Price,
            });
            return result;
        }
    }
}