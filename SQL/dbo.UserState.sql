CREATE TABLE [dbo].[UserState]
(
	[UserStateId] INT NOT NULL PRIMARY KEY,
	[Code] VARCHAR(10) NOT NULL UNIQUE,
	[Description] VARCHAR(500) NOT NULL
);

INSERT INTO [dbo].[UserState] VALUES 
(1, 'Active', 'Активен'),
(2, 'Blocked', 'Удален/Заблокирован');