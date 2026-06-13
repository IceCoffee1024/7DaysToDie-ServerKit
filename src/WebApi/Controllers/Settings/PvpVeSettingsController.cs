using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// PVP/PVE 混合区域配置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/PvpVe")]
    [OpenApiTag("Settings", Description = "配置")]
    public class PvpVeSettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        [HttpGet]
        [Route("")]
        public PvpVeSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<PvpVeSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] PvpVeSettings model)
        {
            ConfigManager.Update(model);
            return Ok();
        }

        /// <summary>
        /// 重置配置
        /// </summary>
        [HttpDelete]
        [Route("")]
        public PvpVeSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<PvpVeSettings>(Locales.Get(language));
            return data;
        }
    }
}
