using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public BookingService(IBookingRepository bookingRepository, IInventoryRepository inventoryRepository)
        {
            _bookingRepository = bookingRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto bookingDto)
        {
            var inventory = await _inventoryRepository.GetInventoryByIdAsync(bookingDto.InventoryId);
            
            if (inventory?.Availability <= 0)
                throw new InvalidOperationException(BookingConstants.CannotModifyCancelledBooking);

            var booking = new Booking
            {
                UserId = bookingDto.UserId,
                PartnerId = bookingDto.PartnerId,
                InventoryId = bookingDto.InventoryId,
                ItemType = bookingDto.ItemType,
                BookingDate = DateTime.UtcNow,
                Status = (int)Enum.BookingStatus.Pending,
                Amount = bookingDto.Amount,
                CreatedDate = DateTime.UtcNow
            };

            return await _bookingRepository.CreateBookingAsync(booking);
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(long bookingId)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            return booking ?? throw new KeyNotFoundException(BookingConstants.BookingNotFound);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(BookingSearchDto searchDto)
        {
            return await _bookingRepository.GetAllBookingsAsync(searchDto);
        }

        public async Task<BookingResponseDto> UpdateBookingAsync(long bookingId, BookingRequestDto bookingDto)
        {
            var existingBooking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            
            if (existingBooking == null)
                throw new KeyNotFoundException(BookingConstants.BookingNotFound);
            
            if (existingBooking.Status == (int)Enum.BookingStatus.Cancelled)
                throw new InvalidOperationException(BookingConstants.CannotModifyCancelledBooking);

            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = bookingDto.UserId,
                PartnerId = bookingDto.PartnerId,
                InventoryId = bookingDto.InventoryId,
                ItemType = bookingDto.ItemType,
                BookingDate = DateTime.UtcNow,
                Status = existingBooking.Status,
                Amount = bookingDto.Amount,
                ModifiedDate = DateTime.UtcNow
            };

            return await _bookingRepository.UpdateBookingAsync(booking);
        }

        public async Task<bool> DeleteBookingAsync(long bookingId)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            return booking != null && await _bookingRepository.DeleteBookingAsync(bookingId);
        }

        public async Task<BookingResponseDto> UpdateBookingStatusAsync(long bookingId, int newStatus)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            return booking == null ? throw new KeyNotFoundException(BookingConstants.BookingNotFound) : await _bookingRepository.UpdateBookingStatusAsync(bookingId, newStatus);
        }
    }
}
