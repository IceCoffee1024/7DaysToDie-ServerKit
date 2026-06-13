using IceCoffee.SimpleCRUD.OptionalAttributes;
using System.Xml.Linq;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// 出生点管理
    /// </summary>
    [Authorize]
    [RoutePrefix("api/SpawnPoint")]
    public class SpawnPointController : ApiController
    {
        /// <summary>
        /// 获取地图默认出生点列表（从 spawnpoints.xml 文件读取）
        /// </summary>
        [HttpGet]
        [Route(nameof(GetMapSpawnPoints))]
        public IHttpActionResult GetMapSpawnPoints()
        {
            try
            {
                // 获取世界路径
                var worldName = GameManager.Instance.World.ChunkCache.Name;
                var worldLocation = PathAbstractions.WorldsSearchPaths.GetLocation(worldName, null, null);
                var worldPath = worldLocation.FullPath;
                var spawnPointsFile = System.IO.Path.Combine(worldPath, "spawnpoints.xml");

                CustomLogger.Debug($"尝试读取出生点文件：{spawnPointsFile}");

                if (!SdFile.Exists(spawnPointsFile))
                {
                    CustomLogger.Warn($"出生点文件不存在：{spawnPointsFile}");
                    return Ok(new List<object>());
                }

                // 读取 XML 文件
                var result = new List<object>();
                var xmlDoc = XDocument.Load(spawnPointsFile);
                var spawnPointElements = xmlDoc.Root?.Elements("spawnpoint");

                if (spawnPointElements == null)
                {
                    CustomLogger.Warn("spawnpoints.xml 文件中没有找到 spawnpoint 元素");
                    return Ok(new List<object>());
                }

                int index = 1;
                foreach (var element in spawnPointElements)
                {
                    var positionAttr = element.Attribute("position");
                    if (positionAttr != null)
                    {
                        result.Add(new
                        {
                            position = positionAttr.Value,
                            description = $"地图出生点 {index}"
                        });
                        index++;
                    }
                }

                CustomLogger.Debug($"成功读取地图出生点：共 {result.Count} 个");
                return Ok(result);
            }
            catch (Exception ex)
            {
                CustomLogger.Error($"获取地图出生点失败：{ex.Message}\n{ex.StackTrace}");
                return InternalServerError(ex);
            }
        }
    }
}
