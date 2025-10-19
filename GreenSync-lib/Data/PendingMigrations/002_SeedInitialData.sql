/*
=====================================================================================================
GreenSync Database Seed Data
=====================================================================================================
Migration: 002_SeedInitialData.sql
Created: 2025-10-19
Purpose: Insert initial seed data with realistic Jamaica coordinates and test users
Target: Azure SQL Server
=====================================================================================================
*/

BEGIN TRANSACTION;

-- =====================================================================================================
-- SEED USERS (Sample users for testing - Jamaica-based)
-- =====================================================================================================

-- Insert sample users
DECLARE @AdminUserId UNIQUEIDENTIFIER = 'F1A2B3C4-D5E6-7890-ABCD-EF1234567890';
DECLARE @DriverUserId1 UNIQUEIDENTIFIER = 'F2A2B3C4-D5E6-7890-ABCD-EF1234567891';
DECLARE @DriverUserId2 UNIQUEIDENTIFIER = 'F3A2B3C4-D5E6-7890-ABCD-EF1234567892';
DECLARE @ResidentUserId1 UNIQUEIDENTIFIER = 'F4A2B3C4-D5E6-7890-ABCD-EF1234567893';
DECLARE @ResidentUserId2 UNIQUEIDENTIFIER = 'F5A2B3C4-D5E6-7890-ABCD-EF1234567894';
DECLARE @CommercialUserId1 UNIQUEIDENTIFIER = 'F6A2B3C4-D5E6-7890-ABCD-EF1234567895';

INSERT INTO [dbo].[Users] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [FirstName], [LastName], [AccountType], [Address], [DefaultLatitude], [DefaultLongitude], [EmailConfirmed], [PhoneNumber], [IsActive], [CreatedAt])
VALUES 
    (@AdminUserId, 'admin@greensync.jm', 'ADMIN@GREENSYNC.JM', 'admin@greensync.jm', 'ADMIN@GREENSYNC.JM', 'System', 'Administrator', 2, 'Ministry of Housing, Kingston, Jamaica', 17.997, -76.794, 1, '+1876-555-0001', 1, GETUTCDATE()),
    
    (@DriverUserId1, 'marcus.brown@greensync.jm', 'MARCUS.BROWN@GREENSYNC.JM', 'marcus.brown@greensync.jm', 'MARCUS.BROWN@GREENSYNC.JM', 'Marcus', 'Brown', 0, 'Spanish Town, St. Catherine, Jamaica', 17.991, -76.957, 1, '+1876-555-0101', 1, GETUTCDATE()),
    
    (@DriverUserId2, 'keisha.campbell@greensync.jm', 'KEISHA.CAMPBELL@GREENSYNC.JM', 'keisha.campbell@greensync.jm', 'KEISHA.CAMPBELL@GREENSYNC.JM', 'Keisha', 'Campbell', 0, 'Portmore, St. Catherine, Jamaica', 17.940, -76.883, 1, '+1876-555-0102', 1, GETUTCDATE()),
    
    (@ResidentUserId1, 'devon.williams@gmail.com', 'DEVON.WILLIAMS@GMAIL.COM', 'devon.williams@gmail.com', 'DEVON.WILLIAMS@GMAIL.COM', 'Devon', 'Williams', 0, 'New Kingston, Jamaica', 18.019, -76.782, 1, '+1876-555-0201', 1, GETUTCDATE()),
    
    (@ResidentUserId2, 'sophia.johnson@yahoo.com', 'SOPHIA.JOHNSON@YAHOO.COM', 'sophia.johnson@yahoo.com', 'SOPHIA.JOHNSON@YAHOO.COM', 'Sophia', 'Johnson', 0, 'Half Way Tree, Jamaica', 18.013, -76.794, 1, '+1876-555-0202', 1, GETUTCDATE()),
    
    (@CommercialUserId1, 'manager@jamaicaplaza.jm', 'MANAGER@JAMAICAPLAZA.JM', 'manager@jamaicaplaza.jm', 'MANAGER@JAMAICAPLAZA.JM', 'Patricia', 'Reid', 1, 'Constant Spring Road, Kingston, Jamaica', 18.050, -76.780, 1, '+1876-555-0301', 1, GETUTCDATE());

