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
    public partial class LotteryController
    {
        private static readonly List<string> RewardCsvHeaders = new()
        {
            "类型", "显示名称", "主体", "数量", "质量", "耐久度", "权重"
        };

        /// <summary>
        /// 导出抽奖池所有奖品（每行一个）
        /// </summary>
        [HttpGet]
        [Route("{id:int}/Rewards/ExportCsv")]
        public async Task<IHttpActionResult> ExportRewardsCsv(int id)
        {
            try
            {
                var lottery = await _lotteryRepository.GetByIdAsync(id);
                if (lottery == null) return NotFound();

                var items = await _lotteryItemRepository.GetItemsWithWeightByLotteryIdAsync(id);
                var commands = await _lotteryCommandRepository.GetCommandsWithWeightByLotteryIdAsync(id);

                var rows = new List<List<string>>();
                foreach (var i in items)
                {
                    string type = i.RewardType == 2 ? RewardBindingCsvHelper.TypePoints : RewardBindingCsvHelper.TypeItem;
                    string subject = i.RewardType == 2 ? "points" : i.ItemName;
                    rows.Add(new List<string>
                    {
                        type,
                        i.DisplayName ?? "",
                        subject,
                        i.Count.ToString(),
                        i.Quality.ToString(),
                        i.Durability.ToString(),
                        i.Weight.ToString(),
                    });
                }
                foreach (var c in commands)
                {
                    rows.Add(new List<string>
                    {
                        RewardBindingCsvHelper.TypeCommand,
                        c.DisplayName ?? "",
                        c.Command ?? "",
                        c.InMainThread ? "0" : "1",
                        "0",
                        "0",
                        c.Weight.ToString(),
                    });
                }

                var csvContent = CsvHelper.GenerateCsv(RewardCsvHeaders, rows);
                var bytes = WithBom(csvContent);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"lottery_{id}_rewards_{DateTime.Now:yyyyMMddHHmmss}.csv"
                };
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, $"[抽奖奖品导出] 失败 LotteryId={id}");
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// 导入抽奖池奖品 CSV（全量替换该抽奖池绑定的奖品）
        /// </summary>
        [HttpPost]
        [Route("{id:int}/Rewards/ImportCsv")]
        public async Task<IHttpActionResult> ImportRewardsCsv(int id)
        {
            try
            {
                if (id <= 0) return BadRequest("无效的抽奖池ID");
                var lottery = await _lotteryRepository.GetByIdAsync(id);
                if (lottery == null) return NotFound();

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

                    var rewards = new List<RewardBindingCsvHelper.RewardEntry>();
                    var failures = new List<CsvImportFailure>();
                    int rowNumber = 2;
                    foreach (var raw in parseResult.Rows)
                    {
                        var row = new Dictionary<string, string>();
                        foreach (var kvp in raw)
                        {
                            var key = kvp.Key.Trim('\uFEFF', '\u200B', '\uFFFE', ' ', '\t');
                            row[key] = kvp.Value;
                        }

                        var (entry, errs) = ParseRewardRow(row);
                        if (errs.Count > 0)
                        {
                            failures.Add(new CsvImportFailure { RowNumber = rowNumber, RawData = row, Errors = errs });
                        }
                        else
                        {
                            rewards.Add(entry);
                        }
                        rowNumber++;
                    }

                    var importResult = new CsvImportResult
                    {
                        TotalCount = parseResult.Rows.Count,
                        FailureCount = failures.Count,
                        SuccessCount = rewards.Count,
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
                        // 先清空抽奖池现有绑定
                        await _lotteryItemRepository.DeleteByLotteryIdAsync(id);
                        await _lotteryCommandRepository.DeleteByLotteryIdAsync(id);

                        // 写入物品/命令清单
                        var written = await RewardBindingCsvHelper.WriteRewardsToListsAsync(rewards, _itemListRepository, _commandListRepository);

                        var lotteryItems = new List<T_LotteryItem>();
                        var lotteryCommands = new List<T_LotteryCommand>();
                        foreach (var w in written)
                        {
                            if (w.RewardType == 1)
                            {
                                var existing = lotteryCommands.FirstOrDefault(x => x.CommandId == w.Id);
                                if (existing != null) existing.Weight = w.Weight;
                                else lotteryCommands.Add(new T_LotteryCommand { LotteryId = id, CommandId = w.Id, Weight = w.Weight });
                            }
                            else
                            {
                                var existing = lotteryItems.FirstOrDefault(x => x.ItemId == w.Id);
                                if (existing != null) existing.Weight = w.Weight;
                                else lotteryItems.Add(new T_LotteryItem { LotteryId = id, ItemId = w.Id, Weight = w.Weight });
                            }
                        }

                        if (lotteryItems.Count > 0) await _lotteryItemRepository.InsertAsync(lotteryItems);
                        if (lotteryCommands.Count > 0) await _lotteryCommandRepository.InsertAsync(lotteryCommands);

                        importResult.Success = true;
                        CustomLogger.Info($"[抽奖奖品导入] 成功 LotteryId={id}, 总数={rewards.Count}");
                    }
                    catch (Exception ex)
                    {
                        importResult.Success = false;
                        importResult.ErrorMessage = $"数据导入失败: {ex.Message}";
                        CustomLogger.Error(ex, $"[抽奖奖品导入] 失败 LotteryId={id}");
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
                CustomLogger.Error(ex, $"[抽奖奖品导入] 异常 LotteryId={id}");
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// 下载抽奖奖品 CSV 模板
        /// </summary>
        [HttpGet]
        [Route("Rewards/CsvTemplate")]
        public IHttpActionResult DownloadRewardsCsvTemplate()
        {
            try
            {
                var rows = new List<List<string>>
                {
                    new List<string> { "物品", "幸运手枪", "gunPistol", "1", "6", "100", "10" },
                    new List<string> { "命令", "送传送Buff", "buffplayer {PlayerId} cyberHack", "0", "0", "0", "5" },
                    new List<string> { "积分", "经验奖励", "points", "100", "0", "0", "8" },
                };

                var csvContent = CsvHelper.GenerateCsv(RewardCsvHeaders, rows);
                var bytes = WithBom(csvContent);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = "lottery_rewards_template.csv"
                };
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "[抽奖奖品模板下载] 失败");
                return InternalServerError(ex);
            }
        }

        // ----------------------------------------------------------------------
        // 工具
        // ----------------------------------------------------------------------

        private static (RewardBindingCsvHelper.RewardEntry entry, List<string> errors) ParseRewardRow(Dictionary<string, string> row)
        {
            var errors = new List<string>();

            string typeStr = row.TryGetValue("类型", out var t) ? (t ?? string.Empty).Trim() : string.Empty;
            int rewardType = typeStr switch
            {
                RewardBindingCsvHelper.TypeCommand => 1,
                RewardBindingCsvHelper.TypePoints => 2,
                RewardBindingCsvHelper.TypeItem => 0,
                "" => 0,
                _ => -1,
            };
            if (rewardType < 0)
            {
                errors.Add($"未知的类型：{typeStr}（应为 物品 / 命令 / 积分）");
            }

            string display = row.TryGetValue("显示名称", out var d) ? (d ?? string.Empty).Trim() : string.Empty;
            string subject = row.TryGetValue("主体", out var s) ? (s ?? string.Empty).Trim() : string.Empty;

            if (rewardType == 0 && string.IsNullOrWhiteSpace(subject))
            {
                errors.Add("物品类型必须填写主体（物品名称）");
            }
            if (rewardType == 1 && string.IsNullOrWhiteSpace(subject))
            {
                errors.Add("命令类型必须填写主体（命令内容）");
            }

            int count = 0;
            if (row.TryGetValue("数量", out var cnt) && !string.IsNullOrWhiteSpace(cnt))
            {
                if (!int.TryParse(cnt.Trim(), out count) || count < 0)
                {
                    errors.Add("数量必须是大于等于0的整数");
                }
            }
            if (rewardType == 1 && count != 0 && count != 1)
            {
                errors.Add("命令类型的数量必须是 0(主线程) 或 1(非主线程)");
            }

            int quality = 0;
            int durability = 0;
            if (row.TryGetValue("质量", out var q) && !string.IsNullOrWhiteSpace(q)) int.TryParse(q.Trim(), out quality);
            if (row.TryGetValue("耐久度", out var dur) && !string.IsNullOrWhiteSpace(dur)) int.TryParse(dur.Trim(), out durability);

            int weight = 1;
            if (row.TryGetValue("权重", out var w) && !string.IsNullOrWhiteSpace(w))
            {
                if (!int.TryParse(w.Trim(), out weight) || weight < 1)
                {
                    errors.Add("权重必须是大于等于1的整数");
                }
            }

            return (new RewardBindingCsvHelper.RewardEntry
            {
                RewardType = rewardType < 0 ? 0 : rewardType,
                DisplayName = display,
                Subject = subject,
                Count = rewardType == 1 ? (count == 1 ? 1 : 0) : (count <= 0 ? 1 : count),
                Quality = quality,
                Durability = durability,
                Weight = weight,
            }, errors);
        }

        private static byte[] WithBom(string content)
        {
            var preamble = Encoding.UTF8.GetPreamble();
            var bytes = Encoding.UTF8.GetBytes(content);
            var combined = new byte[preamble.Length + bytes.Length];
            preamble.CopyTo(combined, 0);
            bytes.CopyTo(combined, preamble.Length);
            return combined;
        }
    }
}
