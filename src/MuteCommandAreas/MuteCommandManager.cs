using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SdtdServerKit.MuteCommandAreas
{
    /// <summary>
    /// 区域命令禁用管理器（支持数据库持久化）
    /// </summary>
    public static class MuteCommandManager
    {
        private static readonly List<MuteCommandArea> _areas = new List<MuteCommandArea>();
        private static IMuteCommandAreaRepository? _repository;
        private static bool _isInitialized = false;

        /// <summary>
        /// 初始化管理器（从数据库加载数据）
        /// </summary>
        public static async void Initialize(IMuteCommandAreaRepository repository)
        {
            _repository = repository;
            await LoadFromDatabaseAsync();
            _isInitialized = true;
        }

        /// <summary>
        /// 从数据库加载数据到内存
        /// </summary>
        private static async System.Threading.Tasks.Task LoadFromDatabaseAsync()
        {
            if (_repository == null) return;

            try
            {
                var records = await _repository.GetAllAsync();
                lock (_areas)
                {
                    _areas.Clear();

                    // 按区域分组，合并同一区域的多个命令
                    var grouped = records.GroupBy(r => new { r.MinX, r.MinZ, r.MaxX, r.MaxZ });

                    foreach (var group in grouped)
                    {
                        var area = new MuteCommandArea
                        {
                            PosA = new Vector2i(group.Key.MinX, group.Key.MinZ),
                            PosB = new Vector2i(group.Key.MaxX, group.Key.MaxZ),
                            MutedCommands = group.Select(r => r.Command).ToList()
                        };
                        _areas.Add(area);
                    }
                }

                CustomLogger.Debug($"区域命令禁用管理器：已加载 {_areas.Count} 个禁用区域");
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "区域命令禁用管理器：从数据库加载数据失败");
            }
        }

        /// <summary>
        /// 添加区域命令禁用
        /// </summary>
        public static bool MuteCommand(int x1, int z1, int x2, int z2, string cmd)
        {
            int minX = Math.Min(x1, x2), maxX = Math.Max(x1, x2);
            int minZ = Math.Min(z1, z2), maxZ = Math.Max(z1, z2);

            lock (_areas)
            {
                // 查找是否已存在相同区域
                var existingArea = _areas.FirstOrDefault(a =>
                    a.PosA.x == minX && a.PosA.y == minZ && a.PosB.x == maxX && a.PosB.y == maxZ);

                if (existingArea != null)
                {
                    // 区域已存在，添加命令到列表
                    if (!existingArea.MutedCommands.Contains(cmd))
                    {
                        existingArea.MutedCommands.Add(cmd);
                    }
                }
                else
                {
                    // 创建新区域
                    var area = new MuteCommandArea
                    {
                        PosA = new Vector2i(minX, minZ),
                        PosB = new Vector2i(maxX, maxZ),
                        MutedCommands = new List<string> { cmd }
                    };
                    _areas.Add(area);
                }
            }

            // 保存到数据库
            if (_repository != null && _isInitialized)
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        var entity = new T_MuteCommandArea
                        {
                            MinX = minX,
                            MinZ = minZ,
                            MaxX = maxX,
                            MaxZ = maxZ,
                            Command = cmd,
                            CreatedAt = DateTime.Now
                        };
                        await _repository.InsertAsync(entity);
                        CustomLogger.Debug($"区域命令禁用：已保存到数据库 - 区域({minX},{minZ})-({maxX},{maxZ}), 命令: {cmd}");
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Error(ex, "区域命令禁用：保存到数据库失败");
                    }
                });
            }

            return true;
        }

        /// <summary>
        /// 取消区域命令禁用
        /// </summary>
        public static bool UnMuteCommand(int x1, int z1, int x2, int z2, string cmd)
        {
            int minX = Math.Min(x1, x2), maxX = Math.Max(x1, x2);
            int minZ = Math.Min(z1, z2), maxZ = Math.Max(z1, z2);

            bool removed = false;
            lock (_areas)
            {
                var area = _areas.FirstOrDefault(a =>
                    a.PosA.x == minX && a.PosA.y == minZ && a.PosB.x == maxX && a.PosB.y == maxZ);

                if (area != null)
                {
                    removed = area.MutedCommands.Remove(cmd);

                    // 如果区域没有命令了，删除整个区域
                    if (area.MutedCommands.Count == 0)
                    {
                        _areas.Remove(area);
                    }
                }
            }

            // 从数据库删除
            if (removed && _repository != null && _isInitialized)
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await _repository.DeleteByAreaAndCommandAsync(minX, minZ, maxX, maxZ, cmd);
                        CustomLogger.Debug($"区域命令禁用：已从数据库删除 - 区域({minX},{minZ})-({maxX},{maxZ}), 命令: {cmd}");
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Error(ex, "区域命令禁用：从数据库删除失败");
                    }
                });
            }

            return removed;
        }

        /// <summary>
        /// 更新区域命令禁用（修改区域坐标或命令列表）
        /// </summary>
        public static bool UpdateArea(int oldX1, int oldZ1, int oldX2, int oldZ2, int newX1, int newZ1, int newX2, int newZ2, string newCommands)
        {
            int oldMinX = Math.Min(oldX1, oldX2), oldMaxX = Math.Max(oldX1, oldX2);
            int oldMinZ = Math.Min(oldZ1, oldZ2), oldMaxZ = Math.Max(oldZ1, oldZ2);
            int newMinX = Math.Min(newX1, newX2), newMaxX = Math.Max(newX1, newX2);
            int newMinZ = Math.Min(newZ1, newZ2), newMaxZ = Math.Max(newZ1, newZ2);

            // 解析新的命令列表
            var newCommandList = newCommands.Split(new[] { ',', '，' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(c => c.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();

            if (newCommandList.Count == 0)
            {
                return false;
            }

            lock (_areas)
            {
                // 查找旧区域
                var oldArea = _areas.FirstOrDefault(a =>
                    a.PosA.x == oldMinX && a.PosA.y == oldMinZ && a.PosB.x == oldMaxX && a.PosB.y == oldMaxZ);

                if (oldArea == null)
                {
                    return false;
                }

                // 移除旧区域
                _areas.Remove(oldArea);

                // 添加新区域
                var newArea = new MuteCommandArea
                {
                    PosA = new Vector2i(newMinX, newMinZ),
                    PosB = new Vector2i(newMaxX, newMaxZ),
                    MutedCommands = newCommandList
                };
                _areas.Add(newArea);
            }

            // 更新数据库
            if (_repository != null && _isInitialized)
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        // 删除旧区域的所有记录
                        await _repository.DeleteByAreaAsync(oldMinX, oldMinZ, oldMaxX, oldMaxZ);

                        // 插入新区域的所有命令
                        foreach (var cmd in newCommandList)
                        {
                            var entity = new T_MuteCommandArea
                            {
                                MinX = newMinX,
                                MinZ = newMinZ,
                                MaxX = newMaxX,
                                MaxZ = newMaxZ,
                                Command = cmd,
                                CreatedAt = DateTime.Now
                            };
                            await _repository.InsertAsync(entity);
                        }

                        CustomLogger.Debug($"区域命令禁用：已更新 - 旧区域({oldMinX},{oldMinZ})-({oldMaxX},{oldMaxZ}) -> 新区域({newMinX},{newMinZ})-({newMaxX},{newMaxZ}), 命令: {newCommands}");
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Error(ex, "区域命令禁用：更新数据库失败");
                    }
                });
            }

            return true;
        }

        /// <summary>
        /// 检查指定坐标是否禁用了指定命令
        /// </summary>
        public static bool IsCommandMuted(int x, int z, string cmd)
        {
            lock (_areas)
            {
                foreach (var area in _areas)
                {
                    if (x >= area.PosA.x && x <= area.PosB.x && z >= area.PosA.y && z <= area.PosB.y)
                    {
                        if (area.MutedCommands.Contains(cmd))
                            return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// 获取所有区域命令禁用列表
        /// </summary>
        public static List<MuteCommandArea> GetAll()
        {
            lock (_areas)
            {
                return new List<MuteCommandArea>(_areas);
            }
        }

        /// <summary>
        /// 清空所有区域命令禁用
        /// </summary>
        public static void Clear()
        {
            lock (_areas)
            {
                _areas.Clear();
            }

            // 从数据库清空
            if (_repository != null && _isInitialized)
            {
                System.Threading.Tasks.Task.Run(async () =>
                {
                    try
                    {
                        await _repository.DeleteAllAsync();
                    }
                    catch (Exception ex)
                    {
                        CustomLogger.Error(ex, "区域命令禁用：从数据库清空失败");
                    }
                });
            }
        }
    }
}
