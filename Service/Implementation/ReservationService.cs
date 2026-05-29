using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepository;

        public ReservationService(IReservationRepository reservationRepository)
        {
            _reservationRepository = reservationRepository;
        }

        public async Task<ReservationResponseDto> CreateReservationAsync(ReservationRequestDto reservationDto)
        {
            try
            {
                var reservation = new Reservation
                {
                    BookingId = reservationDto.BookingId,
                    Details = reservationDto.Details,
                    StartDate = reservationDto.StartDate,
                    EndDate = reservationDto.EndDate,
                    Status = (int)Enum.ReservationStatus.Confirmed,
                    CreatedDate = DateTime.UtcNow
                };

                return await _reservationRepository.CreateReservationAsync(reservation);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ReservationConstants.ReservationCreatedSuccess, ex);
            }
        }

        public async Task<ReservationResponseDto> GetReservationByIdAsync(long reservationId)
        {
            return await _reservationRepository.GetReservationByIdAsync(reservationId);
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetAllReservationsAsync(ReservationSearchDto searchDto)
        {
            return await _reservationRepository.GetAllReservationsAsync(searchDto);
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetBookingReservationsAsync(long bookingId)
        {
            return await _reservationRepository.GetReservationsByBookingIdAsync(bookingId);
        }

        public async Task<ReservationResponseDto> UpdateReservationAsync(long reservationId, ReservationRequestDto reservationDto)
        {
            try
            {
                var reservation = new Reservation
                {
                    ReservationId = reservationId,
                    BookingId = reservationDto.BookingId,
                    Details = reservationDto.Details,
                    StartDate = reservationDto.StartDate,
                    EndDate = reservationDto.EndDate,
                    ModifiedDate = DateTime.UtcNow
                };

                return await _reservationRepository.UpdateReservationAsync(reservation);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(ReservationConstants.ReservationUpdateSuccess, ex);
            }
        }

        public async Task<bool> DeleteReservationAsync(long reservationId)
        {
            return await _reservationRepository.DeleteReservationAsync(reservationId);
        }

        public async Task<ReservationResponseDto> UpdateReservationStatusAsync(long reservationId, int newStatus)
        {
            return await _reservationRepository.UpdateReservationStatusAsync(reservationId, newStatus);
        }
    }
}
