namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Set the size of history on UndoPrefab operation.
    /// </summary>
    public class SetUndoSize : ConsoleCmdBase
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string getDescription()
        {
            return "Set the size of history on UndoPrefab operation";
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string getHelp()
        {
            return "Usage:\n" +
                "   1. ty-SetUndoSize {size}\n" +
                "   2. ty-SetUndoSize\n" +
                "1. Sets the setundosize history size. If the size is less than or equal to 0, the history prefab of the current active session will be cleared. If this command is executed on a dedicated server, the history prefab of all clients will be cleared.\n" +
                "2. Gets the setundosize history size\n";
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <returns></returns>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-SetUndoSize",
                "ty-sus"
            };
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            try
            {
                if (args.Count == 0)
                {
                    Log("Undo history size is " + UndoPrefab.GetMaxUndoHistorySize());
                }
                else
                {
                    if (int.TryParse(args[0], out int value) == false)
                    {
                        Log("ERR: Invalid undo history size. It must be a number.");
                        return;
                    }

                    UndoPrefab.SetMaxUndoHistorySize(value, senderInfo.GetEntityId());
                    if(value > 0)
                    {
                        Log("Undo history size set to " + value);
                    }
                    else
                    {
                        Log("Undo history cleared.");
                    }
                }
            }
            catch (Exception ex)
            {
                Log("Error in SetUndoSize.Execute" + Environment.NewLine + ex.ToString());
            }
        }
    }
}
