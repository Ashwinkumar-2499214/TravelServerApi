using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Enum;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly INotificationService _notificationService;

    public BookingsController(
        IBookingService bookingService,
        INotificationService notificationService)
    {
        _bookingService = bookingService;
        _notificationService = notificationService;
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

        if (data != null)
        {
            // Trigger booking confirmation notification
            try
            {
                    await _notificationService.TriggerBookingNotificationAsync(
                    bookingDto.UserId,
                    $"Your booking has been confirmed. Item: {data.ItemType}, Amount: ${data.Amount}.",
                    NotificationCategory.BookingConfirmation
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification trigger failed for booking creation: {ex.Message}");
            }

            return Ok(new { message = BookingConstants.BookingCreatedSuccess, data });
        }

        return BadRequest(new { message = GeneralConstants.InvalidInput });
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

        if (data != null)
        {
            // Trigger booking update notification
            try
            {
                    await _notificationService.TriggerBookingNotificationAsync(
                    bookingDto.UserId,
                    $"Your booking for {data.ItemType} has been updated. New Amount: ${data.Amount}.",
                    NotificationCategory.SystemAlert
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification trigger failed for booking update: {ex.Message}");
            }

            return Ok(new { message = BookingConstants.BookingUpdateSuccess, data });
        }

        return NotFound(new { message = BookingConstants.BookingNotFound });
    }

    [HttpDelete("{bookingId}")]
    [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,ComplianceOfficer,Traveler")]
    public async Task<IActionResult> DeleteBooking(long bookingId)
    {
        if (bookingId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        // Get booking details before deletion to send notification
        var existingBooking = await _bookingService.GetBookingByIdAsync(bookingId);

        var result = await _bookingService.DeleteBookingAsync(bookingId);

        if (result && existingBooking != null)
        {
            // Trigger booking cancellation notification
            try
            {
                    await _notificationService.TriggerBookingNotificationAsync(
                    existingBooking.UserId,
                    $"Your booking for {existingBooking.ItemType} has been cancelled.",
                    NotificationCategory.BookingCancellation
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification trigger failed for booking deletion: {ex.Message}");
            }

            return Ok(new { message = BookingConstants.BookingDeleteSuccess });
        }

        return NotFound(new { message = BookingConstants.BookingNotFound });
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

        if (data != null)
        {
            // Trigger booking status update notification
            try
            {
                var statusMessage = GetBookingStatusMessage(statusDto.NewStatus);
                    await _notificationService.TriggerBookingNotificationAsync(
                    data.UserId,
                    $"Your booking for {data.ItemType} status has been updated to: {statusMessage}.",
                    NotificationCategory.SystemAlert
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Notification trigger failed for booking status update: {ex.Message}");
            }

            return Ok(new { message = GeneralConstants.OperationSuccess, data });
        }

        return NotFound(new { message = BookingConstants.BookingNotFound });
    }

    /// <summary>
    /// Helper method to convert booking status code to readable message
    /// </summary>
    private static string GetBookingStatusMessage(int statusCode)
    {
        return statusCode switch
        {
            0 => "Pending",
            1 => "Confirmed",
            2 => "Cancelled",
            3 => "Completed",
            _ => "Unknown"
        };
    }
}