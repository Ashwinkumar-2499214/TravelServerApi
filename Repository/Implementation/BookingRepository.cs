using Microsoft.EntityFrameworkCore;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;

namespace TravelEaseServer.Repository.Implementation
{
    public class BookingRepository : IBookingRepository
    {
        private readonly AppDbContext _context;

        public BookingRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(Booking booking)
        {
            try
            {
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                await _context.Entry(booking).Reference(b => b.User).LoadAsync();
                return MapBookingToDto(booking);
            }
            catch (DbUpdateException ex)
            {
                var sqlErrorMessage = ex.InnerException?.Message ?? ex.Message;
                throw new InvalidOperationException($"Database Rejected: {sqlErrorMessage}", ex);
            }
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(long bookingId)
        {
            try
            {
                var booking = await _context.Bookings
                    .AsNoTracking()
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);

                return booking != null ? MapBookingToDto(booking) : null;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving booking with ID {bookingId}.", ex);
            }
        }

        public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(BookingSearchDto searchDto)
        {
            try
            {
                var query = _context.Bookings.AsNoTracking().Include(b => b.User).AsQueryable();

                // Apply filters
                if (searchDto.UserId.HasValue)
                {
                    query = query.Where(b => b.UserId == searchDto.UserId.Value);
                }

                if (searchDto.PartnerId.HasValue)
                {
                    query = query.Where(b => b.PartnerId == searchDto.PartnerId.Value);
                }

                if (searchDto.Status.HasValue)
                {
                    query = query.Where(b => b.Status == searchDto.Status.Value);
                }

                if (searchDto.FromDate.HasValue)
                {
                    query = query.Where(b => b.BookingDate >= searchDto.FromDate.Value);
                }

                if (searchDto.ToDate.HasValue)
                {
                    query = query.Where(b => b.BookingDate <= searchDto.ToDate.Value);
                }

                // Apply pagination
                int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
                var bookings = await query
                    .OrderByDescending(b => b.BookingDate)
                    .Skip(skip)
                    .Take(searchDto.PageSize)
                    .ToListAsync();

                return bookings.Select(MapBookingToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error retrieving bookings.", ex);
            }
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(long userId)
        {
            try
            {
                var bookings = await _context.Bookings
                    .AsNoTracking()
                    .Include(b => b.User)
                    .Where(b => b.UserId == userId)
                    .OrderByDescending(b => b.BookingDate)
                    .ToListAsync();

                return bookings.Select(MapBookingToDto);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Error retrieving bookings for user with ID {userId}.", ex);
            }
        }

        public async Task<BookingResponseDto> UpdateBookingAsync(Booking booking)
        {
            try
            {
                var existingBooking = await _context.Bookings
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);
                if (existingBooking == null)
                {
                    throw new KeyNotFoundException($"Booking with ID {booking.BookingId} not found.");
                }

                existingBooking.ItemType = booking.ItemType;
                existingBooking.BookingDate = booking.BookingDate;
                existingBooking.Status = booking.Status;
                existingBooking.Amount = booking.Amount;
                existingBooking.ModifiedDate = DateTime.UtcNow;

                _context.Bookings.Update(existingBooking);
                await _context.SaveChangesAsync();

                return MapBookingToDto(existingBooking);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating booking with ID {booking.BookingId}.", ex);
            }
        }

        public async Task<bool> DeleteBookingAsync(long bookingId)
        {
            try
            {
                var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
                if (booking == null)
                {
                    return false;
                }

                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error deleting booking with ID {bookingId}.", ex);
            }
        }

        public async Task<BookingResponseDto> UpdateBookingStatusAsync(long bookingId, int status)
        {
            try
            {
                var booking = await _context.Bookings
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b => b.BookingId == bookingId);
                if (booking == null)
                {
                    throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");
                }

                booking.Status = status;
                booking.ModifiedDate = DateTime.UtcNow;

                _context.Bookings.Update(booking);
                await _context.SaveChangesAsync();

                return MapBookingToDto(booking);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Error updating booking status with ID {bookingId}.", ex);
            }
        }

        private BookingResponseDto MapBookingToDto(Booking booking)
        {
            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                UserId = booking.UserId,
                UserName = booking.User?.Name ?? string.Empty,
                PartnerId = booking.PartnerId,
                InventoryId = booking.InventoryId,
                ItemType = booking.ItemType,
                BookingDate = booking.BookingDate,
                Status = booking.Status,
                Amount = booking.Amount,
                CreatedDate = booking.CreatedDate
            };
        }
    }
}