-- Assign users to roles
INSERT INTO [dbo].[UserRoles] ([UserId], [RoleId])
VALUES 
    (@AdminUserId, 'A1B2C3D4-E5F6-7890-ABCD-EF1234567890'), -- Administrator role
    (@DriverUserId1, 'C3D4E5F6-A7B8-9012-CDEF-345678901234'), -- Driver role
    (@DriverUserId2, 'C3D4E5F6-A7B8-9012-CDEF-345678901234'), -- Driver role
    (@ResidentUserId1, 'B2C3D4E5-F6A7-8901-BCDE-F23456789012'), -- User role
    (@ResidentUserId2, 'B2C3D4E5-F6A7-8901-BCDE-F23456789012'), -- User role
    (@CommercialUserId1, 'B2C3D4E5-F6A7-8901-BCDE-F23456789012'); -- User role

-- =====================================================================================================
-- SEED FLEET VEHICLES (Jamaica-appropriate waste collection trucks)
-- =====================================================================================================

DECLARE @Vehicle1Id UNIQUEIDENTIFIER = 'A1A2B3C4-D5E6-7890-ABCD-EF1234567890';
DECLARE @Vehicle2Id UNIQUEIDENTIFIER = 'A2A2B3C4-D5E6-7890-ABCD-EF1234567891';
DECLARE @Vehicle3Id UNIQUEIDENTIFIER = 'A3A2B3C4-D5E6-7890-ABCD-EF1234567892';

INSERT INTO [dbo].[FleetVehicles] ([Id], [LicensePlate], [Make], [Model], [Year], [VIN], [Status], [Capacity], [FuelLevel], [Mileage], [CurrentLatitude], [CurrentLongitude], [LastGPSUpdate], [AssignedDriverId], [CreatedAt])
VALUES 
    (@Vehicle1Id, 'GS001JA', 'Isuzu', 'NPR Eco-Max', 2022, '1FDGF5GT8NEA12345', 0, 15.50, 85, 25400, 17.997, -76.794, GETUTCDATE(), @DriverUserId1, GETUTCDATE()),
    (@Vehicle2Id, 'GS002JA', 'Mitsubishi', 'Fuso Canter', 2021, '1FDGF5GT8NEA12346', 0, 12.75, 72, 31200, 17.991, -76.957, GETUTCDATE(), @DriverUserId2, GETUTCDATE()),
    (@Vehicle3Id, 'GS003JA', 'Hino', '300 Series', 2023, '1FDGF5GT8NEA12347', 0, 18.00, 91, 18750, 17.940, -76.883, GETUTCDATE(), NULL, GETUTCDATE());

-- =====================================================================================================
-- SEED ECO-CREDIT ACCOUNTS
-- =====================================================================================================

INSERT INTO [dbo].[EcoCredits] ([Id], [UserId], [CurrentBalance], [TotalEarned], [TotalRedeemed], [CreatedAt], [LastUpdated])
VALUES 
    (NEWID(), @ResidentUserId1, 125.50, 125.50, 0, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), @ResidentUserId2, 89.25, 89.25, 0, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), @CommercialUserId1, 450.75, 450.75, 0, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), @DriverUserId1, 75.00, 75.00, 0, GETUTCDATE(), GETUTCDATE()),
    (NEWID(), @DriverUserId2, 62.50, 62.50, 0, GETUTCDATE(), GETUTCDATE());

-- =====================================================================================================
-- SEED WASTE REPORTS (Realistic Jamaica locations)
-- =====================================================================================================

