# GreenSync AI Knowledge Base

## Database Schema Implementation - October 19, 2025

### Overview
This document contains contextual knowledge about the GreenSync AI-powered waste management platform's database schema implementation. The system has been designed to support a comprehensive waste collection and route optimization platform using .NET 9.0, Entity Framework Core, and Azure SQL Server.

### Architecture Summary
- **Framework**: .NET 9.0 with ASP.NET Core MVC
- **Database**: Azure SQL Server with Entity Framework Core
- **Authentication**: ASP.NET Core Identity with custom user extensions
- **Pattern**: Clean Architecture with Repository patterns
- **Key Features**: Real-time reporting, AI route optimization, eco-credit rewards

### Database Schema Design

#### Core Entity Relationships
```
ApplicationUser (1) ←→ (1) EcoCredit
ApplicationUser (1) ←→ (*) Report
ApplicationUser (1) ←→ (*) Route (as Driver)
ApplicationUser (1) ←→ (*) FleetVehicle (as AssignedDriver)

FleetVehicle (1) ←→ (*) MaintenanceRecord
FleetVehicle (1) ←→ (*) Route
FleetVehicle (1) ←→ (*) Report (assignment)

Route (1) ←→ (*) RouteWaypoint
Route (1) ←→ (*) Report (assignment)

EcoCredit (1) ←→ (*) EcoCreditTransaction
Report (1) ←→ (*) EcoCreditTransaction (optional relation)
```

#### Key Models Created

##### 1. ApplicationUser (extends IdentityUser<Guid>)
- **Purpose**: Extended ASP.NET Core Identity user with GreenSync-specific properties
- **Key Features**: 
  - User types (Resident, Commercial, Driver, Administrator, Supervisor)
  - Default location for geo-services
  - Soft delete support
  - Full audit trail (created, updated, deleted timestamps)
- **Navigation Properties**: Reports, EcoCredit, EcoCreditTransactions, AssignedRoutes, AssignedVehicles

##### 2. Report
- **Purpose**: Waste reports submitted by users
- **Key Features**:
  - GPS coordinates with address
  - Status tracking (Reported → Assigned → InProgress → Collected)
  - Priority levels (Low, Medium, High, Critical)
  - Waste type classification
  - Volume estimation
  - Eco-credits earned integration
- **Database Annotations**: Proper indexes on status, priority, location coordinates

##### 3. FleetVehicle
- **Purpose**: Waste collection vehicles management
- **Key Features**:
  - Complete vehicle specifications (make, model, year, VIN, capacity)
  - Real-time status tracking
  - GPS tracking capabilities
  - Fuel level monitoring
  - Maintenance scheduling
  - Driver assignment
- **Business Rules**: Capacity constraints (0.1-100 m³), fuel level validation (0-100%)

##### 4. Route & RouteWaypoint
- **Purpose**: AI-optimized collection routes
- **Key Features**:
  - Route metadata (duration, distance, fuel savings, efficiency score)
  - Ordered waypoints with arrival estimates
  - Vehicle and driver assignments
  - Performance tracking (actual vs. estimated)
- **Optimization Support**: Designed for machine learning algorithms

##### 5. EcoCredit & EcoCreditTransaction
- **Purpose**: Environmental incentive reward system
- **Key Features**:
  - Balance tracking with precision decimal handling
  - Complete transaction audit trail
  - Multiple transaction types (Earned, Redeemed, Bonus, Penalty, Adjustment)
  - Report integration for earning credits
- **Financial Integrity**: ACID compliance with proper decimal precision

##### 6. MaintenanceRecord
- **Purpose**: Vehicle maintenance tracking
- **Key Features**:
  - Maintenance type classification
  - Cost tracking with decimal precision
  - Mileage-based scheduling
  - Audit trail for compliance

#### Database Design Patterns Applied

##### 1. Soft Delete Pattern
- All major entities implement `IsDeleted` flag with `DeletedAt` timestamp
- Global query filters in DbContext prevent deleted records from normal queries
- Maintains data integrity while allowing "deletion" functionality

