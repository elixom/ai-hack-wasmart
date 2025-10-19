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

### Known Considerations

#### Current Limitations
- Initial implementation uses TSQL scripts rather than EF migrations
- Some complex business rules may require application-level validation
- Geographic queries may benefit from Azure SQL spatial features in future

#### Planned Enhancements
- Implementation of EF Code-First migrations for ongoing development
- Addition of spatial data types for advanced geographic queries
- Integration of Azure OpenAI for enhanced route optimization
- Real-time SignalR integration for live updates

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
