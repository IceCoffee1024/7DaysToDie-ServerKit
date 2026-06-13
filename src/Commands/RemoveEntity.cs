namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Remove Entity
    /// </summary>
    public class RemoveEntity : ConsoleCmdBase
    {
        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        /// <returns>The description of the command.</returns>
        public override string getDescription()
        {
            return "Removes an entity from the game";
        }

        /// <summary>
        /// Gets the help text for the command.
        /// </summary>
        /// <returns>The help text for the command.</returns>
        public override string getHelp()
        {
            return "Removes an entity from the game\n" +
                "Usage: ty-re {EntityId}\n" +
                "Usage: ty-RemoveEntity {EntityId}";
        }

        /// <summary>
        /// Gets the list of commands associated with the command.
        /// </summary>
        /// <returns>The list of commands associated with the command.</returns>
        public override string[] getCommands()
        {
            return new string[] { "ty-RemoveEntity", "ty-re" };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="_params">The list of parameters passed to the command.</param>
        /// <param name="_senderInfo">The information about the command sender.</param>
        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count != 1)
                {
                    Log(string.Format("Wrong number of arguments, expected 1, found '{0}'", _params.Count));
                    return;
                }

                if (int.TryParse(_params[0], out int entityId) == false)
                {
                    Log(string.Format("Invalid entityId '{0}'", entityId));
                    return;
                }

                if (GameManager.Instance.World.RemoveEntity(entityId, EnumRemoveEntityReason.Despawned) != null)
                {
                    Log(string.Format("Entity '{0}' removed", entityId));
                }
                else
                {
                    Log(string.Format("Entity '{0}' not found. Unable to remove", entityId));
                }
            }
            catch (Exception ex)
            {
                Log(string.Format("Error in RemoveEntity.Execute: {0}", ex.Message));
            }
        }
    }
}
