/*
=====================================================================================================
GreenSync Database Initial Schema Creation
=====================================================================================================
Migration: 001_CreateInitialTables.sql
Created: 2025-10-19
Purpose: Create initial database schema with all core tables, indexes, and constraints
Target: Azure SQL Server
=====================================================================================================
*/

-- Enable snapshot isolation for better concurrency
ALTER DATABASE CURRENT SET ALLOW_SNAPSHOT_ISOLATION ON;
ALTER DATABASE CURRENT SET READ_COMMITTED_SNAPSHOT ON;

BEGIN TRANSACTION;

GO

-- =====================================================================================================
-- IDENTITY TABLES (ASP.NET Core Identity)
-- =====================================================================================================

-- Roles table
CREATE TABLE [dbo].[Roles] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(256) NULL,
    [NormalizedName] NVARCHAR(256) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL,
    CONSTRAINT [IX_Roles_NormalizedName] UNIQUE ([NormalizedName])
);

-- Users table (extends Identity)
CREATE TABLE [dbo].[Users] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserName] NVARCHAR(256) NULL,
    [NormalizedUserName] NVARCHAR(256) NULL,
    [Email] NVARCHAR(256) NULL,
    [NormalizedEmail] NVARCHAR(256) NULL,
    [EmailConfirmed] BIT NOT NULL DEFAULT 0,
    [PasswordHash] NVARCHAR(MAX) NULL,
    [SecurityStamp] NVARCHAR(MAX) NULL,
    [ConcurrencyStamp] NVARCHAR(MAX) NULL,
    [PhoneNumber] NVARCHAR(MAX) NULL,
    [PhoneNumberConfirmed] BIT NOT NULL DEFAULT 0,
    [TwoFactorEnabled] BIT NOT NULL DEFAULT 0,
    [LockoutEnd] DATETIMEOFFSET NULL,
    [LockoutEnabled] BIT NOT NULL DEFAULT 1,
    [AccessFailedCount] INT NOT NULL DEFAULT 0,
    
    -- Custom fields
    [FirstName] NVARCHAR(100) NOT NULL,
    [LastName] NVARCHAR(100) NOT NULL,
    [Address] NVARCHAR(500) NULL,
    [DefaultLatitude] FLOAT NULL,
    [DefaultLongitude] FLOAT NULL,
    [AccountType] INT NOT NULL DEFAULT 0, -- 0=Resident, 1=Commercial, 2=Municipal
    [IsActive] BIT NOT NULL DEFAULT 1,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2(7) NULL,
    
    CONSTRAINT [IX_Users_NormalizedUserName] UNIQUE ([NormalizedUserName]),
    CONSTRAINT [IX_Users_NormalizedEmail] UNIQUE ([NormalizedEmail])
);

-- User Roles junction table
CREATE TABLE [dbo].[UserRoles] (
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_UserRoles_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id]) ON DELETE CASCADE
);

-- User Claims table
CREATE TABLE [dbo].[UserClaims] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [ClaimType] NVARCHAR(MAX) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,
    
    CONSTRAINT [FK_UserClaims_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);

-- User Logins table
CREATE TABLE [dbo].[UserLogins] (
    [LoginProvider] NVARCHAR(450) NOT NULL,
    [ProviderKey] NVARCHAR(450) NOT NULL,
    [ProviderDisplayName] NVARCHAR(MAX) NULL,
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    
    CONSTRAINT [PK_UserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_UserLogins_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);

-- User Tokens table
CREATE TABLE [dbo].[UserTokens] (
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [LoginProvider] NVARCHAR(450) NOT NULL,
    [Name] NVARCHAR(450) NOT NULL,
    [Value] NVARCHAR(MAX) NULL,
    
    CONSTRAINT [PK_UserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_UserTokens_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE
);

-- Role Claims table
CREATE TABLE [dbo].[RoleClaims] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [RoleId] UNIQUEIDENTIFIER NOT NULL,
    [ClaimType] NVARCHAR(MAX) NULL,
    [ClaimValue] NVARCHAR(MAX) NULL,
    
    CONSTRAINT [FK_RoleClaims_Roles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id]) ON DELETE CASCADE
);

