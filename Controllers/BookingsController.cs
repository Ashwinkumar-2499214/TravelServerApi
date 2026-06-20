using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,Traveler")]
    public async Task<IActionResult> GetAllBookings([FromQuery] BookingSearchDto searchDto)
    {
        if (!ModelState.IsValid || searchDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _bookingService.GetAllBookingsAsync(searchDto);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = BookingConstants.BookingNotFound });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,Traveler")]
    public async Task<IActionResult> CreateBooking([FromBody] BookingRequestDto bookingDto)
    {
        if (!ModelState.IsValid || bookingDto == null || bookingDto.UserId <= 0 || bookingDto.PartnerId <= 0 || bookingDto.InventoryId <= 0 || bookingDto.Amount <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _bookingService.CreateBookingAsync(bookingDto);

        return data != null
            ? Ok(new { message = BookingConstants.BookingCreatedSuccess, data })
            : BadRequest(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("{bookingId}")]
    [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,Traveler")]
    public async Task<IActionResult> GetBookingById(long bookingId)
    {
        if (bookingId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _bookingService.GetBookingByIdAsync(bookingId);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = BookingConstants.BookingNotFound });
    }

    [HttpPut("{bookingId}")]
    [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager")]
    public async Task<IActionResult> UpdateBooking(long bookingId, [FromBody] BookingRequestDto bookingDto)
    {
        if (bookingId <= 0 || !ModelState.IsValid || bookingDto == null || bookingDto.UserId <= 0 || bookingDto.PartnerId <= 0 || bookingDto.InventoryId <= 0 || bookingDto.Amount <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _bookingService.UpdateBookingAsync(bookingId, bookingDto);

        return data != null
            ? Ok(new { message = BookingConstants.BookingUpdateSuccess, data })
            : NotFound(new { message = BookingConstants.BookingNotFound });
    }

    [HttpDelete("{bookingId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteBooking(long bookingId)
    {
        if (bookingId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var result = await _bookingService.DeleteBookingAsync(bookingId);

        return result
            ? Ok(new { message = BookingConstants.BookingDeleteSuccess })
            : NotFound(new { message = BookingConstants.BookingNotFound });
    }

    [HttpPatch("{bookingId}/status")]
    [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,ComplianceOfficer")]
    public async Task<IActionResult> UpdateBookingStatus(long bookingId, [FromBody] BookingStatusUpdateDto statusDto)
    {
        if (bookingId <= 0 || !ModelState.IsValid || statusDto == null || statusDto.NewStatus < 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _bookingService.UpdateBookingStatusAsync(bookingId, statusDto.NewStatus);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = BookingConstants.BookingNotFound });
    }
}