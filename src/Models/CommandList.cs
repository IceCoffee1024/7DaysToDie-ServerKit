using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdtdServerKit.Models
{
    public class CommandList
    {
        public required string Command { get; set; }

        public bool InMainThread { get; set; }

        public string? Description { get; set; }

        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }
    }
}
