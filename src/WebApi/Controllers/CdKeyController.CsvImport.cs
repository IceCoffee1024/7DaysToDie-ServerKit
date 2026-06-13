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
    public partial class CdKeysController
    {
        /// <summary>
        /// 导入CDKey CSV
        /// </summary>
        [HttpPost]
        [Route("ImportCsv")]
        public async Task<IHttpActionResult> ImportCsv()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent()) return BadRequest("请求必须是multipart/form-data格式");

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
                        foreach (var err in parseResult.Errors)
                        {
                            result.Failures.Add(new CsvImportFailure { RowNumber = err.RowNumber, Errors = new List<string> { err.Message } });
                        }
                        return Ok(result);
                    }

                    var normalizedRows = NormalizeRows(parseResult.Rows);
                    var validEntities = new List<CdKey>();
                    var rowRewards = new List<List<RewardBindingCsvHelper.RewardEntry>>();
                    var failures = new List<CsvImportFailure>();
                    int rowNumber = 2;

                    foreach (var row in normalizedRows)
                    {
                        var errors = ValidateCdKeyRow(row);
                        if (errors.Count > 0)
                        {
                            failures.Add(new CsvImportFailure { RowNumber = rowNumber, RawData = row, Errors = errors });
                        }
                        else
                        {
                            try
                            {
                                validEntities.Add(ConvertToCdKeyEntity(row));
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
                        var cdKeyItemRepo = ModApi.ServiceContainer.Resolve<ICdKeyItemRepository>();
                        var cdKeyCmdRepo = ModApi.ServiceContainer.Resolve<ICdKeyCommandRepository>();
                        await cdKeyItemRepo.DeleteAllAsync(true);
                        await cdKeyCmdRepo.DeleteAllAsync(true);
                        await _cdKeyRepository.DeleteAllAsync(true);

                        var insertedIds = new List<int>();
                        foreach (var entity in validEntities)
                        {
                            int id = await _cdKeyRepository.InsertAsync<int>(entity);
                            insertedIds.Add(id);
                        }

                        var keyItems = new List<CdKeyItem>();
                        var keyCommands = new List<CdKeyCommand>();

                        for (int i = 0; i < insertedIds.Count; i++)
                        {
                            var keyId = insertedIds[i];
                            var written = await RewardBindingCsvHelper.WriteRewardsToListsAsync(rowRewards[i], _itemListRepository, _commandListRepository);
                            foreach (var w in written)
                            {
                                if (w.RewardType == 1)
                                {
                                    if (!keyCommands.Any(x => x.CdKeyId == keyId && x.CommandId == w.Id))
                                        keyCommands.Add(new CdKeyCommand { CdKeyId = keyId, CommandId = w.Id });
                                }
                                else
                                {
                                    if (!keyItems.Any(x => x.CdKeyId == keyId && x.ItemId == w.Id))
                                        keyItems.Add(new CdKeyItem { CdKeyId = keyId, ItemId = w.Id });
                                }
                            }
                        }

                        if (keyItems.Count > 0) await cdKeyItemRepo.InsertAsync(keyItems);
                        if (keyCommands.Count > 0) await cdKeyCmdRepo.InsertAsync(keyCommands);

                        importResult.Success = true;
                        CustomLogger.Info($"[CDKey导入] 成功 - 总数: {importResult.TotalCount}, 成功: {importResult.SuccessCount}");
                    }
                    catch (Exception ex)
                    {
                        importResult.Success = false;
                        importResult.ErrorMessage = $"数据导入失败: {ex.Message}";
                        CustomLogger.Error(ex, "CDKey CSV导入失败");
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
                CustomLogger.Error(ex, "CDKey CSV导入异常");
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// 下载CDKey CSV模板
        /// </summary>
        [HttpGet]
        [Route("CsvTemplate")]
        public IHttpActionResult DownloadCsvTemplate()
        {
            try
            {
                var headers = new List<string>
                {
                    "密钥", "兑换次数", "最大兑换次数", "过期时间", "描述", RewardBindingCsvHelper.ColumnName
                };
                var rows = new List<List<string>>
                {
                    new List<string>
                    {
                        "GIFT-EXAMPLE-2026",
                        "0",
                        "-1",
                        "2026-12-31 23:59:59",
                        "示例 CDKey 礼包（最大兑换次数 -1 表示无限制）",
                        "物品|玉米|plantedGraceCorn1|1; 命令|发公告|say {PlayerName} 兑换了礼包|0; 积分|奖励积分|points|100",
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
                    FileName = "cdkey_template.csv"
                };
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "下载CDKey CSV模板失败");
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
            if (row.TryGetValue(RewardBindingCsvHelper.ColumnName, out var bindings) && !string.IsNullOrWhiteSpace(bindings))
            {
                return RewardBindingCsvHelper.Parse(bindings);
            }
            row.TryGetValue(RewardBindingCsvHelper.LegacyItemColumn, out var itemsTxt);
            row.TryGetValue(RewardBindingCsvHelper.LegacyCmdColumn, out var cmdsTxt);
            return RewardBindingCsvHelper.ParseLegacy(itemsTxt, cmdsTxt);
        }

        private List<string> ValidateCdKeyRow(Dictionary<string, string> row)
        {
            var errors = new List<string>();

            if (!row.TryGetValue("密钥", out var key) || string.IsNullOrWhiteSpace(key))
            {
                errors.Add("缺少必填字段: 密钥");
            }
            else if (key.Length > 200)
            {
                errors.Add("密钥长度不能超过200个字符");
            }

            if (row.TryGetValue("兑换次数", out var redeem) && !string.IsNullOrWhiteSpace(redeem))
            {
                if (!int.TryParse(redeem, out var v) || v < 0)
                {
                    errors.Add("兑换次数必须是大于等于0的整数");
                }
            }

            // 最大兑换次数：允许任意整数，0 或负数视为无限制
            if (row.TryGetValue("最大兑换次数", out var maxRedeem) && !string.IsNullOrWhiteSpace(maxRedeem))
            {
                if (!int.TryParse(maxRedeem, out _))
                {
                    errors.Add("最大兑换次数必须是整数（0 或负数表示无限制）");
                }
            }

            if (row.TryGetValue("过期时间", out var expiry) && !string.IsNullOrWhiteSpace(expiry))
            {
                if (!DateTime.TryParse(expiry, out _))
                {
                    errors.Add("过期时间格式无效，期望格式：yyyy-MM-dd HH:mm:ss");
                }
            }

            return errors;
        }

        private CdKey ConvertToCdKeyEntity(Dictionary<string, string> row)
        {
            string GetCol(params string[] keys)
            {
                foreach (var k in keys)
                {
                    if (row.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v)) return v;
                }
                return string.Empty;
            }

            DateTime? expiry = null;
            var expiryStr = GetCol("过期时间");
            if (!string.IsNullOrWhiteSpace(expiryStr) && DateTime.TryParse(expiryStr, out var e))
            {
                expiry = e;
            }

            var redeemStr = GetCol("兑换次数");
            var maxRedeemStr = GetCol("最大兑换次数");
            var desc = GetCol("描述", "说明");

            return new CdKey
            {
                Key = GetCol("密钥"),
                RedeemCount = string.IsNullOrWhiteSpace(redeemStr) ? 0 : int.Parse(redeemStr),
                MaxRedeemCount = string.IsNullOrWhiteSpace(maxRedeemStr) ? 0 : int.Parse(maxRedeemStr),
                ExpiryAt = expiry,
                Description = string.IsNullOrEmpty(desc) ? null : desc,
                CreatedAt = DateTime.Now,
            };
        }
    }
}
