using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// Colored Chat Settings Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/ColoredChat")]
    [OpenApiTag("Settings", Description = "配置")]
    public class ColoredChatSettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public ColoredChatSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<ColoredChatSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] ColoredChatSettings model)
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
        public ColoredChatSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<ColoredChatSettings>(Locales.Get(language));
            return data;
        }
    }
}