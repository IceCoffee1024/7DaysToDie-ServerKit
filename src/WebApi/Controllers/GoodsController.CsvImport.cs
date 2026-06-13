using SdtdServerKit.Data.Entities;
using SdtdServerKit.Data.IRepositories;
using SdtdServerKit.Utilities;
using SdtdServerKit.WebApi.Controllers.RewardBinding;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SdtdServerKit.WebApi.Controllers
{
    public partial class GoodsController
    {
        /// <summary>
        /// 导入商品CSV
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
                    if (provider.FileData.Count == 0) return BadRequest("未选择文件");

                    var fileData = provider.FileData[0];
                    var fileInfo = new FileInfo(fileData.LocalFileName);
                    if (fileInfo.Length > 10 * 1024 * 1024) return BadRequest("文件大小超过10MB");

                    var extension = Path.GetExtension(fileData.Headers.ContentDisposition?.FileName?.Trim('"') ?? "").ToLower();
                    if (extension != ".csv" && extension != ".txt") return BadRequest("文件格式不支持，仅支持.csv和.txt文件");

                    CsvParseResult parseResult;
                    using (var fileStream = File.OpenRead(fileData.LocalFileName))
                    {
                        parseResult = new CsvParser().Parse(fileStream);
                    }
                    if (!parseResult.Success) return BadRequest(parseResult.ErrorMessage ?? "CSV解析失败");

                    if (parseResult.Errors.Count > 0)
                    {
                        var result = new CsvImportResult
                        {
                            Success = false,
                            ErrorMessage = "CSV文件包含格式错误",
                            TotalCount = parseResult.Rows.Count + parseResult.Errors.Count,
                            FailureCount = parseResult.Errors.Count,
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

                    var normalizedRows = NormalizeRows(parseResult.Rows);

                    var validEntities = new List<T_Goods>();
                    var rowRewards = new List<List<RewardBindingCsvHelper.RewardEntry>>();
                    var failures = new List<CsvImportFailure>();
                    int rowNumber = 2;

                    foreach (var row in normalizedRows)
                    {
                        var errors = ValidateGoodsRow(row);
                        if (errors.Count > 0)
                        {
                            failures.Add(new CsvImportFailure { RowNumber = rowNumber, RawData = row, Errors = errors });
                        }
                        else
                        {
                            try
                            {
                                validEntities.Add(ConvertToGoodsEntity(row));
                                rowRewards.Add(ParseBindingsFromRow(row));
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
                        TotalCount = normalizedRows.Count,
                        FailureCount = failures.Count,
                        SuccessCount = validEntities.Count,
                        Failures = failures,
                    };

                    if (failures.Count > 0)
                    {
                        importResult.Success = false;
                        importResult.ErrorMessage = $"数据验证失败，共{failures.Count}条记录有错误";
                        return Ok(importResult);
                    }

                    try
                    {
                        var goodsItemRepo = ModApi.ServiceContainer.Resolve<IGoodsItemRepository>();
                        var goodsCmdRepo = ModApi.ServiceContainer.Resolve<IGoodsCommandRepository>();
                        await goodsItemRepo.DeleteAllAsync(true);
                        await goodsCmdRepo.DeleteAllAsync(true);
                        await _goodsRepository.DeleteAllAsync(true);

                        var usedIds = new HashSet<int>();
                        int nextId = 1;
                        foreach (var entity in validEntities)
                        {
                            while (usedIds.Contains(nextId)) nextId++;
                            entity.Id = nextId;
                            usedIds.Add(nextId);
                            nextId++;
                        }
                        await _goodsRepository.InsertAsync(validEntities);

                        // 处理绑定内容
                        var goodsItems = new List<T_GoodsItem>();
                        var goodsCommands = new List<T_GoodsCommand>();

                        for (int i = 0; i < validEntities.Count; i++)
                        {
                            var goodsId = validEntities[i].Id;
                            var written = await RewardBindingCsvHelper.WriteRewardsToListsAsync(rowRewards[i], _itemListRepository, _commandListRepository);

                            foreach (var w in written)
                            {
                                if (w.RewardType == 1)
                                {
                                    if (!goodsCommands.Any(x => x.GoodsId == goodsId && x.CommandId == w.Id))
                                    {
                                        goodsCommands.Add(new T_GoodsCommand { GoodsId = goodsId, CommandId = w.Id });
                                    }
                                }
                                else
                                {
                                    if (!goodsItems.Any(x => x.GoodsId == goodsId && x.ItemId == w.Id))
                                    {
                                        goodsItems.Add(new T_GoodsItem { GoodsId = goodsId, ItemId = w.Id });
                                    }
                                }
                            }
                        }

                        if (goodsItems.Count > 0) await goodsItemRepo.InsertAsync(goodsItems);
                        if (goodsCommands.Count > 0) await goodsCmdRepo.InsertAsync(goodsCommands);

                        importResult.Success = true;
                        CustomLogger.Info($"[商品导入] CSV导入成功 - 总数: {importResult.TotalCount}, 成功: {importResult.SuccessCount}");
                    }
                    catch (Exception ex)
                    {
                        importResult.Success = false;
                        importResult.ErrorMessage = $"数据导入失败: {ex.Message}";
                        CustomLogger.Error(ex, "商品CSV导入失败");
                    }
                    return Ok(importResult);
                }
                finally
                {
                    try { if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true); } catch { }
                }
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "商品CSV导入异常");
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// 下载商品CSV模板
        /// </summary>
        [HttpGet]
        [Route("CsvTemplate")]
        public IHttpActionResult DownloadCsvTemplate()
        {
            try
            {
                var headers = new List<string> { "商品名称", "售价", "说明", RewardBindingCsvHelper.ColumnName };
                var rows = new List<List<string>>
                {
                    new List<string>
                    {
                        "示例商品",
                        "100",
                        "这是一个示例商品",
                        "物品|手枪|gunPistol|1; 物品|9mm子弹|ammo9mmBullet|100; 命令|发个公告|say {PlayerName} 购买了商品|0; 积分|奖励积分|points|50",
                    }
                };

                var csvContent = CsvHelper.GenerateCsv(headers, rows);
                var preamble = Encoding.UTF8.GetPreamble();
                var contentBytes = Encoding.UTF8.GetBytes(csvContent);
                var bytes = new byte[preamble.Length + contentBytes.Length];
                preamble.CopyTo(bytes, 0);
                contentBytes.CopyTo(bytes, preamble.Length);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "goods_template.csv"
                };
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "下载商品CSV模板失败");
                return InternalServerError(ex);
            }
        }

        private static List<Dictionary<string, string>> NormalizeRows(List<Dictionary<string, string>> raw)
        {
            var normalized = new List<Dictionary<string, string>>();
            foreach (var row in raw)
            {
                var dict = new Dictionary<string, string>();
                foreach (var kvp in row)
                {
                    var key = kvp.Key.Trim('\uFEFF', '\u200B', '\uFFFE', ' ', '\t');
                    dict[key] = kvp.Value;
                }
                normalized.Add(dict);
            }
            return normalized;
        }

        private static List<RewardBindingCsvHelper.RewardEntry> ParseBindingsFromRow(Dictionary<string, string> row)
        {
            // 优先用新表头
            if (row.TryGetValue(RewardBindingCsvHelper.ColumnName, out var bindings) && !string.IsNullOrWhiteSpace(bindings))
            {
                return RewardBindingCsvHelper.Parse(bindings);
            }
            // 兼容旧表头
            row.TryGetValue(RewardBindingCsvHelper.LegacyItemColumn, out var itemsTxt);
            row.TryGetValue(RewardBindingCsvHelper.LegacyCmdColumn, out var cmdsTxt);
            return RewardBindingCsvHelper.ParseLegacy(itemsTxt, cmdsTxt);
        }

        private List<string> ValidateGoodsRow(Dictionary<string, string> row)
        {
            var errors = new List<string>();
            if (!row.ContainsKey("商品名称") || string.IsNullOrWhiteSpace(row["商品名称"]))
            {
                errors.Add("缺少必填字段: 商品名称");
            }
            if (!row.ContainsKey("售价") || string.IsNullOrWhiteSpace(row["售价"]))
            {
                errors.Add("缺少必填字段: 售价");
            }
            else if (!int.TryParse(row["售价"], out var price) || price < 0)
            {
                errors.Add("售价必须是大于等于0的整数");
            }
            return errors;
        }

        private T_Goods ConvertToGoodsEntity(Dictionary<string, string> row)
        {
            return new T_Goods
            {
                Name = row["商品名称"],
                Price = int.Parse(row["售价"]),
                Description = row.ContainsKey("说明") && !string.IsNullOrWhiteSpace(row["说明"]) ? row["说明"] : null,
                CreatedAt = DateTime.Now
            };
        }
    }
}
