using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// 积分日志配置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/PointLog")]
    [OpenApiTag("Settings", Description = "配置")]
    public class PointLogSettingsController : ApiController
    {
        /// <summary>获取配置</summary>
        [HttpGet]
        [Route("")]
        public PointLogSettings GetSettings([FromUri] Language language)
        {
            return ConfigManager.GetRequired<PointLogSettings>(Locales.Get(language));
        }

        /// <summary>更新配置</summary>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] PointLogSettings model)
        {
            ConfigManager.Update(model);
            return Ok();
        }

        /// <summary>重置配置</summary>
        [HttpDelete]
        [Route("")]
        public PointLogSettings ResetSettings([FromUri] Language language)
        {
            return ConfigManager.LoadDefault<PointLogSettings>(Locales.Get(language));
        }
    }
}
