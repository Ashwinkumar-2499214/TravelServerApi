using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IInventoryRepository
    {
        Task<InventoryResponseDto> CreateInventoryAsync(Inventory inventory);
        Task<InventoryResponseDto> GetInventoryByIdAsync(long inventoryId);
        Task<IEnumerable<InventoryResponseDto>> GetAllInventoryAsync(InventorySearchDto searchDto);
        Task<IEnumerable<InventoryResponseDto>> GetInventoryByPartnerIdAsync(long partnerId);
        Task<InventoryResponseDto> UpdateInventoryAsync(Inventory inventory);
        Task<bool> DeleteInventoryAsync(long inventoryId);
        Task<InventoryResponseDto> UpdateAvailabilityAsync(long inventoryId, int availability, int status);
    }
}
