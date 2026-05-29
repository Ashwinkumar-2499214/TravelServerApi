using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IInventoryService
    {
        Task<InventoryResponseDto> CreateInventoryAsync(InventoryRequestDto inventoryDto);
        Task<InventoryResponseDto> GetInventoryByIdAsync(long inventoryId);
        Task<IEnumerable<InventoryResponseDto>> GetAllInventoryAsync(InventorySearchDto searchDto);
        Task<IEnumerable<InventoryResponseDto>> GetPartnerInventoryAsync(long partnerId);
        Task<InventoryResponseDto> UpdateInventoryAsync(long inventoryId, InventoryRequestDto inventoryDto);
        Task<bool> DeleteInventoryAsync(long inventoryId);
        Task<InventoryResponseDto> UpdateAvailabilityAsync(long inventoryId, int availability, int status);
    }
}
