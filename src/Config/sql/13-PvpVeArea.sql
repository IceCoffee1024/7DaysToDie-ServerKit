--PVP/PVE 混合区域
CREATE TABLE IF NOT EXISTS T_PvpVeArea(
    Id INTEGER PRIMARY KEY AUTOINCREMENT,    --唯一Id
    CreatedAt TEXT NOT NULL,                 --创建日期
    MinX INTEGER NOT NULL,                   --区域最小X坐标
    MinZ INTEGER NOT NULL,                   --区域最小Z坐标
    MaxX INTEGER NOT NULL,                   --区域最大X坐标
    MaxZ INTEGER NOT NULL,                   --区域最大Z坐标
    KillMode INTEGER NOT NULL DEFAULT 2,     --杀戮模式: 0=无伤害, 1=队友伤害, 2=陌生人伤害, 3=所有人伤害
    DropOnDeath INTEGER NOT NULL DEFAULT 0,  --死亡掉包模式: 0=不掉包, 1=全部掉落, 2=只掉腰带, 3=只掉背包
    LandClaimOnline INTEGER NOT NULL DEFAULT 4,  --在线领地石硬度加成 (0表示无敌)
    LandClaimOffline INTEGER NOT NULL DEFAULT 8, --离线领地石硬度加成 (0表示无敌)
    BuffName TEXT NOT NULL DEFAULT '',       --区域提示Buff名称
    Name TEXT                                --区域备注名称（可选）
);
--创建索引
CREATE INDEX IF NOT EXISTS Index_PvpVeArea_0 ON T_PvpVeArea(MinX, MinZ, MaxX, MaxZ);
