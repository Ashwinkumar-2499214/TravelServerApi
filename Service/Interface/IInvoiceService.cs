using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IInvoiceService
    {
        Task<InvoiceResponseDto> CreateInvoiceAsync(InvoiceRequestDto invoiceDto);
        Task<InvoiceResponseDto> GetInvoiceByIdAsync(long invoiceId);
        Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync(InvoiceSearchDto searchDto);
        Task<IEnumerable<InvoiceResponseDto>> GetBookingInvoicesAsync(long bookingId);
        Task<InvoiceResponseDto> UpdateInvoiceAsync(long invoiceId, InvoiceRequestDto invoiceDto);
        Task<bool> DeleteInvoiceAsync(long invoiceId);
        Task<InvoiceResponseDto> UpdateInvoiceStatusAsync(long invoiceId, int newStatus);
    }
}
