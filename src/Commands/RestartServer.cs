using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Represents a command to restart the server.
    /// </summary>
    public class RestartServer : ConsoleCmdBase
    {
        /// <inheritdoc />
        public override string getDescription()
        {
            return "Restart server, optional parameter [{delay}] and [-f/force].";
        }

        /// <inheritdoc />
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-rs [-f/force]\n" +
                "  2. ty-rs {delay} [-f/force]\n" +
                "1. Restart server by shutdown or force restart by kill process\n" +
                "2. Delay for a specified number of seconds before restarting";
        }

        /// <inheritdoc />
        public override string[] getCommands()
        {
            return new[] { "ty-RestartServer", "ty-rs" };
        }

        /// <inheritdoc />
        public override async void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            Log("Server is restarting..., please wait.");

            if (_isRestarting)
            {
                return;
            }

            bool force = ContainsCaseInsensitive(args, "-f") || ContainsCaseInsensitive(args, "force");
            if (force)
            {
                args.RemoveAll(i => string.Equals(i, "-f", StringComparison.OrdinalIgnoreCase) || string.Equals(i, "force", StringComparison.OrdinalIgnoreCase));
            }

            if (args.Count > 0 && int.TryParse(args[0], out int delay))
            {
                for (int i = 0; i < delay; i++)
                {
                    await Task.Delay(1000);
                    Log($"{delay - i}");
                }
            }

            if (force)
            {
                PrepareRestart(true);
            }
            else
            {
                PrepareRestart(false);
            }
        }

        private static volatile bool _isRestarting;
        /// <summary>
        /// Gets a value indicating whether the server is restarting.
        /// </summary>
        public static bool IsRestarting => _isRestarting;

        /// <summary>
        /// Prepares the server for restart.
        /// </summary>
        /// <param name="force">Indicates whether to force restart the server.</param>
        public static void PrepareRestart(bool force = false)
        {
            SdtdConsole.Instance.ExecuteSync("sa", ModApi.CmdExecuteDelegate);

            _isRestarting = true;

            if (force)
            {
                Restart();
            }
            else
            {
                SdtdConsole.Instance.ExecuteSync("shutdown", ModApi.CmdExecuteDelegate);
            }
        }

        private static void Restart()
        {
            string? scriptName = null;
            string? serverPath = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                scriptName = "restart-windows.bat";
                serverPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startdedicated.bat");
                string path = Path.Combine(ModApi.ModInstance.Path, scriptName);
                Process.Start(path, string.Format("{0} \"{1}\"", Process.GetCurrentProcess().Id, serverPath));
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                scriptName = "restart-linux.sh";
                serverPath = AppDomain.CurrentDomain.BaseDirectory;
                Process.Start("chmod", " +x " + Path.Combine(ModApi.ModInstance.Path, scriptName)).WaitForExit();
                string path = Path.Combine(ModApi.ModInstance.Path, scriptName);
                Process.Start(path, string.Format("{0} {1}", Process.GetCurrentProcess().Id, ModApi.AppSettings.ServerSettingsFileName));
            }
        }

        /// <summary>
        /// Handles the game shutdown event.
        /// </summary>
        private static void OnGameShutdown()
        {
            if (_isRestarting)
            {
                Restart();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestartServer"/> class.
        /// </summary>
        public RestartServer()
        {
            ModEventHub.GameShutdown -= OnGameShutdown;
            ModEventHub.GameShutdown += OnGameShutdown;
        }
    }
}