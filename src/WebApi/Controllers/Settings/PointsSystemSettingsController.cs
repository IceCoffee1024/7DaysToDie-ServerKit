using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// 积分系统配置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/PointsSystem")]
    [OpenApiTag("Settings", Description = "配置")]
    public class PointsSystemSettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public PointsSystemSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<PointsSystemSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] PointsSystemSettings model)
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
        public PointsSystemSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<PointsSystemSettings>(Locales.Get(language));
            return data;
        }
    }
}