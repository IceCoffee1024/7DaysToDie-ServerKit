--Colored chat
CREATE TABLE IF NOT EXISTS T_ColoredChat(
	Id TEXT NOT NULL PRIMARY KEY,					--Player Id
	CreatedAt TEXT NOT NULL,						--Created At
	CustomName TEXT,								--Custom Name
	NameColor TEXT,									--Name Color
	TextColor TEXT,									--Text Color
	Description TEXT								--Description
);
