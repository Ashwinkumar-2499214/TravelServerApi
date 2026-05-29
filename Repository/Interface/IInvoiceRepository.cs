using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IInvoiceRepository
    {
        Task<InvoiceResponseDto> CreateInvoiceAsync(Invoice invoice);
        Task<InvoiceResponseDto> GetInvoiceByIdAsync(long invoiceId);
        Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync(InvoiceSearchDto searchDto);
        Task<IEnumerable<InvoiceResponseDto>> GetInvoicesByBookingIdAsync(long bookingId);
        Task<InvoiceResponseDto> UpdateInvoiceAsync(Invoice invoice);
        Task<bool> DeleteInvoiceAsync(long invoiceId);
        Task<InvoiceResponseDto> UpdateInvoiceStatusAsync(long invoiceId, int status);
    }
}
