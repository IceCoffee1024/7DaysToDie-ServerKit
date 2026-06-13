using System.Runtime;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Represents a command for garbage collection.
    /// </summary>
    public class GarbageCollection : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Use the framework's own methods for garbage collection.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Usage: ty-gc";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-GarbageCollection",
                "ty-gc"
            };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            MemoryPools.Cleanup();
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}