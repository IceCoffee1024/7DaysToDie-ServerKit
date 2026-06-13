using SdtdServerKit.Hooks;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Represents a command that removes a player's items.
    /// </summary>
    public class RemovePlayerItems : ConsoleCmdBase
    {
        /// <inheritdoc/>
        public override string getDescription()
        {
            return "Removes a player's items from the player's inventory.";
        }

        /// <inheritdoc/>
        public override string getHelp()
        {
            return "Removes items with the specified name from a player.\n" +
                "Usage: ty-rpi {PlayerId/EntityId/PlayerName} {ItemName}\n";
        }

        /// <inheritdoc/>
        public override string[] getCommands()
        {
            return new string[] { "ty-RemovePlayerItems", "ty-rpi" };
        }

        /// <inheritdoc/>
        public override void Execute(List<string> args, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (args.Count < 2)
                {
                    Log("Wrong number of arguments, expected 2, found " + args.Count + ".");
                    return;
                }

                var cInfo = ConsoleHelper.ParseParamIdOrName(args[0]);
                string itemName = args[1];

                if (cInfo != null && GameManager.Instance.World.Players.dict.TryGetValue(cInfo.entityId, out EntityPlayer player))
                {
                    // Remove the name of block shape
                    int index = itemName.IndexOf(':');
                    if (index != -1)
                    {
                        itemName = itemName.Substring(0, index);
                    }

                    string actionName = WorldStaticDataHook.ActionPrefix + WorldStaticDataHook.TagPrefix + itemName;

                    if (GameEventManager.GameEventSequences.ContainsKey(actionName))
                    {
                        GameEventManager.Current.HandleAction(actionName, null, player, false, "");
                        cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageGameEventResponse>()
                            .Setup(actionName, cInfo.entityId, string.Empty, string.Empty, NetPackageGameEventResponse.ResponseTypes.Approved));

                        Log("Removed item: {0},action: {1} from inventory of online player id '{2}' named '{3}'",
                            itemName, actionName, cInfo.CrossplatformId.CombinedString, cInfo.playerName);
                        return;
                    }
                    else
                    {
                        Log("Unable to locate {0} in the game events list", actionName);
                        return;
                    }
                }
                else
                {
                    var userId = PlatformUserIdentifierAbs.FromCombinedString(args[0]);
                    if (userId != null)
                    {
                        var playerDataFile = new PlayerDataFile();
                        string playerDataDir = GameIO.GetPlayerDataDir();
                        playerDataFile.Load(playerDataDir, userId.CombinedString);

                        if (playerDataFile.bLoaded)
                        {
                            bool removedFromBag = RemoveItemValue(playerDataFile.bag, itemName);
                            bool removedFromInventory = RemoveItemValue(playerDataFile.inventory, itemName);

                            if (removedFromBag || removedFromInventory)
                            {
                                playerDataFile.Save(playerDataDir, userId.CombinedString);

                                Log("Removed item: {0} from inventory of offline player id '{1}'", itemName, userId.CombinedString);
                                return;
                            }

                            Log("Item: {0} not found in inventory of offline player id '{1}'", itemName, userId.CombinedString);
                            return;
                        }
                    }
                }

                Log("Unable to locate player '{0}' online or offline", args[0]);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Error in RemoveItem.Execute");
            }
        }

        private static bool RemoveItemValue(ItemStack[] items, string itemName)
        {
            bool removed = false;
            for (int i = 0, len = items.Length; i < len; i++)
            {
                var item = items[i];
                if (item.itemValue?.ItemClass?.GetItemName() == itemName)
                {
                    item.itemValue = ItemValue.None;
                    removed = true;
                }
            }

            return removed;
        }
    }
}