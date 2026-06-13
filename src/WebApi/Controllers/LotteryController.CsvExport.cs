using SdtdServerKit.Utilities;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SdtdServerKit.WebApi.Controllers
{
    public partial class LotteryController
    {
        /// <summary>
        /// 导出抽奖列表CSV
        /// </summary>
        [HttpGet]
        [Route("ExportCsv")]
        public async Task<IHttpActionResult> ExportCsv()
        {
            try
            {
                // 获取所有抽奖配置
                var items = await _lotteryRepository.GetAllOrderByIdAsync();

                // 构建CSV数据
                var headers = new List<string> { "ID", "名称", "是否启用", "抽奖命令", "抽奖间隔(秒)", "抽奖消耗", "说明", "创建时间" };
                var rows = new List<List<string>>();

                foreach (var item in items)
                {
                    rows.Add(new List<string>
                    {
                        item.Id.ToString(),
                        item.Name ?? "",
                        item.IsEnabled ? "是" : "否",
                        item.DrawCommand ?? "",
                        item.DrawInterval.ToString(),
                        item.DrawCost.ToString(),
                        item.Description ?? "",
                        item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")
                    });
                }

                var csvContent = CsvHelper.GenerateCsv(headers, rows);
                var bytes = Encoding.UTF8.GetBytes(csvContent);

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(bytes)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("text/csv");
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = $"lottery_{DateTime.Now:yyyyMMddHHmmss}.csv"
                };

                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "导出抽奖列表CSV失败");
                return InternalServerError(ex);
            }
        }
    }
}
