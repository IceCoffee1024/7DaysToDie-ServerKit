--配置
CREATE TABLE IF NOT EXISTS T_Settings(
	Name TEXT NOT NULL PRIMARY KEY,		--配置名称
	SerializedValue TEXT				--配置序列化值
);