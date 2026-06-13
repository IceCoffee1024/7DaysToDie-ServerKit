using SdtdServerKit.Data.IRepositories;
using System;

namespace SdtdServerKit.PublicApi
{
    /// <summary>
    /// 对外公开的积分系统 API
    /// </remarks>
    public static class PointsApi
    {
        /// <summary>
        /// 检查 ServerKit 积分系统是否已经就绪
        /// </summary>
        public static bool IsAvailable()
        {
            try
            {
                if (ModApi.ServiceContainer == null)
                {
                    return false;
                }
                return ModApi.ServiceContainer.Resolve<IPointsInfoRepository>() != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取玩家积分
        /// </summary>
        public static int Get(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return 0;
            }

            try
            {
                var repo = ModApi.ServiceContainer.Resolve<IPointsInfoRepository>();
                return repo.GetPointsByIdAsync(userId).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                CustomLogger.Warn("[PointsApi.Get] 获取积分失败: userId={0}, err={1}", userId, ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// 增加玩家积分（玩家不存在时会自动创建记录）
        /// </summary>
        public static bool Add(string userId, int points, out int balance)
        {
            balance = 0;
            if (string.IsNullOrEmpty(userId) || points == 0)
            {
                if (!string.IsNullOrEmpty(userId))
                {
                    balance = Get(userId);
                    return true;
                }
                return false;
            }

            try
            {
                var repo = ModApi.ServiceContainer.Resolve<IPointsInfoRepository>();
                int affected = repo.ChangePointsAsync(userId, points).GetAwaiter().GetResult();
                if (affected <= 0)
                {
                    balance = Get(userId);
                    return false;
                }
                var entity = repo.GetByIdAsync(userId).GetAwaiter().GetResult();
                balance = entity?.Points ?? 0;
                Managers.PointLogger.Log(Models.PointLogCategory.External, userId, entity?.PlayerName,
                    points, balance, "外部 Mod 调用 PointsApi.Add");
                return true;
            }
            catch (Exception ex)
            {
                CustomLogger.Warn("[PointsApi.Add] 增加积分失败: userId={0}, points={1}, err={2}", userId, points, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 扣除玩家积分（余额不足时拒绝扣减，不会变成负数）
        /// </summary>
        public static bool Sub(string userId, int points, out int balance)
        {
            balance = 0;
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }
            if (points <= 0)
            {
                balance = Get(userId);
                return false;
            }

            try
            {
                var repo = ModApi.ServiceContainer.Resolve<IPointsInfoRepository>();
                int affected = repo.ChangePointsAsync(userId, -points).GetAwaiter().GetResult();
                var entity = repo.GetByIdAsync(userId).GetAwaiter().GetResult();
                balance = entity?.Points ?? 0;
                if (affected > 0)
                {
                    Managers.PointLogger.Log(Models.PointLogCategory.External, userId, entity?.PlayerName,
                        -points, balance, "外部 Mod 调用 PointsApi.Sub");
                }
                return affected > 0;
            }
            catch (Exception ex)
            {
                CustomLogger.Warn("[PointsApi.Sub] 扣除积分失败: userId={0}, points={1}, err={2}", userId, points, ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 直接设置玩家积分（玩家不存在时会自动创建记录）
        /// </summary>
        public static bool Set(string userId, int points)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return false;
            }

            try
            {
                var repo = ModApi.ServiceContainer.Resolve<IPointsInfoRepository>();
                var existing = repo.GetByIdAsync(userId).GetAwaiter().GetResult();
                if (existing == null)
                {
                    var entity = new Data.Entities.T_PointsInfo()
                    {
                        Id = userId,
                        CreatedAt = DateTime.Now,
                        LastSignInAt = null,
                        PlayerName = string.Empty,
                        Points = points
                    };
                    repo.InsertAsync(entity).GetAwaiter().GetResult();
                }
                else
                {
                    existing.Points = points;
                    repo.UpdateAsync(existing).GetAwaiter().GetResult();
                }
                Managers.PointLogger.Log(Models.PointLogCategory.External, userId, existing?.PlayerName,
                    points, points, "外部 Mod 调用 PointsApi.Set",
                    Models.PointChangeType.Set);
                return true;
            }
            catch (Exception ex)
            {
                CustomLogger.Warn("[PointsApi.Set] 设置积分失败: userId={0}, points={1}, err={2}", userId, points, ex.Message);
                return false;
            }
        }
    }
}