GO

-- =====================================================================================================
-- FLEET MANAGEMENT TABLES
-- =====================================================================================================

-- Fleet Vehicles table
CREATE TABLE [dbo].[FleetVehicles] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [LicensePlate] NVARCHAR(20) NOT NULL,
    [Make] NVARCHAR(100) NOT NULL,
    [Model] NVARCHAR(100) NOT NULL,
    [Year] INT NOT NULL,
    [VIN] NVARCHAR(17) NULL,
    [Status] INT NOT NULL DEFAULT 0, -- 0=Available, 1=InUse, 2=Maintenance, 3=OutOfService, 4=Refueling
    [Capacity] DECIMAL(6,2) NOT NULL,
    [FuelLevel] INT NOT NULL DEFAULT 100,
    [Mileage] INT NOT NULL DEFAULT 0,
    [LastMaintenanceDate] DATE NULL,
    [NextMaintenanceDate] DATE NULL,
    [CurrentLatitude] FLOAT NULL,
    [CurrentLongitude] FLOAT NULL,
    [LastGPSUpdate] DATETIME2(7) NULL,
    [AssignedDriverId] UNIQUEIDENTIFIER NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2(7) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2(7) NULL,
    
    CONSTRAINT [FK_FleetVehicles_Users_AssignedDriverId] FOREIGN KEY ([AssignedDriverId]) REFERENCES [Users]([Id]) ON DELETE SET NULL,
    CONSTRAINT [IX_FleetVehicles_LicensePlate] UNIQUE ([LicensePlate]),
    CONSTRAINT [CK_FleetVehicles_Year] CHECK ([Year] BETWEEN 1900 AND 2100),
    CONSTRAINT [CK_FleetVehicles_Capacity] CHECK ([Capacity] BETWEEN 0.1 AND 100.0),
    CONSTRAINT [CK_FleetVehicles_FuelLevel] CHECK ([FuelLevel] BETWEEN 0 AND 100)
);

-- Maintenance Records table
CREATE TABLE [dbo].[MaintenanceRecords] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [VehicleId] UNIQUEIDENTIFIER NOT NULL,
    [Type] INT NOT NULL DEFAULT 0, -- 0=Routine, 1=Preventive, 2=Repair, 3=Emergency, etc.
    [Description] NVARCHAR(1000) NOT NULL,
    [Cost] DECIMAL(10,2) NULL,
    [MaintenanceDate] DATE NOT NULL DEFAULT CAST(GETUTCDATE() AS DATE),
    [PerformedBy] NVARCHAR(200) NULL,
    [MileageAtMaintenance] INT NULL,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    
    CONSTRAINT [FK_MaintenanceRecords_FleetVehicles_VehicleId] FOREIGN KEY ([VehicleId]) REFERENCES [FleetVehicles]([Id]) ON DELETE CASCADE
);

GO

-- =====================================================================================================
-- WASTE REPORTING TABLES
-- =====================================================================================================

-- Reports table
CREATE TABLE [dbo].[Reports] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Location] NVARCHAR(500) NOT NULL,
    [Latitude] FLOAT NOT NULL,
    [Longitude] FLOAT NOT NULL,
    [Status] INT NOT NULL DEFAULT 0, -- 0=Reported, 1=Assigned, 2=InProgress, 3=Collected, 4=Cancelled
    [Description] NVARCHAR(1000) NOT NULL DEFAULT '',
    [ImageUrl] NVARCHAR(500) NULL,
    [Timestamp] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [CollectedAt] DATETIME2(7) NULL,
    [AssignedVehicleId] UNIQUEIDENTIFIER NULL,
    [AssignedRouteId] UNIQUEIDENTIFIER NULL,
    [Priority] INT NOT NULL DEFAULT 1, -- 0=Low, 1=Medium, 2=High, 3=Critical
    [EstimatedVolume] DECIMAL(6,2) NOT NULL DEFAULT 0,
    [WasteType] INT NOT NULL DEFAULT 0, -- 0=General, 1=Recyclable, 2=Organic, 3=Hazardous, 4=Electronic, 5=Bulky
    [UpdatedAt] DATETIME2(7) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2(7) NULL,
    [CreditsEarned] DECIMAL(10,2) NOT NULL DEFAULT 0,
    
    CONSTRAINT [FK_Reports_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Reports_FleetVehicles_AssignedVehicleId] FOREIGN KEY ([AssignedVehicleId]) REFERENCES [FleetVehicles]([Id]) ON DELETE SET NULL
);

