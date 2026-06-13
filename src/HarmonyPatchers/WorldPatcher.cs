using HarmonyLib;

namespace SdtdServerKit.HarmonyPatchers
{
    [HarmonyPatch(typeof(World))]
    internal class WorldPatcher
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(World.SpawnEntityInWorld))]
        public static void After_SpawnEntityInWorld(Entity _entity)
        {
            if (_entity is EntityAlive entityAlive)
            {
                ModEventHub.OnEntitySpawned(new EntityInfo()
                {
                    EntityId = entityAlive.entityId,
                    EntityName = entityAlive.EntityName,
                    Position = entityAlive.position.ToPosition(),
                    EntityType = (SdtdServerKit.Models.EntityType)entityAlive.entityType
                });
            }
        }

        public static bool Before_AddFallingBlock(World __instance, Vector3i _blockPos)
        {
            if (__instance == null)
            {
                return true;
            }
            HandleFallingBlockCoroutine(__instance, _blockPos);
            return false;
        }

        private static void HandleFallingBlockCoroutine(World w, Vector3i block)
        {
            BlockValue blockValue = w.GetBlock(block);

            if (blockValue.isair || blockValue.ischild || blockValue.Block.StabilityIgnore)
            {
                return;
            }

            List<BlockChangeInfo> blockChanges = new List<BlockChangeInfo>()
            {
                new BlockChangeInfo(block, new BlockValue(0U), true, false)
            };

            GameManager.Instance.SetBlocksRPC(blockChanges, null);
        }
    }
}