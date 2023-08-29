CREATE TABLE [dbo].[UserGroup]
(
	[UserGroupId] INT NOT NULL PRIMARY KEY,
	[Code] VARCHAR(10) NOT NULL UNIQUE,
	[Description] VARCHAR(1000) NOT NULL
);

INSERT INTO [dbo].[UserGroup] VALUES 
(1, 'Admin', 'Администратор'),
(2, 'User', 'Пользователь');