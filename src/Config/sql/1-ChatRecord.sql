--聊天日志
CREATE TABLE IF NOT EXISTS T_ChatRecord(
	Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,	--Id
	CreatedAt TEXT NOT NULL,						--创建日期
	PlayerId TEXT,									--玩家跨平台Id
	EntityId INTEGER NOT NULL,						--实体Id
	SenderName TEXT NOT NULL,						--发送者名称
	ChatType INTEGER NOT NULL,						--聊天类型
	Message TEXT									--消息
);
--创建索引
CREATE INDEX IF NOT EXISTS Index_ChatRecord_0 ON T_ChatRecord(PlayerId);
CREATE INDEX IF NOT EXISTS Index_ChatRecord_1 ON T_ChatRecord(EntityId);
CREATE INDEX IF NOT EXISTS Index_ChatRecord_2 ON T_ChatRecord(SenderName);

