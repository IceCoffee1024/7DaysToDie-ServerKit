using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Permissions
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Permissions")]
    public class PermissionsController : ApiController
    {
        /// <summary>
        /// Add permissions
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("")]
        public IEnumerable<string> AddPermissions([FromBody, Required] PermissionEntryAdd permission)
        {
            string command = $"cp add {permission.Command} {permission.PermissionLevel}";
            return SdtdConsole.Instance.ExecuteSync(command, ModApi.CmdExecuteDelegate);
        }

        /// <summary>
        /// Get permissions
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IEnumerable<PermissionEntry> GetPermissions()
        {
            var permissions = new List<PermissionEntry>();
            foreach (var item in GameManager.Instance.adminTools.Commands.GetCommands().Values)
            {
                permissions.Add(new PermissionEntry()
                {
                    Command = item.Command,
                    PermissionLevel = item.PermissionLevel,
                    Description = SdtdConsole.Instance.GetCommand(item.Command)?.GetDescription(),
                });
            }

            return permissions;
        }

        /// <summary>
        /// Remove permissions
        /// </summary>
        /// <param name="cmds"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("")]
        public IEnumerable<string> RemovePermissions([FromUri, Required, MinLength(1)] string[] cmds)
        {
            var executeResult = new List<string>();
            foreach (var item in cmds)
            {
                string command = $"cp remove {item}";
                var result = SdtdConsole.Instance.ExecuteSync(command, ModApi.CmdExecuteDelegate);
                executeResult.AddRange(result);
            }

            return executeResult;
        }
    }
}
