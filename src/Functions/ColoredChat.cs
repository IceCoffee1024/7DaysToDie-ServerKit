using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;
using SdtdServerKit.Models;
using static ModEvents;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// Colored Chat Function
    /// </summary>
    public class ColoredChat : FunctionBase<ColoredChatSettings>
    {
        private readonly IColoredChatRepository _coloredChatRepository;

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="coloredChatRepository"></param>
        public ColoredChat(IColoredChatRepository coloredChatRepository)
        {
            _coloredChatRepository = coloredChatRepository;
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        protected override void OnSettingsChanged()
        {
            if (Settings.IsEnabled)
            {
                ModEvents.ChatMessage.RegisterHandler(OnChatMessage);
            }
            else
            {
                ModEvents.ChatMessage.UnregisterHandler(OnChatMessage);
            }
        }

        //private bool OnChatMessage(ClientInfo? clientInfo, EChatType eChatType, int senderEntityId, string message, string mainName, List<int> recipientEntityIds)
        private EModEventResult OnChatMessage(ref SChatMessageData sChatMessageData)
        {
            var clientInfo = sChatMessageData.ClientInfo;
            string message = sChatMessageData.Message;
            int senderEntityId = sChatMessageData.SenderEntityId;
            string? mainName = sChatMessageData.MainName;
            EChatType eChatType = sChatMessageData.ChatType;
            var recipientEntityIds = sChatMessageData.RecipientEntityIds;

            if (LivePlayerManager.TryGetByEntityId(senderEntityId, out var managedPlayer) && managedPlayer != null)
            {
                string playerName = managedPlayer.PlayerName;
                var coloredChat = _coloredChatRepository.GetById(managedPlayer.PlayerId);
                if (coloredChat != null)
                {
                    if(string.IsNullOrEmpty(coloredChat.CustomName) == false)
                    {
                        //playerName = StringTemplate.Render(coloredChat.CustomName!, new { PlayerName = playerName });
                        playerName = coloredChat.CustomName!.Replace("{PlayerName}", playerName);
                    }

                    message = $"[{coloredChat.NameColor}]{playerName}[{GetDefaultColor(eChatType)}]: [{coloredChat.TextColor}]{message}";
                }
                else
                {
                    message = $"[{GetDefaultColor(eChatType)}]{playerName}: {message}";
                }

                if (recipientEntityIds != null)
                {
                    foreach (var entityId in recipientEntityIds)
                    {
                        if (LivePlayerManager.TryGetByEntityId(entityId, out var recipientPlayer))
                        {
                            recipientPlayer!.ClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChat>()
                                .Setup(eChatType, -1, message, null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported));
                        }
                    }
                }
                else
                {
                    ConnectionManager.Instance.SendPackage(NetPackageManager.GetPackage<NetPackageChat>()
                        .Setup(eChatType, -1, message, null, EMessageSender.None, GeneratedTextManager.BbCodeSupportMode.Supported), true, -1, -1, -1, null, 192);
                }

                return EModEventResult.StopHandlersAndVanilla;
            }

            return EModEventResult.Continue;
        }

        private string GetDefaultColor(EChatType eChatType)
        {
            switch (eChatType)
            {
                case EChatType.Global:
                    return !string.IsNullOrEmpty(Settings.GlobalDefault) ? Settings.GlobalDefault! : "FFFFFF";
                case EChatType.Whisper:
                    return !string.IsNullOrEmpty(Settings.WhisperDefault) ? Settings.WhisperDefault! : "D00000";
                case EChatType.Friends:
                    return !string.IsNullOrEmpty(Settings.FriendsDefault) ? Settings.FriendsDefault! : "00BB00";
                case EChatType.Party:
                    return !string.IsNullOrEmpty(Settings.PartyDefault) ? Settings.PartyDefault! : "FFCC00";
                default:
                    throw new ArgumentOutOfRangeException(nameof(eChatType), eChatType, null);
            }
        }
    }
}
