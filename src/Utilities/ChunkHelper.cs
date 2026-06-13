using SdtdServerKit.Managers;

namespace SdtdServerKit.Utilities
{
    /// <summary>
    /// Provides a set of helper methods for chunks.
    /// </summary>
    public static class ChunkHelper
    {
        /// <summary>
        /// Forces the reload of the specified chunks.
        /// </summary>
        /// <param name="chunks"></param>
        public static void ForceReload(IEnumerable<Chunk> chunks)
        {
            var managedPlayers = LivePlayerManager.GetAll();
            Parallel.ForEach(managedPlayers, managedPlayer =>
            {
                foreach (var chunk in chunks)
                {
                    try
                    {
                        var chunkPosition = chunk.GetWorldPos();
                        var playerPosition = managedPlayer.EntityPlayer.GetPosition();
                        if (Math.Abs(playerPosition.x - chunkPosition.x) < 200F && Math.Abs(playerPosition.z - chunkPosition.z) < 200F)
                        {
                            managedPlayer.ClientInfo.SendPackage(NetPackageManager.GetPackage<NetPackageChunk>().Setup(chunk, true));
                        }
                    }
                    catch (Exception)
                    {
                        // CustomLogger.Warn(ex, "Error in ChunkHelper.ForceReload");
                    }
                }
            });

            ConnectionManager.Instance.FlushClientSendQueues();
        }

        /// <summary>
        /// Get chunks in area.
        /// </summary>
        /// <param name="offsetPosition"></param>
        /// <param name="prefabSize"></param>
        public static IEnumerable<Chunk> GetChunksInArea(Vector3i offsetPosition, Vector3i prefabSize)
        {
            var chunkSet = new HashSet<Chunk>();
            for (int i = 0; i < prefabSize.x; i++)
            {
                for (int j = 0; j < prefabSize.z; j++)
                {
                    for (int k = 0; k < prefabSize.y; k++)
                    {
                        int x = offsetPosition.x + i;
                        int y = offsetPosition.y + k;
                        int z = offsetPosition.z + j;
                        if (GameManager.Instance.World.IsChunkAreaLoaded(x, y, z) == false)
                        {
                            throw new Exception("The prefab is too far away or target area is not loaded. Chunk not loaded on that area.");
                        }

                        var chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(x, y, z);
                        chunkSet.Add(chunk);
                    }
                }
            }

            return chunkSet;
        }

        /// <summary>
        /// Calculate stability
        /// </summary>
        /// <param name="offsetPosition"></param>
        /// <param name="prefabSize"></param>
        public static void CalculateStability(Vector3i offsetPosition, Vector3i prefabSize)
        {
            var stabilityCalculator = new StabilityCalculator();
            stabilityCalculator.Init(GameManager.Instance.World);
            for (int i = 0; i < prefabSize.x; i++)
            {
                for (int j = 0; j < prefabSize.z; j++)
                {
                    for (int k = 0; k < prefabSize.y; k++)
                    {
                        var vector3i = new Vector3i(offsetPosition.x + i, offsetPosition.y + k, offsetPosition.z + j);
                        if (GameManager.Instance.World.GetBlock(vector3i).Equals(BlockValue.Air) == false)
                        {
                            stabilityCalculator.BlockPlacedAt(vector3i, false);
                        }
                    }
                }
            }
            stabilityCalculator.Cleanup();
        }

        /// <summary>
        /// Remove entity in area.
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="z1"></param>
        /// <param name="x2"></param>
        /// <param name="z2"></param>
        public static void RemoveEntityInArea(int x1, int z1, int x2, int z2)
        {
            // Ensures x1 is the smaller value, x2 is the larger value, and so are z1 and z2.
            if (x2 < x1)
            {
                (x1, x2) = (x2, x1);
            }
            if (z2 < z1)
            {
                (z1, z2) = (z2, z1);
            }

            var beRemoves = new List<EntityAlive>();

            foreach (var entity in GameManager.Instance.World.Entities.list)
            {
                if (entity is EntityAlive entityAlive && entityAlive.IsAlive() && entityAlive is EntityEnemy) // EntityClass.list[entityAlive.entityClass].bIsEnemyEntity
                {
                    // Checks if an entity is within a specified region
                    var vector3i = new Vector3i(entityAlive.GetPosition());
                    if (vector3i.x >= x1 && vector3i.x <= x2 && vector3i.z >= z1 && vector3i.z <= z2)
                    {
                        beRemoves.Add(entityAlive);
                    }
                }
            }

            foreach (EntityAlive entityAlive in beRemoves)
            {
                GameManager.Instance.World.RemoveEntity(entityAlive.entityId, EnumRemoveEntityReason.Despawned);
            }
        }

        /// <summary>
        /// Add prefab to RWG
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="offsetPosition"></param>
        /// <returns>Prefab instance id</returns>
        public static int AddPrefabToRWG(Prefab prefab, Vector3i offsetPosition)
        {
            var dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
            int prefabInstanceId = dynamicPrefabDecorator.GetNextId();
            var prefabInstance = new PrefabInstance(prefabInstanceId, prefab.location, offsetPosition, (byte)prefab.GetLocalRotation(), prefab, 0)
            {
                bPrefabCopiedIntoWorld = true
            };
            dynamicPrefabDecorator.GetDynamicPrefabs().Add(prefabInstance);
            dynamicPrefabDecorator.GetPOIPrefabs().Add(prefabInstance);
            dynamicPrefabDecorator.Save(PathAbstractions.WorldsSearchPaths.GetLocation(GameManager.Instance.World.ChunkCache.Name, null, null).FullPath);
            return prefabInstanceId;
        }

        /// <summary>
        /// Remove prefab from RWG
        /// </summary>
        /// <param name="prefabInstanceId"></param>
        /// <returns></returns>
        public static bool RemovePrefabFromRWG(int prefabInstanceId)
        {
            var dynamicPrefabDecorator = GameManager.Instance.GetDynamicPrefabDecorator();
            var dynamicPrefabs = dynamicPrefabDecorator.GetDynamicPrefabs();
            var POIPrefabs = dynamicPrefabDecorator.GetPOIPrefabs();
            foreach (var prefabInstance in dynamicPrefabs)
            {
                if (prefabInstance != null && prefabInstance.id == prefabInstanceId)
                {
                    dynamicPrefabs.Remove(prefabInstance);
                    POIPrefabs.Remove(prefabInstance);
                    dynamicPrefabDecorator.Save(PathAbstractions.WorldsSearchPaths.GetLocation(GameManager.Instance.World.ChunkCache.Name, null, null).FullPath);
                    return true;
                }
            }

            return false;
        }
    }
}
