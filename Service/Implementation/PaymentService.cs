using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentResponseDto> CreatePaymentAsync(PaymentRequestDto paymentDto)
        {
            try
            {
                var payment = new Payment
                {
                    InvoiceId = paymentDto.InvoiceId,
                    Amount = paymentDto.Amount,
                    PaymentDate = DateTime.UtcNow,
                    Method = paymentDto.Method,
                    Status = (int)Enum.PaymentStatus.Processing,
                    TransactionReference = paymentDto.TransactionReference,
                    CreatedDate = DateTime.UtcNow
                };

                return await _paymentRepository.CreatePaymentAsync(payment);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(PaymentConstants.PaymentCreatedSuccess, ex);
            }
        }

        public async Task<PaymentResponseDto> GetPaymentByIdAsync(long paymentId)
        {
            return await _paymentRepository.GetPaymentByIdAsync(paymentId);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetAllPaymentsAsync(PaymentSearchDto searchDto)
        {
            return await _paymentRepository.GetAllPaymentsAsync(searchDto);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetInvoicePaymentsAsync(long invoiceId)
        {
            return await _paymentRepository.GetPaymentsByInvoiceIdAsync(invoiceId);
        }

        public async Task<PaymentResponseDto> UpdatePaymentStatusAsync(long paymentId, int newStatus)
        {
            return await _paymentRepository.UpdatePaymentStatusAsync(paymentId, newStatus);
        }

        public async Task<bool> ProcessRefundAsync(long paymentId, decimal refundAmount, string reason)
        {
            return await _paymentRepository.ProcessRefundAsync(paymentId, refundAmount);
        }
    }
}
