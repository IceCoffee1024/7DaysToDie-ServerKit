namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Command to save all world static xml data to a folder.
    /// </summary>
    public class SaveXmlsToFolder : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Save all world static xml data to a folder.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-sxtf \"C:/Users/Administrator/Desktop/Temp\"" +
                "1. Save all world static xml data to a specified folder.";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-SaveXmlsToFolder",
                "ty-sxtf",
            };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            string path;
            if (args.Count < 1)
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "xmls");
            }
            else
            {
                path = args[0];
            }

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
                Log("The specified directory: {0} does not exist, so it was created.", path);
            }

            WorldStaticData.SaveXmlsToFolder(path);
            Log("All world static xml data has been saved to the specified folder: {0}", path);
        }
    }
}