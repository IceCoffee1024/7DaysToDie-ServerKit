namespace SdtdServerKit.Commands
{
    /// <summary>
    /// 列出玩家当前活跃的 Buff
    /// </summary>
    public class ListPlayerBuffs : ConsoleCmdBase
    {
        public override string getDescription()
        {
            return "列出玩家当前活跃的Buff";
        }

        public override string getHelp()
        {
            return "列出玩家当前活跃的Buff\n" +
                "用法:\n" +
                "  ty-lpbuffs              - 列出所有在线玩家的Buff\n" +
                "  ty-lpbuffs <玩家名/entityId> - 列出指定玩家的Buff";
        }

        public override string[] getCommands()
        {
            return new string[] { "ty-ListPlayerBuffs", "ty-lpbuffs" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 1)
                {
                    Log("参数错误，期望 0 或 1 个参数，实际传入 {0} 个", _params.Count);
                    return;
                }

                if (_params.Count == 1)
                {
                    ClientInfo clientInfo = ConsoleHelper.ParseParamIdOrName(_params[0], true, false);
                    if (clientInfo == null)
                    {
                        Log("未找到玩家: {0}", _params[0]);
                        return;
                    }

                    if (!GameManager.Instance.World.Players.dict.TryGetValue(clientInfo.entityId, out EntityPlayer entityPlayer))
                    {
                        Log("未找到玩家实体: {0}", _params[0]);
                        return;
                    }

                    PrintPlayerBuffs(entityPlayer, clientInfo.playerName);
                }
                else
                {
                    var players = GameManager.Instance.World.Players.list;
                    if (players.Count == 0)
                    {
                        Log("当前没有在线玩家");
                        return;
                    }

                    foreach (EntityPlayer entityPlayer in players)
                    {
                        ClientInfo clientInfo = ConsoleHelper.ParseParamIdOrName(entityPlayer.entityId.ToString(), true, false);
                        string playerName = clientInfo != null ? clientInfo.playerName : "Unknown";
                        PrintPlayerBuffs(entityPlayer, playerName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("执行 ListPlayerBuffs 时出错: {0}", ex.Message);
            }
        }

        private void PrintPlayerBuffs(EntityPlayer player, string playerName)
        {
            var activeBuffs = player.Buffs.ActiveBuffs;
            int buffCount = 0;

            Log("========== 玩家 [{0}] (EntityId: {1}) 的活跃Buff ==========", playerName, player.entityId);

            foreach (var buffValue in activeBuffs)
            {
                try
                {
                    string buffName = buffValue.BuffName ?? "N/A";
                    string localizedName = buffValue.BuffClass?.LocalizedName ?? buffName;
                    string description = buffValue.BuffClass?.Description ?? "";

                    if (string.IsNullOrEmpty(description))
                    {
                        Log("  [{0}] {1} (ID: {2})", buffCount + 1, localizedName, buffName);
                    }
                    else
                    {
                        Log("  [{0}] {1} (ID: {2}) - {3}", buffCount + 1, localizedName, buffName, description);
                    }

                    buffCount++;
                }
                catch (Exception ex)
                {
                    Log("  读取Buff信息时出错: {0}", ex.Message);
                }
            }

            if (buffCount == 0)
            {
                Log("  (无活跃Buff)");
            }
            else
            {
                Log("  共 {0} 个活跃Buff", buffCount);
            }

            Log("");
        }
    }
}
