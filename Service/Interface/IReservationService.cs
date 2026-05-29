using TravelEaseServer.Dto;

namespace TravelEaseServer.Service.Interface
{
    public interface IReservationService
    {
        Task<ReservationResponseDto> CreateReservationAsync(ReservationRequestDto reservationDto);
        Task<ReservationResponseDto> GetReservationByIdAsync(long reservationId);
        Task<IEnumerable<ReservationResponseDto>> GetAllReservationsAsync(ReservationSearchDto searchDto);
        Task<IEnumerable<ReservationResponseDto>> GetBookingReservationsAsync(long bookingId);
        Task<ReservationResponseDto> UpdateReservationAsync(long reservationId, ReservationRequestDto reservationDto);
        Task<bool> DeleteReservationAsync(long reservationId);
        Task<ReservationResponseDto> UpdateReservationStatusAsync(long reservationId, int newStatus);
    }
}
