using System;
using System.Collections.Generic;
using UnityEngine;

namespace SdtdServerKit.Functions
{
    /// <summary>
    /// 系统房（POI）防护区域管理器
    /// </summary>
    internal static class PoiProtectionZone
    {
        private const int GridCellSize = 16;

        private static readonly object _lock = new object();
        private static readonly Dictionary<int, BoundsInt> _landClaimZones = new Dictionary<int, BoundsInt>();
        private static readonly Dictionary<int, BoundsInt> _bedZones = new Dictionary<int, BoundsInt>();
        private static readonly Dictionary<long, List<int>> _landClaimGrid = new Dictionary<long, List<int>>();
        private static readonly Dictionary<long, List<int>> _bedGrid = new Dictionary<long, List<int>>();
        private static volatile bool _initialized;

        /// <summary>
        /// 是否已经初始化
        /// </summary>
        public static bool Initialized => _initialized;

        /// <summary>
        /// 初始化 POI 防护区域，从所有 POI 创建领地石/睡袋禁放范围。
        /// 应在游戏世界加载完成后（GameStartDone）调用。
        /// </summary>
        public static void Initialize()
        {
            try
            {
                lock (_lock)
                {
                    _landClaimZones.Clear();
                    _bedZones.Clear();
                    _landClaimGrid.Clear();
                    _bedGrid.Clear();

                    var gameManager = GameManager.Instance;
                    if (gameManager == null)
                    {
                        CustomLogger.Warn("系统房防护：GameManager 尚未就绪，跳过初始化");
                        return;
                    }

                    var decorator = gameManager.GetDynamicPrefabDecorator();
                    if (decorator == null)
                    {
                        CustomLogger.Warn("系统房防护：DynamicPrefabDecorator 为空，跳过初始化");
                        return;
                    }

                    // 计算领地石/睡袋的禁放半径
                    int landClaimSize = GameStats.GetInt(EnumGameStats.LandClaimSize);
                    int landClaimRadius = landClaimSize % 2 == 1 ? (landClaimSize - 1) / 2 : landClaimSize / 2;
                    int bedrollRadius = GamePrefs.GetInt(EnumGamePrefs.BedrollDeadZoneSize);

                    var landClaimPadding = new Vector3i(landClaimRadius, 0, landClaimRadius);
                    var bedrollPadding = new Vector3i(bedrollRadius, 0, bedrollRadius);

                    var pois = decorator.GetPOIPrefabs();
                    if (pois == null)
                    {
                        CustomLogger.Warn("系统房防护：POI 列表为空，跳过初始化");
                        return;
                    }

                    foreach (var poi in pois)
                    {
                        if (poi == null || poi.boundingBoxSize == Vector3i.zero)
                        {
                            continue;
                        }

                        if (_landClaimZones.ContainsKey(poi.id) || _bedZones.ContainsKey(poi.id))
                        {
                            continue;
                        }

                        var landBounds = new BoundsInt(
                            poi.boundingBoxPosition - landClaimPadding,
                            poi.boundingBoxSize + (landClaimPadding * 2));

                        var bedBounds = new BoundsInt(
                            poi.boundingBoxPosition - bedrollPadding,
                            poi.boundingBoxSize + (bedrollPadding * 2));

                        _landClaimZones.Add(poi.id, landBounds);
                        _bedZones.Add(poi.id, bedBounds);

                        AddToGrid(_landClaimGrid, poi.id, landBounds);
                        AddToGrid(_bedGrid, poi.id, bedBounds);
                    }

                    _initialized = true;
                    CustomLogger.Debug($"系统房防护：已加载领地石防护区 {_landClaimZones.Count} 个，睡袋防护区 {_bedZones.Count} 个");
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "系统房防护：初始化失败");
            }
        }

        /// <summary>
        /// 清理防护区域数据。
        /// </summary>
        public static void Cleanup()
        {
            lock (_lock)
            {
                _landClaimZones.Clear();
                _bedZones.Clear();
                _landClaimGrid.Clear();
                _bedGrid.Clear();
                _initialized = false;
            }
        }

        /// <summary>
        /// 判断指定坐标是否在任意 POI 的领地石防护区域内
        /// </summary>
        public static bool IsInLandClaimZone(Vector3i pos)
        {
            if (!_initialized)
            {
                return false;
            }

            lock (_lock)
            {
                return ContainsInZones(pos, _landClaimGrid, _landClaimZones);
            }
        }

        /// <summary>
        /// 判断指定坐标是否在任意 POI 的睡袋防护区域内
        /// </summary>
        public static bool IsInBedZone(Vector3i pos)
        {
            if (!_initialized)
            {
                return false;
            }

            lock (_lock)
            {
                return ContainsInZones(pos, _bedGrid, _bedZones);
            }
        }

        private static long MakeGridKey(int cellX, int cellZ)
        {
            return ((long)cellX << 32) | (uint)cellZ;
        }

        private static void AddToGrid(Dictionary<long, List<int>> grid, int poiId, BoundsInt bounds)
        {
            int minCellX = Mathf.FloorToInt((float)bounds.xMin / GridCellSize);
            int maxCellX = Mathf.FloorToInt((float)(bounds.xMax - 1) / GridCellSize);
            int minCellZ = Mathf.FloorToInt((float)bounds.zMin / GridCellSize);
            int maxCellZ = Mathf.FloorToInt((float)(bounds.zMax - 1) / GridCellSize);

            for (int cx = minCellX; cx <= maxCellX; cx++)
            {
                for (int cz = minCellZ; cz <= maxCellZ; cz++)
                {
                    long key = MakeGridKey(cx, cz);
                    if (!grid.TryGetValue(key, out var list))
                    {
                        list = new List<int>(2);
                        grid[key] = list;
                    }
                    list.Add(poiId);
                }
            }
        }

        private static bool ContainsInZones(Vector3i pos,
            Dictionary<long, List<int>> grid,
            Dictionary<int, BoundsInt> zones)
        {
            int cellX = Mathf.FloorToInt((float)pos.x / GridCellSize);
            int cellZ = Mathf.FloorToInt((float)pos.z / GridCellSize);
            if (!grid.TryGetValue(MakeGridKey(cellX, cellZ), out var candidates))
            {
                return false;
            }

            for (int i = 0; i < candidates.Count; i++)
            {
                if (zones.TryGetValue(candidates[i], out var bounds) && bounds.Contains(pos))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