INSERT INTO [dbo].[Reports] ([Id], [Location], [Latitude], [Longitude], [Status], [Description], [Timestamp], [UserId], [Priority], [EstimatedVolume], [WasteType], [CreditsEarned])
VALUES 
    -- Kingston area reports
    (NEWID(), 'Half Way Tree Square, Kingston', 18.013, -76.794, 0, 'Large pile of general waste near bus stop', DATEADD(hour, -2, GETUTCDATE()), @ResidentUserId1, 2, 5.25, 0, 15.50),
    (NEWID(), 'New Kingston Plaza, Kingston', 18.019, -76.782, 0, 'Commercial waste overflow behind shopping center', DATEADD(hour, -1, GETUTCDATE()), @CommercialUserId1, 3, 12.75, 0, 30.50),
    (NEWID(), 'Hope Pastures, Kingston 6', 18.045, -76.774, 1, 'Recyclable materials separated for collection', DATEADD(day, -1, GETUTCDATE()), @ResidentUserId2, 1, 3.50, 1, 17.00),
    
    -- Spanish Town area reports  
    (NEWID(), 'Spanish Town Hospital Road', 17.988, -76.960, 0, 'Medical waste requiring special handling', DATEADD(hour, -3, GETUTCDATE()), @ResidentUserId1, 3, 2.25, 3, 24.50),
    (NEWID(), 'Emancipation Square, Spanish Town', 17.990, -76.954, 0, 'Event cleanup required after market day', DATEADD(day, -1, GETUTCDATE()), @AdminUserId, 2, 8.75, 0, 22.50),
    
    -- Portmore area reports
    (NEWID(), 'Portmore Mall, St. Catherine', 17.943, -76.878, 0, 'Organic waste from food court', DATEADD(hour, -4, GETUTCDATE()), @CommercialUserId1, 1, 6.25, 2, 20.50),
    (NEWID(), 'Independence City, Portmore', 17.937, -76.888, 2, 'Electronic waste collection point', DATEADD(day, -2, GETUTCDATE()), @ResidentUserId2, 2, 1.75, 4, 18.50),
    
    -- Old Harbour area reports
    (NEWID(), 'Old Harbour Main Street', 17.939, -77.108, 0, 'Bulky items left on roadside', DATEADD(hour, -6, GETUTCDATE()), @ResidentUserId1, 2, 15.50, 5, 33.00),
    (NEWID(), 'Old Harbour Bay Beach', 17.925, -77.112, 1, 'Beach cleanup initiative waste', DATEADD(day, -1, GETUTCDATE()), @AdminUserId, 1, 4.25, 0, 13.50),
    
    -- May Pen area reports
    (NEWID(), 'May Pen Market Square', 17.964, -77.245, 0, 'Market waste accumulation', DATEADD(hour, -5, GETUTCDATE()), @ResidentUserId2, 2, 7.75, 2, 23.50),
    
    -- Mandeville area reports
    (NEWID(), 'Manchester High School, Mandeville', 18.044, -77.502, 0, 'School ground cleanup needed', DATEADD(day, -1, GETUTCDATE()), @CommercialUserId1, 1, 3.75, 0, 12.50),
    
    -- Ocho Rios area reports
    (NEWID(), 'Ocho Rios Craft Market', 18.408, -77.103, 3, 'Tourist area requires immediate attention', DATEADD(hour, -1, GETUTCDATE()), @AdminUserId, 3, 6.50, 0, 33.00),
    
    -- Port Antonio area reports
    (NEWID(), 'Port Antonio Marina', 18.179, -76.451, 0, 'Marina waste management needed', DATEADD(hour, -7, GETUTCDATE()), @ResidentUserId1, 2, 4.25, 0, 16.50),
    
    -- Montego Bay area reports
    (NEWID(), 'Hip Strip, Montego Bay', 18.470, -77.919, 1, 'Tourist district waste collection', DATEADD(day, -2, GETUTCDATE()), @CommercialUserId1, 2, 9.25, 0, 24.50),
    (NEWID(), 'Sam Sharpe Square, Montego Bay', 18.475, -77.924, 0, 'Public square general cleanup', DATEADD(hour, -8, GETUTCDATE()), @ResidentUserId2, 1, 2.75, 0, 10.50);

