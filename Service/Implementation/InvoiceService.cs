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

        public async Task<InvoiceResponseDto> CreateInvoiceAsync(InvoiceRequestDto dto)
        {
            decimal baseAmount = dto.BaseAmount > 0 ? dto.BaseAmount : dto.Amount;
            decimal tax = dto.TaxAmount;
            decimal discount = dto.DiscountAmount;
            decimal total = baseAmount + tax - discount;

            var invoice = new Invoice
            {
                BookingId = dto.BookingId,
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
                BaseAmount = baseAmount,
                TaxAmount = tax,
                DiscountAmount = discount,
                Amount = total > 0 ? total : dto.Amount,
                InvoiceDate = DateTime.UtcNow,
                DueDate = dto.DueDate == default ? DateTime.UtcNow.AddDays(7) : dto.DueDate,
                Status = (int)Enum.InvoiceStatus.Issued,
                Description = dto.Description ?? "Hotel Booking Invoice",
                CreatedDate = DateTime.UtcNow
            };

            return await _invoiceRepository.CreateInvoiceAsync(invoice);
        }

        public async Task<InvoiceResponseDto> GetInvoiceByIdAsync(long invoiceId) =>
            await _invoiceRepository.GetInvoiceByIdAsync(invoiceId);

        public async Task<IEnumerable<InvoiceResponseDto>> GetAllInvoicesAsync(InvoiceSearchDto searchDto) =>
            await _invoiceRepository.GetAllInvoicesAsync(searchDto);

        public async Task<IEnumerable<InvoiceResponseDto>> GetBookingInvoicesAsync(long bookingId) =>
            await _invoiceRepository.GetInvoicesByBookingIdAsync(bookingId);

        public async Task<InvoiceResponseDto> UpdateInvoiceAsync(long invoiceId, InvoiceRequestDto dto)
        {
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                BookingId = dto.BookingId,
                BaseAmount = dto.BaseAmount,
                TaxAmount = dto.TaxAmount,
                DiscountAmount = dto.DiscountAmount,
                Amount = dto.Amount,
                DueDate = dto.DueDate,
                Description = dto.Description,
                ModifiedDate = DateTime.UtcNow
            };
            return await _invoiceRepository.UpdateInvoiceAsync(invoice);
        }

        public async Task<bool> DeleteInvoiceAsync(long invoiceId) =>
            await _invoiceRepository.DeleteInvoiceAsync(invoiceId);

        public async Task<InvoiceResponseDto> UpdateInvoiceStatusAsync(long invoiceId, int newStatus) =>
            await _invoiceRepository.UpdateInvoiceStatusAsync(invoiceId, newStatus);
    }
}
