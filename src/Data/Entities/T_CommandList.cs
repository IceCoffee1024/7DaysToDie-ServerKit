using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IceCoffee.SimpleCRUD.OptionalAttributes;

namespace SdtdServerKit.Data.Entities
{
    public class T_CommandList
    {
        [PrimaryKey, IgnoreInsert, IgnoreUpdate]
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }

        public required string Command { get; set; }

        public bool InMainThread { get; set; }

        public string? Description { get; set; }
    }

}
