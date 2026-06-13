using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// 区域命令禁用设置控制器
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/MuteCommandArea")]
    [OpenApiTag("Settings", Description = "配置")]
    public class MuteCommandAreaSettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public MuteCommandAreaSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<MuteCommandAreaSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] MuteCommandAreaSettings model)
        {
            ConfigManager.Update(model);
            return Ok();
        }

        /// <summary>
        /// 重置配置
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public MuteCommandAreaSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<MuteCommandAreaSettings>(Locales.Get(language));
            return data;
        }
    }
}
