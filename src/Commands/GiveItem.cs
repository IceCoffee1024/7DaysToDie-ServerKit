using UnityEngine;
using static InvGameItem;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// Gives a item directly to a player's inventory. Drops to the ground if inventory is full.
    /// </summary>
    public class GiveItem : ConsoleCmdBase
    {
        /// <summary>
        /// Gets the description of the command.
        /// </summary>
        /// <returns>The description of the command.</returns>
        public override string getDescription()
        {
            return "Gives a item directly to a player's inventory. Drops to the ground if inventory is full.";
        }

        /// <summary>
        /// Gets the help text for the command.
        /// </summary>
        /// <returns>The help text for the command.</returns>
        public override string getHelp()
        {
            return "Usage:\n" +
                "  1. ty-gi {PlayerId/EntityId/PlayerName} {ItemName} {Count} {Quality} {Durability}\n" +
                "  2. ty-gi {PlayerId/EntityId/PlayerName} {ItemName} {Count} {Quality}\n" +
                "  3. ty-gi {PlayerId/EntityId/PlayerName} {ItemName} {Count}\n" +
                "  4. ty-gi {PlayerId/EntityId/PlayerName} {ItemName}\n" +
                "  5. ty-gi all {ItemName} {Count} {Quality} {Durability}\n " +
                "  6. ty-gi all {ItemName} {Count} {Quality}\n " +
                "  7. ty-gi all {ItemName} {Count}\n " +
                "  8. ty-gi all {ItemName}\n " +
                "1. Gives a player the item with specified count, quality and durability\n" +
                "2. Gives a player the item with specified count, quality and 100 percent durability\n" +
                "3. Gives a player the item with specified count, 1 quality and 100 percent durability\n" +
                "4. Gives a player the item with 1 count 1 quality and 100 percent durability\n" +
                "5. Gives all players the item with specified count, quality and durability\n" +
                "6. Gives all players the item with specified count, quality and 100 percent durability\n" +
                "7. Gives all players the item with specified count, 1 quality and 100 percent durability\n" +
                "8. Gives all players the item with 1 count 1 quality and 100 percent durability\n";
        }

        /// <summary>
        /// Gets the list of commands for the command.
        /// </summary>
        /// <returns>The list of commands for the command.</returns>
        public override string[] getCommands()
        {
            return new string[] { "ty-GiveItem", "ty-gi", "ty-give" };
        }

        /// <summary>
        /// Executes the command.
        /// </summary>
        /// <param name="args">The list of arguments for the command.</param>
        /// <param name="senderInfo">The information of the command sender.</param>
        public override void Execute(List<string> args, CommandSenderInfo senderInfo)
        {
            try
            {
                int argCount = args.Count;

                if (argCount < 2 || argCount > 5)
                {
                    Log(string.Format("Wrong number of arguments, expected 2 to 5, found {0}", argCount));
                    return;
                }

                string arg0 = args[0];
                if (arg0.Length < 3 || arg0.Length > 36)
                {
                    Log(string.Format("Can not give item: Invalid id {0}", arg0));
                    return;
                }

                string arg1 = args[1];
                if (arg1.Length == 0)
                {
                    Log(string.Format("Can not give item: Invalid name {0}", arg1));
                    return;
                }
                else
                {
                    World world = GameManager.Instance.World;
                    var playersDict = world.Players.dict;
                    int itemCount = 1;
                    if (argCount > 2 && int.TryParse(args[2], out int count))
                    {
                        if (count > 0 && count < 1000000)
                        {
                            itemCount = count;
                        }
                    }

                    ItemValue itemValue = ItemClass.GetItem(arg1, true);
                    if (itemValue.type == ItemValue.None.type)
                    {
                        Log("ERR: Item not found.");
                        return;
                    }

                    itemValue = new ItemValue(itemValue.type);
                    if (itemValue.HasQuality)
                    {
                        itemValue.Quality = 1;
                        if (args.Count > 3 && ushort.TryParse(args[3], out var itemQuality))
                        {
                            if (itemQuality > 0)
                            {
                                itemValue.Quality = itemQuality;
                            }
                        }
                    }

                    var itemClass = itemValue.ItemClass;
                    if (itemClass != null && itemClass is not ItemClassModifier && itemClass.Stacknumber.Value <= 1)
                    {
                        itemValue.Modifications = new ItemValue[Math.Min(255, (int)EffectManager.GetValue(PassiveEffects.ModSlots, itemValue, global::Utils.FastMax(0, itemValue.Quality - 1)))];
                        itemValue.CosmeticMods = new ItemValue[itemClass.HasAnyTags(ItemClassModifier.CosmeticItemTags) ? 1 : 0];
                    }

                    if (args.Count > 4 && float.TryParse(args[4], out float durability))
                    {
                        if (durability > 0 && durability < 101)
                        {
                            float newDurability = itemValue.MaxUseTimes - (durability / 100 * itemValue.MaxUseTimes);
                            itemValue.UseTimes = newDurability;
                        }
                    }

                    if (arg0.ToLower() == "all")
                    {
                        foreach (var cInfo in ConnectionManager.Instance.Clients.List)
                        {
                            if (playersDict.TryGetValue(cInfo.entityId, out EntityPlayer player)
                                && player.IsSpawned() && !player.IsDead())
                            {
                                var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData()
                                {
                                    entityClass = EntityClass.FromString("item"),
                                    id = EntityFactory.nextEntityID++,
                                    itemStack = new ItemStack(itemValue, itemCount),
                                    pos = player.position,
                                    rot = new Vector3(20f, 0f, 20f),
                                    lifetime = 60f,
                                    belongsPlayerId = cInfo.entityId
                                });

                                world.SpawnEntityInWorld(entityItem);
                                cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, cInfo.entityId));
                                world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);
                                Log(string.Format("Gave {0} to {1}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name, cInfo.playerName));
                            }
                            else
                            {
                                Log(string.Format("Player with pltfmId Id {0} has not spawned. Unable to give item", cInfo.PlatformId.CombinedString));
                            }
                        }
                    }
                    else
                    {
                        ClientInfo cInfo = ConsoleHelper.ParseParamIdOrName(arg0);
                        if (cInfo != null && playersDict.TryGetValue(cInfo.entityId, out EntityPlayer player)
                            && player.IsSpawned() && !player.IsDead())
                        {
                            var entityItem = (EntityItem)EntityFactory.CreateEntity(new EntityCreationData()
                            {
                                entityClass = EntityClass.FromString("item"),
                                id = EntityFactory.nextEntityID++,
                                itemStack = new ItemStack(itemValue, itemCount),
                                pos = player.position,
                                rot = new Vector3(20F, 0F, 20F),
                                lifetime = 60F,
                                belongsPlayerId = cInfo.entityId
                            });

                            world.SpawnEntityInWorld(entityItem);
                            cInfo.SendPackage(NetPackageManager.GetPackage<NetPackageEntityCollect>().Setup(entityItem.entityId, cInfo.entityId));
                            world.RemoveEntity(entityItem.entityId, EnumRemoveEntityReason.Despawned);

                            Log(string.Format("Gave {0} to {1}", itemValue.ItemClass.GetLocalizedItemName() ?? itemValue.ItemClass.Name, cInfo.PlatformId.CombinedString));
                        }
                        else
                        {
                            Log(string.Format("Player with id {0} is not logged on or loaded in yet", arg0));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Error in giveitem console command executing");
            }
        }
    }
}