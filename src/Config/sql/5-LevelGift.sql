--等级礼包
CREATE TABLE IF NOT EXISTS T_LevelGift_v1(
	Id TEXT NOT NULL PRIMARY KEY,					--Player Id (玩家礼包) 或 礼包唯一ID (通用礼包)
	CreatedAt TEXT NOT NULL,						--创建日期
	Name TEXT NOT NULL,								--礼包名称
	PlayerName TEXT,								--玩家名称（仅玩家礼包使用）
	RequiredLevel INTEGER NOT NULL,					--等级要求
	ClaimState INTEGER NOT NULL,					--领取状态
	TotalClaimCount INTEGER NOT NULL,				--总领取次数
	LastClaimAt TEXT,								--上次领取日期
	Description TEXT,								--说明
	GiftType INTEGER NOT NULL DEFAULT 0				--礼包类型：0=玩家礼包，1=通用礼包
);

--通用礼包领取记录表
CREATE TABLE IF NOT EXISTS T_LevelGiftClaimRecord_v1(
	PlayerId TEXT NOT NULL,							--玩家ID
	GiftId TEXT NOT NULL,							--礼包ID
	ClaimAt TEXT NOT NULL,							--领取时间
	PRIMARY KEY (PlayerId, GiftId),
	FOREIGN KEY (GiftId) REFERENCES T_LevelGift_v1(Id) ON DELETE CASCADE
);

--启用外键
PRAGMA FOREIGN_KEYS = ON;

CREATE TABLE IF NOT EXISTS T_LevelGiftItem_v1(
	LevelGiftId TEXT NOT NULL,						--等级礼包Id
	ItemId INTEGER NOT NULL,						--物品Id
	PRIMARY KEY (LevelGiftId, ItemId),
	FOREIGN KEY (LevelGiftId) REFERENCES T_LevelGift_v1(Id) ON DELETE CASCADE,
	FOREIGN KEY (ItemId) REFERENCES T_ItemList(Id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS T_LevelGiftCommand_v1(
	LevelGiftId TEXT NOT NULL,						--等级礼包Id
	CommandId INTEGER NOT NULL,						--命令Id
	PRIMARY KEY (LevelGiftId, CommandId),
	FOREIGN KEY (LevelGiftId) REFERENCES T_LevelGift_v1(Id) ON DELETE CASCADE,
	FOREIGN KEY (CommandId) REFERENCES T_CommandList(Id) ON DELETE CASCADE
);
