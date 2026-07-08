using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryResponseDto> CreateInventoryAsync(Inventory inventory)
        {
            try
            {
                _context.Inventories.Add(inventory);
                await _context.SaveChangesAsync();
                return MapInventoryToDto(inventory);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating inventory in database.", ex);
            }
        }

        public async Task<InventoryResponseDto> GetInventoryByIdAsync(long inventoryId)
        {
            try
            {
                var inventory = await _context.Inventories
                    .AsNoTracking()
                    .Include(i => i.Media)
                    .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);

                return inventory != null ? MapInventoryToDto(inventory) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving inventory with ID {inventoryId}.", ex);
            }
        }

        public async Task<IEnumerable<InventoryResponseDto>> GetAllInventoryAsync(InventorySearchDto searchDto)
        {
            try
            {
                var query = _context.Inventories.AsNoTracking();

                // Apply filters
                if (searchDto.PartnerId.HasValue)
                {
                    query = query.Where(i => i.PartnerId == searchDto.PartnerId.Value);
                }

                if (!string.IsNullOrWhiteSpace(searchDto.ItemType))
                {
                    query = query.Where(i => i.ItemType.ToLower().Contains(searchDto.ItemType.ToLower()));
                }

                if (searchDto.Status.HasValue)
                {
                    query = query.Where(i => i.Status == (int)searchDto.Status.Value);
                }

                // Apply pagination
                int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var inventories = await query
                    .Include(i => i.Media)
                    .OrderByDescending(i => i.CreatedDate)
                    .Skip(skip)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return inventories.Select(MapInventoryToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving inventory items.", ex);
            }
        }

        public async Task<IEnumerable<InventoryResponseDto>> GetInventoryByPartnerIdAsync(long partnerId)
        {
            try
            {
                var inventories = await _context.Inventories
                    .AsNoTracking()
                    .Include(i => i.Media)
                    .Where(i => i.PartnerId == partnerId)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                return inventories.Select(MapInventoryToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving inventory for partner with ID {partnerId}.", ex);
            }
        }

        public async Task<InventoryResponseDto> UpdateInventoryAsync(Inventory inventory)
        {
            try
            {
                var existingInventory = await _context.Inventories
                    .Include(i => i.Media)
                    .FirstOrDefaultAsync(i => i.InventoryId == inventory.InventoryId);
                if (existingInventory == null)
                {
                    throw new KeyNotFoundException($"Inventory with ID {inventory.InventoryId} not found.");
                }

                existingInventory.ItemType = inventory.ItemType;
                existingInventory.Description = inventory.Description;
                existingInventory.Availability = inventory.Availability > 0 ? inventory.Availability : existingInventory.Availability;
                existingInventory.Price = inventory.Price;
                existingInventory.Status = inventory.Status > 0 ? inventory.Status : existingInventory.Status;
                existingInventory.ModifiedDate = inventory.ModifiedDate;

                _context.Inventories.Update(existingInventory);
                await _context.SaveChangesAsync();

                return MapInventoryToDto(existingInventory);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating inventory with ID {inventory.InventoryId}.", ex);
            }
        }

        public async Task<bool> DeleteInventoryAsync(long inventoryId)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            try
            {
                return await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _context.Database.BeginTransactionAsync();

                    var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.InventoryId == inventoryId);
                    if (inventory == null)
                    {
                        return false;
                    }

                    var associatedBookings = _context.Bookings.Where(b => b.InventoryId == inventoryId);
                    _context.Bookings.RemoveRange(associatedBookings);

                    _context.Inventories.Remove(inventory);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return true;
                });
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error deleting inventory with ID {inventoryId}.", ex);
            }
        }

        public async Task<InventoryResponseDto> UpdateAvailabilityAsync(long inventoryId, int availability, int status)
        {
            try
            {
                var inventory = await _context.Inventories
                    .Include(i => i.Media)
                    .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);
                if (inventory == null)
                {
                    throw new KeyNotFoundException($"Inventory with ID {inventoryId} not found.");
                }

                inventory.Availability = availability >= 0 ? availability : inventory.Availability;
                inventory.Status = status;

                _context.Inventories.Update(inventory);
                await _context.SaveChangesAsync();

                return MapInventoryToDto(inventory);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating availability for inventory with ID {inventoryId}.", ex);
            }
        }

        public async Task<InventoryMediaDto> AddMediaAsync(long inventoryId, InventoryMedia media)
        {
            try
            {
                _context.InventoryMedia.Add(media);
                await _context.SaveChangesAsync();
                return new TravelEaseServer.Dto.InventoryMediaDto
                {
                    MediaId = media.MediaId,
                    FileName = media.FileName,
                    Url = media.Url,
                    MediaType = media.MediaType,
                    UploadedDate = media.UploadedDate
                };
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error saving media.", ex);
            }
        }

        public async Task<bool> DeleteMediaAsync(long mediaId)
        {
            try
            {
                var media = await _context.InventoryMedia.FirstOrDefaultAsync(m => m.MediaId == mediaId);
                if (media == null) return false;
                _context.InventoryMedia.Remove(media);
                await _context.SaveChangesAsync();
                // Delete physical file
                if (File.Exists(media.Url)) File.Delete(media.Url);
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error deleting media.", ex);
            }
        }

        private InventoryResponseDto MapInventoryToDto(Inventory inventory)
        {
            return new InventoryResponseDto
            {
                InventoryId = inventory.InventoryId,
                PartnerId = inventory.PartnerId,
                ItemType = inventory.ItemType,
                Description = inventory.Description,
                Availability = inventory.Availability,
                Price = inventory.Price,
                Status = (TravelEaseServer.Enum.InventoryStatus)inventory.Status,
                CreatedDate = inventory.CreatedDate,
                Media = inventory.Media?.Select(m => new TravelEaseServer.Dto.InventoryMediaDto
                {
                    MediaId = m.MediaId,
                    FileName = m.FileName,
                    Url = m.Url,
                    MediaType = m.MediaType,
                    UploadedDate = m.UploadedDate
                }).ToList() ?? new()
            };
        }
    }
}
