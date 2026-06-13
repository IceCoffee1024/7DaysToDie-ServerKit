--抽奖配置
CREATE TABLE IF NOT EXISTS T_Lottery_v1(
	Id INTEGER NOT NULL PRIMARY KEY,				--抽奖池Id
	CreatedAt TEXT NOT NULL,						--创建日期
	Name TEXT NOT NULL,								--抽奖池名称
	IsEnabled INTEGER NOT NULL,						--是否启用
	DrawCommand TEXT NOT NULL,						--抽奖命令
	DrawInterval INTEGER NOT NULL,					--抽奖间隔(秒)
	DrawCost INTEGER NOT NULL,						--单次消耗积分
	Description TEXT								--说明
);

--启用外键
PRAGMA FOREIGN_KEYS = ON;

--抽奖物品关联表（带权重）
CREATE TABLE IF NOT EXISTS T_LotteryItem_v1(
	LotteryId INTEGER NOT NULL,						--抽奖池Id
	ItemId INTEGER NOT NULL,						--物品Id
	Weight INTEGER NOT NULL DEFAULT 1,				--权重(概率)
	PRIMARY KEY (LotteryId, ItemId),
	FOREIGN KEY (LotteryId) REFERENCES T_Lottery_v1(Id) ON DELETE CASCADE,
	FOREIGN KEY (ItemId) REFERENCES T_ItemList(Id) ON DELETE CASCADE
);

--抽奖记录表（用于冷却时间）
CREATE TABLE IF NOT EXISTS T_LotteryRecord_v1(
	PlayerId TEXT NOT NULL,							--玩家Id
	LotteryId INTEGER NOT NULL,						--抽奖池Id
	PlayerName TEXT,								--玩家名称
	CreatedAt TEXT NOT NULL,						--创建日期
	LastDrawAt TEXT,								--上次抽奖时间
	PRIMARY KEY (PlayerId, LotteryId),
	FOREIGN KEY (LotteryId) REFERENCES T_Lottery_v1(Id) ON DELETE CASCADE
);

--抽奖命令关联表（命令奖品，带权重）
CREATE TABLE IF NOT EXISTS T_LotteryCommand_v1(
	LotteryId INTEGER NOT NULL,						--抽奖池Id
	CommandId INTEGER NOT NULL,						--命令Id
	Weight INTEGER NOT NULL DEFAULT 1,				--权重(概率)
	PRIMARY KEY (LotteryId, CommandId),
	FOREIGN KEY (LotteryId) REFERENCES T_Lottery_v1(Id) ON DELETE CASCADE,
	FOREIGN KEY (CommandId) REFERENCES T_CommandList(Id) ON DELETE CASCADE
);
