using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SdtdServerKit.WebApi.Controllers
{
    public partial class PointsInfoController
    {
        /// <summary>
        /// 导入CSV文件
        /// </summary>
        [HttpPost]
        [Route("ImportCsv")]
        public async Task<IHttpActionResult> ImportCsv()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                {
                    return BadRequest("请求必须是multipart/form-data格式");
                }

                var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(tempPath);

                try
                {
                    var provider = new MultipartFormDataStreamProvider(tempPath);
                    await Request.Content.ReadAsMultipartAsync(provider);

                    if (provider.FileData.Count == 0)
                    {
                        return BadRequest("未选择文件");
                    }

                    var fileData = provider.FileData[0];
                    var fileInfo = new FileInfo(fileData.LocalFileName);

                    if (fileInfo.Length > 10 * 1024 * 1024)
                    {
                        return BadRequest("文件大小超过10MB");
                    }

                    var originalFileName = fileData.Headers.ContentDisposition?.FileName?.Trim('"') ?? "";
                    var extension = Path.GetExtension(originalFileName).ToLower();
                    if (extension != ".csv" && extension != ".txt")
                    {
                        return BadRequest("文件格式不支持，仅支持.csv和.txt文件");
                    }

                    var parser = new CsvParser();
                    CsvParseResult parseResult;
                    using (var fileStream = File.OpenRead(fileData.LocalFileName))
                    {
                        parseResult = parser.Parse(fileStream);
                    }

                    if (!parseResult.Success)
                    {
                        return BadRequest(parseResult.ErrorMessage ?? "CSV解析失败");
                    }

                    if (parseResult.Errors.Count > 0)
                    {
                        var result = new CsvImportResult
                        {
                            Success = false,
                            ErrorMessage = "CSV文件包含格式错误",
                            TotalCount = parseResult.Rows.Count + parseResult.Errors.Count,
                            FailureCount = parseResult.Errors.Count
                        };

                        foreach (var error in parseResult.Errors)
                        {
                            result.Failures.Add(new CsvImportFailure
                            {
                                RowNumber = error.RowNumber,
                                Errors = new List<string> { error.Message }
                            });
                        }

                        return Ok(result);
                    }

                    var validEntities = new List<T_PointsInfo>();
                    var failures = new List<CsvImportFailure>();
                    int rowNumber = 2;

                    foreach (var row in parseResult.Rows)
                    {
                        var errors = ValidatePointsInfoRow(row);
                        if (errors.Count > 0)
                        {
                            failures.Add(new CsvImportFailure
                            {
                                RowNumber = rowNumber,
                                RawData = row,
                                Errors = errors
                            });
                        }
                        else
                        {
                            try
                            {
                                var entity = ConvertToPointsInfoEntity(row);
                                validEntities.Add(entity);
                            }
                            catch (Exception ex)
                            {
                                failures.Add(new CsvImportFailure
                                {
                                    RowNumber = rowNumber,
                                    RawData = row,
                                    Errors = new List<string> { $"数据转换失败: {ex.Message}" }
                                });
                            }
                        }
                        rowNumber++;
                    }

                    var importResult = new CsvImportResult
                    {
                        TotalCount = parseResult.Rows.Count,
                        FailureCount = failures.Count,
                        SuccessCount = validEntities.Count,
                        Failures = failures
                    };

                    if (failures.Count > 0)
                    {
                        importResult.Success = false;
                        importResult.ErrorMessage = $"数据验证失败，共{failures.Count}条记录有错误";
                        return Ok(importResult);
                    }

                    try
                    {
                        await _repository.InsertAsync(validEntities);

                        importResult.Success = true;
                        CustomLogger.Info($"CSV导入成功 - 模块: 积分管理, 总数: {importResult.TotalCount}, 成功: {importResult.SuccessCount}");
                    }
                    catch (Exception ex)
                    {
                        importResult.Success = false;
                        importResult.ErrorMessage = $"数据导入失败: {ex.Message}";
                        CustomLogger.Error(ex, "积分CSV导入失败");
                    }

                    return Ok(importResult);
                }
                finally
                {
                    try
                    {
                        if (Directory.Exists(tempPath))
                        {
                            Directory.Delete(tempPath, true);
                        }
                    }
                    catch
                    {
                    }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "积分CSV导入异常");
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// 下载CSV模板
        /// </summary>
        [HttpGet]
        [Route("CsvTemplate")]
        public IHttpActionResult DownloadCsvTemplate()
        {
            try
            {
                var headers = new List<string> { "玩家ID", "玩家名称", "积分", "上次签到日期" };
                var exampleRow = new List<string> { "player123", "示例玩家", "1000", "2024-01-01" };
                var rows = new List<List<string>> { exampleRow };

                var csvContent = CsvHelper.GenerateCsv(headers, rows);
                var bytes = Encoding.UTF8.GetBytes(csvContent);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "points_template.csv"
                };

                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "下载积分CSV模板失败");
                return InternalServerError(ex);
            }
        }

        private List<string> ValidatePointsInfoRow(Dictionary<string, string> row)
        {
            var errors = new List<string>();

            if (!row.ContainsKey("玩家ID") || string.IsNullOrWhiteSpace(row["玩家ID"]))
            {
                errors.Add("缺少必填字段: 玩家ID");
            }

            if (row.ContainsKey("积分") && !string.IsNullOrWhiteSpace(row["积分"]))
            {
                if (!int.TryParse(row["积分"], out var points) || points < 0)
                {
                    errors.Add("积分必须是大于等于0的整数");
                }
            }

            if (row.ContainsKey("上次签到日期") && !string.IsNullOrWhiteSpace(row["上次签到日期"]))
            {
                if (!DateTime.TryParse(row["上次签到日期"], out _))
                {
                    errors.Add("上次签到日期格式不正确");
                }
            }

            return errors;
        }

        private T_PointsInfo ConvertToPointsInfoEntity(Dictionary<string, string> row)
        {
            return new T_PointsInfo
            {
                Id = row["玩家ID"],
                PlayerName = row.ContainsKey("玩家名称") && !string.IsNullOrWhiteSpace(row["玩家名称"]) ? row["玩家名称"] : null,
                Points = row.ContainsKey("积分") && !string.IsNullOrWhiteSpace(row["积分"]) ? int.Parse(row["积分"]) : 0,
                LastSignInAt = row.ContainsKey("上次签到日期") && !string.IsNullOrWhiteSpace(row["上次签到日期"]) ? DateTime.Parse(row["上次签到日期"]) : null,
                CreatedAt = DateTime.Now
            };
        }
    }
}
