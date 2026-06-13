using System.Text;

namespace SdtdServerKit.Utilities
{
    /// <summary>
    /// CSV解析结果
    /// </summary>
    public class CsvParseResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<string> Headers { get; set; } = new();
        public List<Dictionary<string, string>> Rows { get; set; } = new();
        public List<CsvParseError> Errors { get; set; } = new();
    }

    /// <summary>
    /// CSV解析错误
    /// </summary>
    public class CsvParseError
    {
        public int RowNumber { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    /// <summary>
    /// CSV解析器
    /// </summary>
    public class CsvParser
    {
        /// <summary>
        /// 解析CSV文件
        /// </summary>
        public CsvParseResult Parse(Stream stream)
        {
            var result = new CsvParseResult();

            try
            {
                var encoding = DetectEncoding(stream);
                stream.Position = 0;

                var reader = new StreamReader(stream, encoding, true, 1024, leaveOpen: true);

                var headerLine = reader.ReadLine();
                if (string.IsNullOrWhiteSpace(headerLine))
                {
                    result.Success = false;
                    result.ErrorMessage = "文件内容为空";
                    return result;
                }

                result.Headers = ParseCsvLine(headerLine);

                int rowNumber = 2;
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        rowNumber++;
                        continue;
                    }

                    try
                    {
                        var values = ParseCsvLine(line);
                        if (values.Count != result.Headers.Count)
                        {
                            result.Errors.Add(new CsvParseError
                            {
                                RowNumber = rowNumber,
                                Message = $"列数不匹配，期望{result.Headers.Count}列，实际{values.Count}列"
                            });
                        }
                        else
                        {
                            var row = new Dictionary<string, string>();
                            for (int i = 0; i < result.Headers.Count; i++)
                            {
                                row[result.Headers[i]] = values[i];
                            }
                            result.Rows.Add(row);
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new CsvParseError
                        {
                            RowNumber = rowNumber,
                            Message = $"解析错误: {ex.Message}"
                        });
                    }

                    rowNumber++;
                }

                reader.Dispose();
                result.Success = result.Errors.Count == 0;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"文件解析失败: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 检测文件编码
        /// </summary>
        private Encoding DetectEncoding(Stream stream)
        {
            var buffer = new byte[4];
            stream.Read(buffer, 0, 4);
            stream.Position = 0;

            if (buffer[0] == 0xEF && buffer[1] == 0xBB && buffer[2] == 0xBF)
            {
                return new UTF8Encoding(true);
            }

            try
            {
                var reader = new StreamReader(stream, Encoding.UTF8, true, 1024, leaveOpen: true);
                reader.ReadToEnd();
                reader.Dispose();
                stream.Position = 0;
                return Encoding.UTF8;
            }
            catch
            {
                stream.Position = 0;
                return Encoding.GetEncoding("GBK");
            }
        }

        /// <summary>
        /// 解析CSV行
        /// </summary>
        private List<string> ParseCsvLine(string line)
        {
            var values = new List<string>();
            var currentValue = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];

                if (c == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        currentValue.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (c == ',' && !inQuotes)
                {
                    values.Add(currentValue.ToString().Trim());
                    currentValue.Clear();
                }
                else
                {
                    currentValue.Append(c);
                }
            }

            values.Add(currentValue.ToString().Trim());
            return values;
        }
    }
}
