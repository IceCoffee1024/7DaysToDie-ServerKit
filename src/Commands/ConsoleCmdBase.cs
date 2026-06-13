namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Base class for console commands.
    /// </summary>
    public abstract class ConsoleCmdBase : ConsoleCmdAbstract
    {
        /// <summary>
        /// Logs a message to the console.
        /// </summary>
        /// <param name="line">The message to log.</param>
        protected virtual void Log(string line)
        {
            SdtdConsole.Instance.Output(CustomLogger.Prefix + line);
        }

        /// <summary>
        /// Logs a formatted message to the console.
        /// </summary>
        /// <param name="line">The format string of the message to log.</param>
        /// <param name="args">The arguments to format the message.</param>
        protected virtual void Log(string line, params object[] args)
        {
            SdtdConsole.Instance.Output(CustomLogger.Prefix + string.Format(line, args));
        }

        /// <summary>
        /// Checks if the arguments contain the specified name.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="name"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        protected static bool ContainsCaseInsensitive(List<string> args, string name, int startIndex = 0)
        {
            for (int i = startIndex; i < args.Count; i++)
            {
                if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}