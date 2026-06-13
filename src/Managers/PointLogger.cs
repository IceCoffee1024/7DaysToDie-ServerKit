using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Models;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SdtdServerKit.Managers
{
    /// <summary>
    /// 积分日志记录
    /// </summary>
    public static class PointLogger
    {
        private const int BatchSize = 64;

        private static readonly ConcurrentQueue<T_PointLog> _queue = new ConcurrentQueue<T_PointLog>();
        private static readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
        private static readonly object _stateLock = new object();

        private static IPointLogRepository? _repository;
        private static PointLogSettings? _settings;
        private static volatile bool _isRunning;
        private static CancellationTokenSource? _cts;
        private static Task? _workerTask;

        public static int PendingCount => _queue.Count;

        public static bool IsRunning => _isRunning;

        internal static void Start(IPointLogRepository repository, PointLogSettings settings)
        {
            lock (_stateLock)
            {
                if (_isRunning) return;

                _repository = repository ?? throw new ArgumentNullException(nameof(repository));
                _settings = settings ?? throw new ArgumentNullException(nameof(settings));
                _cts = new CancellationTokenSource();
                _isRunning = true;

                var token = _cts.Token;
                _workerTask = Task.Run(() => WorkerLoopAsync(token));
            }
        }

        /// <summary>更新分类开关</summary>
        internal static void UpdateSettings(PointLogSettings settings)
        {
            _settings = settings;
        }

        internal static void Stop()
        {
            CancellationTokenSource? cts;
            Task? worker;
            lock (_stateLock)
            {
                if (!_isRunning) return;
                _isRunning = false; 
                cts = _cts;
                worker = _workerTask;
                _cts = null;
                _workerTask = null;
            }

            try
            {
                cts?.Cancel();
                _signal.Release();  
                worker?.Wait(TimeSpan.FromSeconds(30));
            }
            catch (Exception ex)
            {
                CustomLogger.Debug(ex, "积分日志：停止 worker 时发生异常");
            }
            finally
            {
                cts?.Dispose();
            }

            int dropped = 0;
            while (_queue.TryDequeue(out _)) { dropped++; }
            if (dropped > 0)
            {
                CustomLogger.Debug("积分日志：worker 已退出，丢弃残留 {0} 条日志", dropped);
            }

            while (_signal.Wait(0)) { /* drain residual */ }

            _repository = null;
            _settings = null;

        }

        /// <summary>
        /// 记录一条积分变动日志
        /// </summary>
        public static void Log(
            PointLogCategory category,
            string playerId,
            string? playerName,
            int spend,
            int balance,
            string? note = null,
            PointChangeType? changeType = null)
        {
            if (!_isRunning) return;
            if (string.IsNullOrEmpty(playerId)) return;

            var settings = _settings;
            if (settings == null || !settings.IsEnabled) return;
            if (!IsCategoryEnabled(settings, category)) return;

            var ct = changeType ?? InferChangeType(spend);

            var entity = new T_PointLog
            {
                CreatedAt = DateTime.Now,
                PlayerId = playerId,
                PlayerName = playerName,
                Category = category,
                ChangeType = ct,
                Spend = spend,
                Balance = balance,
                Note = note,
            };

            _queue.Enqueue(entity);
            try { _signal.Release(); } catch { /* SemaphoreFullException 时忽略 */ }
        }

        private static PointChangeType InferChangeType(int spend)
        {
            if (spend > 0) return PointChangeType.Add;
            if (spend < 0) return PointChangeType.Sub;
            return PointChangeType.NoChange;
        }

        private static bool IsCategoryEnabled(PointLogSettings s, PointLogCategory c)
        {
            return c switch
            {
                PointLogCategory.Shop => s.LogShop,
                PointLogCategory.SignIn => s.LogSignIn,
                PointLogCategory.Transfer => s.LogTransfer,
                PointLogCategory.Teleport => s.LogTeleport,
                PointLogCategory.ZombieKill => s.LogZombieKill,
                PointLogCategory.Lottery => s.LogLottery,
                PointLogCategory.CdKey => s.LogCdKey,
                PointLogCategory.LevelGift => s.LogLevelGift,
                PointLogCategory.VipGift => s.LogVipGift,
                PointLogCategory.WebApi => s.LogWebApi,
                PointLogCategory.External => s.LogExternal,
                PointLogCategory.Other => s.LogOther,
                _ => s.LogOther,
            };
        }

        private static async Task WorkerLoopAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    await _signal.WaitAsync(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                await DrainBatchAsync();
            }

            await DrainAllAsync();
        }

        /// <summary>批量处理</summary>
        private static async Task DrainBatchAsync()
        {
            var repo = _repository;
            if (repo == null) return;

            int processed = 0;
            while (processed < BatchSize && _queue.TryDequeue(out var entity))
            {
                try
                {
                    await repo.InsertAsync(entity);
                }
                catch (Exception ex)
                {
                    CustomLogger.Warn(ex, "积分日志：写入失败 (Category={0}, PlayerId={1})", entity.Category, entity.PlayerId);
                }
                processed++;
            }
        }

        /// <summary>把队列全部清空</summary>
        private static async Task DrainAllAsync()
        {
            var repo = _repository;
            if (repo == null) return;

            while (_queue.TryDequeue(out var entity))
            {
                try
                {
                    await repo.InsertAsync(entity);
                }
                catch (Exception ex)
                {
                    CustomLogger.Warn(ex, "积分日志：关闭时写入失败 (Category={0}, PlayerId={1})", entity.Category, entity.PlayerId);
                }
            }
        }
    }
}
