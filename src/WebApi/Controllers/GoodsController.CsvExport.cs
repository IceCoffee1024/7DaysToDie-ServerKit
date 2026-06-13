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
        /// 导出商品列表CSV
        /// </summary>
        [HttpGet]
        [Route("ExportCsv")]
        public async Task<IHttpActionResult> ExportCsv()
        {
            try
            {
                var goods = await _goodsRepository.GetAllAsync();

                // 新表头：商品名称, 售价, 说明, 绑定内容
                var headers = new List<string> { "商品名称", "售价", "说明", RewardBindingCsvHelper.ColumnName };
                var rows = new List<List<string>>();

                foreach (var item in goods)
                {
                    var items = await _itemListRepository.GetListByGoodsIdAsync(item.Id);
                    var commands = await _commandListRepository.GetListByGoodsIdAsync(item.Id);
                    var entries = RewardBindingCsvHelper.BuildEntries(items, commands, null, null);
                    var bindingText = RewardBindingCsvHelper.Serialize(entries, includeWeight: false);

                    rows.Add(new List<string>
                    {
                        item.Name ?? "",
                        item.Price.ToString(),
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
                    FileName = $"goods_{DateTime.Now:yyyyMMddHHmmss}.csv"
                };
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "导出商品列表CSV失败");
                return InternalServerError(ex);
            }
        }
    }
}
