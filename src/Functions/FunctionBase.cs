using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Hooks;
using SdtdServerKit.Managers;
using SdtdServerKit.Variables;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// Function base class
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    public abstract class FunctionBase<TSettings> : IFunction where TSettings : ISettings
    {
        /// <summary>
        /// Settings
        /// </summary>
        public TSettings Settings
        {
            get
            {
                if (_settings == null)
                {
                    throw new InvalidOperationException($"The settings for function '{_functionName}' is null!");
                }

                return _settings;
            }
        }

        private readonly string _functionName;
        private bool _isRunning;
        private TSettings? _settings;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// Function base constructor
        /// </summary>
        public FunctionBase()
        {
            _functionName = this.GetType().Name;
        }

        void IFunction.LoadSettings()
        {
            string language;
            try
            {
                language = GamePrefs.GetString(EnumGamePrefs.Language);
            }
            catch
            {
                language = Language.English.ToString();
            }

            _settings = ConfigManager.GetRequired<TSettings>(Locales.Get(language), persist: false);
            if (_settings != null)
            {
                OnSettingsChanged(_settings);
            }
            ConfigManager.SettingsChanged += OnSettingsChanged;
        }

        /// <summary>
        /// Function name
        /// </summary>
        public string Name => _functionName;

        /// <summary>
        /// Format command
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="player">Player</param>
        /// <returns>Formatted command</returns>
        protected virtual string FormatCmd(string cmd, IPlayerBase player)
        {
            return StringTemplate.Render(cmd, new VariablesBase()
            {
                PlayerId = player.PlayerId,
                PlayerName = player.PlayerName,
                EntityId = player.EntityId
            });
        }

        /// <summary>
        /// Called when capturing player chat messages, returns true if the message is handled by the current function
        /// </summary>
        /// <param name="cmd">Command</param>
        /// <param name="managedPlayer">Managed player</param>
        /// <returns>True if the message is handled by the current function</returns>
        protected virtual Task<bool> OnChatCmd(string cmd, ManagedPlayer managedPlayer)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Disabled function
        /// </summary>
        protected virtual void OnDisableFunction()
        {
            ChatMessageHook.RemoveHook(OnChatCmd);
            PlayerLevelUpHook.RemoveHook(OnPlayerLevelUp);
        }

        /// <summary>
        /// Enabled function
        /// </summary>
        protected virtual void OnEnableFunction()
        {
            ChatMessageHook.AddHook(OnChatCmd);
            PlayerLevelUpHook.AddHook(OnPlayerLevelUp);
        }

        protected virtual Task OnPlayerLevelUp(ManagedPlayer managedPlayer, int newLevel)
        {
            return Task.CompletedTask;
        }

        ///// <summary>
        ///// Enabled function, regardless of whether there are players on the server, the return value will be set to _isRunning
        ///// </summary>
        //protected virtual bool OnEnableFunctionNonePlayer()
        //{
        //    return false;
        //}

        /// <summary>
        /// If the settings are changed, update the settings and call the protected OnSettingsChanged method
        /// </summary>
        /// <param name="settings">Settings</param>
        private void OnSettingsChanged(ISettings settings)
        {
            if (settings is TSettings changedSettings)
            {
                lock (this)
                {
                    _settings = changedSettings;
                    if (_settings.IsEnabled)
                    {
                        PrivateEnableFunction();
                    }
                    else
                    {
                        PrivateDisableFunction();
                    }
                    OnSettingsChanged();
                }
            }
        }

        /// <summary>
        /// Called when the configuration changes, will be automatically called once during function initialization
        /// </summary>
        protected virtual void OnSettingsChanged()
        {
        }

        /// <summary>
        /// Send global message
        /// </summary>
        /// <param name="message">Message</param>
        protected void SendGlobalMessage(string message)
        {
            Utilities.Utils.SendGlobalMessage(new GlobalMessage()
            {
                Message = message,
                SenderName = ConfigManager.GlobalSettings.GlobalServerName
            });
        }

        /// <summary>
        /// Send private message
        /// </summary>
        /// <param name="playerIdOrName">Player ID or name</param>
        /// <param name="message">Message</param>
        protected void SendMessageToPlayer(string playerIdOrName, string message)
        {
            Utilities.Utils.SendPrivateMessage(new PrivateMessage()
            {
                TargetPlayerIdOrName = playerIdOrName,
                Message = message,
                SenderName = ConfigManager.GlobalSettings.WhisperServerName
            });
        }

        /// <summary>
        /// Prevent duplicate settings
        /// </summary>
        private void PrivateDisableFunction()
        {
            // If the function is not disabled
            if (_isRunning)
            {
                _isRunning = false;
                OnDisableFunction();
            }
        }

        /// <summary>
        /// Prevent duplicate settings
        /// </summary>
        private void PrivateEnableFunction()
        {
            // If the function is not running.
            if (_isRunning == false)
            {
                //_isRunning = OnEnableFunctionNonePlayer();

                // Only there are players on the server.
                //if (ConnectionManager.Instance.Clients.Count > 0)
                {
                    _isRunning = true;
                    OnEnableFunction();
                }
            }
        }
    }
}