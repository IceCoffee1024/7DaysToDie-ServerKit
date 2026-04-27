using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SdtdServerKit.WebApi.Controllers
{
    public partial class CommandListController
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

                    var validEntities = new List<T_CommandList>();
                    var failures = new List<CsvImportFailure>();
                    int rowNumber = 2;

                    foreach (var row in parseResult.Rows)
                    {
                        var errors = ValidateCommandListRow(row);
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
                                var entity = ConvertToCommandListEntity(row);
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
                        CustomLogger.Info($"CSV导入成功 - 模块: 命令清单, 总数: {importResult.TotalCount}, 成功: {importResult.SuccessCount}");
                    }
                    catch (Exception ex)
                    {
                        importResult.Success = false;
                        importResult.ErrorMessage = $"数据导入失败: {ex.Message}";
                        CustomLogger.Error(ex, "命令清单CSV导入失败");
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
                CustomLogger.Error(ex, "命令清单CSV导入异常");
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
                var headers = new List<string> { "命令", "主线程执行", "描述" };
                var exampleRow = new List<string> { "say \"Hello World\"", "false", "这是一个示例命令" };
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
                    FileName = "commandlist_template.csv"
                };

                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "下载命令清单CSV模板失败");
                return InternalServerError(ex);
            }
        }

        private List<string> ValidateCommandListRow(Dictionary<string, string> row)
        {
            var errors = new List<string>();

            if (!row.ContainsKey("命令") || string.IsNullOrWhiteSpace(row["命令"]))
            {
                errors.Add("缺少必填字段: 命令");
            }
            else if (row["命令"].Length > 500)
            {
                errors.Add("命令长度不能超过500个字符");
            }

            if (row.ContainsKey("主线程执行") && !string.IsNullOrWhiteSpace(row["主线程执行"]))
            {
                if (!bool.TryParse(row["主线程执行"], out _))
                {
                    errors.Add("主线程执行必须是true或false");
                }
            }

            if (row.ContainsKey("描述") && !string.IsNullOrWhiteSpace(row["描述"]) && row["描述"].Length > 500)
            {
                errors.Add("描述长度不能超过500个字符");
            }

            return errors;
        }

        private T_CommandList ConvertToCommandListEntity(Dictionary<string, string> row)
        {
            return new T_CommandList
            {
                Command = row["命令"],
                InMainThread = row.ContainsKey("主线程执行") && !string.IsNullOrWhiteSpace(row["主线程执行"]) ? bool.Parse(row["主线程执行"]) : false,
                Description = row.ContainsKey("描述") && !string.IsNullOrWhiteSpace(row["描述"]) ? row["描述"] : null,
                CreatedAt = DateTime.Now
            };
        }
    }
}
