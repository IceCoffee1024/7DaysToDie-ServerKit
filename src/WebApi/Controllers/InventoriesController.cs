using System.ComponentModel.DataAnnotations;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Player Inventory
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Inventories")]
    public class InventoriesController : ApiController
    {
        /// <summary>
        /// 获取指定玩家的背包
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{playerId}")]
        [ResponseType(typeof(Models.Inventory))]
        public IHttpActionResult GetPlayerInventory(string playerId, [FromUri]Language language)
        {
            var userId = PlatformUserIdentifierAbs.FromCombinedString(playerId);
            if(userId == null)
            {
                return NotFound();
            }

            var clientInfo = ConnectionManager.Instance.Clients.ForUserId(userId);
            if (clientInfo != null)
            {
                return Ok(clientInfo.latestPlayerData.GetInventory(language));
            }

            var playerDataFile = new PlayerDataFile();
            playerDataFile.Load(GameIO.GetPlayerDataDir(), userId.CombinedString);
            if (playerDataFile.bLoaded)
            {
                return Ok(playerDataFile.GetInventory(language));
            }

            return NotFound();
        }

        /// <summary>
        /// 获取指定玩家的背包
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public Dictionary<string, Models.Inventory> GetPlayersInventory([FromUri, Required, MinLength(1)] string[] playerIds, [FromUri] Language language)
        {
            var result = new Dictionary<string, Models.Inventory>(playerIds.Length);
            foreach (var playerId in playerIds)
            {
                var userId = PlatformUserIdentifierAbs.FromCombinedString(playerId);
                if (userId != null)
                {
                    var clientInfo = ConnectionManager.Instance.Clients.ForUserId(userId);
                    if (clientInfo != null)
                    {
                        result.Add(playerId, clientInfo.latestPlayerData.GetInventory(language));
                    }
                    else
                    {
                        var playerDataFile = new PlayerDataFile();
                        playerDataFile.Load(GameIO.GetPlayerDataDir(), userId.CombinedString);
                        if (playerDataFile.bLoaded)
                        {
                            result.Add(playerId, playerDataFile.GetInventory(language));
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// 删除指定玩家背包中的物品
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("{playerId}/{itemName}")]
        [ResponseType(typeof(IEnumerable<string>))]
        public IHttpActionResult RemovePlayerItems(string playerId, string itemName)
        {
            var userId = PlatformUserIdentifierAbs.FromCombinedString(playerId);
            if (userId == null)
            {
                return NotFound();
            }

            string command = $"ty-rpi {playerId} {itemName}";
            var result = SdtdConsole.Instance.ExecuteSync(command, ModApi.CmdExecuteDelegate);

            return Ok(result);
        }

        /// <summary>
        /// 批量删除指定玩家背包中的物品
        /// </summary>
        /// <returns></returns>
        [HttpDelete]
        [Route("{playerId}")]
        [ResponseType(typeof(IEnumerable<string>))]
        public IHttpActionResult RemoveItems(string playerId, [FromUri, Required, MinLength(1)] string[] itemNames)
        {
            var userId = PlatformUserIdentifierAbs.FromCombinedString(playerId);
            if (userId == null)
            {
                return NotFound();
            }

            var result = new List<string>();
            foreach (var itemName in itemNames)
            {
                string command = $"ty-rpi {playerId} {itemName}";
                result.AddRange(SdtdConsole.Instance.ExecuteSync(command, ModApi.CmdExecuteDelegate));
            }

            return Ok(result);
        }
    }
}
