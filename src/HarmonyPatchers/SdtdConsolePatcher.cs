using HarmonyLib;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Managers;
using SdtdServerKit.MuteCommandAreas;
using System.Reflection;

namespace SdtdServerKit.HarmonyPatchers
{
    /// <summary>
    /// 聊天消息拦截，实现区域命令禁用
    /// </summary>
    [HarmonyPatch]
    internal class ChatMessageHookPatcher
    {
        static MethodBase TargetMethod()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var chatMessageHookType = assembly.GetType("SdtdServerKit.Hooks.ChatMessageHook");
            
            if (chatMessageHookType == null)
            {
                CustomLogger.Error("[区域命令禁用] 找不到 ChatMessageHook 类型");
                return null!;
            }

            var method = chatMessageHookType.GetMethod("OnChatMessage", 
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            
            if (method == null)
            {
                CustomLogger.Error("[区域命令禁用] 找不到 OnChatMessage 方法");
                return null!;
            }

            CustomLogger.Debug("[区域命令禁用] 成功定位到 ChatMessageHook.OnChatMessage 方法");
            return method;
        }

        /// <summary>
        /// 在 OnChatMessage 方法执行前拦截，检查命令是否在禁用区域内
        /// </summary>
        [HarmonyPrefix]
        public static bool Before_OnChatMessage(ChatMessage chatMessage)
        {
            try
            {
                if (!FunctionManager.TryGetFunction<Functions.MuteCommandArea>(out var function) || function == null || !function.IsRunning)
                {
                    return true; 
                }

                if (chatMessage.ChatType != ChatType.Global)
                {
                    return true;
                }

                string? playerId = chatMessage.PlayerId;
                if (string.IsNullOrEmpty(playerId))
                {
                    return true;
                }

                if (!LivePlayerManager.TryGetByPlayerId(playerId, out var player) || player == null)
                {
                    return true;
                }

                string cmd = chatMessage.Message;
                string chatPrefix = ConfigManager.GlobalSettings.ChatCommandPrefix;

                if (!string.IsNullOrEmpty(chatPrefix))
                {
                    if (!cmd.StartsWith(chatPrefix))
                    {
                        return true; 
                    }
                    cmd = cmd.Substring(chatPrefix.Length);
                }

                if (string.IsNullOrEmpty(cmd))
                {
                    return true;
                }

                string commandName = cmd.Trim().Split(new[] { ' ', ':', '：' }, StringSplitOptions.RemoveEmptyEntries)[0];

                EntityPlayer? entityPlayer = null;
                if (GameManager.Instance.World.Players.dict.ContainsKey(player.EntityId))
                {
                    entityPlayer = GameManager.Instance.World.Players.dict[player.EntityId];
                }

                if (entityPlayer == null)
                {
                    return true;
                }

                var position = entityPlayer.GetBlockPosition();
                int x = position.x;
                int z = position.z;

                CustomLogger.Debug($"[区域命令禁用] 拦截聊天命令: '{commandName}', 玩家: {player.PlayerName}, 位置: ({x}, {z})");

                if (MuteCommandManager.IsCommandMuted(x, z, commandName))
                {
                    CustomLogger.Debug($"[区域命令禁用] 已拦截命令 '{commandName}' - 玩家: {player.PlayerName}, 位置: ({x}, {z})");

                    var settings = function.Settings;
                    string tip = settings.MutedCommandTip
                        .Replace("{command}", commandName)
                        .Replace("{playerName}", player.PlayerName)
                        .Replace("{x}", x.ToString())
                        .Replace("{z}", z.ToString());

                    Utilities.Utils.SendPrivateMessage(new Models.PrivateMessage()
                    {
                        Message = tip,
                        TargetPlayerIdOrName = playerId,
                    });

                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "[区域命令禁用] 检查聊天命令时发生错误");
                return true;
            }
        }
    }
}
