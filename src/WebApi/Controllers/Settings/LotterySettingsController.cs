using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// 抽奖配置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/Lottery")]
    [OpenApiTag("Settings", Description = "配置")]
    public class LotterySettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public LotterySettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<LotterySettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] LotterySettings model)
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
        public LotterySettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<LotterySettings>(Locales.Get(language));
            return data;
        }
    }
}
