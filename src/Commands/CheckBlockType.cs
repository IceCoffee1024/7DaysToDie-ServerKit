using SdtdServerKit.Managers;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Checks the type of block by position or under your feet.
    /// </summary>
    public class CheckBlockType : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Checks the type of block by position or under your feet.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-CheckBlockType {x} {y} {z}\n" +
                "  2. ty-CheckBlockType\n" +
                "1. check the block at x, y, z.\n" +
                "2. check the block under your feet.";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-CheckBlockType",
                "ty-cbt"
            };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            try
            {
                if (args.Count != 0 && args.Count != 3)
                {
                    Log("ERR: Wrong number of arguments, expected 0 or 3, found " + args.Count.ToString() + ".");
                    Log(this.GetHelp());
                }
                else
                {
                    int x, y ,z;
                    if (args.Count == 0)
                    {
                        if (LivePlayerManager.TryGetByEntityId(senderInfo.GetEntityId(), out var managedPlayer) == false)
                        {
                            Log("ERR: Unable to get your position.");
                            return;
                        }

                        var playerBlockPosition = managedPlayer!.EntityPlayer.GetBlockPosition();
                        x = playerBlockPosition.x;
                        y = playerBlockPosition.y - 1;
                        z = playerBlockPosition.z;
                    }
                    else
                    {
                        int.TryParse(args[0], out x);
                        int.TryParse(args[1], out y);
                        int.TryParse(args[2], out z);
                    }

                    var position = new Vector3i(x, y, z);
                    var block = GameManager.Instance.World.GetBlock(position);
                    Log($"checking at {position}, block type: {block.type}, block name: {block.Block.GetBlockName()}.");
                }
            }
            catch (Exception ex)
            {
                Log("Error in CheckBlockType.Execute" + Environment.NewLine + ex.ToString());
            }
        }
    }
}
