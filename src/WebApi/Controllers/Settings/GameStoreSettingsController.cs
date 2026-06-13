using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// 游戏商店配置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/GameStore")]
    [OpenApiTag("Settings", Description = "配置")]
    public class GameStoreSettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public GameStoreSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<GameStoreSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] GameStoreSettings model)
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
        public GameStoreSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<GameStoreSettings>(Locales.Get(language));
            return data;
        }
    }
}