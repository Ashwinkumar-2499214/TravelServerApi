using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation;

public class InventoryRepository : IInventoryRepository
{
    private readonly AppDbContext _context;

    public InventoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Inventory> CreateInventoryAsync(Inventory inventory)
    {
        _context.Inventories.Add(inventory);
        await _context.SaveChangesAsync();
        return inventory;
    }

    public async Task<Inventory?> GetInventoryByIdAsync(long inventoryId)
    {
        return await _context.Inventories
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InventoryId == inventoryId);
    }

    public async Task<IEnumerable<Inventory>> GetAllInventoryAsync(long? partnerId, string? itemType, int? status, int pageNumber, int pageSize)
    {
        var query = _context.Inventories.AsNoTracking();

        if (partnerId.HasValue)
        {
            query = query.Where(i => i.PartnerId == partnerId.Value);
        }

        if (!string.IsNullOrWhiteSpace(itemType))
        {
            var typeLower = itemType.ToLower();
            query = query.Where(i => i.ItemType.ToLower().Contains(typeLower));
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        int skip = (pageNumber - 1) * pageSize;

        return await query
            .OrderByDescending(i => i.CreatedDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventory>> GetInventoryByPartnerIdAsync(long partnerId)
    {
        return await _context.Inventories
            .AsNoTracking()
            .Where(i => i.PartnerId == partnerId)
            .OrderByDescending(i => i.CreatedDate)
            .ToListAsync();
    }

    public async Task<Inventory?> UpdateInventoryAsync(Inventory inventory)
    {
        var existingInventory = await _context.Inventories.FirstOrDefaultAsync(i => i.InventoryId == inventory.InventoryId);
        if (existingInventory == null)
        {
            return null;
        }

        existingInventory.ItemType = inventory.ItemType;
        existingInventory.Description = inventory.Description;
        existingInventory.Availability = inventory.Availability;
        existingInventory.Price = inventory.Price;
        existingInventory.Status = inventory.Status;

        _context.Inventories.Update(existingInventory);
        await _context.SaveChangesAsync();

        return existingInventory;
    }

    public async Task<bool> DeleteInventoryAsync(long inventoryId)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.InventoryId == inventoryId);
        if (inventory == null)
        {
            return false;
        }

        _context.Inventories.Remove(inventory);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Inventory?> UpdateAvailabilityAsync(long inventoryId, int availability, int status)
    {
        var inventory = await _context.Inventories.FirstOrDefaultAsync(i => i.InventoryId == inventoryId);
        if (inventory == null)
        {
            return null;
        }

        inventory.Availability = availability;
        inventory.Status = status;

        _context.Inventories.Update(inventory);
        await _context.SaveChangesAsync();

        return inventory;
    }
}