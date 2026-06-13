using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using SdtdServerKit.Variables;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// CdKeyRedeem function
    /// </summary>
    public class CdKeyRedeem : FunctionBase<CdKeyRedeemSettings>
    {
        private readonly ICdKeyRepository _cdKeyRepository;
        private readonly ICdKeyRedeemRecordRepository _cdKeyRedeemRecordRepository;
        private readonly IItemListRepository _itemListRepository;
        private readonly ICommandListRepository _commandListRepository;
        private readonly IPointsInfoRepository _pointsInfoRepository;

        /// <inheritdoc/>
        public CdKeyRedeem(ICdKeyRepository cdKeyRepository, IItemListRepository itemListRepository, ICommandListRepository commandListRepository, ICdKeyRedeemRecordRepository cdKeyRedeemRecordRepository, IPointsInfoRepository pointsInfoRepository)
        {
            _cdKeyRepository = cdKeyRepository;
            _itemListRepository = itemListRepository;
            _commandListRepository = commandListRepository;
            _cdKeyRedeemRecordRepository = cdKeyRedeemRecordRepository;
            _pointsInfoRepository = pointsInfoRepository;
        }
        /// <inheritdoc/>
        protected override async Task<bool> OnChatCmd(string message, ManagedPlayer managedPlayer)
        {
            var cdKey = await _cdKeyRepository.GetByKeyAsync(message);
            if (cdKey == null)
            {
                return false;
            }
            else
            {
                string playerId = managedPlayer.PlayerId;

                if (cdKey.MaxRedeemCount > 0 && cdKey.RedeemCount >= cdKey.MaxRedeemCount)
                {
                    SendMessageToPlayer(playerId, Settings.HasReachedMaxRedemptionLimitTip);
                    return true;
                }

                if (cdKey.ExpiryAt.HasValue && cdKey.ExpiryAt.Value < DateTime.Now)
                {
                    SendMessageToPlayer(playerId, Settings.HasRedemptionCodeExpiredTip);
                    return true;
                }

                var cdKeyRedeemRecord = await _cdKeyRedeemRecordRepository.GetByKeyAndPlayerIdAsync(cdKey.Key, playerId);
                if (cdKeyRedeemRecord != null)
                {
                    SendMessageToPlayer(playerId, Settings.HasAlreadyRedeemedTip);
                    return true;
                }

                int rowsAffected = await _cdKeyRepository.UpdateRedeemCount(cdKey.Id);
                if (rowsAffected == 0)
                {
                    SendMessageToPlayer(playerId, Settings.HasAlreadyRedeemedTip);
                    return true;
                }

                var itemList = await _itemListRepository.GetListByCdKeyIdAsync(cdKey.Id);
                foreach (var item in itemList)
                {
                    if (item.RewardType == 2)
                    {
                        await _pointsInfoRepository.ChangePointsAsync(playerId, item.Count, allowNegative: true);
                        int balance = await _pointsInfoRepository.GetPointsByIdAsync(playerId);
                        Managers.PointLogger.Log(Models.PointLogCategory.CdKey, playerId, managedPlayer.PlayerName,
                            item.Count, balance, $"CDKey 兑换: {cdKey.Key}");
                    }
                    else
                    {
                        Utilities.Utils.GiveItem(playerId, item.ItemName, item.Count, item.Quality, item.Durability);
                    }
                    await Task.Delay(20);
                }

                var commandList = await _commandListRepository.GetListByCdKeyIdAsync(cdKey.Id);
                foreach (var item in commandList)
                {
                    foreach (var cmd in item.Command.Split('\n'))
                    {
                        Utilities.Utils.ExecuteConsoleCommand(FormatCmd(cmd, managedPlayer, cdKey), item.InMainThread);
                    }
                    await Task.Delay(20);
                }

                var redeemRecord = new CdKeyRedeemRecord()
                {
                    CreatedAt = DateTime.Now,
                    Key = cdKey.Key,
                    PlayerId = playerId,
                    PlayerName = managedPlayer.PlayerName,
                };
                await _cdKeyRedeemRecordRepository.InsertAsync(redeemRecord);

                SendMessageToPlayer(playerId, FormatCmd(Settings.RedeemSuccessTip, managedPlayer, cdKey));

                CustomLogger.Info("PlayerId: {0}, playerName: {1}, {1} redeemed: {2}.", playerId, managedPlayer.PlayerName, cdKey.Key);
            }

            return true;
        }

        private static string FormatCmd(string message, ManagedPlayer player, CdKey cdKey)
        {
            string result = StringTemplate.Render(message, new CdKeyRedeemVariables()
            {
                EntityId = player.EntityId,
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                CdKeyDescription = cdKey.Description,
            });
            return result;
        }
    }
}