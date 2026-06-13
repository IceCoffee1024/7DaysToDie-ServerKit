using static System.Net.Mime.MediaTypeNames;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// ResetPlayer class represents a command to reset a player's profile.
    /// </summary>
    public class ResetPlayer : ConsoleCmdBase
    {
        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        /// <returns>The description of the command.</returns>
        public override string getDescription()
        {
            return "Reset a player's profile. * WARNING * Can not be undone without a backup.";
        }

        /// <summary>
        /// Gets the help message for the command.
        /// </summary>
        /// <returns>The help message for the command.</returns>
        public override string getHelp()
        {
            return "Usage: ty-rpp {PlayerId/EntityId/PlayerName}";
        }

        /// <summary>
        /// Gets the list of commands associated with the ResetPlayer command.
        /// </summary>
        /// <returns>The list of commands associated with the ResetPlayer command.</returns>
        public override string[] getCommands()
        {
            return new string[] { "ty-ResetPlayerProfile", "ty-rpp" };
        }

        /// <summary>
        /// Executes the ResetPlayer command.
        /// </summary>
        /// <param name="args">The list of arguments passed to the command.</param>
        /// <param name="_senderInfo">The information of the command sender.</param>
        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (args.Count != 1)
                {
                    Log("Wrong number of arguments, expected 1, found '{0}'", args.Count);
                    return;
                }
                
                var cInfo = ConsoleHelper.ParseParamIdOrName(args[0]);
                if (cInfo != null && GameManager.Instance.World.Players.dict.TryGetValue(cInfo.entityId, out EntityPlayer entityPlayer))
                {
                    SdtdConsole.Instance.ExecuteSync(string.Format("kick {0}", cInfo.CrossplatformId.CombinedString), ModApi.CmdExecuteDelegate);
                    //Log("Kicking Player " + cInfo.playerName + ": ResetPlayer");
                    //GameUtils.EKickReason ekickReason = GameUtils.EKickReason.ManualKick;
                    //GameUtils.KickPlayerForClientInfo(cInfo, new GameUtils.KickPlayerData(ekickReason, 0, default(DateTime), "ResetPlayer"));
                    
                    GameManager.Instance.World.aiDirector.RemoveEntity(entityPlayer);
                    ResetProfileExec(cInfo.CrossplatformId);
                    return;
                }

                var userId = PlatformUserIdentifierAbs.FromCombinedString(args[0]);
                if (userId != null)
                {
                    var persistentPlayersDict = GameManager.Instance.GetPersistentPlayerList().Players;
                    if (persistentPlayersDict.TryGetValue(userId, out var persistentPlayerData))
                    {
                        ResetProfileExec(persistentPlayerData.PrimaryId);
                        return;
                    }
                }

                Log("Unable to find player data for id '{0}'", args[0]);
            }
            catch (Exception e)
            {
                Log(string.Format("Error in ResetPlayer.Execute: {0}", e.Message));
            }
        }

        /// <summary>
        /// Executes the profile reset for the specified player.
        /// </summary>
        /// <param name="_id">The identifier of the player.</param>
        public void ResetProfileExec(PlatformUserIdentifierAbs _id)
        {
            try
            {
                RemovePersistentData(_id);
                RemoveProfile(_id);
            }
            catch (Exception e)
            {
                Log(string.Format("Error in ResetPlayer.ResetProfileExec: {0}", e.Message));
            }
        }

        /// <summary>
        /// Removes the persistent data for the specified player.
        /// </summary>
        /// <param name="uId">The identifier of the player.</param>
        public static void RemovePersistentData(PlatformUserIdentifierAbs uId)
        {
            var list = GameManager.Instance.GetPersistentPlayerList();
            if (list.Players.ContainsKey(uId))
            {
                list.Players.Remove(uId);
                GameManager.Instance.SavePersistentPlayerData();
            }
        }

        /// <summary>
        /// Removes the profile file for the specified player.
        /// </summary>
        /// <param name="uId">The identifier of the player.</param>
        public void RemoveProfile(PlatformUserIdentifierAbs uId)
        {
            try
            {
                string dir = string.Format("{0}/Player/{1}", GameIO.GetSaveGameDir(), uId.CombinedString);
                if (Directory.Exists(dir) == false)
                {
                    Log(string.Format("Could not find directory '{0}' for player profile reset", dir));
                }
                else
                {
                    Directory.Delete(dir, true);
                    Log(string.Format("Directory '{0}' deleted for player profile reset", dir));
                }
                Log(string.Format("Removing .ttp file in three seconds"));

                Task.Run(async () => {
                    await Task.Delay(3000);
                    DelayedProfileDeletion(uId);
                });
            }
            catch (Exception e)
            {
                Log(string.Format("Error in ResetPlayer.RemoveProfile: {0}", e.Message));
            }
        }

        /// <summary>
        /// Deletes the delayed profile file for the specified player.
        /// </summary>
        /// <param name="uId">The identifier of the player.</param>
        public void DelayedProfileDeletion(PlatformUserIdentifierAbs uId)
        {
            try
            {
                string filepath = string.Format("{0}/Player/{1}.ttp", GameIO.GetSaveGameDir(), uId.CombinedString);
                if (File.Exists(filepath) == false)
                {
                    Log(string.Format("Could not find file '{0}' for player profile reset", filepath));
                }
                else
                {
                    File.Delete(filepath);
                    Log(string.Format("File '{0}' deleted for player profile reset", filepath));
                }
            }
            catch (Exception e)
            {
                Log(string.Format("Error in ResetPlayer.DelayedProfileDeletion: {0}", e.Message));
            }
        }

    }
}
