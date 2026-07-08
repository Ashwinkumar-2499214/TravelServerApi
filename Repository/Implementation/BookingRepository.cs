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
                await _context.Entry(booking).Reference(b => b.Partner).LoadAsync();
                await _context.Entry(booking).Reference(b => b.Inventory).LoadAsync();
                return MapToDto(booking);
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException($"Database error: {ex.InnerException?.Message ?? ex.Message}", ex);
            }
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(long bookingId)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.Partner)
                .Include(b => b.Inventory)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId);

            return booking != null ? MapToDto(booking) : null;
        }

        public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(BookingSearchDto searchDto)
        {
            var query = _context.Bookings
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.Partner)
                .Include(b => b.Inventory)
                .AsQueryable();

            if (searchDto.UserId.HasValue)
                query = query.Where(b => b.UserId == searchDto.UserId.Value);
            if (searchDto.PartnerId.HasValue)
                query = query.Where(b => b.PartnerId == searchDto.PartnerId.Value);
            if (searchDto.Status.HasValue)
                query = query.Where(b => b.Status == searchDto.Status.Value);
            if (searchDto.FromDate.HasValue)
                query = query.Where(b => b.CheckInDate >= searchDto.FromDate.Value);
            if (searchDto.ToDate.HasValue)
                query = query.Where(b => b.CheckOutDate <= searchDto.ToDate.Value);

            int skip = (searchDto.PageNumber - 1) * searchDto.PageSize;
            var bookings = await query
                .OrderByDescending(b => b.CreatedDate)
                .Skip(skip)
                .Take(searchDto.PageSize)
                .ToListAsync();

            return bookings.Select(MapToDto);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetBookingsByUserIdAsync(long userId)
        {
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.Partner)
                .Include(b => b.Inventory)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CreatedDate)
                .ToListAsync();

            return bookings.Select(MapToDto);
        }

        public async Task<bool> HasDuplicateBookingAsync(long userId, long inventoryId, string roomType, DateTime checkIn, DateTime checkOut, long? excludeBookingId = null)
        {
            var query = _context.Bookings
                .Where(b => b.UserId == userId
                    && b.InventoryId == inventoryId
                    && b.RoomType == roomType
                    && b.Status != (int)Enum.BookingStatus.Cancelled
                    && b.CheckInDate < checkOut
                    && b.CheckOutDate > checkIn);

            if (excludeBookingId.HasValue)
                query = query.Where(b => b.BookingId != excludeBookingId.Value);

            return await query.AnyAsync();
        }

        public async Task<BookingResponseDto> UpdateBookingAsync(Booking booking)
        {
            var existing = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Partner)
                .Include(b => b.Inventory)
                .FirstOrDefaultAsync(b => b.BookingId == booking.BookingId)
                ?? throw new KeyNotFoundException($"Booking {booking.BookingId} not found.");

            existing.ItemType = booking.ItemType;
            existing.CheckInDate = booking.CheckInDate;
            existing.CheckOutDate = booking.CheckOutDate;
            existing.NumberOfGuests = booking.NumberOfGuests;
            existing.NumberOfRooms = booking.NumberOfRooms;
            existing.RoomType = booking.RoomType;
            existing.SpecialRequests = booking.SpecialRequests;
            existing.Amount = booking.Amount;
            existing.ModifiedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapToDto(existing);
        }

        public async Task<bool> DeleteBookingAsync(long bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
            if (booking == null) return false;
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BookingResponseDto> UpdateBookingStatusAsync(long bookingId, int status)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Partner)
                .Include(b => b.Inventory)
                .FirstOrDefaultAsync(b => b.BookingId == bookingId)
                ?? throw new KeyNotFoundException($"Booking {bookingId} not found.");

            booking.Status = status;
            booking.ModifiedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return MapToDto(booking);
        }

        private static BookingResponseDto MapToDto(Booking b)
        {
            int nights = b.CheckOutDate > b.CheckInDate
                ? (int)(b.CheckOutDate.Date - b.CheckInDate.Date).TotalDays
                : 0;

            return new BookingResponseDto
            {
                BookingId = b.BookingId,
                UserId = b.UserId,
                UserName = b.User?.Name ?? string.Empty,
                PartnerId = b.PartnerId,
                HotelName = b.Partner?.Name ?? b.Inventory?.ItemType ?? string.Empty,
                InventoryId = b.InventoryId,
                ItemType = b.ItemType,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                NumberOfNights = nights,
                NumberOfGuests = b.NumberOfGuests,
                NumberOfRooms = b.NumberOfRooms,
                RoomType = b.RoomType,
                SpecialRequests = b.SpecialRequests,
                BookingDate = b.BookingDate,
                Status = b.Status,
                Amount = b.Amount,
                CreatedDate = b.CreatedDate
            };
        }
    }
}
