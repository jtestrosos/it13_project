SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

USE db34512;
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Medicines]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE Medicines ADD IsDeleted BIT NOT NULL DEFAULT 0;
END
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Suppliers]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE Suppliers ADD IsDeleted BIT NOT NULL DEFAULT 0;
END
GO
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[Sales]') AND name = 'IsDeleted')
BEGIN
    ALTER TABLE Sales ADD IsDeleted BIT NOT NULL DEFAULT 0;
END
GO
