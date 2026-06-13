--物品清单
CREATE TABLE IF NOT EXISTS T_ItemList(
	Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,	--Id
	CreatedAt TEXT NOT NULL,						--创建日期
	DisplayName TEXT,								--显示名称
	RewardType INTEGER NOT NULL DEFAULT 0,			--奖品类型 0=物品 2=积分
	ItemName TEXT NOT NULL,							--物品名称
	[Count] INTEGER NOT NULL,						--数量
	Quality INTEGER NOT NULL,						--质量
	Durability INTEGER NOT NULL,					--耐久度百分比
	Description TEXT								--说明
);

