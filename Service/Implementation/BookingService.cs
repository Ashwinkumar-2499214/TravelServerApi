using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Model;
using TravelEaseServer.Repository.Interface;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Service.Implementation
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IInventoryRepository _inventoryRepository;

        public BookingService(IBookingRepository bookingRepository, IInventoryRepository inventoryRepository)
        {
            _bookingRepository = bookingRepository;
            _inventoryRepository = inventoryRepository;
        }

        public async Task<BookingResponseDto> CreateBookingAsync(BookingRequestDto dto)
        {
            if (dto.CheckOutDate <= dto.CheckInDate)
                throw new ArgumentException("Check-out date must be after check-in date.");
            if (dto.NumberOfGuests <= 0)
                throw new ArgumentException("Number of guests must be greater than 0.");
            if (dto.NumberOfRooms <= 0)
                throw new ArgumentException("Number of rooms must be greater than 0.");
            if (string.IsNullOrWhiteSpace(dto.RoomType))
                throw new ArgumentException("Room type is required.");

            var inventory = await _inventoryRepository.GetInventoryByIdAsync(dto.InventoryId);
            if (inventory?.Availability <= 0)
                throw new InvalidOperationException("Selected inventory is not available.");

            var isDuplicate = await _bookingRepository.HasDuplicateBookingAsync(
                dto.UserId, dto.InventoryId, dto.RoomType, dto.CheckInDate, dto.CheckOutDate);
            if (isDuplicate)
                throw new InvalidOperationException("An active booking already exists for this room and date range.");

            int nights = (int)(dto.CheckOutDate.Date - dto.CheckInDate.Date).TotalDays;
            decimal totalAmount = (inventory?.Price ?? 0) * nights * dto.NumberOfRooms;

            var booking = new Booking
            {
                UserId = dto.UserId,
                PartnerId = dto.PartnerId,
                InventoryId = dto.InventoryId,
                ItemType = dto.ItemType,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                NumberOfGuests = dto.NumberOfGuests,
                NumberOfRooms = dto.NumberOfRooms,
                RoomType = dto.RoomType,
                SpecialRequests = dto.SpecialRequests,
                BookingDate = DateTime.UtcNow,
                Status = (int)Enum.BookingStatus.Pending,
                Amount = totalAmount,
                CreatedDate = DateTime.UtcNow
            };

            var result = await _bookingRepository.CreateBookingAsync(booking);

            // Decrement inventory availability after successful booking
            var newAvailability = inventory.Availability - dto.NumberOfRooms;
            if (newAvailability < 0) newAvailability = 0;
            int newStatus = newAvailability == 0
                ? (int)Enum.InventoryStatus.SoldOut
                : newAvailability <= 3
                    ? (int)Enum.InventoryStatus.Limited
                    : (int)Enum.InventoryStatus.Available;
            await _inventoryRepository.UpdateAvailabilityAsync(dto.InventoryId, newAvailability, newStatus);

            return result;
        }

        public async Task<BookingResponseDto> GetBookingByIdAsync(long bookingId)
        {
            return await _bookingRepository.GetBookingByIdAsync(bookingId)
                ?? throw new KeyNotFoundException(BookingConstants.BookingNotFound);
        }

        public async Task<IEnumerable<BookingResponseDto>> GetAllBookingsAsync(BookingSearchDto searchDto)
        {
            return await _bookingRepository.GetAllBookingsAsync(searchDto);
        }

        public async Task<BookingResponseDto> UpdateBookingAsync(long bookingId, BookingRequestDto dto)
        {
            var existing = await _bookingRepository.GetBookingByIdAsync(bookingId)
                ?? throw new KeyNotFoundException(BookingConstants.BookingNotFound);

            if (existing.Status == (int)Enum.BookingStatus.Cancelled)
                throw new InvalidOperationException(BookingConstants.CannotModifyCancelledBooking);

            if (dto.CheckOutDate <= dto.CheckInDate)
                throw new ArgumentException("Check-out date must be after check-in date.");

            var isDuplicate = await _bookingRepository.HasDuplicateBookingAsync(
                dto.UserId, dto.InventoryId, dto.RoomType, dto.CheckInDate, dto.CheckOutDate, bookingId);
            if (isDuplicate)
                throw new InvalidOperationException("An active booking already exists for this room and date range.");

            var booking = new Booking
            {
                BookingId = bookingId,
                UserId = dto.UserId,
                PartnerId = dto.PartnerId,
                InventoryId = dto.InventoryId,
                ItemType = dto.ItemType,
                CheckInDate = dto.CheckInDate,
                CheckOutDate = dto.CheckOutDate,
                NumberOfGuests = dto.NumberOfGuests,
                NumberOfRooms = dto.NumberOfRooms,
                RoomType = dto.RoomType,
                SpecialRequests = dto.SpecialRequests,
                Amount = await CalculateBookingAmount(dto),
                ModifiedDate = DateTime.UtcNow
            };

            return await _bookingRepository.UpdateBookingAsync(booking);
        }

        public async Task<bool> DeleteBookingAsync(long bookingId)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId);
            return booking != null && await _bookingRepository.DeleteBookingAsync(bookingId);
        }

        public async Task<BookingResponseDto> UpdateBookingStatusAsync(long bookingId, int newStatus)
        {
            var booking = await _bookingRepository.GetBookingByIdAsync(bookingId)
                ?? throw new KeyNotFoundException(BookingConstants.BookingNotFound);
            return await _bookingRepository.UpdateBookingStatusAsync(bookingId, newStatus);
        }

        private async Task<decimal> CalculateBookingAmount(BookingRequestDto dto)
        {
            var inventory = await _inventoryRepository.GetInventoryByIdAsync(dto.InventoryId);
            int nights = (int)(dto.CheckOutDate.Date - dto.CheckInDate.Date).TotalDays;
            return (inventory?.Price ?? 0) * nights * dto.NumberOfRooms;
        }
    }
}
