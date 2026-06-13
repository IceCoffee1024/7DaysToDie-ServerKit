--Task Schedule
CREATE TABLE IF NOT EXISTS T_TaskSchedule(
	Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,	--Id
	CreatedAt TEXT NOT NULL,						--Created At
	Name TEXT NOT NULL UNIQUE,						--Name
	CronExpression TEXT NOT NULL,					--Cron Expression	
	IsEnabled INTEGER NOT NULL,					    --Is Enabled
	LastRunAt TEXT,									--Last Run At
	Description TEXT								--Description
);

PRAGMA FOREIGN_KEYS = ON;

CREATE TABLE IF NOT EXISTS T_TaskScheduleCommand(
	TaskScheduleId INTEGER NOT NULL,				
	CommandId INTEGER NOT NULL,						
	PRIMARY KEY (TaskScheduleId, CommandId),
	FOREIGN KEY (TaskScheduleId) REFERENCES T_TaskSchedule(Id) ON DELETE CASCADE,
	FOREIGN KEY (CommandId) REFERENCES T_CommandList(Id) ON DELETE CASCADE
);