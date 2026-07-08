using Microsoft.AspNetCore.Http;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class InventoryService : IInventoryService
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }

        public async Task<InventoryResponseDto> CreateInventoryAsync(InventoryRequestDto inventoryDto)
        {
            try
            {
                var inventory = new Inventory
                {
                    PartnerId = inventoryDto.PartnerId,
                    ItemType = inventoryDto.ItemType,

                    Description = inventoryDto.Description,
                    Availability = inventoryDto.Availability,
                    Price = inventoryDto.Price,
                    Status = (int)inventoryDto.Status,
                    CreatedDate = DateTime.UtcNow
                };

                return await _inventoryRepository.CreateInventoryAsync(inventory);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(InventoryConstants.InventoryCreatedSuccess, ex);
            }
        }

        public async Task<InventoryResponseDto> GetInventoryByIdAsync(long inventoryId)
        {
            return await _inventoryRepository.GetInventoryByIdAsync(inventoryId);
        }

        public async Task<IEnumerable<InventoryResponseDto>> GetAllInventoryAsync(InventorySearchDto searchDto)
        {
            return await _inventoryRepository.GetAllInventoryAsync(searchDto);
        }

        public async Task<IEnumerable<InventoryResponseDto>> GetPartnerInventoryAsync(long partnerId)
        {
            return await _inventoryRepository.GetInventoryByPartnerIdAsync(partnerId);
        }

        public async Task<InventoryResponseDto> UpdateInventoryAsync(long inventoryId, InventoryRequestDto inventoryDto)
        {
            try
            {
                var inventory = new Inventory
                {
                    InventoryId = inventoryId,
                    PartnerId = inventoryDto.PartnerId,
                    ItemType = inventoryDto.ItemType,
                    Description = inventoryDto.Description,
                    Availability = inventoryDto.Availability,
                    Price = inventoryDto.Price,
                    Status = (int)inventoryDto.Status,
                    ModifiedDate = DateTime.UtcNow
                };

                return await _inventoryRepository.UpdateInventoryAsync(inventory);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(InventoryConstants.InventoryUpdateSuccess, ex);
            }
        }

        public async Task<bool> DeleteInventoryAsync(long inventoryId)
        {
            return await _inventoryRepository.DeleteInventoryAsync(inventoryId);
        }

        public async Task<InventoryResponseDto> UpdateAvailabilityAsync(long inventoryId, int availability, int status)
        {
            return await _inventoryRepository.UpdateAvailabilityAsync(inventoryId, availability, status);
        }

        public async Task<InventoryMediaDto> AddMediaAsync(long inventoryId, IFormFile file)
        {
            var allowedImages = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var allowedVideos = new[] { ".mp4", ".mov", ".avi", ".webm" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            string mediaType;
            if (allowedImages.Contains(ext)) mediaType = "image";
            else if (allowedVideos.Contains(ext)) mediaType = "video";
            else throw new InvalidOperationException("Unsupported file type.");

            var uploadsFolder = Path.Combine("wwwroot", "uploads", "inventory", inventoryId.ToString());
            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            var media = new TravelEaseServer.Model.InventoryMedia
            {
                InventoryId = inventoryId,
                FileName = file.FileName,
                Url = $"/uploads/inventory/{inventoryId}/{uniqueName}",
                MediaType = mediaType,
                UploadedDate = DateTime.UtcNow
            };

            return await _inventoryRepository.AddMediaAsync(inventoryId, media);
        }

        public async Task<bool> DeleteMediaAsync(long mediaId)
        {
            return await _inventoryRepository.DeleteMediaAsync(mediaId);
        }
    }
}
