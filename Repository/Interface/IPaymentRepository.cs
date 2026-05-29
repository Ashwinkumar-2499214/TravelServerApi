using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IPaymentRepository
    {
        Task<PaymentResponseDto> CreatePaymentAsync(Payment payment);
        Task<PaymentResponseDto> GetPaymentByIdAsync(long paymentId);
        Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync(PaymentSearchDto searchDto);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByInvoiceIdAsync(long invoiceId);
        Task<PaymentResponseDto> UpdatePaymentAsync(Payment payment);
        Task<PaymentResponseDto> UpdatePaymentStatusAsync(long paymentId, int status);
        Task<bool> ProcessRefundAsync(long paymentId, decimal refundAmount);
    }
}
