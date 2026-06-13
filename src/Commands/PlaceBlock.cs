namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Place one block at a time without the need of chunkreloading (RPC).
    /// </summary>
    public class PlaceBlock : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Place one block at a time without the need of chunkreloading (RPC).";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-PlaceBlock {blockIdOrName} {x} {y} {z}\n" +
                "1. Place one block on position x,y,z";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-PlaceBlock",
                "ty-pb"
            };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (args.Count != 4)
                {
                    Log("Invalid number of parameters. Expected 4.");
                    return;
                }

                string blockIdOrName = args[0];
                int x, y, z;
                int.TryParse(args[1], out x);
                int.TryParse(args[2], out y);
                int.TryParse(args[3], out z);

                if (GameManager.Instance.World.GetChunkSync(World.toChunkXZ(x), World.toChunkXZ(z)) == null)
                {
                    Log("The chunk at given position is not loaded. Aborting.");
                    return;
                }

                if (Utilities.Utils.TryGetBlockValue(blockIdOrName, out var blockValue) == false)
                {
                    Log("ERR: Invalid block name or ID: " + blockIdOrName);
                    return;
                }

                var position = new Vector3i(x, y, z);
                var blockChangeInfo = new BlockChangeInfo(position, blockValue, true, false);
                GameManager.Instance.SetBlocksRPC(new List<BlockChangeInfo>() { blockChangeInfo }, null);
                Log($"Block {blockIdOrName} placed at: {position}");
            }
            catch (Exception ex)
            {
                Log("Error in PlaceBlock.Execute" + Environment.NewLine + ex.ToString());
            }
        }
    }
}
