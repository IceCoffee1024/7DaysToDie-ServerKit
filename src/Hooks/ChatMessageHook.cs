using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.Functions;
using SdtdServerKit.Managers;
using System.Collections.Immutable;

namespace SdtdServerKit.Hooks
{
    internal delegate Task<bool> ChatHook(string cmd, ManagedPlayer managedPlayer);
    /// <summary>
    /// 表示聊天消息钩子的静态类。
    /// </summary>
    internal static class ChatMessageHook
    {
        private static ImmutableList<ChatHook> _chatHooks = ImmutableList<ChatHook>.Empty;
        private static readonly Cache _cache;

        static ChatMessageHook()
        {
            _cache = new Cache();
        }

        /// <summary>
        /// 添加聊天钩子。
        /// </summary>
        /// <param name="chatHook">要添加的聊天钩子。</param>
        public static void AddHook(ChatHook chatHook)
        {
            _chatHooks = _chatHooks.Add(chatHook);
        }

        /// <summary>
        /// 移除聊天钩子。
        /// </summary>
        /// <param name="chatHook">要移除的聊天钩子。</param>
        public static void RemoveHook(ChatHook chatHook)
        {
            _chatHooks = _chatHooks.Remove(chatHook);
        }

        /// <summary>
        /// 处理聊天命令。
        /// </summary>
        /// <param name="chatHook">要处理的聊天钩子。</param>
        /// <param name="cmd">聊天命令。</param>
        /// <param name="managedPlayer">在线玩家。</param>
        /// <returns>表示聊天命令是否被处理的任务。</returns>
        private static async Task<bool> HandleChatCmd(ChatHook chatHook, string cmd, ManagedPlayer managedPlayer)
        {
            try
            {
                return await chatHook.Invoke(cmd, managedPlayer);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Error in ChatMessageHook.HandleChatMessage");

                Utilities.Utils.SendPrivateMessage(new PrivateMessage()
                {
                    Message = ConfigManager.GlobalSettings.HandleChatMessageError,
                    //SenderName = ConfigManager.GlobalSettings.ServerName,
                    TargetPlayerIdOrName = managedPlayer.PlayerId,
                });

                return false;
            }
        }

        /// <summary>
        /// 处理聊天消息。
        /// </summary>
        /// <param name="chatMessage">聊天消息。</param>
        internal static async Task OnChatMessage(ChatMessage chatMessage)
        {
            try
            {
                string? playerId = chatMessage.PlayerId;
                if (playerId != null
                    && chatMessage.ChatType == ChatType.Global
                    && LivePlayerManager.TryGetByPlayerId(playerId, out var player) 
                    && player != null)
                {
                    string cmd = chatMessage.Message;
                    string chatPrefix = ConfigManager.GlobalSettings.ChatCommandPrefix;

                    if (string.IsNullOrEmpty(chatPrefix) == false)
                    {
                        if (cmd.StartsWith(chatPrefix) == false)
                        {
                            return;
                        }
                        else
                        {
                            cmd = cmd.Substring(chatPrefix.Length);
                        }
                    }

                    var chatHook = _cache.Get(cmd);
                    if (chatHook != null && chatHook.Target is IFunction function && function.IsRunning)
                    {
                        bool isHandled = await HandleChatCmd(chatHook, cmd, player);

                        if (isHandled)
                        {
                            return;
                        }
                        else
                        {
                            _cache.Remove(cmd);
                        }
                    }

                    foreach (var hook in _chatHooks)
                    {
                        if (hook == chatHook)
                        {
                            continue;
                        }

                        bool isHandled = await HandleChatCmd(hook, cmd, player);
                        // 如果命令被其他功能处理
                        if (isHandled && cmd.Length <= 8)
                        {
                            _cache.Set(cmd, hook);
                            return;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "Error in ChatMessageHook.OnChatMessage");

                Utilities.Utils.SendPrivateMessage(new PrivateMessage()
                {
                    Message = ConfigManager.GlobalSettings.HandleChatMessageError,
                    TargetPlayerIdOrName = chatMessage.PlayerId!,
                });
            }
        }

        /// <summary>
        /// 表示聊天钩子的缓存。
        /// </summary>
        class Cache
        {
            private const int CacheSize = 64;
            private readonly (string? Message, ChatHook? ChatHook)[] _cache;
            public Cache()
            {
                _cache = new (string? Message, ChatHook? ChatHook)[CacheSize];
            }

            /// <summary>
            /// 获取指定消息的聊天钩子。
            /// </summary>
            /// <param name="message">消息。</param>
            /// <returns>指定消息的聊天钩子，如果不存在则返回 null。</returns>
            public ChatHook? Get(string message)
            {
                for (int i = 0; i < CacheSize; i++)
                {
                    if (_cache[i].Message == message)
                    {
                        return _cache[i].ChatHook;
                    }
                }

                return null;
            }

            /// <summary>
            /// 移除指定消息的聊天钩子。
            /// </summary>
            /// <param name="message">消息。</param>
            public void Remove(string message)
            {
                for (int i = 0; i < CacheSize; i++)
                {
                    if (_cache[i].Message == message)
                    {
                        _cache[i] = default;
                        return;
                    }
                }
            }

            /// <summary>
            /// 设置指定消息的聊天钩子。
            /// </summary>
            /// <param name="message">消息。</param>
            /// <param name="chatHook">聊天钩子。</param>
            public void Set(string message, ChatHook chatHook)
            {
                for (int i = 0; i < CacheSize; i++)
                {
                    if (_cache[i].Message == null)
                    {
                        _cache[i] = (message, chatHook);
                        return;
                    }
                }
            }
        }
    }
}