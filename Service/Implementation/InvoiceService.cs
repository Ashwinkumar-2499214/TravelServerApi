using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }

        public async Task<InvoiceResponseDto> CreateInvoiceAsync(InvoiceRequestDto invoiceDto)
        {
            try
            {
                var invoice = new Invoice
                {
                    BookingId = invoiceDto.BookingId,
                    Amount = invoiceDto.Amount,
                    InvoiceDate = DateTime.UtcNow,
                    DueDate = invoiceDto.DueDate,
                    Status = (int)Enum.InvoiceStatus.Issued,
                    Description = invoiceDto.Description,
                    CreatedDate = DateTime.UtcNow
                };

                return await _invoiceRepository.CreateInvoiceAsync(invoice);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(InvoiceConstants.InvoiceCreatedSuccess, ex);
            }
        }

        public async Task<InvoiceResponseDto> GetInvoiceByIdAsync(long invoiceId)
        {
            return await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync(InvoiceSearchDto searchDto)
        {
            return await _invoiceRepository.GetAllInvoicesAsync(searchDto);
        }

        public async Task<IEnumerable<InvoiceResponseDto>> GetBookingInvoicesAsync(long bookingId)
        {
            return await _invoiceRepository.GetInvoicesByBookingIdAsync(bookingId);
        }

        public async Task<InvoiceResponseDto> UpdateInvoiceAsync(long invoiceId, InvoiceRequestDto invoiceDto)
        {
            try
            {
                var invoice = new Invoice
                {
                    InvoiceId = invoiceId,
                    BookingId = invoiceDto.BookingId,
                    Amount = invoiceDto.Amount,
                    DueDate = invoiceDto.DueDate,
                    Description = invoiceDto.Description,
                    ModifiedDate = DateTime.UtcNow
                };

                return await _invoiceRepository.UpdateInvoiceAsync(invoice);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(InvoiceConstants.InvoiceUpdateSuccess, ex);
            }
        }

        public async Task<bool> DeleteInvoiceAsync(long invoiceId)
        {
            return await _invoiceRepository.DeleteInvoiceAsync(invoiceId);
        }

        public async Task<InvoiceResponseDto> UpdateInvoiceStatusAsync(long invoiceId, int newStatus)
        {
            return await _invoiceRepository.UpdateInvoiceStatusAsync(invoiceId, newStatus);
        }
    }
}
