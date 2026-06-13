using SdtdServerKit.Managers;
using System.Collections.Immutable;

namespace SdtdServerKit.Hooks
{
    internal delegate Task LevelUpHook(ManagedPlayer managedPlayer, int newLevel);

    /// <summary>
    /// 玩家升级钩子
    /// </summary>
    internal static class PlayerLevelUpHook
    {
        private static ImmutableList<LevelUpHook> _hooks = ImmutableList<LevelUpHook>.Empty;

        /// <summary>
        /// 添加升级钩子
        /// </summary>
        /// <param name="hook">要添加的钩子</param>
        public static void AddHook(LevelUpHook hook)
        {
            _hooks = _hooks.Add(hook);
        }

        /// <summary>
        /// 移除升级钩子
        /// </summary>
        /// <param name="hook">要移除的钩子</param>
        public static void RemoveHook(LevelUpHook hook)
        {
            _hooks = _hooks.Remove(hook);
        }

        /// <summary>
        /// 触发玩家升级事件
        /// </summary>
        /// <param name="managedPlayer">玩家</param>
        /// <param name="newLevel">新等级</param>
        internal static async Task OnPlayerLevelUp(ManagedPlayer managedPlayer, int newLevel)
        {
            foreach (var hook in _hooks)
            {
                try
                {
                    await hook.Invoke(managedPlayer, newLevel);
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex, "玩家升级钩子执行错误");
                }
            }
        }
    }
}
