using System.Collections.Generic;

namespace SdtdServerKit.Models
{
    /// <summary>
    /// 统计
    /// </summary>
    public class Stats
    {
        /// <summary>
        /// 服务端正常运行时间
        /// </summary>
        public float Uptime { get; set; }

        /// <summary>
        /// 游戏时间
        /// </summary>
        public GameTime GameTime { get; set; } = null!;

        /// <summary>
        /// 动物数
        /// </summary>
        public int Animals { get; set; }

        /// <summary>
        /// Max Spawned Animals
        /// </summary>
        public int MaxAnimals { get; set; }

        /// <summary>
        /// 僵尸数
        /// </summary>
        public int Zombies { get; set; }

        /// <summary>
        /// Max Spawned Zombies
        /// </summary>
        public int MaxZombies { get; set; }

        /// <summary>
        /// 实体数
        /// </summary>
        public int Entities { get; set; }

        /// <summary>
        /// 在线玩家数
        /// </summary>
        public int OnlinePlayers { get; set; }

        /// <summary>
        /// 最大在线玩家数
        /// </summary>
        public int MaxOnlinePlayers { get; set; }

        /// <summary>
        /// 离线玩家数量
        /// </summary>
        public int OfflinePlayers { get; set; }

        /// <summary>
        /// 是否为血月
        /// </summary>
        public bool IsBloodMoon { get; set; }

        /// <summary>
        /// Frames Per Second
        /// </summary>
        [JsonProperty("fps")]
        public float FPS { get; set; }

        /// <summary>
        /// 堆内存使用量, 以兆字节 (MB) 为单位, 表示当前游戏所使用的堆内存大小
        /// </summary>
        public float Heap { get; set; }

        /// <summary>
        /// 堆内存的最大限制，以兆字节 (MB) 为单位，表示堆内存的最大可用容量
        /// </summary>
        public float MaxHeap { get; set; }

        /// <summary>
        /// 游戏中的区块数量
        /// </summary>
        public int Chunks { get; set; }

        /// <summary>
        /// 指当前游戏中的活跃对象数量
        /// </summary>
        [JsonProperty("cgo")]
        public int CGO { get; set; }

        /// <summary>
        /// 物品数
        /// </summary>
        public int Items { get; set; }

        /// <summary>
        /// CO
        /// </summary>
        public int ChunkObservedEntities { get; set; }

        /// <summary>
        /// 常驻内存大小, 表示当前游戏所占用的物理内存大小
        /// </summary>
        public float ResidentSetSize { get; set; }

        /// <summary>
        /// Server Name
        /// </summary>
        public required string ServerName { get; set; }

        /// <summary>
        /// 地区
        /// </summary>
        public required string Region { get; set; }

        /// <summary>
        /// 语言
        /// </summary>
        public required string Language { get; set; }

        /// <summary>
        /// Server Version
        /// </summary>
        public required string ServerVersion { get; set; }

        /// <summary>
        /// Server IP
        /// </summary>
        public required string ServerIp { get; set; }

        /// <summary>
        /// ServerPort
        /// </summary>
        public int ServerPort { get; set; }

        /// <summary>
        /// Game Mode
        /// </summary>
        public required string GameMode { get; set; }

        /// <summary>
        /// Game World
        /// </summary>
        public required string GameWorld { get; set; }

        /// <summary>
        /// Game Name
        /// </summary>
        public required string GameName { get; set; }

        /// <summary>
        /// Game Difficulty
        /// </summary>
        public int GameDifficulty { get; set; }
    }
}