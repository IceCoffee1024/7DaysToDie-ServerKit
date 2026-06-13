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
        /// 导出VIP礼包列表CSV
        /// </summary>
        [HttpGet]
        [Route("ExportCsv")]
        public async Task<IHttpActionResult> ExportCsv()
        {
            try
            {
                var gifts = await _vipGiftRepository.GetAllAsync();

                // 新表头：玩家ID, 玩家名称, 礼包名称, 是否已领取, 总领取次数, 说明, 绑定内容
                var headers = new List<string>
                {
                    "玩家ID", "玩家名称", "礼包名称", "是否已领取", "总领取次数", "说明", RewardBindingCsvHelper.ColumnName
                };
                var rows = new List<List<string>>();

                foreach (var gift in gifts)
                {
                    var items = await _itemListRepository.GetListByVipGiftIdAsync(gift.Id);
                    var commands = await _commandListRepository.GetListByVipGiftIdAsync(gift.Id);
                    var bindingText = RewardBindingCsvHelper.Serialize(
                        RewardBindingCsvHelper.BuildEntries(items, commands, null, null),
                        includeWeight: false);

                    rows.Add(new List<string>
                    {
                        gift.Id,
                        gift.PlayerName ?? "",
                        gift.Name,
                        gift.ClaimState ? "true" : "false",
                        gift.TotalClaimCount.ToString(),
                        gift.Description ?? "",
                        bindingText,
                    });
                }

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
                    FileName = $"vip_gift_{DateTime.Now:yyyyMMddHHmmss}.csv"
                };

                CustomLogger.Info($"[VIP礼包导出] 成功导出 {gifts.Count()} 条礼包数据");
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "导出VIP礼包列表CSV失败");
                return InternalServerError(ex);
            }
        }
    }
}