GO

-- =====================================================================================================
-- ECO-CREDITS TABLES
-- =====================================================================================================

-- EcoCredits table (one per user)
CREATE TABLE [dbo].[EcoCredits] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [CurrentBalance] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [TotalEarned] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [TotalRedeemed] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [LastUpdated] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2(7) NULL,
    
    CONSTRAINT [FK_EcoCredits_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [IX_EcoCredits_UserId] UNIQUE ([UserId])
);

-- EcoCredit Transactions table
CREATE TABLE [dbo].[EcoCreditTransactions] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [UserId] UNIQUEIDENTIFIER NOT NULL,
    [EcoCreditId] UNIQUEIDENTIFIER NOT NULL,
    [Amount] DECIMAL(10,2) NOT NULL,
    [Type] INT NOT NULL DEFAULT 0, -- 0=Earned, 1=Redeemed, 2=Bonus, 3=Penalty, 4=Adjustment
    [Description] NVARCHAR(500) NOT NULL,
    [TransactionDate] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [RelatedReportId] UNIQUEIDENTIFIER NULL,
    [ReferenceNumber] NVARCHAR(50) NULL,
    [BalanceAfter] DECIMAL(10,2) NOT NULL,
    
    CONSTRAINT [FK_EcoCreditTransactions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EcoCreditTransactions_EcoCredits_EcoCreditId] FOREIGN KEY ([EcoCreditId]) REFERENCES [EcoCredits]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_EcoCreditTransactions_Reports_RelatedReportId] FOREIGN KEY ([RelatedReportId]) REFERENCES [Reports]([Id]) ON DELETE SET NULL
);

GO

-- =====================================================================================================
-- ROUTE OPTIMIZATION TABLES
-- =====================================================================================================

-- Routes table
CREATE TABLE [dbo].[Routes] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [Name] NVARCHAR(200) NOT NULL,
    [Duration] INT NOT NULL DEFAULT 0,
    [FuelSavingsMetric] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [TotalDistance] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [EstimatedFuelCost] DECIMAL(10,2) NOT NULL DEFAULT 0,
    [NumberOfStops] INT NOT NULL DEFAULT 0,
    [AssignedVehicleId] UNIQUEIDENTIFIER NULL,
    [DriverId] UNIQUEIDENTIFIER NULL,
    [Status] INT NOT NULL DEFAULT 0, -- 0=Planned, 1=Assigned, 2=InProgress, 3=Completed, 4=Cancelled
    [CreatedAt] DATETIME2(7) NOT NULL DEFAULT GETUTCDATE(),
    [StartedAt] DATETIME2(7) NULL,
    [CompletedAt] DATETIME2(7) NULL,
    [OptimizationAlgorithm] NVARCHAR(100) NOT NULL DEFAULT 'Distance-based',
    [EfficiencyScore] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [UpdatedAt] DATETIME2(7) NULL,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    [DeletedAt] DATETIME2(7) NULL,
    
    CONSTRAINT [FK_Routes_FleetVehicles_AssignedVehicleId] FOREIGN KEY ([AssignedVehicleId]) REFERENCES [FleetVehicles]([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_Routes_Users_DriverId] FOREIGN KEY ([DriverId]) REFERENCES [Users]([Id]) ON DELETE SET NULL
);

