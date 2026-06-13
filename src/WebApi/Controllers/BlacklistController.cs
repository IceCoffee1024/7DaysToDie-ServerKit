using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Blacklist
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Blacklist")]
    public class BlacklistController : ApiController
    {
        /// <summary>
        /// Add blacklist
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IEnumerable<string> AddBlacklist([FromBody, Required] BlacklistEntry model)
        {
            string command = $"ban add {model.PlayerId} {(int)(model.BannedUntil - DateTime.Now).TotalMinutes} minutes {Utilities.Utils.FormatCommandArgs(model.Reason)} {Utilities.Utils.FormatCommandArgs(model.DisplayName)}";
            return SdtdConsole.Instance.ExecuteSync(command, ModApi.CmdExecuteDelegate);
        }

        /// <summary>
        /// Get blacklist
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IEnumerable<BlacklistEntry> GetBlacklist()
        {
            var blacklist = new List<BlacklistEntry>();
            foreach (var item in GameManager.Instance.adminTools.Blacklist.GetBanned())
            {
                blacklist.Add(new BlacklistEntry()
                {
                    PlayerId = item.UserIdentifier.CombinedString,
                    BannedUntil = item.BannedUntil,
                    Reason = item.BanReason,
                    DisplayName = item.Name
                });
            }

            return blacklist;
        }

        /// <summary>
        /// Remove blacklist
        /// </summary>
        /// <param name="playerIds"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public IEnumerable<string> RemoveBlacklist([FromUri, Required, MinLength(1)] string[] playerIds)
        {
            var executeResult = new List<string>();
            foreach (var item in playerIds)
            {
                string command = $"ban remove {item}";
                var result = SdtdConsole.Instance.ExecuteSync(command, ModApi.CmdExecuteDelegate);
                executeResult.AddRange(result);
            }

            return executeResult;
        }
    }
}
