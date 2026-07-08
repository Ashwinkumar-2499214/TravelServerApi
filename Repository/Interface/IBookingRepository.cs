using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IBookingRepository
    {
        Task<BookingResponseDto> CreateBookingAsync(Booking booking);
        Task<BookingResponseDto> GetBookingByIdAsync(long bookingId);
        Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(BookingSearchDto searchDto);
        Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(long userId);
        Task<bool> HasDuplicateBookingAsync(long userId, long inventoryId, string roomType, DateTime checkIn, DateTime checkOut, long? excludeBookingId = null);
        Task<BookingResponseDto> UpdateBookingAsync(Booking booking);
        Task<bool> DeleteBookingAsync(long bookingId);
        Task<BookingResponseDto> UpdateBookingStatusAsync(long bookingId, int status);
    }
}
