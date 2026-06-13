using Webserver.WebAPI.APIs.WorldState;

namespace SdtdServerKit.WebApi.Controllers
{
    /// <summary>
    /// Locations
    /// </summary>
    [Authorize]
    [RoutePrefix("api/Locations")]
    public class LocationsController : ApiController
    {
        /// <summary>
        /// 获取位置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("")]
        public IEnumerable<EntityInfo> Get(Models.EntityType entityType)
        {
            var locations = new List<EntityInfo>();

            if (entityType == Models.EntityType.OfflinePlayer)
            {
                var online = GameManager.Instance.World.Players.list.Select(i => ConnectionManager.Instance.Clients.ForEntityId(i.entityId).InternalId).ToHashSet();
                foreach (var item in GameManager.Instance.GetPersistentPlayerList().Players)
                {
                    if(online.Contains(item.Key) == false)
                    {
                        var player = item.Value;
                        locations.Add(new EntityInfoEx()
                        {
                            EntityId = player.EntityId,
                            EntityName = player.PlayerName.DisplayName,
                            Position = player.Position.ToPosition(),
                            EntityType = Models.EntityType.OfflinePlayer,
                            PlayerId = player.PrimaryId.CombinedString,
                        });
                    }
                }
            }
            else if (entityType == Models.EntityType.OnlinePlayer)
            {
                foreach (var player in GameManager.Instance.World.Players.list)
                {
                    locations.Add(new EntityInfoEx()
                    {
                        EntityId = player.entityId,
                        EntityName = player.EntityName,
                        Position = player.GetPosition().ToPosition(),
                        EntityType = Models.EntityType.OnlinePlayer,
                        PlayerId = ConnectionManager.Instance.Clients.ForEntityId(player.entityId).InternalId.CombinedString,
                    });
                }
            }
            else if (entityType == Models.EntityType.Animal)
            {
                foreach (var entity in GameManager.Instance.World.Entities.list)
                {
                    if (entity is EntityAnimal entityAnimal && entity.IsAlive())
                    {
                        locations.Add(new EntityInfo()
                        {
                            EntityId = entityAnimal.entityId,
                            EntityName = entityAnimal.EntityName ?? ("animal class #" + entityAnimal.entityClass),
                            Position = entityAnimal.GetPosition().ToPosition(),
                            EntityType = Models.EntityType.Animal,
                        });
                    }
                }
            }
            else if (entityType == Models.EntityType.Hostiles)
            {
                foreach (var entity in GameManager.Instance.World.Entities.list)
                {
                    if (entity is EntityEnemy entityEnemy && entity.IsAlive())
                    {
                        locations.Add(new EntityInfo()
                        {
                            EntityId = entityEnemy.entityId,
                            EntityName = entityEnemy.EntityName ?? ("enemy class #" + entityEnemy.entityClass),
                            Position = entityEnemy.GetPosition().ToPosition(),
                            EntityType = (Models.EntityType)entityEnemy.entityType
                        });
                    }
                }
            }
            else if (entityType == Models.EntityType.Zombie)
            {
                foreach (var entity in GameManager.Instance.World.Entities.list)
                {
                    if (entity is EntityZombie entityZombie && entity.IsAlive())
                    {
                        locations.Add(new EntityInfo()
                        {
                            EntityId = entityZombie.entityId,
                            EntityName = entityZombie.EntityName ?? ("zombie class #" + entityZombie.entityClass),
                            Position = entityZombie.GetPosition().ToPosition(),
                            EntityType = (Models.EntityType)entityZombie.entityType
                        });
                    }
                }
            }
            else if (entityType == Models.EntityType.Bandit)
            {
                foreach (var entity in GameManager.Instance.World.Entities.list)
                {
                    if (entity is EntityBandit entityBandit && entity.IsAlive())
                    {
                        locations.Add(new EntityInfo()
                        {
                            EntityId = entityBandit.entityId,
                            EntityName = entityBandit.EntityName ?? ("bandit class #" + entityBandit.entityClass),
                            Position = entityBandit.GetPosition().ToPosition(),
                            EntityType = (Models.EntityType)entityBandit.entityType
                        });
                    }
                }
            }
            else if (entityType == Models.EntityType.Vehicle)
            {
                var vehicleManager = VehicleManager.Instance;
                if (vehicleManager != null)
                {
                    var vehicleManagerType = vehicleManager.GetType();
                    
                    var bindingFlags = System.Reflection.BindingFlags.Public | 
                                      System.Reflection.BindingFlags.NonPublic | 
                                      System.Reflection.BindingFlags.Instance;
                    
                    var vehiclesActiveField = vehicleManagerType.GetField("vehiclesActive", bindingFlags);
                    var vehiclesUnloadedField = vehicleManagerType.GetField("vehiclesUnloaded", bindingFlags);
                    
                    // 处理已加载的载具（vehiclesActive）
                    if (vehiclesActiveField != null)
                    {
                        var vehiclesActive = vehiclesActiveField.GetValue(vehicleManager) as System.Collections.IList;
                        if (vehiclesActive != null)
                        {
                            foreach (var vehicle in vehiclesActive)
                            {
                                if (vehicle is EntityVehicle entityVehicle)
                                {
                                    try
                                    {
                                        var vehicleInfo = CreateVehicleInfo(entityVehicle);
                                        if (vehicleInfo != null)
                                        {
                                            locations.Add(vehicleInfo);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        CustomLogger.Warn($"获取已加载载具信息失败 EntityId={entityVehicle.entityId}: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                    
                    // 处理已卸载的载具（vehiclesUnloaded）
                    if (vehiclesUnloadedField != null)
                    {
                        var vehiclesUnloaded = vehiclesUnloadedField.GetValue(vehicleManager) as System.Collections.IList;
                        if (vehiclesUnloaded != null)
                        {
                            foreach (var creationData in vehiclesUnloaded)
                            {
                                try
                                {
                                    var creationDataType = creationData.GetType();
                                    var idField = creationDataType.GetField("id");
                                    var posField = creationDataType.GetField("pos");
                                    var entityClassField = creationDataType.GetField("entityClass");
                                    
                                    if (idField != null && posField != null && entityClassField != null)
                                    {
                                        int entityId = (int)idField.GetValue(creationData);
                                        var position = (UnityEngine.Vector3)posField.GetValue(creationData);
                                        int entityClass = (int)entityClassField.GetValue(creationData);
                                        
                                        // 获取所有者信息
                                        string ownerId = null;
                                        string ownerName = null;
                                        
                                        
                                        // 获取实体类名和本地化名称
                                        string entityClassName = string.Empty;
                                        string vehicleEntityClass = string.Empty;
                                        string localizedName = string.Empty;
                                        
                                        var entityClassData = EntityClass.GetEntityClass(entityClass);
                                        if (entityClassData != null)
                                        {
                                            entityClassName = entityClassData.entityClassName;
                                            vehicleEntityClass = entityClassData.classname?.Name ?? string.Empty;
                                            
                                            if (!string.IsNullOrEmpty(entityClassName) && Localization.Exists(entityClassName))
                                            {
                                                localizedName = Localization.Get(entityClassName);
                                            }
                                        }
                                        
                                        if (string.IsNullOrEmpty(entityClassName))
                                        {
                                            entityClassName = "vehicle_" + entityClass;
                                        }
                                        
                                        if (string.IsNullOrEmpty(vehicleEntityClass))
                                        {
                                            vehicleEntityClass = "EntityVehicle";
                                        }
                                        
                                        string vehicleName = !string.IsNullOrEmpty(localizedName) ? localizedName : entityClassName;
                                        
                                        locations.Add(new VehicleInfo()
                                        {
                                            EntityId = entityId,
                                            EntityName = vehicleName,
                                            EntityClassName = entityClassName,
                                            Position = new Models.Position(position.x, position.y, position.z),
                                            EntityType = Models.EntityType.Vehicle,
                                            OwnerId = ownerId,
                                            OwnerName = ownerName,
                                            IsLocked = false, // 已卸载的载具无法获取锁定状态
                                            VehicleEntityClass = vehicleEntityClass,
                                            LocalizedName = localizedName
                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    CustomLogger.Warn($"获取已卸载载具信息失败: {ex.Message}");
                                }
                            }
                        }
                    }
                }
                
                CustomLogger.Debug($"载具查询完成，找到 {locations.Count} 个载具");
            }

            return locations;
        }

        /// <summary>
        /// 获取位置
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("{entityId:int}")]
        [ResponseType(typeof(EntityInfo))]
        public IHttpActionResult Get(int entityId)
        {
            if(GameManager.Instance.World.Players.dict.TryGetValue(entityId, out var player))
            {
                return Ok(new EntityInfo()
                {
                    EntityId = player.entityId,
                    EntityName = player.EntityName,
                    Position = player.GetPosition().ToPosition(),
                    EntityType = Models.EntityType.OnlinePlayer,
                });
            }

            if (GameManager.Instance.World.Entities.dict.TryGetValue(entityId, out var entity))
            {
                string entityName = (entity is EntityAlive entityAlive) ? entityAlive.EntityName : "entity class #" + entity.entityClass;
                return Ok(new EntityInfo()
                {
                    EntityId = entity.entityId,
                    EntityName = entityName,
                    Position = entity.GetPosition().ToPosition(),
                    EntityType = (Models.EntityType)entity.entityType,
                });
            }

            return NotFound();
        }

        /// <summary>
        /// 从EntityVehicle创建VehicleInfo
        /// </summary>
        private static VehicleInfo CreateVehicleInfo(EntityVehicle entityVehicle)
        {
            var owner = entityVehicle.GetOwner();
            var ownerName = owner != null ? GameManager.Instance.GetPersistentPlayerList().GetPlayerData(owner)?.PlayerName?.DisplayName : null;
            
            // 获取实体类名
            string entityClassName = string.Empty;
            string vehicleEntityClass = string.Empty;
            
            if (entityVehicle.entityClass >= 0 && entityVehicle.entityClass < EntityClass.list.Count)
            {
                var entityClass = EntityClass.list[entityVehicle.entityClass];
                if (entityClass != null)
                {
                    entityClassName = entityClass.entityClassName;
                    vehicleEntityClass = entityClass.classname?.Name ?? string.Empty;
                }
            }
            
            if (string.IsNullOrEmpty(vehicleEntityClass))
            {
                vehicleEntityClass = entityVehicle.GetType().Name;
            }
            
            if (string.IsNullOrEmpty(entityClassName))
            {
                entityClassName = "vehicle_" + entityVehicle.entityClass;
            }
            
            // 获取本地化名称
            string localizedName = string.Empty;
            if (!string.IsNullOrEmpty(entityClassName))
            {
                if (Localization.Exists(entityClassName))
                {
                    localizedName = Localization.Get(entityClassName);
                }
            }
            
            // 获取载具名称
            string vehicleName = entityVehicle.EntityName;
            if (string.IsNullOrEmpty(vehicleName))
            {
                vehicleName = !string.IsNullOrEmpty(localizedName) ? localizedName : entityClassName;
            }
            
            return new VehicleInfo()
            {
                EntityId = entityVehicle.entityId,
                EntityName = vehicleName,
                EntityClassName = entityClassName,
                Position = entityVehicle.GetPosition().ToPosition(),
                EntityType = Models.EntityType.Vehicle,
                OwnerId = owner?.CombinedString,
                OwnerName = ownerName,
                IsLocked = entityVehicle.IsLocked(),
                VehicleEntityClass = vehicleEntityClass,
                LocalizedName = localizedName
            };
        }
    }
}
