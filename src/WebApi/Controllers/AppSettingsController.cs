using System.Text;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// App Settings
    /// </summary>
    [Authorize]
    [RoutePrefix("api/AppSettings")]
    public class AppSettingsController : ApiController
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public AppSettings Get()
        {
            return ModApi.AppSettings;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("")]
        public IHttpActionResult Put([FromBody] AppSettings appSettings)
        {
            string json = JsonConvert.SerializeObject(appSettings);
            string path = Path.Combine(ModApi.ModInstance.Path, "appsettings.json");
            File.WriteAllText(path, json, Encoding.UTF8);
            return Ok();
        }
    }
}
