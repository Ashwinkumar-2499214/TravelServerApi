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
    public class ItinerariesController : ControllerBase
    {
        private readonly IItineraryService _itineraryService;
        private readonly INotificationService _notificationService;

        public ItinerariesController(IItineraryService itineraryService, INotificationService notificationService)
        {
            _itineraryService = itineraryService;
            _notificationService = notificationService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,Traveler")]
        public async Task<IActionResult> GetAllItineraries([FromQuery] ItinerarySearchDto searchDto)
        {
            if (!ModelState.IsValid || searchDto == null)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var itineraries = await _itineraryService.GetAllItinerariesAsync(searchDto);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = itineraries });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> CreateItinerary([FromBody] ItineraryRequestDto itineraryDto)
        {
            if (!ModelState.IsValid || itineraryDto == null)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var result = await _itineraryService.CreateItineraryAsync(itineraryDto);

            if (result != null)
            {
                try
                {
                    await _notificationService.TriggerItineraryNotificationAsync(
                        itineraryDto.UserId,
                        $"Your itinerary \"{itineraryDto.Title}\" has been created successfully.",
                        NotificationCategory.ItineraryUpdate
                    );
                }
                catch (Exception ex) { Console.WriteLine($"Notification failed for itinerary creation: {ex.Message}"); }
            }

            return Ok(new { message = ItineraryConstants.ItineraryCreatedSuccess, data = result });
        }

        [HttpGet("{itineraryId}")]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> GetItineraryById(long itineraryId)
        {
            if (itineraryId <= 0)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var itinerary = await _itineraryService.GetItineraryByIdAsync(itineraryId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = itinerary });
        }

        [HttpPut("{itineraryId}")]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> UpdateItinerary(long itineraryId, [FromBody] ItineraryRequestDto itineraryDto)
        {
            if (!ModelState.IsValid || itineraryDto == null || itineraryId <= 0)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var result = await _itineraryService.UpdateItineraryAsync(itineraryId, itineraryDto);

            if (result != null)
            {
                try
                {
                    await _notificationService.TriggerItineraryNotificationAsync(
                        itineraryDto.UserId,
                        $"Your itinerary \"{itineraryDto.Title}\" has been updated successfully.",
                        NotificationCategory.ItineraryUpdate
                    );
                }
                catch (Exception ex) { Console.WriteLine($"Notification failed for itinerary update: {ex.Message}"); }
            }

            return Ok(new { message = ItineraryConstants.ItineraryUpdateSuccess, data = result });
        }

        [HttpDelete("{itineraryId}")]
        [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,ComplianceOfficer,Traveler")]
        public async Task<IActionResult> DeleteItinerary(long itineraryId)
        {
            if (itineraryId <= 0)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var existing = await _itineraryService.GetItineraryByIdAsync(itineraryId);
            var result = await _itineraryService.DeleteItineraryAsync(itineraryId);

            if (!result)
                return BadRequest(new { message = ItineraryConstants.ItineraryNotFound });

            if (existing != null)
            {
                try
                {
                    await _notificationService.TriggerItineraryNotificationAsync(
                        existing.UserId,
                        $"Your itinerary \"{existing.Title}\" has been deleted.",
                        NotificationCategory.SystemAlert
                    );
                }
                catch (Exception ex) { Console.WriteLine($"Notification failed for itinerary deletion: {ex.Message}"); }
            }

            return Ok(new { message = ItineraryConstants.ItineraryDeleteSuccess });
        }

        [HttpPatch("{itineraryId}/status")]
        [Authorize(Roles = "Admin,TravelAgent,CorporateTravelManager,ComplianceOfficer")]
        public async Task<IActionResult> UpdateItineraryStatus(long itineraryId, [FromBody] ItineraryStatusUpdateDto statusDto)
        {
            if (!ModelState.IsValid || statusDto == null || itineraryId <= 0)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var result = await _itineraryService.UpdateItineraryStatusAsync(itineraryId, statusDto.NewStatus);

            if (result != null)
            {
                try
                {
                    await _notificationService.TriggerItineraryNotificationAsync(
                        result.UserId,
                        $"Your itinerary \"{result.Title}\" status has been updated to: {(ItineraryStatus)statusDto.NewStatus}.",
                        NotificationCategory.ItineraryUpdate
                    );
                }
                catch (Exception ex) { Console.WriteLine($"Notification failed for itinerary status update: {ex.Message}"); }
            }

            return Ok(new { message = ItineraryConstants.ItineraryStatusUpdateSuccess, data = result });
        }

        [HttpPost("{itineraryId}/bookings")]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> AddBookingToItinerary(long itineraryId, [FromBody] ItineraryBookingAddDto bookingDto)
        {
            if (!ModelState.IsValid || bookingDto == null || itineraryId <= 0)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var result = await _itineraryService.AddBookingToItineraryAsync(itineraryId, bookingDto.BookingId);
            if (!result)
                return BadRequest(new { message = GeneralConstants.OperationFailed });

            return Ok(new { message = ItineraryConstants.BookingAddedSuccess });
        }

        [HttpDelete("{itineraryId}/bookings/{bookingId}")]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> RemoveBookingFromItinerary(long itineraryId, long bookingId)
        {
            if (itineraryId <= 0 || bookingId <= 0)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var result = await _itineraryService.RemoveBookingFromItineraryAsync(itineraryId, bookingId);
            if (!result)
                return BadRequest(new { message = GeneralConstants.OperationFailed });

            return Ok(new { message = ItineraryConstants.BookingRemovedSuccess });
        }
    }
}
