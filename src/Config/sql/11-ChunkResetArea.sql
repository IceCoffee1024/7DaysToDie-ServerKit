--定时重置区域
CREATE TABLE IF NOT EXISTS T_ChunkResetArea(
	Id INTEGER PRIMARY KEY AUTOINCREMENT,	--唯一Id
	CreatedAt TEXT NOT NULL,				--创建日期
	MinX INTEGER NOT NULL,					--区域最小X坐标（已对齐chunk边界）
	MinZ INTEGER NOT NULL,					--区域最小Z坐标（已对齐chunk边界）
	MaxX INTEGER NOT NULL,					--区域最大X坐标（已对齐chunk边界）
	MaxZ INTEGER NOT NULL					--区域最大Z坐标（已对齐chunk边界）
);
--创建索引
CREATE INDEX IF NOT EXISTS Index_ChunkResetArea_0 ON T_ChunkResetArea(MinX, MinZ, MaxX, MaxZ);
