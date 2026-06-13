using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// 自定义商人保护区域配置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/TraderProtect")]
    [OpenApiTag("Settings", Description = "配置")]
    public class TraderProtectSettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        [HttpGet]
        [Route("")]
        public TraderProtectSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<TraderProtectSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] TraderProtectSettings model)
        {
            ConfigManager.Update(model);
            return Ok();
        }

        /// <summary>
        /// 重置配置
        /// </summary>
        [HttpDelete]
        [Route("")]
        public TraderProtectSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<TraderProtectSettings>(Locales.Get(language));
            return data;
        }
    }
}
