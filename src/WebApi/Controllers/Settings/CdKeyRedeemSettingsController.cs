using NSwag.Annotations;
using SdtdServerKit.FunctionSettings;
using SdtdServerKit.Managers;

namespace SdtdServerKit.WebApi.Controllers.Settings
{
    /// <summary>
    /// CdKey Redeem Settings Controller
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Settings/CdKeyRedeem")]
    [OpenApiTag("Settings", Description = "Configuration")]
    public class CdKeyRedeemSettingsController : ApiController
    {
        /// <summary>
        /// Get the settings
        /// </summary>
        /// <param name="language">The language to get the settings for</param>
        /// <returns>The CdKey redeem settings</returns>
        [HttpGet]
        [Route("")]
        public CdKeyRedeemSettings GetSettings([FromUri] Language language)
        {
            var data = ConfigManager.GetRequired<CdKeyRedeemSettings>(Locales.Get(language));
            return data;
        }

        /// <summary>
        /// Update the settings
        /// </summary>
        /// <param name="model">The settings model to update</param>
        /// <returns>HTTP action result</returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult UpdateSettings([FromBody] CdKeyRedeemSettings model)
        {
            ConfigManager.Update(model);
            return Ok();
        }

        /// <summary>
        /// Reset the settings to default
        /// </summary>
        /// <param name="language">The language to reset the settings for</param>
        /// <returns>The default CdKey redeem settings</returns>
        [HttpDelete]
        [Route("")]
        public CdKeyRedeemSettings ResetSettings([FromUri] Language language)
        {
            var data = ConfigManager.LoadDefault<CdKeyRedeemSettings>(Locales.Get(language));
            return data;
        }
    }
}