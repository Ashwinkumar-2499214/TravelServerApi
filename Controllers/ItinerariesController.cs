using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;
using Microsoft.AspNetCore.Authorization;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ItinerariesController : ControllerBase
    {
        private readonly IItineraryService _itineraryService;

        public ItinerariesController(IItineraryService itineraryService)
        {
            _itineraryService = itineraryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllItineraries([FromQuery] ItinerarySearchDto searchDto)
        {
            if (!ModelState.IsValid || searchDto == null)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var itineraries = await _itineraryService.GetAllItinerariesAsync(searchDto);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = itineraries });
        }

        [HttpPost]
        public async Task<IActionResult> CreateItinerary([FromBody] ItineraryRequestDto itineraryDto)
        {
            if (!ModelState.IsValid || itineraryDto == null)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var result = await _itineraryService.CreateItineraryAsync(itineraryDto);
            return Ok(new { message = ItineraryConstants.ItineraryCreatedSuccess, data = result });
        }

        [HttpGet("{itineraryId}")]
        public async Task<IActionResult> GetItineraryById(long itineraryId)
        {
            // If condition to handle basic parameter validation
            if (itineraryId <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var itinerary = await _itineraryService.GetItineraryByIdAsync(itineraryId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = itinerary });
        }

        [HttpPut("{itineraryId}")]
        public async Task<IActionResult> UpdateItinerary(long itineraryId, [FromBody] ItineraryRequestDto itineraryDto)
        {
            if (!ModelState.IsValid || itineraryDto == null || itineraryId <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var result = await _itineraryService.UpdateItineraryAsync(itineraryId, itineraryDto);
            return Ok(new { message = ItineraryConstants.ItineraryUpdateSuccess, data = result });
        }

        [HttpDelete("{itineraryId}")]
        public async Task<IActionResult> DeleteItinerary(long itineraryId)
        {
            if (itineraryId <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var result = await _itineraryService.DeleteItineraryAsync(itineraryId);
            if (!result)
            {
                return BadRequest(new { message = ItineraryConstants.ItineraryNotFound });
            }

            return Ok(new { message = ItineraryConstants.ItineraryDeleteSuccess });
        }

        [HttpPatch("{itineraryId}/status")]
        public async Task<IActionResult> UpdateItineraryStatus(long itineraryId, [FromBody] ItineraryStatusUpdateDto statusDto)
        {
            if (!ModelState.IsValid || statusDto == null || itineraryId <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var result = await _itineraryService.UpdateItineraryStatusAsync(itineraryId, statusDto.NewStatus);
            return Ok(new { message = ItineraryConstants.ItineraryStatusUpdateSuccess, data = result });
        }

        [HttpPost("{itineraryId}/bookings")]
        public async Task<IActionResult> AddBookingToItinerary(long itineraryId, [FromBody] ItineraryBookingAddDto bookingDto)
        {
            if (!ModelState.IsValid || bookingDto == null || itineraryId <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var result = await _itineraryService.AddBookingToItineraryAsync(itineraryId, bookingDto.BookingId);
            if (!result)
            {
                return BadRequest(new { message = GeneralConstants.OperationFailed });
            }

            return Ok(new { message = ItineraryConstants.BookingAddedSuccess });
        }

        [HttpDelete("{itineraryId}/bookings/{bookingId}")]
        public async Task<IActionResult> RemoveBookingFromItinerary(long itineraryId, long bookingId)
        {
            if (itineraryId <= 0 || bookingId <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }

            var result = await _itineraryService.RemoveBookingFromItineraryAsync(itineraryId, bookingId);
            if (!result)
            {
                return BadRequest(new { message = GeneralConstants.OperationFailed });
            }

            return Ok(new { message = ItineraryConstants.BookingRemovedSuccess });
        }
    }
}