--自定义商人保护区域
CREATE TABLE IF NOT EXISTS T_TraderProtectArea(
	Id INTEGER PRIMARY KEY AUTOINCREMENT,	--唯一Id
	CreatedAt TEXT NOT NULL,				--创建日期
	MinX INTEGER NOT NULL,					--区域最小X坐标
	MinZ INTEGER NOT NULL,					--区域最小Z坐标
	MaxX INTEGER NOT NULL,					--区域最大X坐标
	MaxZ INTEGER NOT NULL,					--区域最大Z坐标
	Name TEXT									--区域备注名称（可选）
);
--创建索引
CREATE INDEX IF NOT EXISTS Index_TraderProtectArea_0 ON T_TraderProtectArea(MinX, MinZ, MaxX, MaxZ);