##### 2. Audit Trail Pattern
- All entities have `CreatedAt` timestamps
- Most entities include `UpdatedAt` for change tracking
- Supports compliance and troubleshooting requirements

##### 3. GUID Primary Keys
- All entities use GUID (UNIQUEIDENTIFIER) primary keys
- Benefits: Distributed system support, merge-friendly, security
- Generated using `DatabaseGeneratedOption.Identity`

##### 4. Decimal Precision Configuration
- Financial fields: DECIMAL(10,2) for currency amounts
- Measurement fields: DECIMAL(6,2) for volumes, DECIMAL(10,2) for distances
- Percentage fields: DECIMAL(5,2) for efficiency metrics

##### 5. Index Strategy
- Performance indexes on frequently queried fields
- Composite indexes for complex queries (lat/lng, route ordering)
- Unique constraints where appropriate (license plates, email addresses)

#### Entity Framework Configuration Highlights

##### DbContext Features
- **Base**: `IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>`
- **Connection**: Configured for Azure SQL Server
- **Query Filters**: Global soft delete filtering
- **Relationships**: Carefully configured cascade behaviors
- **Seeding**: Initial roles and admin data

##### Relationship Configuration
- **Cascade Delete**: Used sparingly, mainly for dependent entities (maintenance records, waypoints)
- **Restrict**: Used for critical relationships (user-report associations)
- **Set Null**: Used for optional assignments (vehicle-driver, route-vehicle)

##### Performance Optimizations
- Strategic indexing on query-heavy fields
- Composite indexes for geographic queries
- Unique constraints for business rules enforcement
- Proper foreign key relationships for query optimization

#### TSQL Migration Script Features

##### Schema Creation
- Complete Azure SQL Server compatible schema
- Proper constraint definitions with meaningful names
- Check constraints for business rule enforcement
- Optimized for Azure SQL Database tier

##### Views Created
- `vw_ActiveUsers`: User information with roles aggregated
- `vw_FleetStatus`: Real-time vehicle status with driver information
- `vw_ActiveReports`: Report details with user and assignment information

##### Stored Procedures
- `sp_CreateEcoCreditAccount`: Automated eco-credit account creation
- `sp_AddEcoCredits`: Safe credit addition with transaction logging

##### Performance Features
- Strategic indexes for common query patterns
- Geographic indexes for location-based searches
- Temporal indexes for audit and reporting queries

### Implementation Notes

#### Security Considerations
- All user-related operations use GUID identifiers
- Soft delete prevents accidental data loss
- Proper foreign key constraints prevent orphaned records
- Role-based access control through ASP.NET Core Identity

#### Scalability Features
- GUID primary keys support distributed systems
- Indexed queries for performance at scale
- Separate transaction logging for audit requirements
- Partitioning-ready design for future growth

#### Azure SQL Server Optimizations
- DATETIME2(7) for precise timestamps
- UNIQUEIDENTIFIER with NEWID() defaults
- Snapshot isolation configuration for better concurrency
- Modern SQL features (STRING_AGG for role aggregation)

### Integration Points

#### Existing System Integration
- Maintains compatibility with current in-memory services
- Designed to replace mock implementations seamlessly
- Supports existing business logic and validation rules

#### Future Enhancements Ready
- Machine learning model integration points (efficiency scoring)
- Real-time location tracking support
- IoT sensor data integration capability
- Advanced analytics and reporting structure

### Development Guidelines

#### When Adding New Entities
1. Implement audit trail fields (CreatedAt, UpdatedAt, IsDeleted, DeletedAt)
2. Use GUID primary keys with proper generation
3. Add appropriate indexes for expected query patterns
4. Configure relationships with appropriate cascade behaviors
5. Add entity to DbContext with proper configuration

#### When Modifying Existing Entities
1. Create new migration script in PendingMigrations folder
2. Update Entity Framework model accordingly
3. Consider backward compatibility impact
4. Update related views and stored procedures if necessary
5. Test with existing data scenarios

### Validation and Testing Strategy

#### Schema Validation
- All entities properly configured with data annotations
- Foreign key relationships correctly established
- Business rule constraints implemented at database level
- Index strategy validated against expected query patterns

