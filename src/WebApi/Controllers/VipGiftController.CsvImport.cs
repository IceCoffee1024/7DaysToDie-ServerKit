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
                    var validEntities = new List<T_VipGift>();
                    var rowRewards = new List<List<RewardBindingCsvHelper.RewardEntry>>();
                    var failures = new List<CsvImportFailure>();
                    int rowNumber = 2;

                    foreach (var row in normalizedRows)
                    {
                        var errors = ValidateVipGiftRow(row);
                        if (errors.Count > 0)
                        {
                            failures.Add(new CsvImportFailure { RowNumber = rowNumber, RawData = row, Errors = errors });
                        }
                        else
                        {
                            try
                            {
                                validEntities.Add(ConvertToVipGiftEntity(row));
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
                        var vipGiftItemRepo = ModApi.ServiceContainer.Resolve<IVipGiftItemRepository>();
                        var vipGiftCmdRepo = ModApi.ServiceContainer.Resolve<IVipGiftCommandRepository>();
                        await vipGiftItemRepo.DeleteAllAsync(true);
                        await vipGiftCmdRepo.DeleteAllAsync(true);
                        await _vipGiftRepository.DeleteAllAsync(true);
                        await _vipGiftRepository.InsertAsync(validEntities);

                        var giftItems = new List<T_VipGiftItem>();
                        var giftCommands = new List<T_VipGiftCommand>();

                        for (int i = 0; i < validEntities.Count; i++)
                        {
                            var giftId = validEntities[i].Id;
                            var written = await RewardBindingCsvHelper.WriteRewardsToListsAsync(rowRewards[i], _itemListRepository, _commandListRepository);
                            foreach (var w in written)
                            {
                                if (w.RewardType == 1)
                                {
                                    if (!giftCommands.Any(x => x.VipGiftId == giftId && x.CommandId == w.Id))
                                        giftCommands.Add(new T_VipGiftCommand { VipGiftId = giftId, CommandId = w.Id });
                                }
                                else
                                {
                                    if (!giftItems.Any(x => x.VipGiftId == giftId && x.ItemId == w.Id))
                                        giftItems.Add(new T_VipGiftItem { VipGiftId = giftId, ItemId = w.Id });
                                }
                            }
                        }

                        if (giftItems.Count > 0) await vipGiftItemRepo.InsertAsync(giftItems);
                        if (giftCommands.Count > 0) await vipGiftCmdRepo.InsertAsync(giftCommands);

                        importResult.Success = true;
                        CustomLogger.Info($"[VIP礼包导入] 成功 - 总数: {importResult.TotalCount}, 成功: {importResult.SuccessCount}");
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
                    try { if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true); } catch { }
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
                var headers = new List<string>
                {
                    "玩家ID", "玩家名称", "礼包名称", "是否已领取", "总领取次数", "说明", RewardBindingCsvHelper.ColumnName
                };
                var exampleRow = new List<string>
                {
                    "EOS_0002b08158444a9eb3b28e1234567890",
                    "玩家昵称",
                    "新手礼包",
                    "false",
                    "0",
                    "示例 VIP 礼包",
                    "物品|玉米|plantedGraceCorn1|1; 命令|发公告|say 欢迎玩家 {PlayerName}|0; 积分|奖励|points|100",
                };

                var csvContent = CsvHelper.GenerateCsv(headers, new List<List<string>> { exampleRow });
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

        private List<string> ValidateVipGiftRow(Dictionary<string, string> row)
        {
            var errors = new List<string>();

            // 玩家ID
            string? playerId = null;
            if (row.TryGetValue("玩家ID", out var pid)) playerId = pid;
            else if (row.TryGetValue("ID", out var legacyId)) playerId = legacyId;
            if (string.IsNullOrWhiteSpace(playerId))
            {
                errors.Add("缺少必填字段: 玩家ID");
            }

            // 礼包名称
            string? giftName = null;
            if (row.TryGetValue("礼包名称", out var gn)) giftName = gn;
            else if (row.TryGetValue("名称", out var legacyName)) giftName = legacyName;
            if (string.IsNullOrWhiteSpace(giftName))
            {
                errors.Add("缺少必填字段: 礼包名称");
            }
            else if (giftName.Length > 100)
            {
                errors.Add("礼包名称长度不能超过100个字符");
            }

            // 是否已领取
            string? claim = null;
            if (row.TryGetValue("是否已领取", out var c1)) claim = c1;
            else if (row.TryGetValue("领取状态", out var c2)) claim = c2;
            if (!string.IsNullOrWhiteSpace(claim) && !bool.TryParse(claim, out _))
            {
                errors.Add("是否已领取必须是 true 或 false");
            }

            if (row.TryGetValue("总领取次数", out var totalStr) && !string.IsNullOrWhiteSpace(totalStr))
            {
                if (!int.TryParse(totalStr, out var total) || total < 0)
                {
                    errors.Add("总领取次数必须是大于等于0的整数");
                }
            }

            return errors;
        }

        private T_VipGift ConvertToVipGiftEntity(Dictionary<string, string> row)
        {
            string GetCol(params string[] keys)
            {
                foreach (var k in keys)
                {
                    if (row.TryGetValue(k, out var v) && !string.IsNullOrWhiteSpace(v)) return v;
                }
                return string.Empty;
            }

            var playerId = GetCol("玩家ID", "ID");
            var name = GetCol("礼包名称", "名称");
            var playerName = GetCol("玩家名称");
            var claimStr = GetCol("是否已领取", "领取状态");
            var totalStr = GetCol("总领取次数");
            var desc = GetCol("说明", "描述");

            return new T_VipGift
            {
                Id = playerId,
                PlayerName = string.IsNullOrEmpty(playerName) ? null : playerName,
                Name = name,
                ClaimState = !string.IsNullOrWhiteSpace(claimStr) && bool.Parse(claimStr),
                TotalClaimCount = string.IsNullOrWhiteSpace(totalStr) ? 0 : int.Parse(totalStr),
                Description = string.IsNullOrEmpty(desc) ? null : desc,
                CreatedAt = DateTime.Now,
            };
        }
    }
}
