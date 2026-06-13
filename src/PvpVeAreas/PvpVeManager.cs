using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SdtdServerKit.PvpVeAreas
{
    /// <summary>
    /// PVP/PVE 混合区域管理器
    /// </summary>
    public static class PvpVeManager
    {
        private static readonly object _writeLock = new object();
        private static volatile PvpVeArea[] _snapshot = Array.Empty<PvpVeArea>();
        private static IPvpVeAreaRepository? _repository;
        private static volatile bool _isInitialized;

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// 当前自定义区域数量
        /// </summary>
        public static int Count => _snapshot.Length;

        /// <summary>
        /// 初始化管理器
        /// </summary>
        public static void Initialize(IPvpVeAreaRepository repository)
        {
            lock (_writeLock)
            {
                if (_isInitialized)
                {
                    return;
                }

                try
                {
                    _repository = repository;
                    LoadSnapshotFromDatabaseUnsafe();
                    _isInitialized = true;
                    CustomLogger.Debug($"PVP/PVE 混合区域：已从数据库加载 {_snapshot.Length} 个自定义区域");
                }
                catch (Exception ex)
                {
                    _repository = null;
                    _snapshot = Array.Empty<PvpVeArea>();
                    _isInitialized = false;
                    CustomLogger.Error(ex, "PVP/PVE 混合区域：初始化失败");
                }
            }
        }

        /// <summary>
        /// 关闭管理器
        /// </summary>
        public static void Shutdown()
        {
            lock (_writeLock)
            {
                if (!_isInitialized)
                {
                    return;
                }

                _snapshot = Array.Empty<PvpVeArea>();
                _repository = null;
                _isInitialized = false;
            }
            CustomLogger.Debug("PVP/PVE 混合区域：管理器已关闭");
        }

        /// <summary>
        /// 获取所有自定义区域
        /// </summary>
        public static List<PvpVeArea> GetAll()
        {
            return new List<PvpVeArea>(_snapshot);
        }

        /// <summary>
        /// 添加 PVP/PVE 混合区域
        /// </summary>
        public static async Task<PvpVeArea> AddAreaAsync(int x1, int z1, int x2, int z2,
            int killMode, int dropOnDeath, int landClaimOnline, int landClaimOffline,
            string buffName, string? name)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("PVP/PVE 混合区域管理器尚未初始化");
            }

            var repo = _repository;
            if (repo == null)
            {
                throw new InvalidOperationException("PVP/PVE 混合区域仓储未注入");
            }

            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);
            int minZ = Math.Min(z1, z2);
            int maxZ = Math.Max(z1, z2);

            var entity = new T_PvpVeArea
            {
                CreatedAt = DateTime.Now,
                MinX = minX,
                MinZ = minZ,
                MaxX = maxX,
                MaxZ = maxZ,
                KillMode = ClampKillMode(killMode),
                DropOnDeath = ClampDropOnDeath(dropOnDeath),
                LandClaimOnline = Math.Max(0, landClaimOnline),
                LandClaimOffline = Math.Max(0, landClaimOffline),
                BuffName = buffName ?? string.Empty,
                Name = name,
            };

            await repo.InsertAsync(entity);

            PvpVeArea area;
            lock (_writeLock)
            {
                LoadSnapshotFromDatabaseUnsafe();
                area = _snapshot
                    .Where(a => a.PosA.x == minX && a.PosA.y == minZ && a.PosB.x == maxX && a.PosB.y == maxZ)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefault()
                    ?? new PvpVeArea
                    {
                        Id = entity.Id,
                        PosA = new Vector2i(minX, minZ),
                        PosB = new Vector2i(maxX, maxZ),
                        KillMode = entity.KillMode,
                        DropOnDeath = entity.DropOnDeath,
                        LandClaimOnline = entity.LandClaimOnline,
                        LandClaimOffline = entity.LandClaimOffline,
                        BuffName = entity.BuffName,
                        Name = entity.Name,
                        CreatedAt = entity.CreatedAt,
                    };
            }

            CustomLogger.Debug($"PVP/PVE 混合区域：已添加 Id={area.Id}, ({minX},{minZ}) - ({maxX},{maxZ}), " +
                $"killMode={area.KillMode}, dropOnDeath={area.DropOnDeath}, " +
                $"landClaim(在线/离线)={area.LandClaimOnline}/{area.LandClaimOffline}, buff={area.BuffName}, 名称: {name ?? "(无)"}");
            return area;
        }

        /// <summary>
        /// 删除指定 Id 的区域
        /// </summary>
        public static async Task<bool> RemoveAreaAsync(int id)
        {
            if (!_isInitialized) return false;
            var repo = _repository;
            if (repo == null) return false;

            var target = _snapshot.FirstOrDefault(a => a.Id == id);

            int affected = await repo.DeleteByIdAsync(id);
            if (affected <= 0) return false;

            lock (_writeLock)
            {
                LoadSnapshotFromDatabaseUnsafe();
            }

            if (target != null)
            {
                CustomLogger.Debug($"PVP/PVE 混合区域：已删除 Id={id}, ({target.PosA.x},{target.PosA.y}) - ({target.PosB.x},{target.PosB.y}), 名称: {target.Name ?? "(无)"}");
            }
            else
            {
                CustomLogger.Debug($"PVP/PVE 混合区域：已删除 Id={id}");
            }
            return true;
        }

        /// <summary>
        /// 清空所有自定义区域
        /// </summary>
        public static async Task ClearAllAsync()
        {
            if (!_isInitialized) return;
            var repo = _repository;
            if (repo == null) return;

            await repo.DeleteAllAsync();
            lock (_writeLock)
            {
                _snapshot = Array.Empty<PvpVeArea>();
            }
            CustomLogger.Debug("PVP/PVE 混合区域：已清空所有自定义区域");
        }

        /// <summary>
        /// 在快照中查找包含指定坐标的区域
        /// </summary>
        public static PvpVeArea? FindArea(int x, int z)
        {
            var snapshot = _snapshot;
            PvpVeArea? best = null;
            long bestArea = long.MaxValue;
            int bestId = int.MinValue;

            for (int i = 0; i < snapshot.Length; i++)
            {
                var a = snapshot[i];
                if (!a.Contains(x, z)) continue;

                long w = (long)a.PosB.x - a.PosA.x + 1;
                long h = (long)a.PosB.y - a.PosA.y + 1;
                long area = w * h;

                if (best == null
                    || area < bestArea
                    || (area == bestArea && a.Id > bestId))
                {
                    best = a;
                    bestArea = area;
                    bestId = a.Id;
                }
            }
            return best;
        }

        /// <summary>
        /// 从数据库加载到内存快照
        /// </summary>
        private static void LoadSnapshotFromDatabaseUnsafe()
        {
            if (_repository == null) return;

            var records = _repository.GetAllAsync().GetAwaiter().GetResult();
            _snapshot = records.Select(r => new PvpVeArea
            {
                Id = r.Id,
                PosA = new Vector2i(r.MinX, r.MinZ),
                PosB = new Vector2i(r.MaxX, r.MaxZ),
                KillMode = r.KillMode,
                DropOnDeath = r.DropOnDeath,
                LandClaimOnline = r.LandClaimOnline,
                LandClaimOffline = r.LandClaimOffline,
                BuffName = r.BuffName ?? string.Empty,
                Name = r.Name,
                CreatedAt = r.CreatedAt,
            }).ToArray();
        }

        private static int ClampKillMode(int v) => Math.Max(0, Math.Min(3, v));
        private static int ClampDropOnDeath(int v) => Math.Max(0, Math.Min(3, v));
    }
}
