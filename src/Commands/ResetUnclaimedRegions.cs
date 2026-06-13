using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SdtdServerKit.Commands
{
    /// <summary>
    /// 重置所有无领地石的区域（Region）
    /// </summary>
    public class ResetUnclaimedRegions : ConsoleCmdBase
    {
        public override string getDescription()
        {
            return "重置所有没有领地石(LCB)的Region区域";
        }

        public override string getHelp()
        {
            return "重置所有没有领地石(LCB)的Region区域，有领地石的Region会被跳过保护。\n" +
                "  ty-rur         - 重置所有无领地石的Region\n" +
                "  ty-rur list    - 列出所有有领地石保护的Region（不执行重置）";
        }

        public override string[] getCommands()
        {
            return new string[] { "ty-ResetUnclaimedRegions", "ty-rur" };
        }

        public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
        {
            try
            {
                if (_params.Count > 1)
                {
                    Log("参数错误，期望 0 或 1 个参数");
                    return;
                }

                // 获取有领地石保护的 Region 列表
                var claimedRegions = GetClaimedRegions();

                if (_params.Count == 1 && _params[0].Equals("list", StringComparison.OrdinalIgnoreCase))
                {
                    // 仅列出有保护的 Region
                    Log("有领地石保护的Region列表（共 {0} 个）：", claimedRegions.Count);
                    foreach (var region in claimedRegions)
                    {
                        Log("  {0}", region);
                    }
                    return;
                }

                ChunkProviderGenerateWorld? chunkProvider = GameManager.Instance.World.ChunkCache.ChunkProvider as ChunkProviderGenerateWorld;
                if (chunkProvider == null)
                {
                    Log("重置失败：无法获取 ChunkProviderGenerateWorld");
                    return;
                }

                string regionDir = GameIO.GetSaveGameDir() + "/Region";
                if (!Directory.Exists(regionDir))
                {
                    Log("重置失败：Region 目录不存在");
                    return;
                }

                var regionFiles = Directory.GetFiles(regionDir, "*.7rg");

                Log("开始重置无领地石区域，共 {0} 个Region文件待处理...", regionFiles.Length);
                ThreadManager.StartCoroutine(ResetUnclaimedRegionsCoroutine(chunkProvider, regionFiles, claimedRegions));
            }
            catch (Exception ex)
            {
                Log("执行 ResetUnclaimedRegions 时出错: {0}", ex.Message);
            }
        }

        private IEnumerator ResetUnclaimedRegionsCoroutine(
            ChunkProviderGenerateWorld chunkProvider,
            string[] regionFiles,
            HashSet<string> claimedRegions)
        {
            World world = GameManager.Instance.World;
            ChunkCluster chunkCache = world.ChunkCache;

            const ChunkProtectionLevel protectionLevel = ChunkProtectionLevel.None;

            int resetRegionCount = 0;
            int resetChunkCount = 0;
            int regeneratedChunkCount = 0;
            int skippedCount = 0;

            var syncedChunks = new HashSetLong();
            var regeneratedChunks = new HashSetLong();

            foreach (string regionFilePath in regionFiles)
            {
                string fileName = Path.GetFileName(regionFilePath);

                try
                {
                    // 跳过有领地石的 Region
                    if (claimedRegions.Contains(fileName))
                    {
                        skippedCount++;
                        continue;
                    }

                    // 解析 region 坐标 (r.X.Z.7rg)
                    if (!TryParseRegionCoords(fileName, out int regionX, out int regionZ))
                    {
                        Log("跳过无法解析的文件: {0}", fileName);
                        continue;
                    }

                    // 立即重置该 Region，返回被重置的所有 chunk key
                    HashSetLong resetChunks = chunkProvider.ResetRegion(regionX, regionZ, protectionLevel);

                    // 对当前已同步（在线玩家附近）的 chunk 立即重新生成全新地形并推送给客户端
                    syncedChunks.Clear();
                    regeneratedChunks.Clear();
                    foreach (long key in resetChunks)
                    {
                        if (chunkCache.ContainsChunkSync(key))
                        {
                            syncedChunks.Add(key);
                        }
                    }

                    if (syncedChunks.Count > 0)
                    {
                        foreach (long key in syncedChunks)
                        {
                            if (chunkProvider.GenerateSingleChunk(chunkCache, key, true))
                            {
                                regeneratedChunks.Add(key);
                            }
                            else
                            {
                                Log("重新生成 chunk 失败，世界坐标 XZ: {0}, {1}",
                                    WorldChunkCache.extractX(key) << 4, WorldChunkCache.extractZ(key) << 4);
                            }
                        }
                        world.m_ChunkManager.ResendChunksToClients(regeneratedChunks);
                        regeneratedChunkCount += regeneratedChunks.Count;
                    }

                    resetRegionCount++;
                    resetChunkCount += resetChunks.Count;
                }
                catch (Exception ex)
                {
                    Log("处理Region文件时出错 {0}: {1}", fileName, ex.Message);
                }

                // 每处理完一个 Region 让出一帧，把卡顿分摊到多帧
                yield return null;
            }

            Log("无领地石区域重置完成：重置了 {0} 个Region（共 {1} 个区块），" +
                "其中 {2} 个已同步区块已立即重新生成并推送给客户端，" +
                "跳过了 {3} 个有领地石保护的区域。" +
                "其余区块将在玩家再次靠近时由游戏自动重新生成。",
                resetRegionCount, resetChunkCount, regeneratedChunkCount, skippedCount);
        }

        /// <summary>
        /// 获取所有有领地石保护的 Region 文件名集合
        /// </summary>
        private HashSet<string> GetClaimedRegions()
        {
            var claimedRegions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            try
            {
                var persistentPlayers = GameManager.Instance.GetPersistentPlayerList();
                if (persistentPlayers == null)
                {
                    return claimedRegions;
                }

                // 遍历所有玩家的领地石位置
                foreach (var playerEntry in persistentPlayers.Players)
                {
                    var playerData = playerEntry.Value;
                    if (playerData?.LPBlocks == null)
                    {
                        continue;
                    }

                    foreach (var claimPos in playerData.LPBlocks)
                    {
                        // 将领地石坐标转换为 Region 文件名
                        string regionName = GetRegionFileName(claimPos.x, claimPos.z);
                        claimedRegions.Add(regionName);
                    }
                }
            }
            catch (Exception ex)
            {
                Log("获取领地石信息时出错: {0}", ex.Message);
            }

            return claimedRegions;
        }

        /// <summary>
        /// 根据世界坐标计算 Region 文件名
        /// </summary>
        private string GetRegionFileName(int worldX, int worldZ)
        {
            int regionX = worldX >= 0 ? worldX / 512 : (worldX - 511) / 512;
            int regionZ = worldZ >= 0 ? worldZ / 512 : (worldZ - 511) / 512;
            return $"r.{regionX}.{regionZ}.7rg";
        }

        /// <summary>
        /// 从文件名解析 Region 坐标
        /// </summary>
        private bool TryParseRegionCoords(string fileName, out int regionX, out int regionZ)
        {
            regionX = 0;
            regionZ = 0;

            // 格式: r.X.Z.7rg
            string name = fileName.Replace("r.", "").Replace(".7rg", "");
            string[] parts = name.Split('.');

            if (parts.Length != 2)
            {
                return false;
            }

            return int.TryParse(parts[0], out regionX) && int.TryParse(parts[1], out regionZ);
        }
    }
}
