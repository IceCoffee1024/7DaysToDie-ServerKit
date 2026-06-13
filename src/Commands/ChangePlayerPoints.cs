using SdtdServerKit.Data.IRepositories;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Represents a command to change player points.
    /// </summary>
    public class ChangePlayerPoints : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Change player points.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Usage: ty-cpp {PlayerId/EntityId/PlayerName} {count}";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[]
            {
                "ty-ChangePlayerPoints",
                "ty-cpp"
            };
        }

        /// <inheritdoc/>
        public override async void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            if (args.Count < 2)
            {
                Log("Wrong number of arguments, expected 2, found " + args.Count + ".");
                return;
            }

            try
            {
                var cInfo = ConsoleHelper.ParseParamIdOrName(args[0]);
                string? playerId = cInfo?.CrossplatformId.CombinedString ?? PlatformUserIdentifierAbs.FromCombinedString(args[0])?.CombinedString;

                if (playerId == null)
                {
                    Log("Unable to locate player '{0}' online or offline", args[0]);
                    return;
                }

                int count = int.Parse(args[1]);

                var repository = ModApi.ServiceContainer.Resolve<IPointsInfoRepository>();
                bool result = (await repository.ChangePointsAsync(playerId, count, allowNegative: true)) == 1;
                if (result)
                {
                    int balance = await repository.GetPointsByIdAsync(playerId);
                    string? playerName = cInfo?.playerName;
                    string? senderName = null;
                    try { senderName = _senderInfo.RemoteClientInfo?.playerName; } catch { /* console */ }
                    Managers.PointLogger.Log(Models.PointLogCategory.WebApi, playerId, playerName,
                        count, balance, $"控制台命令 ty-cpp (sender: {senderName ?? "console"})");
                    Log("Player '{0}' points change {1}.", playerId, count);
                }
                else
                {
                    Log("Failed to change player '{0}' points.", playerId);
                }
            }
            catch (Exception ex)
            {
                Log("Error in ChangePlayerPoints.Execute:" + ex.Message);
            }
        }
    }
}