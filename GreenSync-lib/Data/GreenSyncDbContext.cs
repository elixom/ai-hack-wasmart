using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GreenSync.Lib.Models;

namespace GreenSync.Lib.Data;

/// <summary>
/// Entity Framework DbContext for the GreenSync application
/// Extends IdentityDbContext to include ASP.NET Core Identity tables
/// </summary>
public class GreenSyncDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
{
    public GreenSyncDbContext(DbContextOptions<GreenSyncDbContext> options) : base(options)
    {
    }

    #region DbSets

    /// <summary>
    /// Waste reports submitted by users
    /// </summary>
    public DbSet<Report> Reports { get; set; }

    /// <summary>
    /// User eco-credit accounts
    /// </summary>
    public DbSet<EcoCredit> EcoCredits { get; set; }

    /// <summary>
    /// Eco-credit transaction history
    /// </summary>
    public DbSet<EcoCreditTransaction> EcoCreditTransactions { get; set; }

    /// <summary>
    /// Fleet vehicles for waste collection
    /// </summary>
    public DbSet<FleetVehicle> FleetVehicles { get; set; }

    /// <summary>
    /// Vehicle maintenance records
    /// </summary>
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }

    /// <summary>
    /// Optimized collection routes
    /// </summary>
    public DbSet<Route> Routes { get; set; }

    /// <summary>
    /// Route waypoints/stops
    /// </summary>
    public DbSet<RouteWaypoint> RouteWaypoints { get; set; }

    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure Identity table names
        builder.Entity<ApplicationUser>().ToTable("Users");
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");

        #region ApplicationUser Configuration

        builder.Entity<ApplicationUser>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.AccountType);

            // Soft delete filter
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Configure relationships
            entity.HasMany(e => e.Reports)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EcoCredit)
                  .WithOne(e => e.User)
                  .HasForeignKey<EcoCredit>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.EcoCreditTransactions)
                  .WithOne(e => e.User)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.AssignedRoutes)
                  .WithOne(e => e.Driver)
                  .HasForeignKey(e => e.DriverId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.AssignedVehicles)
                  .WithOne(e => e.AssignedDriver)
                  .HasForeignKey(e => e.AssignedDriverId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        #endregion

        #region Report Configuration

        builder.Entity<Report>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.Priority);
            entity.HasIndex(e => e.WasteType);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => new { e.Latitude, e.Longitude });

            // Soft delete filter
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Configure relationships
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Reports)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AssignedVehicle)
                  .WithMany()
                  .HasForeignKey(e => e.AssignedVehicleId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.AssignedRoute)
                  .WithMany(e => e.AssignedReports)
                  .HasForeignKey(e => e.AssignedRouteId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        #endregion

        #region EcoCredit Configuration

        builder.Entity<EcoCredit>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.HasIndex(e => e.IsDeleted);

            // Soft delete filter
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Configure relationships
            entity.HasOne(e => e.User)
                  .WithOne(e => e.EcoCredit)
                  .HasForeignKey<EcoCredit>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(e => e.TransactionHistory)
                  .WithOne(e => e.EcoCredit)
                  .HasForeignKey(e => e.EcoCreditId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        #region EcoCreditTransaction Configuration

        builder.Entity<EcoCreditTransaction>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.TransactionDate);
            entity.HasIndex(e => e.Type);

            // Configure relationships
            entity.HasOne(e => e.User)
                  .WithMany(e => e.EcoCreditTransactions)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.EcoCredit)
                  .WithMany(e => e.TransactionHistory)
                  .HasForeignKey(e => e.EcoCreditId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.RelatedReport)
                  .WithMany()
                  .HasForeignKey(e => e.RelatedReportId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        #endregion

        #region FleetVehicle Configuration

        builder.Entity<FleetVehicle>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(e => e.LicensePlate).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.IsDeleted);
            entity.HasIndex(e => e.AssignedDriverId);

            // Soft delete filter
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Configure relationships
            entity.HasOne(e => e.AssignedDriver)
                  .WithMany(e => e.AssignedVehicles)
                  .HasForeignKey(e => e.AssignedDriverId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.AssignedRoutes)
                  .WithOne(e => e.AssignedVehicle)
                  .HasForeignKey(e => e.AssignedVehicleId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.MaintenanceRecords)
                  .WithOne(e => e.Vehicle)
                  .HasForeignKey(e => e.VehicleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        #region MaintenanceRecord Configuration

        builder.Entity<MaintenanceRecord>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => e.MaintenanceDate);
            entity.HasIndex(e => e.Type);

            // Configure relationships
            entity.HasOne(e => e.Vehicle)
                  .WithMany(e => e.MaintenanceRecords)
                  .HasForeignKey(e => e.VehicleId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        #region Route Configuration

        builder.Entity<Route>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.AssignedVehicleId);
            entity.HasIndex(e => e.DriverId);
            entity.HasIndex(e => e.IsDeleted);

            // Soft delete filter
            entity.HasQueryFilter(e => !e.IsDeleted);

            // Configure relationships
            entity.HasOne(e => e.AssignedVehicle)
                  .WithMany(e => e.AssignedRoutes)
                  .HasForeignKey(e => e.AssignedVehicleId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Driver)
                  .WithMany(e => e.AssignedRoutes)
                  .HasForeignKey(e => e.DriverId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.AssignedReports)
                  .WithOne(e => e.AssignedRoute)
                  .HasForeignKey(e => e.AssignedRouteId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(e => e.Waypoints)
                  .WithOne(e => e.Route)
                  .HasForeignKey(e => e.RouteId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        #endregion

        #region RouteWaypoint Configuration

        builder.Entity<RouteWaypoint>(entity =>
        {
            // Indexes for performance
            entity.HasIndex(e => e.RouteId);
            entity.HasIndex(e => new { e.RouteId, e.StopOrder }).IsUnique();
            entity.HasIndex(e => e.ReportId);
            entity.HasIndex(e => new { e.Latitude, e.Longitude });

            // Configure relationships
            entity.HasOne(e => e.Route)
                  .WithMany(e => e.Waypoints)
                  .HasForeignKey(e => e.RouteId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Report)
                  .WithMany()
                  .HasForeignKey(e => e.ReportId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        #endregion

        // Configure decimal precision for financial/measurement fields
        ConfigureDecimalPrecision(builder);

        // Seed initial data
        SeedData(builder);
    }

    /// <summary>
    /// Configure decimal precision for monetary and measurement fields
    /// </summary>
    private static void ConfigureDecimalPrecision(ModelBuilder builder)
    {
        // EcoCredit amounts
        builder.Entity<EcoCredit>()
               .Property(e => e.CurrentBalance)
               .HasPrecision(10, 2);

        builder.Entity<EcoCredit>()
               .Property(e => e.TotalEarned)
               .HasPrecision(10, 2);

        builder.Entity<EcoCredit>()
               .Property(e => e.TotalRedeemed)
               .HasPrecision(10, 2);

        // EcoCreditTransaction amounts
        builder.Entity<EcoCreditTransaction>()
               .Property(e => e.Amount)
               .HasPrecision(10, 2);

        builder.Entity<EcoCreditTransaction>()
               .Property(e => e.BalanceAfter)
               .HasPrecision(10, 2);

        // Vehicle specifications
        builder.Entity<FleetVehicle>()
               .Property(e => e.Capacity)
               .HasPrecision(6, 2);

        // Maintenance costs
        builder.Entity<MaintenanceRecord>()
               .Property(e => e.Cost)
               .HasPrecision(10, 2);

        // Route metrics
        builder.Entity<Route>()
               .Property(e => e.FuelSavingsMetric)
               .HasPrecision(5, 2);

        builder.Entity<Route>()
               .Property(e => e.TotalDistance)
               .HasPrecision(10, 2);

        builder.Entity<Route>()
               .Property(e => e.EstimatedFuelCost)
               .HasPrecision(10, 2);

        builder.Entity<Route>()
               .Property(e => e.EfficiencyScore)
               .HasPrecision(5, 2);

        // Report measurements
        builder.Entity<Report>()
               .Property(e => e.EstimatedVolume)
               .HasPrecision(6, 2);

        builder.Entity<Report>()
               .Property(e => e.CreditsEarned)
               .HasPrecision(10, 2);
    }

    /// <summary>
    /// Seed initial data for development and testing
    /// </summary>
    private static void SeedData(ModelBuilder builder)
    {
        // Seed default admin role
        var adminRoleId = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890");
        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = adminRoleId,
                Name = "Administrator",
                NormalizedName = "ADMINISTRATOR",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        );

        // Seed default user role
        var userRoleId = Guid.Parse("B2C3D4E5-F6G7-8901-BCDE-F23456789012");
        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = userRoleId,
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        );

        // Seed driver role
        var driverRoleId = Guid.Parse("C3D4E5F6-G7H8-9012-CDEF-345678901234");
        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = driverRoleId,
                Name = "Driver",
                NormalizedName = "DRIVER",
                ConcurrencyStamp = Guid.NewGuid().ToString()
            }
        );
    }
}
