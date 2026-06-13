using SdtdServerKit.Managers;
using SdtdServerKit.Models;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Player Skills
    /// </summary>
    [Authorize]
    [RoutePrefix("api/PlayerSkills")]
    public class PlayerSkillsController : ApiController
    {
        /// <summary>
        /// Gets player progression
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{playerId}")]
        [ResponseType(typeof(IEnumerable<PlayerSkill>))]
        public IHttpActionResult Get(string playerId, [FromUri] Language language)
        {
            if (LivePlayerManager.TryGetByPlayerId(playerId, out var managedPlayer))
            {
                var progression = managedPlayer!.EntityPlayer.Progression;
                return Ok(GetPlayerSkills(progression, language));
            }
            else
            {
                var playerDataFile = new PlayerDataFile();
                playerDataFile.Load(GameIO.GetPlayerDataDir(), playerId);
                if (playerDataFile.bLoaded == false || playerDataFile.progressionData.Length <= 0L)
                {
                    return NotFound();
                }
                
                using PooledBinaryReader pooledBinaryReader = MemoryPools.poolBinaryReader.AllocSync(false);
                pooledBinaryReader.SetBaseStream(playerDataFile.progressionData);
                
                var entityPlayer = new EntityPlayer();
                entityPlayer.Progression = new Progression(entityPlayer);
                
                var progression = Progression.Read(pooledBinaryReader, entityPlayer);
                return Ok(GetPlayerSkills(progression, language));
            }
        }

        private static List<PlayerSkill> GetPlayerSkills(Progression progression, Language language)
        {
            var result = new List<PlayerSkill>();

            //var attributesMap = Progression.ProgressionClasses.Where(i => i.Value.Type == ProgressionType.Attribute);
            foreach (var item in Progression.ProgressionClasses.Values)
            {
                if (item.Type != ProgressionType.Attribute)
                {
                    continue;
                }

                var progressionValue = progression.GetProgressionValue(item.Name);
                var playerSkill = new PlayerSkill()
                {
                    Name = item.Name,
                    LocalizationName = Utilities.Utils.GetLocalization(item.NameKey, language, true),
                    LocalizationDesc = Utilities.Utils.GetLocalization(item.DescKey, language, true),
                    //LocalizationLongDesc = Utils.GetLocalization(item.LongDescKey, language, true),
                    Level = progressionValue.Level,
                    MinLevel = item.MinLevel,
                    MaxLevel = item.MaxLevel,
                    CostForNextLevel = progressionValue.costForNextLevel,
                    Icon = item.Icon,
                    Type = item.Type.ToString(),
                    Children = GetChildren(progression, item, language),
                };
                result.Add(playerSkill);
            }

            return result;
        }

        private static List<PlayerSkill> GetChildren(Progression progression, ProgressionClass parent, Language language)
        {
            var result = new List<PlayerSkill>();
            foreach (var child in Progression.ProgressionClasses.Values)
            {
                if (child.ParentName != null && child.ParentName == parent.Name)
                {
                    var childProgressionValue = progression.GetProgressionValue(child.Name);
                    var childPlayerSkill = new PlayerSkill()
                    {
                        Name = child.Name,
                        LocalizationName = Utilities.Utils.GetLocalization(child.NameKey, language, true),
                        LocalizationDesc = Utilities.Utils.GetLocalization(child.DescKey, language, true),
                        //LocalizationLongDesc = Utils.GetLocalization(child.LongDescKey, language, true),
                        Level = childProgressionValue.Level,
                        MinLevel = child.MinLevel,
                        MaxLevel = child.MaxLevel,
                        CostForNextLevel = childProgressionValue.costForNextLevel,
                        Icon = child.Icon,
                        Type = child.Type.ToString(),
                        Children = GetChildren(progression, child, language),
                    };
                    result.Add(childPlayerSkill);
                }
            }

            return result;
        }
    }
}
