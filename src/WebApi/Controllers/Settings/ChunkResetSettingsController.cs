using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// 定时区域重置配置
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/ChunkReset")]
    [OpenApiTag("Settings", Description = "配置")]
    public class ChunkResetSettingsController : ApiController
    {
        /// <summary>
        /// 获取配置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public ChunkResetSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<ChunkResetSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// 更新配置
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] ChunkResetSettings model)
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
        public ChunkResetSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<ChunkResetSettings>(Locales.Get(language));
            return data;
        }
    }
}
