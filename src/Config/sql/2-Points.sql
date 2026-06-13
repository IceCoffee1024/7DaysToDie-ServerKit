--积分信息
CREATE TABLE IF NOT EXISTS T_PointsInfo(
	Id TEXT NOT NULL PRIMARY KEY,					--玩家跨平台Id
	CreatedAt TEXT NOT NULL,						--创建日期
	PlayerName TEXT NOT NULL,						--玩家名称
	Points INTEGER NOT NULL,						--拥有积分
	LastSignInDays INTEGER NOT NULL					--上次签到天数
);

--创建索引
CREATE INDEX IF NOT EXISTS Index_PointsInfo_0 ON T_PointsInfo(CreatedAt);
CREATE INDEX IF NOT EXISTS Index_PointsInfo_2 ON T_PointsInfo(PlayerName);
CREATE INDEX IF NOT EXISTS Index_PointsInfo_3 ON T_PointsInfo(Points);

--积分信息 v1
CREATE TABLE IF NOT EXISTS T_PointsInfo_v1(
	Id TEXT NOT NULL PRIMARY KEY,					--玩家跨平台Id
	CreatedAt TEXT NOT NULL,						--创建日期
	PlayerName TEXT NOT NULL,						--玩家名称
	Points INTEGER NOT NULL,						--拥有积分
	LastSignInAt TEXT								--上次签到日期
);

--创建索引
CREATE INDEX IF NOT EXISTS Index_PointsInfo_2 ON T_PointsInfo_v1(PlayerName);
CREATE INDEX IF NOT EXISTS Index_PointsInfo_3 ON T_PointsInfo_v1(Points);

--迁移数据到新表
INSERT INTO T_PointsInfo_v1 (Id, CreatedAt, PlayerName, Points)
SELECT 
    Id, 
    CreatedAt, 
    PlayerName, 
    Points
FROM 
    T_PointsInfo
   WHERE NOT EXISTS(SELECT 1 FROM T_PointsInfo_v1);