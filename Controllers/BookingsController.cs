using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings([FromQuery] BookingSearchDto searchDto)
        {
            return !ModelState.IsValid || searchDto == null 
                ? BadRequest(new { message = GeneralConstants.InvalidInput })
                : Ok(new { message = GeneralConstants.OperationSuccess, data = await _bookingService.GetAllBookingsAsync(searchDto) });
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto bookingDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = GeneralConstants.InvalidInput, errors = ModelState.Values.SelectMany(v => v.Errors) });
            
            if (bookingDto == null)
                return BadRequest(new { message = "Booking data is required" });
            
            if (bookingDto.UserId <= 0)
                return BadRequest(new { message = "Invalid User ID" });
            
            if (bookingDto.PartnerId <= 0)
                return BadRequest(new { message = "Invalid Partner ID" });
            
            if (bookingDto.InventoryId <= 0)
                return BadRequest(new { message = "Invalid Inventory ID" });
            
            if (bookingDto.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than zero" });

            var result = await _bookingService.CreateBookingAsync(bookingDto);
            return Ok(new { message = BookingConstants.BookingCreatedSuccess, data = result });
        }

        [HttpGet("{bookingId}")]
        public async Task<IActionResult> GetBookingById(long bookingId)
        {
            if (bookingId <= 0)
                return BadRequest(new { message = "Invalid Booking ID" });

            var booking = await _bookingService.GetBookingByIdAsync(bookingId);
            return booking != null 
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = booking })
                : NotFound(new { message = BookingConstants.BookingNotFound });
        }

        [HttpPut("{bookingId}")]
        public async Task<IActionResult> UpdateBooking(long bookingId, [FromBody] BookingRequestDto bookingDto)
        {
            if (bookingId <= 0)
                return BadRequest(new { message = "Invalid Booking ID" });

            if (!ModelState.IsValid)
                return BadRequest(new { message = GeneralConstants.InvalidInput, errors = ModelState.Values.SelectMany(v => v.Errors) });
            
            if (bookingDto == null)
                return BadRequest(new { message = "Booking data is required" });
            
            if (bookingDto.UserId <= 0)
                return BadRequest(new { message = "Invalid User ID" });
            
            if (bookingDto.PartnerId <= 0)
                return BadRequest(new { message = "Invalid Partner ID" });
            
            if (bookingDto.InventoryId <= 0)
                return BadRequest(new { message = "Invalid Inventory ID" });
            
            if (bookingDto.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than zero" });

            var result = await _bookingService.UpdateBookingAsync(bookingId, bookingDto);
            return result != null
                ? Ok(new { message = BookingConstants.BookingUpdateSuccess, data = result })
                : NotFound(new { message = BookingConstants.BookingNotFound });
        }

        [HttpDelete("{bookingId}")]
        public async Task<IActionResult> DeleteBooking(long bookingId)
        {
            if (bookingId <= 0)
                return BadRequest(new { message = "Invalid Booking ID" });

            var result = await _bookingService.DeleteBookingAsync(bookingId);
            return result 
                ? Ok(new { message = BookingConstants.BookingDeleteSuccess })
                : NotFound(new { message = BookingConstants.BookingNotFound });
        }

        [HttpPatch("{bookingId}/status")]
        public async Task<IActionResult> UpdateBookingStatus(long bookingId, [FromBody] BookingStatusUpdateDto statusDto)
        {
            if (bookingId <= 0)
                return BadRequest(new { message = "Invalid Booking ID" });

            if (!ModelState.IsValid)
                return BadRequest(new { message = GeneralConstants.InvalidInput, errors = ModelState.Values.SelectMany(v => v.Errors) });
            
            if (statusDto == null)
                return BadRequest(new { message = "Status data is required" });
            
            if (statusDto.NewStatus < 0)
                return BadRequest(new { message = "Invalid status value" });

            var result = await _bookingService.UpdateBookingStatusAsync(bookingId, statusDto.NewStatus);
            return result != null
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = result })
                : NotFound(new { message = BookingConstants.BookingNotFound });
        }
    }
}
