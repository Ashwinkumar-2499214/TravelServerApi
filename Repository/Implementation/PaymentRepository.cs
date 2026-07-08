using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class PaymentRepository : IPaymentRepository
    {
        private readonly AppDbContext _context;

        public PaymentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(Payment payment)
        {
            try
            {
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();
                return MapPaymentToDto(payment);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating payment in database.", ex);
            }
        }

        public async Task<PaymentResponseDto> GetPaymentByIdAsync(long paymentId)
        {
            try
            {
                var payment = await _context.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                return payment != null ? MapPaymentToDto(payment) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payment with ID {paymentId}.", ex);
            }
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync(PaymentSearchDto searchDto)
        {
            try
            {
                var query = _context.Payments.AsNoTracking();

                // Apply filters
                if (searchDto.InvoiceId.HasValue)
                {
                    query = query.Where(p => p.InvoiceId == searchDto.InvoiceId.Value);
                }

                if (searchDto.Status.HasValue)
                {
                    query = query.Where(p => p.Status == searchDto.Status.Value);
                }

                if (searchDto.Method.HasValue)
                {
                    query = query.Where(p => p.Method == searchDto.Method.Value);
                }

                if (searchDto.FromDate.HasValue)
                {
                    query = query.Where(p => p.PaymentDate >= searchDto.FromDate.Value);
                }

                if (searchDto.ToDate.HasValue)
                {
                    query = query.Where(p => p.PaymentDate <= searchDto.ToDate.Value);
                }

                // Apply pagination
                int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var payments = await query
                    .OrderByDescending(p => p.PaymentDate)
                    .Skip(skip)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return payments.Select(MapPaymentToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving payments.", ex);
            }
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByInvoiceIdAsync(long invoiceId)
        {
            try
            {
                var payments = await _context.Payments
                    .AsNoTracking()
                    .Where(p => p.InvoiceId == invoiceId)
                    .OrderByDescending(p => p.PaymentDate)
                    .ToListAsync();

                return payments.Select(MapPaymentToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving payments for invoice with ID {invoiceId}.", ex);
            }
        }

        public async Task<PaymentResponseDto> UpdatePaymentAsync(Payment payment)
        {
            try
            {
                var existingPayment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == payment.PaymentId);
                if (existingPayment == null)
                {
                    throw new KeyNotFoundException($"Payment with ID {payment.PaymentId} not found.");
                }

                existingPayment.Amount = payment.Amount;
                existingPayment.PaymentDate = payment.PaymentDate;
                existingPayment.Method = payment.Method;
                existingPayment.TransactionReference = payment.TransactionReference;
                existingPayment.Status = payment.Status;

                _context.Payments.Update(existingPayment);
                await _context.SaveChangesAsync();

                return MapPaymentToDto(existingPayment);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating payment with ID {payment.PaymentId}.", ex);
            }
        }

        public async Task<PaymentResponseDto> UpdatePaymentStatusAsync(long paymentId, int status)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
                if (payment == null)
                {
                    throw new KeyNotFoundException($"Payment with ID {paymentId} not found.");
                }

                payment.Status = status;

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                return MapPaymentToDto(payment);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating payment status with ID {paymentId}.", ex);
            }
        }

        public async Task<bool> ProcessRefundAsync(long paymentId, decimal refundAmount)
        {
            try
            {
                var payment = await _context.Payments.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
                if (payment == null)
                {
                    throw new KeyNotFoundException($"Payment with ID {paymentId} not found.");
                }

                if (refundAmount <= 0 || refundAmount > payment.Amount)
                {
                    throw new InvalidOperationException("Invalid refund amount.");
                }

                // Update the payment amount to reflect the refund
                payment.Amount -= refundAmount;
                payment.Status = 2; // Assuming 2 is refunded status

                _context.Payments.Update(payment);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error processing refund for payment with ID {paymentId}.", ex);
            }
        }

        private PaymentResponseDto MapPaymentToDto(Payment payment)
        {
            return new PaymentResponseDto
            {
                PaymentId = payment.PaymentId,
                UserId = 0,
                InvoiceId = payment.InvoiceId,
                BookingId = payment.BookingId,
                Amount = payment.Amount,
                Currency = payment.Currency,
                PaymentDate = payment.PaymentDate,
                Method = payment.Method,
                Status = payment.Status,
                TransactionReference = payment.TransactionReference,
                GatewayProvider = payment.GatewayProvider,
                BillingName = payment.BillingName,
                BillingEmail = payment.BillingEmail,
                BillingPhone = payment.BillingPhone,
                Notes = payment.Notes,
                CreatedDate = payment.CreatedDate
            };
        }
    }
}
