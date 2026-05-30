using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation;

public class PartnerRepository : IPartnerRepository
{
    private readonly AppDbContext _context;

    public PartnerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Partner> CreatePartnerAsync(Partner partner)
    {
        _context.Partners.Add(partner);
        await _context.SaveChangesAsync();
        return partner;
    }

    public async Task<Partner?> GetPartnerByIdAsync(long partnerId)
    {
        return await _context.Partners
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PartnerId == partnerId);
    }

    public async Task<IEnumerable<Partner>> GetAllPartnersAsync(string? searchTerm, int? type, int? status, int pageNumber, int pageSize)
    {
        var query = _context.Partners.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term) || p.ContactEmail.ToLower().Contains(term));
        }

        if (type.HasValue)
        {
            query = query.Where(p => p.Type == type.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        int skip = (pageNumber - 1) * pageSize;

        return await query
            .OrderByDescending(p => p.CreatedDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Partner?> UpdatePartnerAsync(Partner partner)
    {
        var existingPartner = await _context.Partners.FirstOrDefaultAsync(p => p.PartnerId == partner.PartnerId);
        if (existingPartner == null)
        {
            return null;
        }

        existingPartner.Name = partner.Name;
        existingPartner.Type = partner.Type;
        existingPartner.ContactEmail = partner.ContactEmail;
        existingPartner.ContactPhone = partner.ContactPhone;
        existingPartner.Address = partner.Address;
        existingPartner.ModifiedDate = DateTime.UtcNow;

        _context.Partners.Update(existingPartner);
        await _context.SaveChangesAsync();

        return existingPartner;
    }

    public async Task<bool> DeletePartnerAsync(long partnerId)
    {
        var partner = await _context.Partners.FirstOrDefaultAsync(p => p.PartnerId == partnerId);
        if (partner == null)
        {
            return false;
        }

        _context.Partners.Remove(partner);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Partner?> UpdatePartnerStatusAsync(long partnerId, int status)
    {
        var partner = await _context.Partners.FirstOrDefaultAsync(p => p.PartnerId == partnerId);
        if (partner == null)
        {
            return null;
        }

        partner.Status = status;
        partner.ModifiedDate = DateTime.UtcNow;

        _context.Partners.Update(partner);
        await _context.SaveChangesAsync();

        return partner;
    }
}