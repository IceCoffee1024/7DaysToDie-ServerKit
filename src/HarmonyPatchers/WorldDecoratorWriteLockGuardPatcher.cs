using HarmonyLib;
using System;
using System.Threading;

namespace SdtdServerKit.HarmonyPatchers
{
    /// <summary>
    /// 兜底释放写锁
    [HarmonyPatch(typeof(WorldDecoratorBlocksFromBiome), nameof(WorldDecoratorBlocksFromBiome.DecorateChunkOverlapping))]
    internal static class WorldDecoratorWriteLockGuardPatcher
    {
        private static readonly AccessTools.FieldRef<WorldDecoratorBlocksFromBiome, ReaderWriterLockSlim> _rwlockRef =
            AccessTools.FieldRefAccess<WorldDecoratorBlocksFromBiome, ReaderWriterLockSlim>("rwlock");

        [HarmonyFinalizer]
        public static void Finalizer(WorldDecoratorBlocksFromBiome __instance, Exception __exception)
        {
            if (__exception == null || __instance == null)
            {
                return;
            }

            try
            {
                ReaderWriterLockSlim rwlock = _rwlockRef(__instance);
                if (rwlock != null && rwlock.IsWriteLockHeld)
                {
                    rwlock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区块装饰异常兜底：释放装饰写锁失败");
            }

        }
    }
}
