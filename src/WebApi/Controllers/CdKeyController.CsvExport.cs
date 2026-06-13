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
        /// 导出CDKey列表CSV
        /// </summary>
        [HttpGet]
        [Route("ExportCsv")]
        public async Task<IHttpActionResult> ExportCsv()
        {
            try
            {
                var cdKeys = await _cdKeyRepository.GetAllAsync();

                var headers = new List<string>
                {
                    "密钥", "兑换次数", "最大兑换次数", "过期时间", "描述", RewardBindingCsvHelper.ColumnName
                };
                var rows = new List<List<string>>();

                foreach (var item in cdKeys)
                {
                    var items = await _itemListRepository.GetListByCdKeyIdAsync(item.Id);
                    var commands = await _commandListRepository.GetListByCdKeyIdAsync(item.Id);
                    var bindingText = RewardBindingCsvHelper.Serialize(
                        RewardBindingCsvHelper.BuildEntries(items, commands, null, null),
                        includeWeight: false);

                    rows.Add(new List<string>
                    {
                        item.Key,
                        item.RedeemCount.ToString(),
                        item.MaxRedeemCount.ToString(),
                        item.ExpiryAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "",
                        item.Description ?? "",
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
                    FileName = $"cdkey_{DateTime.Now:yyyyMMddHHmmss}.csv"
                };
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "导出CDKey列表CSV失败");
                return InternalServerError(ex);
            }
        }
    }
}
