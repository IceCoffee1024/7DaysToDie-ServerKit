using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Represents a command to set player colored chat.
    /// </summary>
    public class SetPlayerColoredChat : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Set player colored chat.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-SetPlayerColoredChat {PlayerId/EntityId/PlayerName} {NameColor} {TextColor} {CustomName} {Description}\n" +
                "  2. ty-SetPlayerColoredChat {PlayerId/EntityId/PlayerName} {NameColor} {TextColor} {CustomName}\n" +
                "  3. ty-SetPlayerColoredChat {PlayerId/EntityId/PlayerName} {NameColor} {TextColor}\n" +
                "  4. ty-SetPlayerColoredChat {PlayerId/EntityId/PlayerName} {NameColor}\n" +
                "1. Set player colored chat with name color, text color, custom name and description.\n" +
                "2. Set player colored chat with name color, text color and custom name.\n" +
                "3. Set player colored chat with name color and text color.\n" +
                "4. Set player colored chat with name color.";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-SetPlayerColoredChat",
                "ty-spcc"
            };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            if (args.Count < 2)
            {
                Log("Wrong number of arguments.");
                Log(getHelp());
                return;
            }

            try
            {
                var cInfo = ConsoleHelper.ParseParamIdOrName(args[0]);
                string? playerId = cInfo?.CrossplatformId.CombinedString ?? PlatformUserIdentifierAbs.FromCombinedString(args[0])?.CombinedString;

                if (playerId == null)
                {
                    Log("Unable to locate player '{0}' online or offline", args[0]);
                    return;
                }

                var entity = new T_ColoredChat()
                {
                    Id = playerId,
                    NameColor = args[1],
                    TextColor = args[2],
                    CustomName = args.Count > 3 ? args[3] : null,
                    Description = args.Count > 4 ? args[4] : null,
                    CreatedAt = DateTime.Now,
                };

                var repository = ModApi.ServiceContainer.Resolve<IColoredChatRepository>();
                bool result = repository.InsertOrReplace(entity) == 1;
                if (result == false)
                {
                    Log("Failed to set player '{0}' colored chat.", playerId);
                }
            }
            catch (Exception ex)
            {
                Log("Error in SetPlayerColoredChat.Execute:" + ex.Message);
            }
        }
    }
}