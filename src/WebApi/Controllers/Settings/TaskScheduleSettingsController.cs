using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// Task Schedule Settings
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/TaskSchedule")]
    [OpenApiTag("Settings")]
    public class TaskScheduleSettingsController : ApiController
    {
        /// <summary>
        /// Get Settings
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public TaskScheduleSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<TaskScheduleSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// Update Settings
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] TaskScheduleSettings model)
        {
            ConfigManager.Update(model);
            return Ok();
        }

        /// <summary>
        /// Reset Settings
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public TaskScheduleSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<TaskScheduleSettings>(Locales.Get(language));
            return data;
        }
    }
}