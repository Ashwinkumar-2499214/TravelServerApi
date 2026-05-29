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
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            return MapBookingToDto(booking);
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(long bookingId)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            return booking != null ? MapBookingToDto(booking) : null;
        }

        public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(BookingSearchDto searchDto)
        {
            var query = _context.Bookings.AsNoTracking();

            query = searchDto.UserId.HasValue ? query.Where(b => b.UserId == searchDto.UserId.Value) : query;
            query = searchDto.PartnerId.HasValue ? query.Where(b => b.PartnerId == searchDto.PartnerId.Value) : query;
            query = searchDto.Status.HasValue ? query.Where(b => b.Status == searchDto.Status.Value) : query;
            query = searchDto.FromDate.HasValue ? query.Where(b => b.BookingDate >= searchDto.FromDate.Value) : query;
            query = searchDto.ToDate.HasValue ? query.Where(b => b.BookingDate <= searchDto.ToDate.Value) : query;

            int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
            var bookings = await query
                .OrderByDescending(b => b.BookingDate)
                .Skip(skip)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return bookings.Select(MapBookingToDto);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(long userId)
        {
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToListAsync();

            return bookings.Select(MapBookingToDto);
        }

        public async Task<BookingResponseDto> UpdateBookingAsync(Booking booking)
        {
            var existingBooking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == booking.BookingId);
            
            if (existingBooking == null)
                throw new KeyNotFoundException($"Booking with ID {booking.BookingId} not found.");

            existingBooking.ItemType = booking.ItemType;
            existingBooking.BookingDate = booking.BookingDate;
            existingBooking.Status = booking.Status;
            existingBooking.Amount = booking.Amount;
            existingBooking.ModifiedDate = DateTime.UtcNow;

            _context.Bookings.Update(existingBooking);
            await _context.SaveChangesAsync();

            return MapBookingToDto(existingBooking);
        }

        public async Task<bool> DeleteBookingAsync(long bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            
            if (booking == null)
                return false;

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BookingResponseDto> UpdateBookingStatusAsync(long bookingId, int status)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            
            if (booking == null)
                throw new KeyNotFoundException($"Booking with ID {bookingId} not found.");

            booking.Status = status;
            booking.ModifiedDate = DateTime.UtcNow;

            _context.Bookings.Update(booking);
            await _context.SaveChangesAsync();

            return MapBookingToDto(booking);
        }

        private BookingResponseDto MapBookingToDto(Booking booking)
        {
            return new BookingResponseDto
            {
                BookingId = booking.BookingId,
                UserId = booking.UserId,
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
