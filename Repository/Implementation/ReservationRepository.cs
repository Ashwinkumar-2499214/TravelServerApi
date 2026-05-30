using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation;

public class ReservationRepository : IReservationRepository
{
    private readonly AppDbContext _context;

    public ReservationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reservation> CreateReservationAsync(Reservation reservation)
    {
        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }

    public async Task<Reservation?> GetReservationByIdAsync(long reservationId)
    {
        return await _context.Reservations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.ReservationId == reservationId);
    }

    public async Task<IEnumerable<Reservation>> GetAllReservationsAsync(long? bookingId, int? status, DateTime? fromDate, DateTime? toDate, int pageNumber, int pageSize)
    {
        var query = _context.Reservations.AsNoTracking();

        if (bookingId.HasValue)
        {
            query = query.Where(r => r.BookingId == bookingId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(r => r.StartDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(r => r.EndDate <= toDate.Value);
        }

        int skip = (pageNumber - 1) * pageSize;

        return await query
            .OrderByDescending(r => r.CreatedDate)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reservation>> GetReservationsByBookingIdAsync(long bookingId)
    {
        return await _context.Reservations
            .AsNoTracking()
            .Where(r => r.BookingId == bookingId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();
    }

    public async Task<Reservation?> UpdateReservationAsync(Reservation reservation)
    {
        var existingReservation = await _context.Reservations.FirstOrDefaultAsync(r => r.ReservationId == reservation.ReservationId);
        if (existingReservation == null)
        {
            return null;
        }

        existingReservation.Details = reservation.Details;
        existingReservation.StartDate = reservation.StartDate;
        existingReservation.EndDate = reservation.EndDate;
        existingReservation.Status = reservation.Status;

        _context.Reservations.Update(existingReservation);
        await _context.SaveChangesAsync();

        return existingReservation;
    }

    public async Task<bool> DeleteReservationAsync(long reservationId)
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

    public async Task<Reservation?> UpdateReservationStatusAsync(long reservationId, int status)
    {
        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.ReservationId == reservationId);
        if (reservation == null)
        {
            return null;
        }

        reservation.Status = status;

        _context.Reservations.Update(reservation);
        await _context.SaveChangesAsync();

        return reservation;
    }
}