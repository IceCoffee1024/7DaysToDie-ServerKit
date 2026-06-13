using IceCoffee.SimpleCRUD.OptionalAttributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdtdServerKit.Data.Entities
{
    public class T_ItemList
    {
        [PrimaryKey, IgnoreInsert, IgnoreUpdate]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        /// <summary>
        /// 奖品类型：0=物品（默认）；2=积分。命令类型不存于此表，存于 T_CommandList。
        /// </summary>
        public int RewardType { get; set; }

        public required string ItemName { get; set; }

        [Column("[Count]")]
        public int Count { get; set; }

        public int Quality { get; set; }

        public int Durability { get; set; }

        public string? Description { get; set; }
    }
}