#### Migration Testing
- TSQL script tested for Azure SQL Server compatibility
- Rollback procedures documented
- Performance impact assessed
- Data integrity verification procedures in place

### Entity Framework Service Implementations - October 19, 2025

#### Service Layer Architecture
Complete Entity Framework implementations have been created to replace all in-memory mock services:

##### 1. EfReportService
- **Purpose**: Waste report management with full database persistence
- **Key Features**:
  - CRUD operations with proper relationship loading
  - Geographic queries using Haversine formula for location-based searches
  - Priority-based sorting and status management
  - Automatic eco-credit calculation based on waste type and volume
  - Soft delete implementation with audit trails
- **Performance**: Optimized queries with Include() for related data, strategic use of ToListAsync()

##### 2. EfEcoCreditService
- **Purpose**: Environmental rewards system with transactional integrity
- **Key Features**:
  - Database transaction support for credit operations
  - Complete transaction history with generated reference numbers
  - Balance validation and adjustment capabilities
  - Top users leaderboard functionality
  - System-wide credit tracking for analytics
- **Financial Integrity**: Uses database transactions to ensure ACID compliance

##### 3. EfRouteService
- **Purpose**: AI-powered route optimization with database persistence
- **Key Features**:
  - Nearest neighbor algorithm implementation with priority weighting
  - Route optimization simulation with fuel savings calculations
  - Complete waypoint management with estimated arrival times
  - Vehicle and driver assignment capabilities
  - Performance metrics tracking (efficiency scores, fuel costs)
- **Algorithm**: Priority-weighted nearest neighbor with distance minimization

##### 4. EfFleetVehicleService
- **Purpose**: Complete vehicle fleet management
- **Key Features**:
  - Vehicle lifecycle management with maintenance scheduling
  - Real-time GPS location tracking capabilities
  - Fuel level monitoring and validation
  - Driver assignment and status management
  - Maintenance prediction based on date and mileage patterns
- **Business Logic**: Enforces capacity and fuel level constraints at service level

##### 5. EfAuthService
- **Purpose**: ASP.NET Core Identity integration with backward compatibility
- **Key Features**:
  - Full ASP.NET Core Identity integration with UserManager and SignInManager
  - Backward compatibility with legacy User interface
  - Proper password hashing and validation
  - Role-based authorization with Identity roles
  - Account lockout and security features
  - Claims-based authentication support
- **Security**: Implements proper ASP.NET Core Identity security patterns

#### Service Integration Patterns

##### Dependency Injection Configuration
```csharp
// Both applications configured with:
builder.Services.AddScoped<IReportService, EfReportService>();
builder.Services.AddScoped<IEcoCreditService, EfEcoCreditService>();
builder.Services.AddScoped<IRouteService, EfRouteService>();
builder.Services.AddScoped<IFleetVehicleService, EfFleetVehicleService>();
builder.Services.AddScoped<IAuthService, EfAuthService>();
```

##### Logging Integration
- All services implement structured logging with ILogger<T>
- Error handling with proper exception logging and context
- Performance monitoring capabilities built-in
- Security event logging for authentication operations

##### Transaction Management
- EcoCreditService uses database transactions for financial operations
- Route optimization operations are atomic
- Proper rollback handling for failed operations

#### Jamaica-Specific Seed Data Implementation

##### Comprehensive Test Data
- **Users**: 6 realistic Jamaica-based users with proper roles
- **Fleet Vehicles**: 3 waste collection vehicles with Jamaica license plates (GS001JA, GS002JA, GS003JA)
- **Waste Reports**: 15 reports covering major Jamaica locations
- **Routes**: 2 optimized routes with real Jamaica geography
- **Eco-Credits**: Complete transaction histories with realistic balances

##### Geographic Coverage
- **Kingston Metro**: Half Way Tree, New Kingston, Hope Pastures, Constant Spring
- **Spanish Town**: Hospital Road, Emancipation Square, Central area
- **Portmore**: Portmore Mall, Independence City
- **Tourism Areas**: Ocho Rios, Montego Bay Hip Strip
- **Regional Centers**: Old Harbour, May Pen, Mandeville, Port Antonio

