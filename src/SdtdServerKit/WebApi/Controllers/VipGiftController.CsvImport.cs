using IceCoffee.SimpleCRUD;
using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SdtdServerKit.WebApi.Controllers
{
    public partial class VipGiftController
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

                    var validEntities = new List<T_VipGift>();
                    var failures = new List<CsvImportFailure>();
                    int rowNumber = 2;

                    foreach (var row in parseResult.Rows)
                    {
                        var errors = ValidateVipGiftRow(row);
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
                                var entity = ConvertToVipGiftEntity(row);
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
                        var existingIds = validEntities.Select(e => e.Id).ToArray();
                        if (existingIds.Length > 0)
                        {
                            await _vipGiftRepository.DeleteByIdsAsync(existingIds, true);
                        }

                        await _vipGiftRepository.InsertAsync(validEntities);

                        await ImportBindingsAsync(parseResult.Rows, validEntities);

                        importResult.Success = true;
                        CustomLogger.Info($"CSV导入成功 - 模块: VIP礼包管理, 总数: {importResult.TotalCount}, 成功: {importResult.SuccessCount}");
                    }
                    catch (Exception ex)
                    {
                        importResult.Success = false;
                        importResult.ErrorMessage = $"数据导入失败: {ex.Message}";
                        CustomLogger.Error(ex, "VIP礼包CSV导入失败");
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
                CustomLogger.Error(ex, "VIP礼包CSV导入异常");
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
                var headers = new List<string> { "ID", "玩家名称", "名称", "领取状态", "总领取次数", "描述", "绑定物品", "绑定命令" };
                var exampleRow = new List<string> { "player123", "玩家昵称", "新手礼包", "false", "0", "这是一个示例礼包", "枪x1, 弹药x100", "give {PlayerId} food 5" };
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
                    FileName = "vipgift_template.csv"
                };

                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "下载VIP礼包CSV模板失败");
                return InternalServerError(ex);
            }
        }

        private List<string> ValidateVipGiftRow(Dictionary<string, string> row)
        {
            var errors = new List<string>();

            if (!row.ContainsKey("ID") || string.IsNullOrWhiteSpace(row["ID"]))
            {
                errors.Add("缺少必填字段: ID");
            }

            if (!row.ContainsKey("名称") || string.IsNullOrWhiteSpace(row["名称"]))
            {
                errors.Add("缺少必填字段: 名称");
            }
            else if (row["名称"].Length > 100)
            {
                errors.Add("名称长度不能超过100个字符");
            }

            if (row.ContainsKey("领取状态") && !string.IsNullOrWhiteSpace(row["领取状态"]))
            {
                if (!bool.TryParse(row["领取状态"], out _))
                {
                    errors.Add("领取状态必须是true或false");
                }
            }

            if (row.ContainsKey("总领取次数") && !string.IsNullOrWhiteSpace(row["总领取次数"]))
            {
                if (!int.TryParse(row["总领取次数"], out var count) || count < 0)
                {
                    errors.Add("总领取次数必须是大于等于0的整数");
                }
            }

            if (row.ContainsKey("描述") && !string.IsNullOrWhiteSpace(row["描述"]) && row["描述"].Length > 500)
            {
                errors.Add("描述长度不能超过500个字符");
            }

            return errors;
        }

        private T_VipGift ConvertToVipGiftEntity(Dictionary<string, string> row)
        {
            return new T_VipGift
            {
                Id = row["ID"],
                PlayerName = row.ContainsKey("玩家名称") && !string.IsNullOrWhiteSpace(row["玩家名称"]) ? row["玩家名称"] : null,
                Name = row["名称"],
                ClaimState = row.ContainsKey("领取状态") && !string.IsNullOrWhiteSpace(row["领取状态"]) ? bool.Parse(row["领取状态"]) : false,
                TotalClaimCount = row.ContainsKey("总领取次数") && !string.IsNullOrWhiteSpace(row["总领取次数"]) ? int.Parse(row["总领取次数"]) : 0,
                Description = row.ContainsKey("描述") && !string.IsNullOrWhiteSpace(row["描述"]) ? row["描述"] : null,
                CreatedAt = DateTime.Now
            };
        }

        /// <summary>
        /// 导入绑定物品和命令
        /// </summary>
        private async Task ImportBindingsAsync(List<Dictionary<string, string>> rows, List<T_VipGift> entities)
        {
            var allItems = (await _itemListRepository.GetAllAsync()).ToList();
            var allCommands = (await _commandListRepository.GetAllAsync()).ToList();

            var itemNameMap = allItems.ToDictionary(i => i.ItemName, i => i.Id, StringComparer.OrdinalIgnoreCase);
            var commandMap = allCommands.ToDictionary(c => c.Command, c => c.Id, StringComparer.OrdinalIgnoreCase);

            var vipGiftItems = new List<T_VipGiftItem>();
            var vipGiftCommands = new List<T_VipGiftCommand>();

            for (int i = 0; i < rows.Count && i < entities.Count; i++)
            {
                var row = rows[i];
                var giftId = entities[i].Id;

                if (row.ContainsKey("绑定物品") && !string.IsNullOrWhiteSpace(row["绑定物品"]))
                {
                    var itemEntries = row["绑定物品"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var entry in itemEntries)
                    {
                        var trimmed = entry.Trim();
                        var xIndex = trimmed.LastIndexOf('x');
                        var itemName = xIndex > 0 ? trimmed.Substring(0, xIndex) : trimmed;

                        if (itemNameMap.TryGetValue(itemName, out var itemId))
                        {
                            vipGiftItems.Add(new T_VipGiftItem { VipGiftId = giftId, ItemId = itemId });
                        }
                    }
                }

                if (row.ContainsKey("绑定命令") && !string.IsNullOrWhiteSpace(row["绑定命令"]))
                {
                    var cmdEntries = row["绑定命令"].Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var entry in cmdEntries)
                    {
                        var trimmed = entry.Trim();
                        if (commandMap.TryGetValue(trimmed, out var cmdId))
                        {
                            vipGiftCommands.Add(new T_VipGiftCommand { VipGiftId = giftId, CommandId = cmdId });
                        }
                    }
                }
            }

            if (vipGiftItems.Count > 0 || vipGiftCommands.Count > 0)
            {
                using var unitOfWork = ModApi.ServiceContainer.Resolve<IUnitOfWorkFactory>().Create();

                if (vipGiftItems.Count > 0)
                {
                    var itemRepo = unitOfWork.GetRepository<IVipGiftItemRepository>();
                    await itemRepo.InsertAsync(vipGiftItems);
                }

                if (vipGiftCommands.Count > 0)
                {
                    var cmdRepo = unitOfWork.GetRepository<IVipGiftCommandRepository>();
                    await cmdRepo.InsertAsync(vipGiftCommands);
                }

                unitOfWork.Commit();
            }
        }
    }
}