-- Route Waypoints table
CREATE TABLE [dbo].[RouteWaypoints] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY DEFAULT NEWID(),
    [RouteId] UNIQUEIDENTIFIER NOT NULL,
    [Latitude] FLOAT NOT NULL,
    [Longitude] FLOAT NOT NULL,
    [Address] NVARCHAR(500) NULL,
    [StopOrder] INT NOT NULL,
    [EstimatedArrivalMinutes] INT NULL,
    [ReportId] UNIQUEIDENTIFIER NULL,
    [ActualArrival] DATETIME2(7) NULL,
    [IsCompleted] BIT NOT NULL DEFAULT 0,
    [CompletedAt] DATETIME2(7) NULL,
    
    CONSTRAINT [FK_RouteWaypoints_Routes_RouteId] FOREIGN KEY ([RouteId]) REFERENCES [Routes]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_RouteWaypoints_Reports_ReportId] FOREIGN KEY ([ReportId]) REFERENCES [Reports]([Id]) ON DELETE SET NULL,
    CONSTRAINT [IX_RouteWaypoints_RouteId_StopOrder] UNIQUE ([RouteId], [StopOrder])
);

GO

-- Add the foreign key constraint for Reports.AssignedRouteId now that Routes table exists
ALTER TABLE [dbo].[Reports] 
ADD CONSTRAINT [FK_Reports_Routes_AssignedRouteId] FOREIGN KEY ([AssignedRouteId]) REFERENCES [Routes]([Id]) ON DELETE SET NULL;

GO

-- =====================================================================================================
-- CREATE INDEXES FOR PERFORMANCE
-- =====================================================================================================

-- Users table indexes
CREATE INDEX [IX_Users_IsDeleted] ON [dbo].[Users] ([IsDeleted]);
CREATE INDEX [IX_Users_AccountType] ON [dbo].[Users] ([AccountType]);
CREATE INDEX [IX_Users_CreatedAt] ON [dbo].[Users] ([CreatedAt]);

-- FleetVehicles table indexes
CREATE INDEX [IX_FleetVehicles_Status] ON [dbo].[FleetVehicles] ([Status]);
CREATE INDEX [IX_FleetVehicles_IsDeleted] ON [dbo].[FleetVehicles] ([IsDeleted]);
CREATE INDEX [IX_FleetVehicles_AssignedDriverId] ON [dbo].[FleetVehicles] ([AssignedDriverId]);

-- MaintenanceRecords table indexes
CREATE INDEX [IX_MaintenanceRecords_VehicleId] ON [dbo].[MaintenanceRecords] ([VehicleId]);
CREATE INDEX [IX_MaintenanceRecords_MaintenanceDate] ON [dbo].[MaintenanceRecords] ([MaintenanceDate]);
CREATE INDEX [IX_MaintenanceRecords_Type] ON [dbo].[MaintenanceRecords] ([Type]);

-- Reports table indexes
CREATE INDEX [IX_Reports_Status] ON [dbo].[Reports] ([Status]);
CREATE INDEX [IX_Reports_Priority] ON [dbo].[Reports] ([Priority]);
CREATE INDEX [IX_Reports_WasteType] ON [dbo].[Reports] ([WasteType]);
CREATE INDEX [IX_Reports_Timestamp] ON [dbo].[Reports] ([Timestamp]);
CREATE INDEX [IX_Reports_IsDeleted] ON [dbo].[Reports] ([IsDeleted]);
CREATE INDEX [IX_Reports_Latitude_Longitude] ON [dbo].[Reports] ([Latitude], [Longitude]);
CREATE INDEX [IX_Reports_UserId] ON [dbo].[Reports] ([UserId]);
CREATE INDEX [IX_Reports_AssignedVehicleId] ON [dbo].[Reports] ([AssignedVehicleId]);
CREATE INDEX [IX_Reports_AssignedRouteId] ON [dbo].[Reports] ([AssignedRouteId]);

-- EcoCredits table indexes
CREATE INDEX [IX_EcoCredits_IsDeleted] ON [dbo].[EcoCredits] ([IsDeleted]);

-- EcoCreditTransactions table indexes
CREATE INDEX [IX_EcoCreditTransactions_UserId] ON [dbo].[EcoCreditTransactions] ([UserId]);
CREATE INDEX [IX_EcoCreditTransactions_TransactionDate] ON [dbo].[EcoCreditTransactions] ([TransactionDate]);
CREATE INDEX [IX_EcoCreditTransactions_Type] ON [dbo].[EcoCreditTransactions] ([Type]);
CREATE INDEX [IX_EcoCreditTransactions_EcoCreditId] ON [dbo].[EcoCreditTransactions] ([EcoCreditId]);

