using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<InvoiceResponseDto> CreateInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
            await _context.Entry(invoice).Reference(i => i.Booking).LoadAsync();
            if (invoice.Booking != null)
            {
                await _context.Entry(invoice.Booking).Reference(b => b.User).LoadAsync();
                await _context.Entry(invoice.Booking).Reference(b => b.Inventory).LoadAsync();
                await _context.Entry(invoice.Booking).Reference(b => b.Partner).LoadAsync();
            }
            return MapToDto(invoice);
        }

        public async Task<InvoiceResponseDto> GetInvoiceByIdAsync(long invoiceId)
        {
            var invoice = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Booking).ThenInclude(b => b.User)
                .Include(i => i.Booking).ThenInclude(b => b.Inventory)
                .Include(i => i.Booking).ThenInclude(b => b.Partner)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            return invoice != null ? MapToDto(invoice) : null;
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync(InvoiceSearchDto searchDto)
        {
            var query = _context.Invoices
                .Include(i => i.Booking).ThenInclude(b => b.User)
                .Include(i => i.Booking).ThenInclude(b => b.Inventory)
                .Include(i => i.Booking).ThenInclude(b => b.Partner)
                .AsNoTracking()
                .AsQueryable();

            if (searchDto.UserId.HasValue)
                query = query.Where(i => i.Booking.UserId == searchDto.UserId.Value);
            if (searchDto.BookingId.HasValue)
                query = query.Where(i => i.BookingId == searchDto.BookingId.Value);
            if (searchDto.Status.HasValue)
                query = query.Where(i => i.Status == searchDto.Status.Value);
            if (searchDto.FromDate.HasValue)
                query = query.Where(i => i.InvoiceDate >= searchDto.FromDate.Value);
            if (searchDto.ToDate.HasValue)
                query = query.Where(i => i.InvoiceDate <= searchDto.ToDate.Value);

            int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
            var invoices = await query
                .OrderByDescending(i => i.InvoiceDate)
                .Skip(skip)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return invoices.Select(MapToDto);
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByBookingIdAsync(long bookingId)
        {
            var invoices = await _context.Invoices
                .AsNoTracking()
                .Include(i => i.Booking).ThenInclude(b => b.User)
                .Include(i => i.Booking).ThenInclude(b => b.Inventory)
                .Include(i => i.Booking).ThenInclude(b => b.Partner)
                .Where(i => i.BookingId == bookingId)
                .OrderByDescending(i => i.InvoiceDate)
                .ToListAsync();

            return invoices.Select(MapToDto);
        }

        public async Task<InvoiceResponseDto> UpdateInvoiceAsync(Invoice invoice)
        {
            var existing = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId)
                ?? throw new KeyNotFoundException($"Invoice {invoice.InvoiceId} not found.");

            existing.Amount = invoice.Amount;
            existing.BaseAmount = invoice.BaseAmount;
            existing.TaxAmount = invoice.TaxAmount;
            existing.DiscountAmount = invoice.DiscountAmount;
            existing.DueDate = invoice.DueDate;
            existing.Description = invoice.Description;
            existing.Status = invoice.Status;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(existing);
        }

        public async Task<bool> DeleteInvoiceAsync(long invoiceId)
        {
            var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
            if (invoice == null) return false;
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<InvoiceResponseDto> UpdateInvoiceStatusAsync(long invoiceId, int status)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Booking).ThenInclude(b => b.User)
                .Include(i => i.Booking).ThenInclude(b => b.Inventory)
                .Include(i => i.Booking).ThenInclude(b => b.Partner)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId)
                ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

            invoice.Status = status;
            invoice.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return MapToDto(invoice);
        }

        private static InvoiceResponseDto MapToDto(Invoice invoice)
        {
            var booking = invoice.Booking;
            int nights = booking != null && booking.CheckOutDate > booking.CheckInDate
                ? (int)(booking.CheckOutDate.Date - booking.CheckInDate.Date).TotalDays : 0;

            return new InvoiceResponseDto
            {
                InvoiceId = invoice.InvoiceId,
                InvoiceNumber = invoice.InvoiceNumber,
                BookingId = invoice.BookingId,
                UserId = booking?.UserId ?? 0,
                UserName = booking?.User?.Name,
                UserEmail = booking?.User?.Email,
                HotelName = booking?.Partner?.Name ?? booking?.Inventory?.ItemType,
                RoomType = booking?.RoomType,
                CheckInDate = booking?.CheckInDate ?? default,
                CheckOutDate = booking?.CheckOutDate ?? default,
                NumberOfNights = nights,
                InventoryName = booking?.Inventory?.ItemType,
                BaseAmount = invoice.BaseAmount,
                TaxAmount = invoice.TaxAmount,
                DiscountAmount = invoice.DiscountAmount,
                Amount = invoice.Amount,
                InvoiceDate = invoice.InvoiceDate,
                DueDate = invoice.DueDate,
                Status = invoice.Status,
                Description = invoice.Description,
                CreatedDate = invoice.CreatedDate
            };
        }
    }
}
