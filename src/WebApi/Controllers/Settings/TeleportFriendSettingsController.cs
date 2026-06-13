using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// 好友传送配置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/TeleportFriend")]
    [OpenApiTag("Settings", Description = "配置")]
    public class TeleportFriendSettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public TeleportFriendSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<TeleportFriendSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] TeleportFriendSettings model)
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
        public TeleportFriendSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<TeleportFriendSettings>(Locales.Get(language));
            return data;
        }
    }
}