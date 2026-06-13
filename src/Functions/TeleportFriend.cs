using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using SdtdServerKit.Models;
using SdtdServerKit.Variables;
using System.Collections.Concurrent;
using Webserver.WebAPI.APIs.WorldState;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// Teleport Friend
    /// </summary>
    public class TeleportFriend : FunctionBase<TeleportFriendSettings>
    {
        class TeleRequest
        {
            public ManagedPlayer SourcePlayer { get; set; }
            public DateTime CreatedAt { get; set; }

            public TeleRequest(ManagedPlayer sourcePlayer, DateTime createdAt)
            {
                SourcePlayer = sourcePlayer;
                CreatedAt = createdAt;
            }
        }
        private readonly SubTimer _timer;
        private readonly IPointsInfoRepository _pointsRepository;
        private readonly ITeleRecordRepository _teleRecordRepository;
        private readonly ConcurrentDictionary<ManagedPlayer, TeleRequest> _teleRequests = new();

        /// <inheritdoc/>
        public TeleportFriend(
            IPointsInfoRepository pointsRepository, 
            ITeleRecordRepository teleRecordRepository)
        {
            _pointsRepository = pointsRepository;
            _teleRecordRepository = teleRecordRepository;
            _timer = new SubTimer(CheckTeleRequest);
        }
        /// <inheritdoc/>
        protected override void OnDisableFunction()
        {
            base.OnDisableFunction();
            GlobalTimer.UnregisterSubTimer(_timer);
        }
        /// <inheritdoc/>
        protected override void OnEnableFunction()
        {
            base.OnEnableFunction();
            GlobalTimer.RegisterSubTimer(_timer);
        }
        /// <inheritdoc/>
        protected override void OnSettingsChanged()
        {
            _timer.IsEnabled = Settings.IsEnabled;
        }
        private void CheckTeleRequest()
        {
            if(_teleRequests.IsEmpty)
            {
                return;
            }

            DateTime now = DateTime.Now;
            foreach (var kv in _teleRequests)
            {
                if((now - kv.Value.CreatedAt).TotalSeconds > Settings.KeepDuration)
                {
                    if(_teleRequests.TryRemove(kv.Key, out var teleRequest))
                    {
                        SendMessageToPlayer(teleRequest.SourcePlayer.PlayerId, FormatCmd(Settings.TargetRejectTeleTip, teleRequest.SourcePlayer, kv.Key.PlayerName));
                    }
                }
            }
        }

        private async Task Tele(ManagedPlayer managedPlayer, ManagedPlayer targetPlayer)
        {
            string srcPlayerId = managedPlayer.PlayerId;
            string targetName = targetPlayer.PlayerName;

            int result = await _pointsRepository.ChangePointsAsync(srcPlayerId, -Settings.PointsRequired);
            if (result == 0)
            {
                SendMessageToPlayer(srcPlayerId, FormatCmd(Settings.PointsNotEnoughTip, managedPlayer, targetName));
                return;
            }

            Utilities.Utils.TeleportPlayer(managedPlayer.EntityId.ToString(), targetPlayer.EntityId.ToString());
            string messageToPlayer = FormatCmd(Settings.TeleSuccessTip, managedPlayer, targetName);
            SendMessageToPlayer(srcPlayerId, messageToPlayer);
            SendMessageToPlayer(targetPlayer.PlayerId, messageToPlayer);

            await _teleRecordRepository.InsertAsync(new T_TeleRecord()
            {
                CreatedAt = DateTime.Now,
                PlayerId = srcPlayerId,
                PlayerName = managedPlayer.PlayerName,
                TargetName = targetName,
                TargetType = TeleTargetType.Friend.ToString(),
                OriginPosition = Utilities.Utils.GetPlayerPosition(managedPlayer.EntityId).ToString(),
                TargetPosition = Utilities.Utils.GetPlayerPosition(targetPlayer.EntityId).ToString(),
            });

            CustomLogger.Debug("Player: {0}, entityId: {1}, teleported to: {2}", managedPlayer.PlayerName, managedPlayer.EntityId, targetName);
        }

        /// <inheritdoc/>
        protected override async Task<bool> OnChatCmd(string message, ManagedPlayer managedPlayer)
        {
            if (message.StartsWith(Settings.TeleCmdPrefix + ConfigManager.GlobalSettings.ChatCommandSeparator, StringComparison.OrdinalIgnoreCase))
            {
                string targetName = message.Substring(Settings.TeleCmdPrefix.Length + ConfigManager.GlobalSettings.ChatCommandSeparator.Length);
                string playerId = managedPlayer.PlayerId;

                var targetClient = ConnectionManager.Instance.Clients.GetForPlayerName(targetName);
                ManagedPlayer targetPlayer;
                if (targetClient == null || LivePlayerManager.TryGetByEntityId(targetClient.entityId, out targetPlayer!) == false)
                {
                    SendMessageToPlayer(playerId, FormatCmd(Settings.TargetNotFoundTip, managedPlayer, targetName));
                    return true;
                }

                var teleRecord = await _teleRecordRepository.GetNewestAsync(playerId, TeleTargetType.Friend);
                if (teleRecord != null)
                {
                    int timeSpan = (int)(DateTime.Now - teleRecord.CreatedAt).TotalSeconds;
                    if (timeSpan < Settings.TeleInterval)// Cooling
                    {
                        SendMessageToPlayer(playerId, FormatCmd(Settings.CoolingTip, managedPlayer, targetName, Settings.TeleInterval - timeSpan));
                        return true;
                    }
                }

                int pointsCount = await _pointsRepository.GetPointsByIdAsync(playerId);
                if (pointsCount < Settings.PointsRequired)// Points not enough
                {
                    SendMessageToPlayer(playerId, FormatCmd(Settings.PointsNotEnoughTip, managedPlayer, targetName));
                    return true;
                }

                if (ConfigManager.GlobalSettings.TeleZombieCheck &&
                    GameManager.Instance.World.Players.dict.TryGetValue(managedPlayer.EntityId, out EntityPlayer player))
                {
                    if (Utilities.Utils.ZombieCheck(player))
                    {
                        SendMessageToPlayer(playerId, ConfigManager.GlobalSettings.TeleDisableTip);
                        return true;
                    }
                }

                if (Settings.IsFriendBypass && Utilities.Utils.IsFriend(managedPlayer.EntityId, targetPlayer.EntityId))
                {
                    await Tele(managedPlayer, targetPlayer);
                }
                else
                {
                    _teleRequests.TryAdd(targetPlayer, new TeleRequest(managedPlayer, DateTime.Now));
                    SendMessageToPlayer(targetPlayer.PlayerId, FormatCmd(Settings.TeleConfirmTip, managedPlayer, targetName));
                }

                return true;
            }
            else if (message.Equals(Settings.RejectTele, StringComparison.OrdinalIgnoreCase))
            {
                if (_teleRequests.TryRemove(managedPlayer, out var teleRequest))
                {
                    SendMessageToPlayer(teleRequest.SourcePlayer.PlayerId, FormatCmd(Settings.TargetRejectTeleTip, teleRequest.SourcePlayer, managedPlayer.PlayerName));
                }
                return true;
            }
            else if (message.Equals(Settings.AcceptTele, StringComparison.OrdinalIgnoreCase))
            {
                if (_teleRequests.TryRemove(managedPlayer, out var teleRequest))
                {
                    await Tele(teleRequest.SourcePlayer, managedPlayer);
                }
            }

            return false;
        }

        private string FormatCmd(string message, ManagedPlayer player, string targetName, int coolingTime = 0)
        {
            return StringTemplate.Render(message, new TeleportFriendVariables()
            {
                EntityId = player.EntityId,
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                PointsRequired = Settings.PointsRequired,
                TargetName = targetName,
                TeleInterval = Settings.TeleInterval,
                CooldownSeconds = coolingTime
            });
        }
    }
}