-- =====================================================================================================
-- SEED MAINTENANCE RECORDS
-- =====================================================================================================

INSERT INTO [dbo].[MaintenanceRecords] ([Id], [VehicleId], [Type], [Description], [Cost], [MaintenanceDate], [PerformedBy], [MileageAtMaintenance], [CreatedAt])
VALUES 
    (NEWID(), @Vehicle1Id, 1, 'Regular oil change and filter replacement', 125.00, DATEADD(day, -15, GETUTCDATE()), 'Kingston Auto Service', 25000, DATEADD(day, -15, GETUTCDATE())),
    (NEWID(), @Vehicle2Id, 0, 'Routine inspection and brake check', 85.50, DATEADD(day, -10, GETUTCDATE()), 'Spanish Town Motors', 30800, DATEADD(day, -10, GETUTCDATE())),
    (NEWID(), @Vehicle3Id, 1, 'Preventive hydraulic system service', 275.75, DATEADD(day, -5, GETUTCDATE()), 'Portmore Heavy Equipment', 18500, DATEADD(day, -5, GETUTCDATE()));

-- Update vehicle next maintenance dates
UPDATE [dbo].[FleetVehicles] SET [NextMaintenanceDate] = DATEADD(day, 75, GETUTCDATE()) WHERE [Id] = @Vehicle1Id;
UPDATE [dbo].[FleetVehicles] SET [NextMaintenanceDate] = DATEADD(day, 85, GETUTCDATE()) WHERE [Id] = @Vehicle2Id;
UPDATE [dbo].[FleetVehicles] SET [NextMaintenanceDate] = DATEADD(day, 90, GETUTCDATE()) WHERE [Id] = @Vehicle3Id;

-- =====================================================================================================
-- SEED SAMPLE ROUTES (Optimized Jamaica routes)
-- =====================================================================================================

DECLARE @Route1Id UNIQUEIDENTIFIER = 'E1A2B3C4-D5E6-7890-ABCD-EF1234567890';
DECLARE @Route2Id UNIQUEIDENTIFIER = 'E2A2B3C4-D5E6-7890-ABCD-EF1234567891';

INSERT INTO [dbo].[Routes] ([Id], [Name], [Duration], [FuelSavingsMetric], [TotalDistance], [EstimatedFuelCost], [NumberOfStops], [AssignedVehicleId], [DriverId], [Status], [OptimizationAlgorithm], [EfficiencyScore], [CreatedAt])
VALUES 
    (@Route1Id, 'Kingston Metro Route A', 180, 22.5, 35.75, 42.50, 6, @Vehicle1Id, @DriverUserId1, 2, 'AI-Optimized Distance Minimization', 87.5, GETUTCDATE()),
    (@Route2Id, 'Spanish Town - Portmore Circuit', 145, 18.3, 28.25, 35.75, 4, @Vehicle2Id, @DriverUserId2, 1, 'Priority-Weighted Nearest Neighbor', 82.3, GETUTCDATE());

-- =====================================================================================================
-- SEED ROUTE WAYPOINTS
-- =====================================================================================================

