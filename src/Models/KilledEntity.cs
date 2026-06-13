namespace SdtdServerKit.Models
{
    /// <summary>
    /// 被击杀者信息
    /// </summary>
    public class KilledEntity
    {
        /// <summary>
        /// 被击杀者实体信息
        /// </summary>
        public EntityInfo DeadEntity { get; set; } = null!;

        /// <summary>
        /// 击杀者实体Id
        /// </summary>
        public int KillerEntityId { get; set; }
    }
}