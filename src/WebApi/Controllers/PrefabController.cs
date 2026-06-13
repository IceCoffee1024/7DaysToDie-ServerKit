using System.ComponentModel;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Prefab
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Prefab")]
    public class PrefabController : ApiController
    {
        /// <summary>
        /// Get all available prefabs.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(AvailablePrefabs))]
        public Paged<AvailablePrefab> AvailablePrefabs([FromUri] AvailablePrefabQuery model)
        {
            var list = new List<AvailablePrefab>();
            foreach (var item in PathAbstractions.PrefabsSearchPaths.GetAvailablePathsList())
            {
                string name = item.Name;
                string localizationName = Utilities.Utils.GetLocalization(name, model.Language, true);
                string fullPath = item.FullPath;

                string? keyword = model.Keyword;
                if (string.IsNullOrEmpty(keyword) == false)
                {
                    if(name.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) == -1 && 
                        localizationName.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) == -1)
                    {
                        continue;
                    }
                }

                list.Add(new AvailablePrefab()
                {
                    Name = name,
                    LocalizationName = localizationName,
                    FullPath = fullPath,
                });
            }

            int pageSize = model.PageSize;
            var items = pageSize < 0 ? list : list.Skip((model.PageNumber - 1) * pageSize).Take(pageSize);
            var result = new Paged<AvailablePrefab>(items, list.Count);
            
            
            //foreach (var item in PrefabManager.AllPrefabDatas.Values)
            //{
            //    string prefabName = item.Name;
            //    string fullPath = item.location.FullPath;
            //    string size = item.size.ToString();
            //    string tags = item.Tags.ToString();
            //    string themeTags = item.ThemeTags.ToString();
            //    int yOffset = item.yOffset;
            //}

            //result.Sort((a, b) => a.PrefabName.CompareTo(b.PrefabName));
            return result;
        }

        /// <summary>
        /// Place prefab.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route(nameof(PlacePrefab))]
        public IEnumerable<string> PlacePrefab([FromBody] PrefabPlace model)
        {
            string cmd = $"ty-PlacePrefab \"{model.PrefabFileName}\" {model.Position} {(int)model.Rotation}";
            if(model.NoSleepers)
            {
                cmd += " noSleepers";
            }
            if(model.AddToRWG)
            {
                cmd += " addToRWG";
            }

            return Utilities.Utils.ExecuteConsoleCommand(cmd, true);
        }

        /// <summary>
        /// Get undo history.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route(nameof(UndoHistory))]
        public IEnumerable<UndoHistory> UndoHistory()
        {
            var list = Commands.UndoPrefab.GetUndoHistoryList();
            if(list == null)
            {
                return Enumerable.Empty<UndoHistory>();
            }

            var result = new List<UndoHistory>();
            for (int i = 0; i < list.Count; i++)
            {
                result.Add(new UndoHistory
                {
                    Id = i,
                    PrefabName = list[i].PrefabName,
                    Position = list[i].OffsetPosition.ToPosition(),
                    CreatedAt = list[i].CreatedAt
                });
            }
           
            return result;
        }

        /// <summary>
        /// Undo prefab.
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route(nameof(UndoPrefab) + "/{id:int}")]
        public IEnumerable<string> UndoPrefab([DefaultValue(0)] int id = 0)
        {
            return Utilities.Utils.ExecuteConsoleCommand($"ty-UndoPrefab {id}", true);
        }
    }
}
