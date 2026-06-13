using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using SdtdServerKit.Variables;
using Webserver.WebAPI.APIs.WorldState;

namespace SdtdServerKit.Functions
{
    public class TeleportHome : FunctionBase<TeleportHomeSettings>
    {
        private readonly IHomeLocationRepository _homeLocationRepository;
        private readonly IPointsInfoRepository _pointsRepository;
        private readonly ITeleRecordRepository _teleRecordRepository;
        /// <inheritdoc/>
        public TeleportHome(
            IPointsInfoRepository pointsInfoRepository,
            ITeleRecordRepository teleRecordRepository,
            IHomeLocationRepository homeLocationRepository)
        {
            _pointsRepository = pointsInfoRepository;
            _teleRecordRepository = teleRecordRepository;
            _homeLocationRepository = homeLocationRepository;
        }
        /// <inheritdoc/>
        protected override async Task<bool> OnChatCmd(string message, ManagedPlayer managedPlayer)
        {
            if (string.Equals(message, Settings.QueryListCmd, StringComparison.OrdinalIgnoreCase))
            {
                int entityId = managedPlayer.EntityId;
                string playerId = managedPlayer.PlayerId;
                var positions = await _homeLocationRepository.GetByPlayerIdAsync(playerId);

                if (positions.Any() == false)
                {
                    SendMessageToPlayer(playerId, Settings.NoHomeTip);
                }
                else
                {
                    foreach (var item in positions)
                    {
                        SendMessageToPlayer(playerId, FormatCmd(Settings.LocationItemTip, managedPlayer, item));
                    }
                }

                return true;
            }
            else if (message.StartsWith(Settings.SetHomeCmdPrefix + ConfigManager.GlobalSettings.ChatCommandSeparator, StringComparison.OrdinalIgnoreCase))
            {
                string homeName = message.Substring(Settings.SetHomeCmdPrefix.Length + ConfigManager.GlobalSettings.ChatCommandSeparator.Length);
                int entityId = managedPlayer.EntityId;
                string playerId = managedPlayer.PlayerId;

                int playerPoints = await _pointsRepository.GetPointsByIdAsync(playerId);
                if (playerPoints < Settings.PointsRequiredForSet)
                {
                    SendMessageToPlayer(playerId, FormatCmd(Settings.SetPointsNotEnoughTip, managedPlayer));
                }
                else
                {
                    var entity = await _homeLocationRepository.GetByPlayerIdAndHomeNameAsync(playerId, homeName);
                    string position = Utilities.Utils.GetPlayerPosition(managedPlayer.EntityId).ToString();

                    // 先扣积分，失败则不执行操作
                    int deductResult = await _pointsRepository.ChangePointsAsync(playerId, -Settings.PointsRequiredForSet);
                    if (deductResult == 0)
                    {
                        SendMessageToPlayer(playerId, FormatCmd(Settings.SetPointsNotEnoughTip, managedPlayer));
                        return true;
                    }

                    // new home postion
                    if (entity == null)
                    {
                        var positionCount = await _homeLocationRepository.GetRecordCountByPlayerIdIdAsync(playerId);
                        if (positionCount >= Settings.SetCountLimit)
                        {
                            // 超出限制，退还积分
                            await _pointsRepository.ChangePointsAsync(playerId, Settings.PointsRequiredForSet);
                            SendMessageToPlayer(playerId, FormatCmd(Settings.OverLimitTip, managedPlayer));
                            return true;
                        }
                        else
                        {
                            entity = new T_HomeLocation()
                            {
                                HomeName = homeName,
                                PlayerId = playerId,
                                CreatedAt = DateTime.Now,
                                PlayerName = managedPlayer.PlayerName,
                                Position = position
                            };
                            await _homeLocationRepository.InsertAsync(entity);
                            SendMessageToPlayer(playerId, FormatCmd(Settings.SetSuccessTip, managedPlayer, entity));
                        }
                    }
                    else
                    {
                        entity.HomeName = homeName;
                        entity.PlayerName = managedPlayer.PlayerName;
                        entity.Position = position;
                        await _homeLocationRepository.UpdateAsync(entity);
                        SendMessageToPlayer(playerId, FormatCmd(Settings.OverwriteSuccessTip, managedPlayer, entity));
                    }

                    CustomLogger.Debug("Player: {0}, entityId: {1}, set home: {2}, position: {3}", managedPlayer.PlayerName, entityId, homeName, position);
                }

                return true;
            }
            else if (message.StartsWith(Settings.DeleteHomeCmdPrefix + ConfigManager.GlobalSettings.ChatCommandSeparator, StringComparison.OrdinalIgnoreCase))
            {
                string homeName = message.Substring(Settings.DeleteHomeCmdPrefix.Length + ConfigManager.GlobalSettings.ChatCommandSeparator.Length);
                int entityId = managedPlayer.EntityId;
                string playerId = managedPlayer.PlayerId;
                int count = await _homeLocationRepository.DeleteByPlayerIdAndHomeNameAsync(playerId, homeName);
                if (count != 1)
                {
                    SendMessageToPlayer(playerId, Settings.HomeNotFoundTip);
                }
                else
                {
                    SendMessageToPlayer(playerId, Settings.DeleteSuccessTip);
                }

                return true;
            }
            else if (message.StartsWith(Settings.TeleHomeCmdPrefix + ConfigManager.GlobalSettings.ChatCommandSeparator, StringComparison.OrdinalIgnoreCase))
            {
                string homeName = message.Substring(Settings.TeleHomeCmdPrefix.Length + ConfigManager.GlobalSettings.ChatCommandSeparator.Length);
                int entityId = managedPlayer.EntityId;
                string playerId = managedPlayer.PlayerId;

                var entity = await _homeLocationRepository.GetByPlayerIdAndHomeNameAsync(playerId, homeName);
                if (entity == null)
                {
                    SendMessageToPlayer(playerId, Settings.HomeNotFoundTip);
                }
                else
                {
                    var teleRecord = await _teleRecordRepository.GetNewestAsync(playerId, TeleTargetType.Home);
                    if (teleRecord != null)
                    {
                        int timeSpan = (int)(DateTime.Now - teleRecord.CreatedAt).TotalSeconds;
                        if (timeSpan < Settings.TeleInterval)// Cooling
                        {
                            SendMessageToPlayer(playerId, FormatCmd(Settings.CoolingTip, managedPlayer, entity, Settings.TeleInterval - timeSpan));
                            return true;
                        }
                    }

                    int points = await _pointsRepository.GetPointsByIdAsync(playerId);
                    if (points < Settings.PointsRequiredForTele)// Points not enough
                    {
                        SendMessageToPlayer(playerId, FormatCmd(Settings.TelePointsNotEnoughTip, managedPlayer, entity));
                    }
                    else
                    {
                        if (ConfigManager.GlobalSettings.TeleZombieCheck &&
                            GameManager.Instance.World.Players.dict.TryGetValue(managedPlayer.EntityId, out EntityPlayer player))
                        {
                            if (Utilities.Utils.ZombieCheck(player))
                            {
                                SendMessageToPlayer(playerId, ConfigManager.GlobalSettings.TeleDisableTip);
                                return true;
                            }
                        }

                        await _pointsRepository.ChangePointsAsync(playerId, -Settings.PointsRequiredForTele);
                        Utilities.Utils.TeleportPlayer(managedPlayer.EntityId.ToString(), entity.Position);

                        SendMessageToPlayer(managedPlayer.PlayerId, FormatCmd(Settings.TeleSuccessTip, managedPlayer, entity));
                        await _teleRecordRepository.InsertAsync(new T_TeleRecord()
                        {
                            CreatedAt = DateTime.Now,
                            PlayerId = playerId,
                            PlayerName = managedPlayer.PlayerName,
                            OriginPosition = Utilities.Utils.GetPlayerPosition(managedPlayer.EntityId).ToString(),
                            TargetPosition = entity.Position,
                            TargetType = TeleTargetType.Home.ToString(),
                            TargetName = entity.HomeName
                        });

                        CustomLogger.Debug("Player: {0}, entityId: {1}, teleported to: {2}", managedPlayer.PlayerName, entityId, entity.HomeName);
                    }
                }

                return true;
            }

            return false;
        }

        private string FormatCmd(string message, ManagedPlayer player, T_HomeLocation position, int cooldownSeconds = 0)
        {
            return StringTemplate.Render(message, new TeleportHomeVariables()
            {
                EntityId = player.EntityId,
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                CooldownSeconds = cooldownSeconds,
                HomeName = position.HomeName,
                TeleInterval = Settings.TeleInterval,
            });
        }
    }
}