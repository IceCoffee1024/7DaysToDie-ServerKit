using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace SdtdServerKit.TraderProtectAreas
{
    /// <summary>
    /// 自定义商人保护区域管理器
    /// </summary>
    public static class TraderProtectManager
    {
        private static readonly object _writeLock = new object();
        private static volatile TraderProtectArea[] _snapshot = Array.Empty<TraderProtectArea>();

        /// <summary>
        /// 通过 Id 查找
        /// </summary>
        private static readonly Dictionary<int, TraderArea> _injectedAreas = new Dictionary<int, TraderArea>();

        private static ITraderProtectAreaRepository? _repository;
        private static volatile bool _isInitialized;

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>
        /// 当前自定义保护区域数量
        /// </summary>
        public static int Count => _snapshot.Length;

        /// <summary>
        /// 初始化管理器
        /// </summary>
        public static void Initialize(ITraderProtectAreaRepository repository)
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
                    InvokeOnMainThread(InjectAllToGameUnsafe);
                    _isInitialized = true;
                    CustomLogger.Debug($"商人保护区域：已从数据库加载并注入 {_snapshot.Length} 个保护区域到游戏世界");
                }
                catch (Exception ex)
                {
                    _repository = null;
                    _snapshot = Array.Empty<TraderProtectArea>();
                    _isInitialized = false;
                    CustomLogger.Error(ex, "商人保护区域：初始化失败");
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

                InvokeOnMainThread(RemoveAllFromGameUnsafe);
                _injectedAreas.Clear();
                _snapshot = Array.Empty<TraderProtectArea>();
                _repository = null;
                _isInitialized = false;
            }
            CustomLogger.Debug("商人保护区域：已从游戏世界移除所有自定义保护区域");
        }

        /// <summary>
        /// 获取所有商人保护区域
        /// </summary>
        public static List<TraderProtectArea> GetAll()
        {
            return new List<TraderProtectArea>(_snapshot);
        }

        /// <summary>
        /// 添加商人保护区域
        /// </summary>
        public static async Task<TraderProtectArea> AddAreaAsync(int x1, int z1, int x2, int z2, string? name)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("商人保护区域管理器尚未初始化");
            }

            var repo = _repository;
            if (repo == null)
            {
                throw new InvalidOperationException("商人保护区域仓储未注入");
            }

            int minX = Math.Min(x1, x2);
            int maxX = Math.Max(x1, x2);
            int minZ = Math.Min(z1, z2);
            int maxZ = Math.Max(z1, z2);

            var entity = new T_TraderProtectArea
            {
                CreatedAt = DateTime.Now,
                MinX = minX,
                MinZ = minZ,
                MaxX = maxX,
                MaxZ = maxZ,
                Name = name,
            };

            await repo.InsertAsync(entity);

            TraderProtectArea area;
            lock (_writeLock)
            {
                LoadSnapshotFromDatabaseUnsafe();

                area = _snapshot
                    .Where(a => a.PosA.x == minX && a.PosA.y == minZ && a.PosB.x == maxX && a.PosB.y == maxZ)
                    .OrderByDescending(a => a.Id)
                    .FirstOrDefault()
                    ?? new TraderProtectArea
                    {
                        Id = entity.Id,
                        PosA = new Vector2i(minX, minZ),
                        PosB = new Vector2i(maxX, maxZ),
                        Name = name,
                        CreatedAt = entity.CreatedAt,
                    };

                if (!_injectedAreas.ContainsKey(area.Id))
                {
                    var areaToInject = area;
                    InvokeOnMainThread(() => InjectOneToGameUnsafe(areaToInject));
                }
            }

            CustomLogger.Debug($"商人保护区域：添加并注入到游戏世界 Id={area.Id}, ({minX},{minZ}) - ({maxX},{maxZ}), 名称: {name ?? "(无)"}");
            return area;
        }

        /// <summary>
        /// 删除商人保护区域
        /// </summary>
        public static async Task<bool> RemoveAreaAsync(int id)
        {
            if (!_isInitialized)
            {
                return false;
            }

            var repo = _repository;
            if (repo == null)
            {
                return false;
            }

            var target = _snapshot.FirstOrDefault(a => a.Id == id);

            int affected = await repo.DeleteByIdAsync(id);
            if (affected <= 0)
            {
                return false;
            }

            lock (_writeLock)
            {
                LoadSnapshotFromDatabaseUnsafe();

                if (_injectedAreas.TryGetValue(id, out var injectedTa))
                {
                    InvokeOnMainThread(() => RemoveOneFromGameUnsafe(injectedTa));
                    _injectedAreas.Remove(id);
                }
            }

            if (target != null)
            {
                CustomLogger.Debug($"商人保护区域：删除并从游戏世界移除 Id={id}, ({target.PosA.x},{target.PosA.y}) - ({target.PosB.x},{target.PosB.y}), 名称: {target.Name ?? "(无)"}");
            }
            else
            {
                CustomLogger.Debug($"商人保护区域：删除并从游戏世界移除 Id={id}");
            }
            return true;
        }

        /// <summary>
        /// 清空所有自定义商人保护区域
        /// </summary>
        public static async Task ClearAllAsync()
        {
            if (!_isInitialized)
            {
                return;
            }

            var repo = _repository;
            if (repo == null)
            {
                return;
            }

            await repo.DeleteAllAsync();

            lock (_writeLock)
            {
                InvokeOnMainThread(RemoveAllFromGameUnsafe);
                _injectedAreas.Clear();
                _snapshot = Array.Empty<TraderProtectArea>();
            }

            CustomLogger.Debug("商人保护区域：已清空所有自定义保护区域并从游戏世界移除");
        }

        /// <summary>
        /// 检查指定坐标是否落在任意自定义商人保护区域内
        /// </summary>
        public static bool IsWithinProtectArea(int x, int z)
        {
            var snapshot = _snapshot;
            if (snapshot.Length == 0)
            {
                return false;
            }
            for (int i = 0; i < snapshot.Length; i++)
            {
                if (snapshot[i].Contains(x, z))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// 从数据库加载到内存快照
        /// </summary>
        private static void LoadSnapshotFromDatabaseUnsafe()
        {
            if (_repository == null)
            {
                return;
            }

            var records = _repository.GetAllAsync().GetAwaiter().GetResult();
            _snapshot = records.Select(r => new TraderProtectArea
            {
                Id = r.Id,
                PosA = new Vector2i(r.MinX, r.MinZ),
                PosB = new Vector2i(r.MaxX, r.MaxZ),
                Name = r.Name,
                CreatedAt = r.CreatedAt,
            }).ToArray();
        }

        /// <summary>
        /// 把当前内存快照中所有区域注入游戏世界
        /// </summary>
        private static void InjectAllToGameUnsafe()
        {
            var decorator = TryGetDecorator();
            if (decorator == null)
            {
                CustomLogger.Warn("商人保护区域：DynamicPrefabDecorator 不可用，无法注入到游戏世界");
                return;
            }

            foreach (var area in _snapshot)
            {
                try
                {
                    var ta = BuildTraderArea(area);
                    decorator.AddTrader(ta);
                    _injectedAreas[area.Id] = ta;
                }
                catch (Exception ex)
                {
                    CustomLogger.Error(ex, $"商人保护区域：注入区域 Id={area.Id} 失败");
                }
            }
        }

        /// <summary>
        /// 把单个区域注入游戏世界
        /// </summary>
        private static void InjectOneToGameUnsafe(TraderProtectArea area)
        {
            var decorator = TryGetDecorator();
            if (decorator == null)
            {
                return;
            }

            try
            {
                var ta = BuildTraderArea(area);
                decorator.AddTrader(ta);
                _injectedAreas[area.Id] = ta;
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, $"商人保护区域：注入区域 Id={area.Id} 失败");
            }
        }

        /// <summary>
        /// 把所有已注入的自定义区域从游戏列表中移除
        /// </summary>
        private static void RemoveAllFromGameUnsafe()
        {
            var decorator = TryGetDecorator();
            if (decorator == null)
            {
                return;
            }

            var traderAreas = decorator.GetTraderAreas();
            if (traderAreas == null)
            {
                return;
            }

            foreach (var ta in _injectedAreas.Values)
            {
                traderAreas.Remove(ta);
            }
            RecomputeProtectSizeXMaxUnsafe(decorator);
        }

        /// <summary>
        /// 把单个 TraderArea 从游戏列表移除
        /// </summary>
        private static void RemoveOneFromGameUnsafe(TraderArea ta)
        {
            var decorator = TryGetDecorator();
            if (decorator == null)
            {
                return;
            }

            var areas = decorator.GetTraderAreas();
            if (areas == null)
            {
                return;
            }

            areas.Remove(ta);
            RecomputeProtectSizeXMaxUnsafe(decorator);
        }

        /// <summary>
        /// 重新计算 ProtectSizeXMax
        /// </summary>
        private static void RecomputeProtectSizeXMaxUnsafe(DynamicPrefabDecorator decorator)
        {
            int max = 0;
            var areas = decorator.GetTraderAreas();
            if (areas != null)
            {
                for (int i = 0; i < areas.Count; i++)
                {
                    if (areas[i].ProtectSize.x > max)
                    {
                        max = areas[i].ProtectSize.x;
                    }
                }
            }
            decorator.ProtectSizeXMax = max;
        }

        /// <summary>
        /// 构造一个 TraderArea 实例
        /// </summary>
        private static TraderArea BuildTraderArea(TraderProtectArea area)
        {
            int minX = Math.Min(area.PosA.x, area.PosB.x);
            int maxX = Math.Max(area.PosA.x, area.PosB.x);
            int minZ = Math.Min(area.PosA.y, area.PosB.y);
            int maxZ = Math.Max(area.PosA.y, area.PosB.y);

            var position = new Vector3i(minX, 0, minZ);
            var size = new Vector3i(maxX - minX, 255, maxZ - minZ);
            var teleportVolumes = new List<Prefab.PrefabTeleportVolume>();
            return new TraderArea(position, size, Vector3i.zero, teleportVolumes);
        }

        /// <summary>
        /// 获取 DynamicPrefabDecorator
        /// </summary>
        private static DynamicPrefabDecorator? TryGetDecorator()
        {
            try
            {
                var gm = GameManager.Instance;
                if (gm == null)
                {
                    return null;
                }
                return gm.GetDynamicPrefabDecorator();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 在主线程同步执行操作
        /// </summary>
        private static void InvokeOnMainThread(Action action)
        {
            try
            {
                var ctx = ModApi.MainThreadSyncContext;
                // 1. 同步上下文未就绪 / 当前已是主线程：直接执行，避免 Send 死锁
                if (ctx == null || System.Threading.SynchronizationContext.Current == ctx)
                {
                    action();
                    return;
                }
                // 2. 游戏世界不可用（关闭中）：跳过，避免主线程消息泵已停时永久阻塞
                if (GameManager.Instance == null || GameManager.Instance.World == null)
                {
                    return;
                }
                // 3. 非主线程：切到主线程同步等待
                ctx.Send(_ => action(), null);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "商人保护区域：主线程操作执行失败");
            }
        }
    }
}
