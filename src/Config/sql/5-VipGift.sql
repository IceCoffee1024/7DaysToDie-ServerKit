--VIP礼包
CREATE TABLE IF NOT EXISTS T_VipGift_v1(
	Id TEXT NOT NULL PRIMARY KEY,					--Player Id
	CreatedAt TEXT NOT NULL,						--创建日期
	Name TEXT NOT NULL,								--礼包名称
	PlayerName TEXT,								--玩家名称
	ClaimState INTEGER NOT NULL,					--领取状态
	TotalClaimCount INTEGER NOT NULL,				--总领取次数
	LastClaimAt TEXT,								--上次领取日期
	Description TEXT								--说明
);

--启用外键
PRAGMA FOREIGN_KEYS = ON;

CREATE TABLE IF NOT EXISTS T_VipGiftItem_v1(
	VipGiftId TEXT NOT NULL,						--VIP礼包Id
	ItemId INTEGER NOT NULL,						--物品Id
	PRIMARY KEY (VipGiftId, ItemId),
	FOREIGN KEY (VipGiftId) REFERENCES T_VipGift_v1(Id) ON DELETE CASCADE,
	FOREIGN KEY (ItemId) REFERENCES T_ItemList(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS T_VipGiftCommand_v1(
	VipGiftId TEXT NOT NULL,						--VIP礼包Id
	CommandId INTEGER NOT NULL,						--命令Id
	PRIMARY KEY (VipGiftId, CommandId),
	FOREIGN KEY (VipGiftId) REFERENCES T_VipGift_v1(Id) ON DELETE CASCADE,
	FOREIGN KEY (CommandId) REFERENCES T_CommandList(Id) ON DELETE CASCADE
);