##### Authentication Integration
- Sample users with proper ASP.NET Core Identity password hashes
- Role assignments (Administrator, User, Driver)
- Account types (Resident, Commercial, Municipal) integrated with business logic

### Known Considerations

#### Current Implementation Status
- ✅ Complete Entity Framework service implementations
- ✅ ASP.NET Core Identity integration with EfAuthService
- ✅ Jamaica-specific seed data with realistic coordinates
- ✅ Database schema optimized for Azure SQL Server
- ✅ All in-memory services replaced with database implementations

#### Current Limitations
- Initial implementation uses TSQL scripts rather than EF migrations for deployment
- Legacy User interface maintained for backward compatibility in IAuthService
- Some complex geographic queries may benefit from Azure SQL spatial features
- Authentication currently uses basic password authentication (SSO planned)

#### Planned Enhancements
- Implementation of EF Code-First migrations for ongoing development
- Google/Microsoft SSO integration as specified in project requirements
- Addition of spatial data types for advanced geographic queries
- Integration of Azure OpenAI for enhanced route optimization

### SignalR Real-Time Communication Implementation - October 19, 2025

#### Overview
Complete SignalR implementation has been added to enable real-time communication between administrators and users in the GreenSync waste management platform.

#### Architecture Components

##### 1. NotificationHub
- **Location**: `GreenSync-lib/Hubs/NotificationHub.cs`
- **Purpose**: Central SignalR hub for managing real-time connections
- **Key Features**:
  - Automatic user grouping based on roles (AdminUsers, RegularUsers)
  - Connection lifecycle management with logging
  - Role-based authorization for admin functions
  - Group management for targeted notifications

##### 2. INotificationService Interface
- **Location**: `GreenSync-lib/Services/INotificationService.cs`
- **Purpose**: Abstraction layer for notification services
- **Methods**:
  - `SendAdminNotificationAsync`: Broadcast to all users
  - `NotifyAdminsOfNewReportAsync`: Alert admins of new waste reports
  - `NotifyUserOfReportStatusChangeAsync`: Update users on report progress
  - `SendNotificationToUserAsync`: Targeted user notifications
  - `SendNotificationToGroupAsync`: Group-based notifications

##### 3. SignalRNotificationService
- **Location**: `GreenSync-lib/Services/SignalRNotificationService.cs`
- **Purpose**: SignalR implementation of notification service
- **Features**:
  - Comprehensive logging for all notification operations
  - Error handling that doesn't fail critical operations
  - Rich notification objects with metadata
  - Support for different notification types (info, success, warning, danger)

##### 4. Admin Notification Controller
- **Location**: `GreenSync-app/Areas/Admin/Controllers/NotificationController.cs`
- **Purpose**: Admin interface for sending notifications
- **Actions**:
  - `Send`: Form-based notification broadcasting
  - `SendToGroup`: API endpoint for group notifications
- **Security**: Role-based authorization (Administrator, Supervisor only)

##### 5. Client-Side JavaScript Manager
- **Location**: `GreenSync-app/wwwroot/js/signalr-notifications.js`
- **Purpose**: Complete client-side notification management
- **Features**:
  - Automatic connection management with retry logic
  - Toast notifications with Bootstrap styling
  - Connection status indicators
  - Notification persistence in localStorage
  - Audio notification sounds
  - XSS protection with HTML escaping

#### Implementation Details

##### Hub Configuration
```csharp
// Program.cs
builder.Services.AddSignalR();
app.MapHub<NotificationHub>("/notificationHub");
```

##### Service Registration
```csharp
builder.Services.AddScoped<INotificationService, SignalRNotificationService>();
```

##### Client Connection
- Automatic connection for authenticated users
- Role-based group assignment
- Reconnection logic with exponential backoff
- Connection status monitoring

##### Notification Types
1. **Admin Notifications**: Broadcasts from administrators to all users
2. **New Report Alerts**: Real-time alerts to admins when users create waste reports
3. **Report Status Updates**: Notifications to users when report status changes
4. **General Notifications**: System-wide announcements

