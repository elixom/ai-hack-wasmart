using GreenSync.Lib.Data;
using GreenSync.Lib.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GreenSync.Lib.Services.EntityFramework;

/// <summary>
/// Entity Framework implementation of the FleetVehicle service
/// </summary>
public class EfFleetVehicleService : IFleetVehicleService
{
    private readonly GreenSyncDbContext _context;
    private readonly ILogger<EfFleetVehicleService> _logger;

    public EfFleetVehicleService(GreenSyncDbContext context, ILogger<EfFleetVehicleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<FleetVehicle>> GetAllVehiclesAsync()
    {
        try
        {
            return await _context.FleetVehicles
                .Include(v => v.AssignedDriver)
                .Include(v => v.MaintenanceRecords.OrderByDescending(m => m.MaintenanceDate))
                .OrderBy(v => v.LicensePlate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all vehicles");
            throw;
        }
    }

    public async Task<FleetVehicle?> GetVehicleByIdAsync(Guid id)
    {
        try
        {
            return await _context.FleetVehicles
                .Include(v => v.AssignedDriver)
                .Include(v => v.MaintenanceRecords.OrderByDescending(m => m.MaintenanceDate))
                .Include(v => v.AssignedRoutes)
                .FirstOrDefaultAsync(v => v.Id == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicle {VehicleId}", id);
            throw;
        }
    }

    public async Task<IEnumerable<FleetVehicle>> GetVehiclesByStatusAsync(VehicleStatus status)
    {
        try
        {
            return await _context.FleetVehicles
                .Include(v => v.AssignedDriver)
                .Where(v => v.Status == status)
                .OrderBy(v => v.LicensePlate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles with status {Status}", status);
            throw;
        }
    }

    public async Task<IEnumerable<FleetVehicle>> GetAvailableVehiclesAsync()
    {
        try
        {
            return await _context.FleetVehicles
                .Include(v => v.AssignedDriver)
                .Where(v => v.Status == VehicleStatus.Available && v.FuelLevel > 20)
                .OrderBy(v => v.LicensePlate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available vehicles");
            throw;
        }
    }

    public async Task<FleetVehicle> CreateVehicleAsync(FleetVehicle vehicle)
    {
        try
        {
            _context.FleetVehicles.Add(vehicle);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created vehicle {LicensePlate} (ID: {VehicleId})", 
                vehicle.LicensePlate, vehicle.Id);
            return vehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating vehicle {LicensePlate}", vehicle.LicensePlate);
            throw;
        }
    }

    public async Task<FleetVehicle?> UpdateVehicleAsync(Guid id, FleetVehicle vehicle)
    {
        try
        {
            var existingVehicle = await _context.FleetVehicles.FindAsync(id);
            if (existingVehicle == null)
            {
                return null;
            }

            // Update properties
            existingVehicle.LicensePlate = vehicle.LicensePlate;
            existingVehicle.Make = vehicle.Make;
            existingVehicle.Model = vehicle.Model;
            existingVehicle.Year = vehicle.Year;
            existingVehicle.VIN = vehicle.VIN;
            existingVehicle.Capacity = vehicle.Capacity;
            existingVehicle.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Updated vehicle {VehicleId}", id);
            return existingVehicle;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteVehicleAsync(Guid id)
    {
        try
        {
            var vehicle = await _context.FleetVehicles.FindAsync(id);
            if (vehicle == null)
            {
                return false;
            }

            // Soft delete
            vehicle.IsDeleted = true;
            vehicle.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted vehicle {VehicleId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
            throw;
        }
    }

    public async Task<bool> AssignDriverAsync(Guid vehicleId, Guid driverId)
    {
        try
        {
            var vehicle = await _context.FleetVehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return false;
            }

            // Check if driver exists and is a driver
            var driver = await _context.Users.FindAsync(driverId);
            if (driver == null)
            {
                return false;
            }

            vehicle.AssignedDriverId = driverId;
            vehicle.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Assigned driver {DriverId} to vehicle {VehicleId}", driverId, vehicleId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning driver {DriverId} to vehicle {VehicleId}", driverId, vehicleId);
            throw;
        }
    }

    public async Task<bool> UnassignDriverAsync(Guid vehicleId)
    {
        try
        {
            var vehicle = await _context.FleetVehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return false;
            }

            var previousDriverId = vehicle.AssignedDriverId;
            vehicle.AssignedDriverId = null;
            vehicle.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Unassigned driver {DriverId} from vehicle {VehicleId}", 
                previousDriverId, vehicleId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unassigning driver from vehicle {VehicleId}", vehicleId);
            throw;
        }
    }

    public async Task<bool> UpdateVehicleStatusAsync(Guid vehicleId, VehicleStatus status)
    {
        try
        {
            var vehicle = await _context.FleetVehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return false;
            }

            vehicle.Status = status;
            vehicle.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated vehicle {VehicleId} status to {Status}", vehicleId, status);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId} status", vehicleId);
            throw;
        }
    }

    public async Task<bool> UpdateVehicleLocationAsync(Guid vehicleId, double latitude, double longitude)
    {
        try
        {
            var vehicle = await _context.FleetVehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return false;
            }

            vehicle.CurrentLatitude = latitude;
            vehicle.CurrentLongitude = longitude;
            vehicle.LastGPSUpdate = DateTime.UtcNow;
            vehicle.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogDebug("Updated vehicle {VehicleId} location to {Latitude}, {Longitude}", 
                vehicleId, latitude, longitude);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId} location", vehicleId);
            throw;
        }
    }

    public async Task<bool> UpdateFuelLevelAsync(Guid vehicleId, int fuelLevel)
    {
        try
        {
            var vehicle = await _context.FleetVehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return false;
            }

            vehicle.FuelLevel = Math.Max(0, Math.Min(100, fuelLevel)); // Ensure 0-100 range
            vehicle.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated vehicle {VehicleId} fuel level to {FuelLevel}%", 
                vehicleId, vehicle.FuelLevel);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating vehicle {VehicleId} fuel level", vehicleId);
            throw;
        }
    }

    public async Task<IEnumerable<FleetVehicle>> GetVehiclesNeedingMaintenanceAsync()
    {
        try
        {
            var today = DateTime.UtcNow.Date;
            
            return await _context.FleetVehicles
                .Include(v => v.AssignedDriver)
                .Where(v => v.NextMaintenanceDate <= today.AddDays(7) || // Due within 7 days
                           v.Status == VehicleStatus.Maintenance ||
                           (v.LastMaintenanceDate.HasValue && 
                            v.LastMaintenanceDate.Value <= today.AddMonths(-6))) // Overdue (6 months)
                .OrderBy(v => v.NextMaintenanceDate)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving vehicles needing maintenance");
            throw;
        }
    }

    public async Task<bool> ScheduleMaintenanceAsync(Guid vehicleId, DateTime scheduledDate)
    {
        try
        {
            var vehicle = await _context.FleetVehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                return false;
            }

            vehicle.NextMaintenanceDate = scheduledDate.Date;
            vehicle.Status = VehicleStatus.Maintenance;
            vehicle.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Scheduled maintenance for vehicle {VehicleId} on {Date}", 
                vehicleId, scheduledDate.Date);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling maintenance for vehicle {VehicleId}", vehicleId);
            throw;
        }
    }

    public Task<FleetVehicle> GetAvailableTruck()
    {
        return _context.FleetVehicles
            .Where(v => v.Status == VehicleStatus.Available && v.FuelLevel > 20)
            .OrderBy(v => v.LastMaintenanceDate)
            .FirstAsync();
    }
}
