--传送记录
CREATE TABLE IF NOT EXISTS T_TeleRecord(
	Id INTEGER PRIMARY KEY AUTOINCREMENT,	--唯一Id
	CreatedAt TEXT NOT NULL,				--创建日期
	PlayerId TEXT NOT NULL,					--玩家跨平台Id
	PlayerName TEXT NOT NULL,				--玩家名称
	TargetType TEXT NOT NULL,				--目标类型
	TargetId TEXT,							--目标Id
	TargetName TEXT NOT NULL,				--目标名称
	OriginPosition TEXT NOT NULL,			--源坐标, 空格分割
	TargetPosition TEXT	NOT NULL			--目的坐标, 空格分割
);
--创建索引
CREATE INDEX IF NOT EXISTS Index_TeleRecord_0 ON T_TeleRecord(PlayerId);
CREATE INDEX IF NOT EXISTS Index_TeleRecord_1 ON T_TeleRecord(PlayerName);
CREATE INDEX IF NOT EXISTS Index_TeleRecord_2 ON T_TeleRecord(TargetType);

--公共回城点
CREATE TABLE IF NOT EXISTS T_CityLocation_v1(
	Id INTEGER PRIMARY KEY,					--唯一Id
	CreatedAt TEXT NOT NULL,				--创建日期
	CityName TEXT NOT NULL,					--城镇名称
	PointsRequired INTEGER NOT NULL,		--传送需要积分
	Position TEXT NOT NULL,					--三维坐标
	ViewDirection TEXT,						--视角方向
	IsEnabled INTEGER NOT NULL DEFAULT 1,	--是否启用
	TeleCommand TEXT						--单独的传送命令
);

--私人回城点
CREATE TABLE IF NOT EXISTS T_HomeLocation(
	Id INTEGER PRIMARY KEY AUTOINCREMENT,	--唯一Id
	CreatedAt TEXT NOT NULL,				--创建日期
	PlayerId TEXT NOT NULL,					--玩家跨平台Id
	PlayerName TEXT NOT NULL,				--玩家名称	
	HomeName TEXT NOT NULL,					--Home名称
	Position TEXT NOT NULL					--三维坐标
);
--创建索引
CREATE UNIQUE INDEX IF NOT EXISTS Index_HomeLocation_0 ON T_HomeLocation(PlayerId, HomeName);