#### Integration Points

##### Report Creation Flow
- User creates waste report in ReportsController
- Automatic notification sent to admin group via NotifyAdminsOfNewReportAsync
- Rich notification includes report details, priority, location, and waste type
- Admin receives toast notification and dashboard updates

##### Admin Broadcasting
- Admin accesses notification interface via `/Admin/Notification/Send`
- Form-based interface with preview functionality
- Support for different notification types and character limits
- Real-time delivery to all connected users

##### User Experience
- Toast notifications appear in top-right corner
- Automatic dismissal with configurable timing
- Notification history stored locally
- Audio alerts for important notifications
- Connection status indicators

#### Security Considerations
- Authorization required for hub connections
- Role-based message routing (AdminUsers/RegularUsers groups)
- Anti-forgery token validation on admin forms
- HTML escaping to prevent XSS attacks
- Secure SignalR connection over HTTPS

#### Performance Features
- Automatic reconnection with exponential backoff
- Connection pooling through SignalR infrastructure
- Efficient group-based message routing
- Client-side notification throttling
- Selective script loading (authenticated users only)

#### Error Handling
- Comprehensive logging at all levels
- Graceful degradation when SignalR unavailable
- Non-blocking notification failures
- Connection retry mechanisms
- User feedback for connection issues

#### Testing Scenarios
1. **Admin Notification Broadcasting**:
   - Admin sends notification via admin panel
   - All connected users receive toast notification
   - Notification appears in user's notification history

2. **Waste Report Creation**:
   - User creates new waste report
   - Admin receives real-time notification with report details
   - Admin can click notification to view dashboard

3. **Connection Management**:
   - Users automatically connect on page load
   - Connection status displayed during reconnection attempts
   - Automatic retry on connection failures

#### Known Limitations
- Layout file formatting issues from auto-formatting (functionality unaffected)
- Requires SignalR JavaScript library from CDN
- Basic audio notification implementation
- No notification persistence beyond localStorage

#### Future Enhancements
- Push notifications for mobile devices
- Advanced notification filtering and preferences
- Notification templates and scheduling
- Integration with email notifications
- Analytics and notification delivery tracking

### Package Dependency Resolution - October 19, 2025

#### SignalR Package Dependencies Fixed
- **Issue**: Build errors with `Microsoft.AspNetCore.SignalR.Core` version 9.0.0 package not found
- **Root Cause**: In .NET 9.0, SignalR is included as part of the ASP.NET Core framework rather than a separate NuGet package
- **Resolution**: Removed explicit `Microsoft.AspNetCore.SignalR.Core` package reference from GreenSync-lib.csproj
- **Result**: SignalR functionality provided through `FrameworkReference Include="Microsoft.AspNetCore.App"`

#### Package Configuration (Post-Fix)
```xml
<!-- GreenSync-lib.csproj - No longer needed for .NET 9.0 -->
<!-- <PackageReference Include="Microsoft.AspNetCore.SignalR.Core" Version="9.0.0" /> -->

<!-- SignalR included via framework reference -->
<FrameworkReference Include="Microsoft.AspNetCore.App" />
```

#### Build Status
- ✅ `dotnet restore` completed successfully
- ✅ `dotnet build` executed without SignalR dependency errors
- ✅ All SignalR functionality remains intact via framework reference

#### Best Practices for .NET 9.0 SignalR
- Use `FrameworkReference Include="Microsoft.AspNetCore.App"` for ASP.NET Core features
- Avoid explicit SignalR package references in .NET 9.0 projects
- SignalR client libraries still require separate packages for JavaScript/TypeScript clients

### Troubleshooting Common Issues

#### Connection Issues
- Verify Azure SQL connection string configuration
- Ensure proper authentication (managed identity preferred)
- Check firewall rules for Azure SQL Database

#### Performance Issues
- Review query execution plans for index usage
- Consider additional indexes for specific query patterns
- Monitor Azure SQL Database DTU/vCore usage

#### Data Integrity Issues
- Verify foreign key relationships are properly maintained
- Check soft delete query filters are applied consistently
- Validate decimal precision for financial calculations
