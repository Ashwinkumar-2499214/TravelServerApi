using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Payment> CreatePaymentAsync(Payment payment)
    {
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<Payment?> GetPaymentByIdAsync(long paymentId)
    {
        return await _context.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
    }

    public async Task<IEnumerable<Payment>> GetAllPaymentsAsync(long? invoiceId, int? status, int? method, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
    {
        var query = _context.Payments.AsNoTracking();

        if (invoiceId.HasValue)
        {
            query = query.Where(p => p.InvoiceId == invoiceId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (method.HasValue)
        {
            query = query.Where(p => p.Method == method.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= toDate.Value);
        }

        int skip = (pageNumber - 1) * pageSize;

        return await query
            .OrderByDescending(p => p.PaymentDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(long invoiceId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.InvoiceId == invoiceId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();
    }

    public async Task<Payment?> UpdatePaymentAsync(Payment payment)
    {
        var existingPayment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == payment.PaymentId);
        if (existingPayment == null)
        {
            return null;
        }

        existingPayment.Amount = payment.Amount;
        existingPayment.PaymentDate = payment.PaymentDate;
        existingPayment.Method = payment.Method;
        existingPayment.TransactionReference = payment.TransactionReference;
        existingPayment.Status = payment.Status;

        _context.Payments.Update(existingPayment);
        await _context.SaveChangesAsync();

        return existingPayment;
    }

    public async Task<Payment?> UpdatePaymentStatusAsync(long paymentId, int status)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        if (payment == null)
        {
            return null;
        }

        payment.Status = status;

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();

        return payment;
    }

    public async Task<bool> ProcessRefundAsync(long paymentId, decimal refundAmount)
    {
        var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        if (payment == null || refundAmount <= 0 || refundAmount > payment.Amount)
        {
            return false;
        }

        payment.Amount -= refundAmount;
        payment.Status = 2;

        _context.Payments.Update(payment);
        await _context.SaveChangesAsync();

        return true;
    }
}