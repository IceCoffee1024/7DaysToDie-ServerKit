using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Whitelist
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Whitelist")]
    public class WhitelistController : ApiController
    {
        /// <summary>
        /// Add whitelist
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IEnumerable<string> AddWhitelist([FromBody, Required] WhitelistEntry model)
        {
            string command = $"whitelist add {model.PlayerId} {Utilities.Utils.FormatCommandArgs(model.DisplayName)}";
            return SdtdConsole.Instance.ExecuteSync(command, ModApi.CmdExecuteDelegate);
        }

        /// <summary>
        /// Get whitelist
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IEnumerable<WhitelistEntry> GetWhitelist()
        {
            var whitelist = new List<WhitelistEntry>();
            foreach (var item in GameManager.Instance.adminTools.Whitelist.GetUsers().Values)
            {
                whitelist.Add(new WhitelistEntry()
                {
                    PlayerId = item.UserIdentifier.CombinedString,
                    DisplayName = item.Name
                });
            }

            return whitelist;
        }

        /// <summary>
        /// Remove whitelist
        /// </summary>
        /// <param name="playerIds"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public IEnumerable<string> RemoveWhitelist([FromUri, Required, MinLength(1)] string[] playerIds)
        {
            var executeResult = new List<string>();
            foreach (var item in playerIds)
            {
                string command = $"whitelist remove {item}";
                var result = SdtdConsole.Instance.ExecuteSync(command, ModApi.CmdExecuteDelegate);
                executeResult.AddRange(result);
            }

            return executeResult;
        }
    }
}
