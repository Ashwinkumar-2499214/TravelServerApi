using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Enum;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;
        private readonly INotificationService _notificationService;

        public ReservationsController(
            IReservationService reservationService,
            INotificationService notificationService)
        {
            _reservationService = reservationService;
            _notificationService = notificationService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> GetAllReservations([FromQuery] ReservationSearchDto searchDto)
        {
            return (ModelState.IsValid && searchDto != null)
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = await _reservationService.GetAllReservationsAsync(searchDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationRequestDto reservationDto)
        {
            if (!(ModelState.IsValid && reservationDto != null))
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var data = await _reservationService.CreateReservationAsync(reservationDto);

            if (data != null)
            {
                // Trigger reservation confirmation notification
                try
                {
                    await _notificationService.TriggerReservationNotificationAsync(
                        reservationDto.UserId,
                        $"Your reservation from {reservationDto.StartDate:yyyy-MM-dd} to {reservationDto.EndDate:yyyy-MM-dd} has been created successfully.",
                        NotificationCategory.BookingConfirmation
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification trigger failed for reservation creation: {ex.Message}");
                }
            }

            return Ok(new { message = ReservationConstants.ReservationCreatedSuccess, data });
        }

        [HttpGet("{reservationId}")]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> GetReservationById(long reservationId)
        {
            return Ok(new { message = GeneralConstants.OperationSuccess, data = await _reservationService.GetReservationByIdAsync(reservationId) });
        }

        [HttpGet("/api/v1/bookings/{bookingId}/reservations")]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> GetBookingReservations(long bookingId)
        {
            return Ok(new { message = GeneralConstants.OperationSuccess, data = await _reservationService.GetBookingReservationsAsync(bookingId) });
        }

        [HttpPut("{reservationId}")]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> UpdateReservation(long reservationId, [FromBody] ReservationRequestDto reservationDto)
        {
            if (!(ModelState.IsValid && reservationDto != null))
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var data = await _reservationService.UpdateReservationAsync(reservationId, reservationDto);

            if (data != null)
            {
                // Trigger reservation update notification
                try
                {
                    await _notificationService.TriggerReservationNotificationAsync(
                        reservationDto.UserId,
                        $"Your reservation from {reservationDto.StartDate:yyyy-MM-dd} to {reservationDto.EndDate:yyyy-MM-dd} has been updated.",
                        NotificationCategory.SystemAlert
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification trigger failed for reservation update: {ex.Message}");
                }
            }

            return Ok(new { message = ReservationConstants.ReservationUpdateSuccess, data });
        }

        [HttpDelete("{reservationId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReservation(long reservationId)
        {
            // Get reservation details before deletion
            var existingReservation = await _reservationService.GetReservationByIdAsync(reservationId);

            var result = await _reservationService.DeleteReservationAsync(reservationId);

            if (result && existingReservation != null)
            {
                // Trigger reservation cancellation notification
                try
                {
                    await _notificationService.TriggerReservationNotificationAsync(
                        existingReservation.UserId,
                        $"Your reservation from {existingReservation.StartDate:yyyy-MM-dd} to {existingReservation.EndDate:yyyy-MM-dd} has been cancelled.",
                        NotificationCategory.BookingCancellation
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification trigger failed for reservation deletion: {ex.Message}");
                }

                return Ok(new { message = ReservationConstants.ReservationDeleteSuccess });
            }

            return BadRequest(new { message = ReservationConstants.ReservationNotFound });
        }

        [HttpPatch("{reservationId}/status")]
        [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,ComplianceOfficer")]
        public async Task<IActionResult> UpdateReservationStatus(long reservationId, [FromBody] ReservationStatusUpdateDto statusDto)
        {
            if (!(ModelState.IsValid && statusDto != null))
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var data = await _reservationService.UpdateReservationStatusAsync(reservationId, statusDto.NewStatus);

            if (data != null)
            {
                // Trigger reservation status update notification
                try
                {
                    var statusMessage = GetReservationStatusMessage(statusDto.NewStatus);
                    await _notificationService.TriggerReservationNotificationAsync(
                        data.UserId,
                        $"Your reservation from {data.StartDate:yyyy-MM-dd} to {data.EndDate:yyyy-MM-dd} status has been updated to: {statusMessage}.",
                        NotificationCategory.SystemAlert
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification trigger failed for reservation status update: {ex.Message}");
                }
            }

            return Ok(new { message = GeneralConstants.OperationSuccess, data });
        }

        /// <summary>
        /// Helper method to convert reservation status code to readable message
        /// </summary>
        private static string GetReservationStatusMessage(int statusCode)
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
}