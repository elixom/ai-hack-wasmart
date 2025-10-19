using GreenSync.Lib.Models;

namespace GreenSync.Lib.Services;

public interface IFleetVehicleService
{
    Task<List<FleetVehicle>> GetAllVehiclesAsync();
    Task<FleetVehicle?> GetVehicleByIdAsync(Guid id);
    Task<IEnumerable<FleetVehicle>> GetVehiclesByStatusAsync(VehicleStatus status);
    Task<IEnumerable<FleetVehicle>> GetAvailableVehiclesAsync();
    Task<FleetVehicle> CreateVehicleAsync(FleetVehicle vehicle);
    Task<FleetVehicle?> UpdateVehicleAsync(Guid id, FleetVehicle vehicle);
    Task<bool> DeleteVehicleAsync(Guid id);
    Task<bool> AssignDriverAsync(Guid vehicleId, Guid driverId);
    Task<bool> UnassignDriverAsync(Guid vehicleId);
    Task<bool> UpdateVehicleStatusAsync(Guid vehicleId, VehicleStatus status);
    Task<bool> UpdateVehicleLocationAsync(Guid vehicleId, double latitude, double longitude);
    Task<bool> UpdateFuelLevelAsync(Guid vehicleId, int fuelLevel);
    Task<IEnumerable<FleetVehicle>> GetVehiclesNeedingMaintenanceAsync();
    Task<bool> ScheduleMaintenanceAsync(Guid vehicleId, DateTime scheduledDate);
    Task<FleetVehicle> GetAvailableTruck();
}
