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
            try
            {
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
                return MapInvoiceToDto(invoice);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating invoice in database.", ex);
            }
        }

        public async Task<InvoiceResponseDto> GetInvoiceByIdAsync(long invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

                return invoice != null ? MapInvoiceToDto(invoice) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoice with ID {invoiceId}.", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync(InvoiceSearchDto searchDto)
        {
            try
            {
                var query = _context.Invoices.AsNoTracking();

                // Apply filters
                if (searchDto.BookingId.HasValue)
                {
                    query = query.Where(i => i.BookingId == searchDto.BookingId.Value);
                }

                if (searchDto.Status.HasValue)
                {
                    query = query.Where(i => i.Status == searchDto.Status.Value);
                }

                if (searchDto.FromDate.HasValue)
                {
                    query = query.Where(i => i.InvoiceDate >= searchDto.FromDate.Value);
                }

                if (searchDto.ToDate.HasValue)
                {
                    query = query.Where(i => i.InvoiceDate <= searchDto.ToDate.Value);
                }

                // Apply pagination
                int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var invoices = await query
                    .OrderByDescending(i => i.InvoiceDate)
                    .Skip(skip)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return invoices.Select(MapInvoiceToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving invoices.", ex);
            }
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByBookingIdAsync(long bookingId)
        {
            try
            {
                var invoices = await _context.Invoices
                    .AsNoTracking()
                    .Where(i => i.BookingId == bookingId)
                    .OrderByDescending(i => i.InvoiceDate)
                    .ToListAsync();

                return invoices.Select(MapInvoiceToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving invoices for booking with ID {bookingId}.", ex);
            }
        }

        public async Task<InvoiceResponseDto> UpdateInvoiceAsync(Invoice invoice)
        {
            try
            {
                var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);
                if (existingInvoice == null)
                {
                    throw new KeyNotFoundException($"Invoice with ID {invoice.InvoiceId} not found.");
                }

                existingInvoice.Amount = invoice.Amount;
                existingInvoice.DueDate = invoice.DueDate;
                existingInvoice.Description = invoice.Description;
                existingInvoice.Status = invoice.Status;

                _context.Invoices.Update(existingInvoice);
                await _context.SaveChangesAsync();

                return MapInvoiceToDto(existingInvoice);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating invoice with ID {invoice.InvoiceId}.", ex);
            }
        }

        public async Task<bool> DeleteInvoiceAsync(long invoiceId)
        {
            try
            {
                var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
                if (invoice == null)
                {
                    return false;
                }

                _context.Invoices.Remove(invoice);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error deleting invoice with ID {invoiceId}.", ex);
            }
        }

        public async Task<InvoiceResponseDto> UpdateInvoiceStatusAsync(long invoiceId, int status)
        {
            try
            {
                var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
                if (invoice == null)
                {
                    throw new KeyNotFoundException($"Invoice with ID {invoiceId} not found.");
                }

                invoice.Status = status;

                _context.Invoices.Update(invoice);
                await _context.SaveChangesAsync();

                return MapInvoiceToDto(invoice);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating invoice status with ID {invoiceId}.", ex);
            }
        }

        private InvoiceResponseDto MapInvoiceToDto(Invoice invoice)
        {
            return new InvoiceResponseDto
            {
                InvoiceId = invoice.InvoiceId,
                BookingId = invoice.BookingId,
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
