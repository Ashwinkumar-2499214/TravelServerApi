using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IPaymentService
    {
        Task<PaymentResponseDto> CreatePaymentAsync(PaymentRequestDto paymentDto);
        Task<PaymentResponseDto> GetPaymentByIdAsync(long paymentId);
        Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync(PaymentSearchDto searchDto);
        Task<IEnumerable<PaymentResponseDto>> GetInvoicePaymentsAsync(long invoiceId);
        Task<PaymentResponseDto> UpdatePaymentStatusAsync(long paymentId, int newStatus);
        Task<bool> ProcessRefundAsync(long paymentId, decimal refundAmount, string reason);
    }
}
