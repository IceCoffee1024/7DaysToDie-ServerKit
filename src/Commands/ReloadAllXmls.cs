namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Represents a command to reload all XML files synchronously.
    /// </summary>
    public class ReloadAllXmls : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Reload all xmls sync.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-ReloadAllXmls",
            };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            WorldStaticData.ReloadAllXmlsSync();
            Log("Reloaded all xmls.");
        }
    }
}