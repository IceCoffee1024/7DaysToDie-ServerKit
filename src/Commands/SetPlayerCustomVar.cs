namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Set player custom var.
    /// </summary>
    public class SetPlayerCustomVar : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Set player custom var.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-setcvar {PlayerId/EntityId/PlayerName} {cvarName} {cvarValue}" +
                "  2. ty-setcvar {cvarName} {cvarValue}" +
                "1. Set player custom var." +
                "2. Set yourself custom var.";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-SetPlayerCustomVar",
                "ty-spcv",
                "ty-setcvar"
            };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            ClientInfo cInfo;
            string name, value;
            if (args.Count == 2)
            {
                cInfo = _senderInfo.RemoteClientInfo;
                name = args[0];
                value = args[1];
            }
            else if (args.Count == 3)
            {
                cInfo = ConsoleHelper.ParseParamIdOrName(args[0]);
                if (cInfo == null)
                {
                    Log("Unable to locate player '{0}' online", args[0]);
                    return;
                }
                name = args[1];
                value = args[2];
            }
            else
            {
                Log("Wrong number of arguments.");
                Log(getHelp());
                return;
            }

            int entityId = cInfo!.entityId;
            if (GameManager.Instance.World.Players.dict.TryGetValue(entityId, out var player))
            {
                player.Buffs.SetCustomVar(name, float.Parse(value));
            }
            else
            {
                Log("Unable to locate player entity id '{0}' online", entityId);
            }
        }
    }
}