namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Removes a player's land claims.
    /// </summary>
    public class RemovePlayerLandClaims : ConsoleCmdBase
    {
        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        /// <returns>The description of the command.</returns>
        public override string getDescription()
        {
            return "Removes a player's land claims.";
        }

        /// <summary>
        /// Gets the help text for the command.
        /// </summary>
        /// <returns>The help text for the command.</returns>
        public override string getHelp()
        {
            return "Removes land claims with the specified id from a player or x y z position\n" +
                "Usage:\n" +
                "  1. ty-rplc {PlayerId/EntityId/PlayerName}\n" +
                "  2. ty-rplc {x} {y} {z}\n";
        }

        /// <summary>
        /// Gets the list of commands associated with the command.
        /// </summary>
        /// <returns>The list of commands associated with the command.</returns>
        public override string[] getCommands()
        {
            return new string[] { "ty-RemovePlayerLandClaims", "ty-rplc" };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="args">The list of arguments passed to the command.</param>
        /// <param name="_senderInfo">The information about the command sender.</param>
        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            try
            {
                var persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
                if (args.Count == 1)
                {
                    var userId = PlatformUserIdentifierAbs.FromCombinedString(args[0]);
                    if (persistentPlayerList.Players.TryGetValue(userId, out var persistentPlayerData))
                    {
                        var blockChangeInfos = new List<BlockChangeInfo>();
                        foreach (var pos in persistentPlayerData.GetLandProtectionBlocks().ToArray())
                        {
                            persistentPlayerList.RemoveLandProtectionBlock(pos);
                            blockChangeInfos.Add(new BlockChangeInfo(pos, BlockValue.Air, true, false));
                        }

                        GameManager.Instance.SetBlocksRPC(blockChangeInfos, null);

                        var entity = GameManager.Instance.World.GetEntity(persistentPlayerData.EntityId);
                        if (entity != null)
                        {
                            entity.PlayOneShot("keystone_destroyed", false, false, false);
                        }
                        GameManager.Instance.SavePersistentPlayerData();
                    }
                }
                else if (args.Count == 3)
                {
                    int x = int.Parse(args[0]);
                    int y = int.Parse(args[1]);
                    int z = int.Parse(args[2]);

                    var pos = new Vector3i(x, y, z);
                    var persistentPlayerData = persistentPlayerList.GetLandProtectionBlockOwner(pos);
                    if (persistentPlayerData != null)
                    {
                        persistentPlayerList.RemoveLandProtectionBlock(pos);

                        var blockChangeInfos = new List<BlockChangeInfo>() { new BlockChangeInfo(pos, BlockValue.Air, true, false) };
                        GameManager.Instance.SetBlocksRPC(blockChangeInfos, null);

                        var entity = GameManager.Instance.World.GetEntity(persistentPlayerData.EntityId);
                        if (entity != null)
                        {
                            entity.PlayOneShot("keystone_destroyed", false, false, false);
                        }

                        GameManager.Instance.SavePersistentPlayerData();
                    }
                }
                else
                {
                    Log("Wrong number of arguments.");
                    return;
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Error in RemovePlayerLandClaims.Execute");
            }
        }
    }
}