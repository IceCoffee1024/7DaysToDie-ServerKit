using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using SdtdServerKit.Triggers;
using SdtdServerKit.Variables;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 游戏公告
    /// </summary>
    public class GameNotice : FunctionBase<GameNoticeSettings>
    {
        private readonly SubTimer _timer;
        private int _lastRotatingIndex;
        /// <inheritdoc/>
        public GameNotice()
        {
            _timer = new SubTimer(SendRotatingNotice);
        }
        /// <inheritdoc/>
        protected override void OnDisableFunction()
        {
            GlobalTimer.UnregisterSubTimer(_timer);
            ModEventHub.PlayerSpawnedInWorld -= OnPlayerSpawnedInWorld;
            ModEventHub.SkyChanged -= OnSkyChanged;
        }
        /// <inheritdoc/>
        protected override void OnEnableFunction()
        {
            GlobalTimer.RegisterSubTimer(_timer);
            ModEventHub.PlayerSpawnedInWorld += OnPlayerSpawnedInWorld;
            ModEventHub.SkyChanged += OnSkyChanged;
        }
        /// <inheritdoc/>
        protected override void OnSettingsChanged()
        {
            _timer.Interval = Settings.RotatingInterval;
            _timer.IsEnabled = Settings.IsEnabled;
            _lastRotatingIndex = 0;
        }

        private void OnPlayerSpawnedInWorld(SpawnedPlayer spawnedPlayer)
        {
            try
            {
                switch (spawnedPlayer.RespawnType)
                {
                    // New player spawning
                    case Models.RespawnType.EnterMultiplayer:
                    // Old player spawning
                    case Models.RespawnType.JoinMultiplayer:
                        SendMessageToPlayer(spawnedPlayer.PlayerId, FormatCmd(Settings.WelcomeNotice, spawnedPlayer));
                        break;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Error in GameNotice.PlayerSpawnedInWorld");
            }
        }

        private void SendRotatingNotice()
        {
            try
            {
                if(LivePlayerManager.Count == 0)
                {
                    return;
                }

                var rotatingNotices = Settings.RotatingNotices;
                if (rotatingNotices != null && rotatingNotices.Length > 0)
                {
                    if(_lastRotatingIndex >= rotatingNotices.Length)
                    {
                        _lastRotatingIndex = 0;
                    }

                    string message = rotatingNotices[_lastRotatingIndex];
                    if(string.IsNullOrEmpty(message) == false)
                    {
                        SendGlobalMessage(FormatCmd(message));
                    }
                    
                    _lastRotatingIndex++;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Error in GameNotice.SendRotatingNotice");
            }
        }

        private string FormatCmd(string message, SpawnedPlayer? spawnedPlayer = null)
        {
            var changed = SkyChangeTrigger.LatestSkyState;
            var gameNoticeVariables = new GameNoticeVariables()
            {
                BloodMoonDays = changed.BloodMoonDaysRemaining,
                BloodMoonStartTime = changed.DuskHour + ":00",
                BloodMoonEndTime = changed.DawnHour + ":00"
            };

            message = StringTemplate.Render(message, gameNoticeVariables);

            if (spawnedPlayer == null)
            {
                return message;
            }

            return base.FormatCmd(message, spawnedPlayer);
        }

        private void OnSkyChanged(SkyChanged changed)
        {
            try
            {
                var gameNoticeVariables = new GameNoticeVariables()
                {
                    BloodMoonDays = changed.BloodMoonDaysRemaining,
                    BloodMoonStartTime = changed.DuskHour + ":00",
                    BloodMoonEndTime = changed.DawnHour + ":00"
                };

                // 黎明
                if (changed.SkyChangeEventType == SkyChangeEventType.Dawn)
                {
                    if (changed.BloodMoonDaysRemaining == 0)
                    {
                        if(string.IsNullOrEmpty(Settings.BloodMoonNotice2) == false)
                        {
                            SendGlobalMessage(StringTemplate.Render(Settings.BloodMoonNotice2, gameNoticeVariables));
                        }
                    }
                    else
                    {
                        if(string.IsNullOrEmpty(Settings.BloodMoonNotice1) == false)
                        {
                            SendGlobalMessage(StringTemplate.Render(Settings.BloodMoonNotice1, gameNoticeVariables));
                        }
                    }
                }
                // 黄昏
                else if (changed.SkyChangeEventType == SkyChangeEventType.Dusk)
                {
                    if (changed.BloodMoonDaysRemaining == 0)
                    {
                        if (string.IsNullOrEmpty(Settings.BloodMoonNotice3) == false)
                        {
                            SendGlobalMessage(StringTemplate.Render(Settings.BloodMoonNotice3, gameNoticeVariables));
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(Settings.BloodMoonNotice1) == false)
                        {
                            SendGlobalMessage(StringTemplate.Render(Settings.BloodMoonNotice1, gameNoticeVariables));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Warn(ex, "Error in GameNotice.OnSkyChanged");
            }
        }

    }
}