using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdtdServerKit.Models
{
    public class AvailablePrefabQuery
    {
        /// <summary>
        /// 页码
        /// </summary>
        [DefaultValue(1)]
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// 每页数量, 值小于 0 时返回所有记录
        /// </summary>
        [DefaultValue(10)]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// 语言
        /// </summary>
        [DefaultValue(Language.English)]
        public Language Language { get; set; } = Language.English;

        /// <summary>
        /// 搜索关键词
        /// </summary>
        public string? Keyword { get; set; }
    }
}
