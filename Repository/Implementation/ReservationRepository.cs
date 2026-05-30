using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ReservationResponseDto> CreateReservationAsync(Reservation reservation)
        {
            try
            {
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                return MapReservationToDto(reservation);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Error creating reservation in database.", ex);
            }
        }

        public async Task<ReservationResponseDto> GetReservationByIdAsync(long reservationId)
        {
            try
            {
                var reservation = await _context.Reservations
                    .AsNoTracking()
                    .FirstOrDefaultAsync(r => r.ReservationId == reservationId);

                return reservation != null ? MapReservationToDto(reservation) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservation with ID {reservationId}.", ex);
            }
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetAllReservationsAsync(ReservationSearchDto searchDto)
        {
            try
            {
                var query = _context.Reservations.AsNoTracking();

                // Apply filters
                if (searchDto.BookingId.HasValue)
                {
                    query = query.Where(r => r.BookingId == searchDto.BookingId.Value);
                }

                if (searchDto.Status.HasValue)
                {
                    query = query.Where(r => r.Status == searchDto.Status.Value);
                }

                if (searchDto.FromDate.HasValue)
                {
                    query = query.Where(r => r.StartDate >= searchDto.FromDate.Value);
                }

                if (searchDto.ToDate.HasValue)
                {
                    query = query.Where(r => r.EndDate <= searchDto.ToDate.Value);
                }

                // Apply pagination
                int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var reservations = await query
                    .OrderByDescending(r => r.CreatedDate)
                    .Skip(skip)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return reservations.Select(MapReservationToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving reservations.", ex);
            }
        }

        public async Task<IEnumerable<ReservationResponseDto>> GetReservationsByBookingIdAsync(long bookingId)
        {
            try
            {
                var reservations = await _context.Reservations
                    .AsNoTracking()
                    .Where(r => r.BookingId == bookingId)
                    .OrderByDescending(r => r.CreatedDate)
                    .ToListAsync();

                return reservations.Select(MapReservationToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving reservations for booking with ID {bookingId}.", ex);
            }
        }

        public async Task<ReservationResponseDto> UpdateReservationAsync(Reservation reservation)
        {
            try
            {
                var existingReservation = await _context.Reservations.FirstOrDefaultAsync(r => r.ReservationId == reservation.ReservationId);
                if (existingReservation == null)
                {
                    throw new KeyNotFoundException($"Reservation with ID {reservation.ReservationId} not found.");
                }

                existingReservation.Details = reservation.Details;
                existingReservation.StartDate = reservation.StartDate;
                existingReservation.EndDate = reservation.EndDate;
                existingReservation.Status = reservation.Status;

                _context.Reservations.Update(existingReservation);
                await _context.SaveChangesAsync();

                return MapReservationToDto(existingReservation);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating reservation with ID {reservation.ReservationId}.", ex);
            }
        }

        public async Task<bool> DeleteReservationAsync(long reservationId)
        {
            try
            {
                var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.ReservationId == reservationId);
                if (reservation == null)
                {
                    return false;
                }

                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error deleting reservation with ID {reservationId}.", ex);
            }
        }

        public async Task<ReservationResponseDto> UpdateReservationStatusAsync(long reservationId, int status)
        {
            try
            {
                var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.ReservationId == reservationId);
                if (reservation == null)
                {
                    throw new KeyNotFoundException($"Reservation with ID {reservationId} not found.");
                }

                reservation.Status = status;

                _context.Reservations.Update(reservation);
                await _context.SaveChangesAsync();

                return MapReservationToDto(reservation);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating reservation status with ID {reservationId}.", ex);
            }
        }

        private ReservationResponseDto MapReservationToDto(Reservation reservation)
        {
            return new ReservationResponseDto
            {
                ReservationId = reservation.ReservationId,
                BookingId = reservation.BookingId,
                Details = reservation.Details,
                StartDate = reservation.StartDate,
                EndDate = reservation.EndDate,
                Status = reservation.Status,
                CreatedDate = reservation.CreatedDate
            };
        }
    }
}
