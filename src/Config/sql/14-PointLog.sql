--积分变动日志
CREATE TABLE IF NOT EXISTS T_PointLog(
    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,  --Id
    CreatedAt TEXT NOT NULL,                        --创建日期
    PlayerId TEXT NOT NULL,                         --玩家跨平台Id
    PlayerName TEXT,                                --玩家名（记录时刻的快照）
    Category INTEGER NOT NULL DEFAULT 99,           --日志分类（1=Shop, 2=SignIn, 3=Transfer, 4=Teleport, 5=ZombieKill, 6=Lottery, 7=CdKey, 8=LevelGift, 9=VipGift, 10=WebApi, 11=Script, 12=External, 99=Other）
    ChangeType INTEGER NOT NULL DEFAULT 0,          --变化类型（0=NoChange, 1=Add, 2=Sub, 3=Set）
    Spend INTEGER NOT NULL DEFAULT 0,               --变动积分（正数为加，负数为减）
    Balance INTEGER NOT NULL DEFAULT 0,             --操作后余额
    Note TEXT                                       --日志详情
);

--创建索引
CREATE INDEX IF NOT EXISTS Index_PointLog_0 ON T_PointLog(PlayerId);
CREATE INDEX IF NOT EXISTS Index_PointLog_1 ON T_PointLog(Category);
CREATE INDEX IF NOT EXISTS Index_PointLog_2 ON T_PointLog(CreatedAt);