INSERT INTO [dbo].[RouteWaypoints] ([Id], [RouteId], [Latitude], [Longitude], [Address], [StopOrder], [EstimatedArrivalMinutes], [IsCompleted])
VALUES 
    -- Route 1 waypoints (Kingston Metro)
    (NEWID(), @Route1Id, 18.013, -76.794, 'Half Way Tree Square, Kingston', 1, 0, 1),
    (NEWID(), @Route1Id, 18.019, -76.782, 'New Kingston Plaza, Kingston', 2, 25, 1),
    (NEWID(), @Route1Id, 18.045, -76.774, 'Hope Pastures, Kingston 6', 3, 45, 0),
    (NEWID(), @Route1Id, 18.050, -76.780, 'Constant Spring Road, Kingston', 4, 65, 0),
    (NEWID(), @Route1Id, 18.025, -76.785, 'Liguanea, Kingston', 5, 85, 0),
    (NEWID(), @Route1Id, 17.997, -76.794, 'Downtown Kingston', 6, 110, 0),
    
    -- Route 2 waypoints (Spanish Town - Portmore)
    (NEWID(), @Route2Id, 17.991, -76.957, 'Spanish Town Central', 1, 0, 0),
    (NEWID(), @Route2Id, 17.988, -76.960, 'Spanish Town Hospital Road', 2, 30, 0),
    (NEWID(), @Route2Id, 17.943, -76.878, 'Portmore Mall, St. Catherine', 3, 65, 0),
    (NEWID(), @Route2Id, 17.937, -76.888, 'Independence City, Portmore', 4, 90, 0);

-- =====================================================================================================
-- SEED ECO-CREDIT TRANSACTIONS
-- =====================================================================================================

DECLARE @EcoCreditId1 UNIQUEIDENTIFIER = (SELECT Id FROM [dbo].[EcoCredits] WHERE [UserId] = @ResidentUserId1);
DECLARE @EcoCreditId2 UNIQUEIDENTIFIER = (SELECT Id FROM [dbo].[EcoCredits] WHERE [UserId] = @CommercialUserId1);

INSERT INTO [dbo].[EcoCreditTransactions] ([Id], [UserId], [EcoCreditId], [Amount], [Type], [Description], [TransactionDate], [ReferenceNumber], [BalanceAfter])
VALUES 
    (NEWID(), @ResidentUserId1, @EcoCreditId1, 15.50, 0, 'Credits earned from waste report at Half Way Tree', DATEADD(hour, -2, GETUTCDATE()), 'ECT20251019120001', 15.50),
    (NEWID(), @ResidentUserId1, @EcoCreditId1, 25.00, 2, 'Welcome bonus for new eco-friendly citizen', DATEADD(day, -7, GETUTCDATE()), 'ECT20251012100001', 40.50),
    (NEWID(), @ResidentUserId1, @EcoCreditId1, 85.00, 0, 'Monthly contribution bonus', DATEADD(day, -3, GETUTCDATE()), 'ECT20251016140001', 125.50),
    
    (NEWID(), @CommercialUserId1, @EcoCreditId2, 30.50, 0, 'Commercial waste report - New Kingston Plaza', DATEADD(hour, -1, GETUTCDATE()), 'ECT20251019130001', 30.50),
    (NEWID(), @CommercialUserId1, @EcoCreditId2, 200.00, 2, 'Commercial partnership bonus', DATEADD(day, -5, GETUTCDATE()), 'ECT20251014110001', 230.50),
    (NEWID(), @CommercialUserId1, @EcoCreditId2, 220.25, 0, 'Bulk recycling contribution', DATEADD(day, -2, GETUTCDATE()), 'ECT20251017160001', 450.75);

GO

COMMIT TRANSACTION;

PRINT 'GreenSync seed data inserted successfully with Jamaica coordinates!';
PRINT 'Sample users created:';
PRINT '- Admin: admin@greensync.jm';
PRINT '- Drivers: marcus.brown@greensync.jm, keisha.campbell@greensync.jm';
PRINT '- Residents: devon.williams@gmail.com, sophia.johnson@yahoo.com';
PRINT '- Commercial: manager@jamaicaplaza.jm';
PRINT '';
PRINT 'Sample data includes:';
PRINT '- 3 Fleet vehicles with Jamaica license plates';
PRINT '- 15 Waste reports across major Jamaica locations';
PRINT '- 2 Optimized routes covering Kingston and Spanish Town/Portmore';
PRINT '- Eco-credit accounts and transactions';
PRINT '- Vehicle maintenance records';
