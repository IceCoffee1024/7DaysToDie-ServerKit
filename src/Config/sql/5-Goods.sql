--商品
CREATE TABLE IF NOT EXISTS T_Goods_v2(
	Id INTEGER NOT NULL PRIMARY KEY,				--Id
	CreatedAt TEXT NOT NULL,						--创建日期
	Name TEXT NOT NULL,								--商品名称
	Price INTEGER NOT NULL,							--售价
	Description TEXT								--说明
);

--启用外键
PRAGMA FOREIGN_KEYS = ON;

CREATE TABLE IF NOT EXISTS T_GoodsItem(
	GoodsId INTEGER NOT NULL,						--商品Id
	ItemId INTEGER NOT NULL,						--物品Id
	PRIMARY KEY (GoodsId, ItemId),
	FOREIGN KEY (GoodsId) REFERENCES T_Goods_v2(Id) ON DELETE CASCADE,
	FOREIGN KEY (ItemId) REFERENCES T_ItemList(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS T_GoodsCommand(
	GoodsId INTEGER NOT NULL,						--商品Id
	CommandId INTEGER NOT NULL,						--命令Id
	PRIMARY KEY (GoodsId, CommandId),
	FOREIGN KEY (GoodsId) REFERENCES T_Goods_v2(Id) ON DELETE CASCADE,
	FOREIGN KEY (CommandId) REFERENCES T_CommandList(Id) ON DELETE CASCADE
);