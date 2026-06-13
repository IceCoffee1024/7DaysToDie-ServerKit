--区域命令禁用
CREATE TABLE IF NOT EXISTS T_MuteCommandArea(
	Id INTEGER PRIMARY KEY AUTOINCREMENT,	--唯一Id
	CreatedAt TEXT NOT NULL,				--创建日期
	MinX INTEGER NOT NULL,					--区域最小X坐标
	MinZ INTEGER NOT NULL,					--区域最小Z坐标
	MaxX INTEGER NOT NULL,					--区域最大X坐标
	MaxZ INTEGER NOT NULL,					--区域最大Z坐标
	Command TEXT NOT NULL					--被禁用的命令名称
);
--创建索引（提高查询性能）
CREATE INDEX IF NOT EXISTS Index_MuteCommandArea_0 ON T_MuteCommandArea(MinX, MinZ, MaxX, MaxZ);
CREATE INDEX IF NOT EXISTS Index_MuteCommandArea_1 ON T_MuteCommandArea(Command);
