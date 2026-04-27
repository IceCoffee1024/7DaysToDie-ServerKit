namespace SdtdServerKit.Models
{
    /// <summary>
    /// 实体信息
    /// </summary>
    public class EntityInfo
    {
        /// <summary>
        /// 实体Id
        /// </summary>
        public int EntityId { get; set; }

        /// <summary>
        /// 实体名称, 如果为空则返回或实体类名
        /// </summary>
        public string EntityName { get; set; }

        /// <summary>
        /// 实体类名
        /// </summary>
        public string EntityClassName { get; set; }

        /// <summary>
        /// 实体类型
        /// </summary>
        public EntityType EntityType { get; set; }

        /// <summary>
        /// 坐标
        /// </summary>
        public Position Position { get; set; }
    }

    /// <summary>
    /// 实体信息
    /// </summary>
    public class EntityInfoEx : EntityInfo
    {
        /// <summary>
        /// 玩家Id
        /// </summary>
        public string PlayerId { get; set; }
    }
}
