using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// 自动备份配置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/AutoBackup")]
    [OpenApiTag("Settings", Description = "配置")]
    public class AutoBackupSettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public AutoBackupSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<AutoBackupSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] AutoBackupSettings model)
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
        public AutoBackupSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<AutoBackupSettings>(Locales.Get(language));
            return data;
        }
    }
}