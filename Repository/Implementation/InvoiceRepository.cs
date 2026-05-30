using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation;

public class InvoiceRepository : IInvoiceRepository
{
    private readonly AppDbContext _context;

    public InvoiceRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Invoice> CreateInvoiceAsync(Invoice invoice)
    {
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
        return invoice;
    }

    public async Task<Invoice?> GetInvoiceByIdAsync(long invoiceId)
    {
        return await _context.Invoices
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
    }

    public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync(long? bookingId, int? status, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
    {
        var query = _context.Invoices.AsNoTracking();

        if (bookingId.HasValue)
        {
            query = query.Where(i => i.BookingId == bookingId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(i => i.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(i => i.InvoiceDate <= toDate.Value);
        }

        int skip = (pageNumber - 1) * pageSize;

        return await query
            .OrderByDescending(i => i.InvoiceDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByBookingIdAsync(long bookingId)
    {
        return await _context.Invoices
            .AsNoTracking()
            .Where(i => i.BookingId == bookingId)
            .OrderByDescending(i => i.InvoiceDate)
            .ToListAsync();
    }

    public async Task<Invoice?> UpdateInvoiceAsync(Invoice invoice)
    {
        var existingInvoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoice.InvoiceId);
        if (existingInvoice == null)
        {
            return null;
        }

        existingInvoice.Amount = invoice.Amount;
        existingInvoice.DueDate = invoice.DueDate;
        existingInvoice.Description = invoice.Description;
        existingInvoice.Status = invoice.Status;

        _context.Invoices.Update(existingInvoice);
        await _context.SaveChangesAsync();

        return existingInvoice;
    }

    public async Task<bool> DeleteInvoiceAsync(long invoiceId)
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

    public async Task<Invoice?> UpdateInvoiceStatusAsync(long invoiceId, int status)
    {
        var invoice = await _context.Invoices.FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        if (invoice == null)
        {
            return null;
        }

        invoice.Status = status;

        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();

        return invoice;
    }
}