-- Routes table indexes
CREATE INDEX [IX_Routes_Status] ON [dbo].[Routes] ([Status]);
CREATE INDEX [IX_Routes_CreatedAt] ON [dbo].[Routes] ([CreatedAt]);
CREATE INDEX [IX_Routes_AssignedVehicleId] ON [dbo].[Routes] ([AssignedVehicleId]);
CREATE INDEX [IX_Routes_DriverId] ON [dbo].[Routes] ([DriverId]);
CREATE INDEX [IX_Routes_IsDeleted] ON [dbo].[Routes] ([IsDeleted]);

-- RouteWaypoints table indexes
CREATE INDEX [IX_RouteWaypoints_RouteId] ON [dbo].[RouteWaypoints] ([RouteId]);
CREATE INDEX [IX_RouteWaypoints_ReportId] ON [dbo].[RouteWaypoints] ([ReportId]);
CREATE INDEX [IX_RouteWaypoints_Latitude_Longitude] ON [dbo].[RouteWaypoints] ([Latitude], [Longitude]);

GO

-- =====================================================================================================
-- SEED INITIAL DATA
-- =====================================================================================================

-- Insert default roles
INSERT INTO [dbo].[Roles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
VALUES 
    ('A1B2C3D4-E5F6-7890-ABCD-EF1234567890', 'Administrator', 'ADMINISTRATOR', NEWID()),
    ('B2C3D4E5-F6G7-8901-BCDE-F23456789012', 'User', 'USER', NEWID()),
    ('C3D4E5F6-G7H8-9012-CDEF-345678901234', 'Driver', 'DRIVER', NEWID());

GO

-- =====================================================================================================
-- CREATE VIEWS FOR COMMON QUERIES
-- =====================================================================================================

-- View for active users with role information
CREATE VIEW [dbo].[vw_ActiveUsers] AS
SELECT 
    u.[Id],
    u.[UserName],
    u.[Email],
    u.[FirstName],
    u.[LastName],
    u.[FirstName] + ' ' + u.[LastName] AS [FullName],
    u.[AccountType],
    u.[IsActive],
    u.[CreatedAt],
    STRING_AGG(r.[Name], ', ') AS [Roles]
FROM [dbo].[Users] u
LEFT JOIN [dbo].[UserRoles] ur ON u.[Id] = ur.[UserId]
LEFT JOIN [dbo].[Roles] r ON ur.[RoleId] = r.[Id]
WHERE u.[IsDeleted] = 0 AND u.[IsActive] = 1
GROUP BY u.[Id], u.[UserName], u.[Email], u.[FirstName], u.[LastName], u.[AccountType], u.[IsActive], u.[CreatedAt];

GO

-- View for fleet vehicle status
CREATE VIEW [dbo].[vw_FleetStatus] AS
SELECT 
    fv.[Id],
    fv.[LicensePlate],
    fv.[Make] + ' ' + fv.[Model] + ' (' + CAST(fv.[Year] AS NVARCHAR(4)) + ')' AS [VehicleInfo],
    fv.[Status],
    fv.[FuelLevel],
    fv.[Capacity],
    fv.[CurrentLatitude],
    fv.[CurrentLongitude],
    u.[FirstName] + ' ' + u.[LastName] AS [AssignedDriverName],
    fv.[LastGPSUpdate]
FROM [dbo].[FleetVehicles] fv
LEFT JOIN [dbo].[Users] u ON fv.[AssignedDriverId] = u.[Id]
WHERE fv.[IsDeleted] = 0;

GO

-- View for active reports with user information
CREATE VIEW [dbo].[vw_ActiveReports] AS
SELECT 
    r.[Id],
    r.[Location],
    r.[Latitude],
    r.[Longitude],
    r.[Status],
    r.[Priority],
    r.[WasteType],
    r.[EstimatedVolume],
    r.[Timestamp],
    r.[CollectedAt],
    u.[FirstName] + ' ' + u.[LastName] AS [ReportedByName],
    u.[Email] AS [ReportedByEmail],
    fv.[LicensePlate] AS [AssignedVehicle],
    rt.[Name] AS [AssignedRoute]
FROM [dbo].[Reports] r
INNER JOIN [dbo].[Users] u ON r.[UserId] = u.[Id]
LEFT JOIN [dbo].[FleetVehicles] fv ON r.[AssignedVehicleId] = fv.[Id]
LEFT JOIN [dbo].[Routes] rt ON r.[AssignedRouteId] = rt.[Id]
WHERE r.[IsDeleted] = 0;

GO

-- =====================================================================================================
-- CREATE STORED PROCEDURES FOR COMMON OPERATIONS
-- =====================================================================================================

-- Stored procedure to create a new eco-credit account for a user
CREATE PROCEDURE [dbo].[sp_CreateEcoCreditAccount]
    @UserId UNIQUEIDENTIFIER,
    @InitialBalance DECIMAL(10,2) = 0
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @EcoCreditId UNIQUEIDENTIFIER = NEWID();
    
    -- Create the eco-credit account
    INSERT INTO [dbo].[EcoCredits] ([Id], [UserId], [CurrentBalance], [TotalEarned])
    VALUES (@EcoCreditId, @UserId, @InitialBalance, @InitialBalance);
    
    -- Create initial transaction if there's an initial balance
    IF @InitialBalance > 0
    BEGIN
        INSERT INTO [dbo].[EcoCreditTransactions] 
        ([UserId], [EcoCreditId], [Amount], [Type], [Description], [BalanceAfter])
        VALUES 
        (@UserId, @EcoCreditId, @InitialBalance, 2, 'Initial account setup bonus', @InitialBalance);
    END
    
    SELECT @EcoCreditId AS [EcoCreditId];
END;

GO

-- Stored procedure to add eco-credits to a user account
CREATE PROCEDURE [dbo].[sp_AddEcoCredits]
    @UserId UNIQUEIDENTIFIER,
    @Amount DECIMAL(10,2),
    @Description NVARCHAR(500),
    @RelatedReportId UNIQUEIDENTIFIER = NULL,
    @TransactionType INT = 0 -- 0=Earned
AS
BEGIN
    SET NOCOUNT ON;
    
    BEGIN TRANSACTION;
    
    DECLARE @EcoCreditId UNIQUEIDENTIFIER;
    DECLARE @NewBalance DECIMAL(10,2);
    
    -- Get or create eco-credit account
    SELECT @EcoCreditId = [Id] FROM [dbo].[EcoCredits] WHERE [UserId] = @UserId;
    
    IF @EcoCreditId IS NULL
    BEGIN
        EXEC [dbo].[sp_CreateEcoCreditAccount] @UserId, 0;
        SELECT @EcoCreditId = [Id] FROM [dbo].[EcoCredits] WHERE [UserId] = @UserId;
    END
    
    -- Update the balance
    UPDATE [dbo].[EcoCredits] 
    SET [CurrentBalance] = [CurrentBalance] + @Amount,
        [TotalEarned] = [TotalEarned] + @Amount,
        [LastUpdated] = GETUTCDATE()
    WHERE [Id] = @EcoCreditId;
    
    -- Get the new balance
    SELECT @NewBalance = [CurrentBalance] FROM [dbo].[EcoCredits] WHERE [Id] = @EcoCreditId;
    
    -- Create transaction record
    INSERT INTO [dbo].[EcoCreditTransactions] 
    ([UserId], [EcoCreditId], [Amount], [Type], [Description], [RelatedReportId], [BalanceAfter])
    VALUES 
    (@UserId, @EcoCreditId, @Amount, @TransactionType, @Description, @RelatedReportId, @NewBalance);
    
    COMMIT TRANSACTION;
    
    SELECT @NewBalance AS [NewBalance];
END;

GO

COMMIT TRANSACTION;

PRINT 'GreenSync initial database schema created successfully!';
