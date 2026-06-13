using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdtdServerKit.Models
{
    /// <summary>
    /// 文件结果
    /// </summary>
    public class FileResult
    {
        /// <summary>
        /// 创建日期
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// 文件名称
        /// </summary>
        public required string Name { get; set; }

        /// <summary>
        /// 大小
        /// </summary>
        public long Size { get; set; }
    }
}
