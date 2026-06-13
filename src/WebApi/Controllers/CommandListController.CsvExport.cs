using SdtdServerKit.Utilities;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SdtdServerKit.WebApi.Controllers
{
    public partial class CommandListController
    {
        /// <summary>
        /// 导出命令列表CSV
        /// </summary>
        [HttpGet]
        [Route("ExportCsv")]
        public async Task<IHttpActionResult> ExportCsv([FromUri] string? keyword = null)
        {
            try
            {
                // 获取所有数据
                var pagedResult = await _repository.GetPagedListAsync(new IceCoffee.SimpleCRUD.Dtos.PaginationQueryDto
                {
                    PageNumber = 1,
                    PageSize = -1,
                    Keyword = keyword
                });

                var items = pagedResult.Items;

                // 构建CSV数据
                var headers = new List<string> { "ID", "显示名称", "命令", "是否在主线程执行", "说明", "创建时间" };
                var rows = new List<List<string>>();

                foreach (var item in items)
                {
                    rows.Add(new List<string>
                    {
                        item.Id.ToString(),
                        item.DisplayName ?? "",
                        item.Command ?? "",
                        item.InMainThread ? "是" : "否",
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
                    FileName = $"command_list_{DateTime.Now:yyyyMMddHHmmss}.csv"
                };

                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                CustomLogger.Error(ex, "导出命令列表CSV失败");
                return InternalServerError(ex);
            }
        }
    }
}
