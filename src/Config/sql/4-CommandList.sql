--命令清单
CREATE TABLE IF NOT EXISTS T_CommandList(
	Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,	--Id
	CreatedAt TEXT NOT NULL,						--创建日期
	DisplayName TEXT,								--显示名称
	Command TEXT NOT NULL,							--命令
	InMainThread INTEGER NOT NULL,					--在主线程执行
	Description TEXT								--说明
);
