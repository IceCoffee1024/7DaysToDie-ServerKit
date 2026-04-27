namespace SdtdServerKit.Models
{
    /// <summary>
    /// CSV导入结果
    /// </summary>
    public class CsvImportResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// 成功导入数量
        /// </summary>
        public int SuccessCount { get; set; }

        /// <summary>
        /// 失败数量
        /// </summary>
        public int FailureCount { get; set; }

        /// <summary>
        /// 失败记录详情
        /// </summary>
        public List<CsvImportFailure> Failures { get; set; } = new();

        /// <summary>
        /// 错误消息
        /// </summary>
        public string? ErrorMessage { get; set; }
    }

    /// <summary>
    /// CSV导入失败记录
    /// </summary>
    public class CsvImportFailure
    {
        /// <summary>
        /// 行号
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// 原始数据
        /// </summary>
        public Dictionary<string, string>? RawData { get; set; }

        /// <summary>
        /// 错误列表
        /// </summary>
        public List<string> Errors { get; set; } = new();
    }
}
