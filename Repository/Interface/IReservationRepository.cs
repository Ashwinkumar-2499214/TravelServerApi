using TravelEaseServer.Dto;
using TravelEaseServer.Model;

namespace TravelEaseServer.Repository.Interface
{
    public interface IReservationRepository
    {
        Task<ReservationResponseDto> CreateReservationAsync(Reservation reservation);
        Task<ReservationResponseDto> GetReservationByIdAsync(long reservationId);
        Task<IEnumerable<ReservationResponseDto>> GetAllReservationsAsync(ReservationSearchDto searchDto);
        Task<IEnumerable<ReservationResponseDto>> GetReservationsByBookingIdAsync(long bookingId);
        Task<ReservationResponseDto> UpdateReservationAsync(Reservation reservation);
        Task<bool> DeleteReservationAsync(long reservationId);
        Task<ReservationResponseDto> UpdateReservationStatusAsync(long reservationId, int status);
    }
}
