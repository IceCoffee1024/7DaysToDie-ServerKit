using HarmonyLib;
using System;

namespace SdtdServerKit.HarmonyPatchers
{
    /// <summary>
    /// 修复原版 <see cref="WorldDecoratorBlocksFromBiome.decoratePrefabs"/> 的索引错位 NRE。
    /// <para>
    /// </summary>
    [HarmonyPatch(typeof(WorldDecoratorBlocksFromBiome), "decoratePrefabs")]
    internal static class WorldDecoratorBiomeNullGuardPatcher
    {
        private static readonly AccessTools.FieldRef<WorldDecoratorBlocksFromBiome, BiomeDefinition[]> _chunkBiomesRef =
            AccessTools.FieldRefAccess<WorldDecoratorBlocksFromBiome, BiomeDefinition[]>("chunkBiomes");

        [HarmonyPrefix]
        public static void Prefix(WorldDecoratorBlocksFromBiome __instance)
        {
            try
            {
                BiomeDefinition[] chunkBiomes = _chunkBiomesRef(__instance);
                if (chunkBiomes == null || chunkBiomes.Length == 0)
                {
                    return;
                }

                BiomeDefinition fallback = null;
                for (int i = 0; i < chunkBiomes.Length; i++)
                {
                    if (chunkBiomes[i] != null)
                    {
                        fallback = chunkBiomes[i];
                        break;
                    }
                }

                if (fallback == null)
                {
                    return;
                }

                for (int i = 0; i < chunkBiomes.Length; i++)
                {
                    if (chunkBiomes[i] == null)
                    {
                        chunkBiomes[i] = fallback;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区块装饰 biome 兜底：填充 chunkBiomes 失败");
            }
        }
    }
}
