using System.Text;

namespace SdtdServerKit.Utilities
{
    /// <summary>
    /// CSV辅助工具类
    /// </summary>
    public static class CsvHelper
    {
        /// <summary>
        /// 转义CSV值
        /// </summary>
        public static string EscapeCsvValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }

        /// <summary>
        /// 生成CSV内容
        /// </summary>
        public static string GenerateCsv(List<string> headers, List<List<string>> rows)
        {
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(",", headers.Select(EscapeCsvValue)));

            foreach (var row in rows)
            {
                sb.AppendLine(string.Join(",", row.Select(EscapeCsvValue)));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 生成失败记录CSV
        /// </summary>
        public static string GenerateFailureCsv(List<CsvImportFailure> failures)
        {
            var sb = new StringBuilder();

            sb.AppendLine("行号,错误信息,原始数据");

            foreach (var failure in failures)
            {
                var errors = string.Join("; ", failure.Errors);
                var rawData = failure.RawData != null
                    ? string.Join(", ", failure.RawData.Select(kv => $"{kv.Key}={kv.Value}"))
                    : "";

                sb.AppendLine($"{failure.RowNumber},{EscapeCsvValue(errors)},{EscapeCsvValue(rawData)}");
            }

            return sb.ToString();
        }
    }
}
