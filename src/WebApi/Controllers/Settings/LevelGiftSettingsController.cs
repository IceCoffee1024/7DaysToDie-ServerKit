using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// 等级礼包配置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/LevelGift")]
    [OpenApiTag("Settings", Description = "配置")]
    public class LevelGiftSettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public LevelGiftSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<LevelGiftSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] LevelGiftSettings model)
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
        public LevelGiftSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<LevelGiftSettings>(Locales.Get(language));
            return data;
        }
    }
}
