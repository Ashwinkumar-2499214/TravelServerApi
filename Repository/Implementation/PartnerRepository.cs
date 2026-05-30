using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class PartnerRepository : IPartnerRepository
    {
        private readonly AppDbContext _context;

        public PartnerRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PartnerResponseDto> CreatePartnerAsync(Partner partner)
        {
            try
            {
                _context.Partners.Add(partner);
                await _context.SaveChangesAsync();
                return MapPartnerToDto(partner);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating partner in database.", ex);
            }
        }

        public async Task<PartnerResponseDto> GetPartnerByIdAsync(long partnerId)
        {
            try
            {
                var partner = await _context.Partners
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PartnerId == partnerId);

                return partner != null ? MapPartnerToDto(partner) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving partner with ID {partnerId}.", ex);
            }
        }

        public async Task<IEnumerable<PartnerResponseDto>> GetAllPartnersAsync(PartnerSearchDto searchDto)
        {
            try
            {
                var query = _context.Partners.AsNoTracking();

                // Apply filters
                if (!string.IsNullOrWhiteSpace(searchDto.SearchTerm))
                {
                    var term = searchDto.SearchTerm.ToLower();
                    query = query.Where(p => p.Name.ToLower().Contains(term) ||
                                           p.ContactEmail.ToLower().Contains(term));
                }

                if (searchDto.Type.HasValue)
                {
                    query = query.Where(p => p.Type == searchDto.Type.Value);
                }

                if (searchDto.Status.HasValue)
                {
                    query = query.Where(p => p.Status == searchDto.Status.Value);
                }

                // Apply pagination
                int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var partners = await query
                    .OrderByDescending(p => p.CreatedDate)
                    .Skip(skip)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return partners.Select(MapPartnerToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving partners.", ex);
            }
        }

        public async Task<PartnerResponseDto> UpdatePartnerAsync(Partner partner)
        {
            try
            {
                var existingPartner = await _context.Partners.FirstOrDefaultAsync(p => p.PartnerId == partner.PartnerId);
                if (existingPartner == null)
                {
                    throw new KeyNotFoundException($"Partner with ID {partner.PartnerId} not found.");
                }

                existingPartner.Name = partner.Name;
                existingPartner.Type = partner.Type;
                existingPartner.ContactEmail = partner.ContactEmail;
                existingPartner.ContactPhone = partner.ContactPhone;
                existingPartner.Address = partner.Address;
                existingPartner.ModifiedDate = DateTime.UtcNow;

                _context.Partners.Update(existingPartner);
                await _context.SaveChangesAsync();

                return MapPartnerToDto(existingPartner);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating partner with ID {partner.PartnerId}.", ex);
            }
        }

        public async Task<bool> DeletePartnerAsync(long partnerId)
        {
            try
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
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error deleting partner with ID {partnerId}.", ex);
            }
        }

        public async Task<PartnerResponseDto> UpdatePartnerStatusAsync(long partnerId, int status)
        {
            try
            {
                var partner = await _context.Partners.FirstOrDefaultAsync(p => p.PartnerId == partnerId);
                if (partner == null)
                {
                    throw new KeyNotFoundException($"Partner with ID {partnerId} not found.");
                }

                partner.Status = status;
                partner.ModifiedDate = DateTime.UtcNow;

                _context.Partners.Update(partner);
                await _context.SaveChangesAsync();

                return MapPartnerToDto(partner);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating partner status with ID {partnerId}.", ex);
            }
        }

        private PartnerResponseDto MapPartnerToDto(Partner partner)
        {
            return new PartnerResponseDto
            {
                PartnerId = partner.PartnerId,
                Name = partner.Name,
                Type = partner.Type,
                ContactEmail = partner.ContactEmail,
                ContactPhone = partner.ContactPhone,
                Address = partner.Address,
                Status = partner.Status,
                CreatedDate = partner.CreatedDate
            };
        }
    }
}
