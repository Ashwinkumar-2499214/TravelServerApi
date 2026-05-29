using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IBookingService
    {
        Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto bookingDto);
        Task<BookingResponseDto> GetBookingByIdAsync(long bookingId);
        Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(BookingSearchDto searchDto);
        Task<BookingResponseDto> UpdateBookingAsync(long bookingId, BookingRequestDto bookingDto);
        Task<bool> DeleteBookingAsync(long bookingId);
        Task<BookingResponseDto> UpdateBookingStatusAsync(long bookingId, int newStatus);
    }
}
