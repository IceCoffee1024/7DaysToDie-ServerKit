using SdtdServerKit.Managers;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Send a message to a single player.
    /// </summary>
    public class SayToPlayer : ConsoleCmdBase
    {
        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        /// <returns>The description of the command.</returns>
        public override string getDescription()
        {
            return "Sends a message to a single player.";
        }

        /// <summary>
        /// Gets the help text for the command.
        /// </summary>
        /// <returns>The help text for the command.</returns>
        public override string getHelp()
        {
            return "Usage:\n" +
                   "  1. ty-pm {PlayerId/EntityId/PlayerName} {Message} {SenderName}\n" +
                   "1. Sends a PM to the player given by the entity id or player id or player name (as given by e.g. `lpi`).";
        }

        /// <summary>
        /// Gets the list of commands associated with the command.
        /// </summary>
        /// <returns>The list of commands associated with the command.</returns>
        public override string[] getCommands()
        {
            return new[] { "ty-SayToPlayer", "ty-pm" };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="args">The list of arguments passed to the command.</param>
        /// <param name="senderInfo">The information of the command sender.</param>
        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            if (args.Count < 2)
            {
                Log(getHelp());
                return;
            }

            int senderEntityId;
                                     
            if (senderInfo.RemoteClientInfo == null) // From console.
            {
                senderEntityId = -1;
            }
            else // From game client.
            {
                senderEntityId = senderInfo.RemoteClientInfo.entityId;
            }

            var receiver = ConsoleHelper.ParseParamIdOrName(args[0]);
            if (receiver == null)
            {
                Log("Unable to locate player '{0}' online", args[0]);
                return;
            }

            string message = args[1];
            string senderName;
            if (args.Count > 2)
            {
                senderName = args[2];
            }
            else
            {
                senderName = ConfigManager.GlobalSettings.WhisperServerName;
            }

            if (string.IsNullOrEmpty(senderName))
            {
                senderName = Localization.Get("xuiChatServer", false);
            }
            
            message = global::Utils.CreateGameMessage(senderName, message);

            GameManager.Instance.ChatMessageServer(
                ModApi.CmdExecuteDelegate,
                EChatType.Whisper,
                _senderEntityId: senderEntityId,
                message,
                new List<int>() { receiver.entityId },
                EMessageSender.None);
        }
    }
}