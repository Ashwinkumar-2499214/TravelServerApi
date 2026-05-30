using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;
using Microsoft.AspNetCore.Authorization;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class ItinerariesController : ControllerBase
    {
        private readonly IItineraryService _itineraryService;

        public ItinerariesController(IItineraryService itineraryService)
        {
            _itineraryService = itineraryService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> GetAllItineraries([FromQuery] ItinerarySearchDto searchDto)
        {
            try
            {
                if (ModelState.IsValid && searchDto != null)
                {
                    var itineraries = await _itineraryService.GetAllItinerariesAsync(searchDto);
                    return Ok(new { message = GeneralConstants.OperationSuccess, data = itineraries });
                }
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = GeneralConstants.InternalServerError, error = ex.Message });
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> CreateItinerary([FromBody] ItineraryRequestDto itineraryDto)
        {
            try
            {
                if (ModelState.IsValid && itineraryDto != null)
                {
                    var result = await _itineraryService.CreateItineraryAsync(itineraryDto);
                    return Ok(new { message = ItineraryConstants.ItineraryCreatedSuccess, data = result });
                }
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = GeneralConstants.InternalServerError, error = ex.Message });
            }
        }

        [HttpGet("{itineraryId}")]
        [Authorize(Roles = "Admin,User,Partner")]
        public async Task<IActionResult> GetItineraryById(long itineraryId)
        {
            try
            {
                var itinerary = await _itineraryService.GetItineraryByIdAsync(itineraryId);
                return Ok(new { message = GeneralConstants.OperationSuccess, data = itinerary });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = ItineraryConstants.ItineraryNotFound });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = GeneralConstants.InternalServerError, error = ex.Message });
            }
        }

        [HttpPut("{itineraryId}")]
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> UpdateItinerary(long itineraryId, [FromBody] ItineraryRequestDto itineraryDto)
        {
            try
            {
                if (ModelState.IsValid && itineraryDto != null)
                {
                    var result = await _itineraryService.UpdateItineraryAsync(itineraryId, itineraryDto);
                    return Ok(new { message = ItineraryConstants.ItineraryUpdateSuccess, data = result });
                }
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = ItineraryConstants.ItineraryNotFound });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = GeneralConstants.InternalServerError, error = ex.Message });
            }
        }

        [HttpDelete("{itineraryId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteItinerary(long itineraryId)
        {
            try
            {
                var result = await _itineraryService.DeleteItineraryAsync(itineraryId);
                if (result)
                    return Ok(new { message = ItineraryConstants.ItineraryDeleteSuccess });
                return BadRequest(new { message = ItineraryConstants.ItineraryNotFound });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = GeneralConstants.InternalServerError, error = ex.Message });
            }
        }

        [HttpPatch("{itineraryId}/status")]
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> UpdateItineraryStatus(long itineraryId, [FromBody] ItineraryStatusUpdateDto statusDto)
        {
            try
            {
                if (ModelState.IsValid && statusDto != null)
                {
                    var result = await _itineraryService.UpdateItineraryStatusAsync(itineraryId, statusDto.NewStatus);
                    return Ok(new { message = ItineraryConstants.ItineraryStatusUpdateSuccess, data = result });
                }
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = ItineraryConstants.ItineraryNotFound });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = GeneralConstants.InternalServerError, error = ex.Message });
            }
        }

        [HttpPost("{itineraryId}/bookings")]
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> AddBookingToItinerary(long itineraryId, [FromBody] ItineraryBookingAddDto bookingDto)
        {
            try
            {
                if (ModelState.IsValid && bookingDto != null)
                {
                    var result = await _itineraryService.AddBookingToItineraryAsync(itineraryId, bookingDto.BookingId);
                    if (result)
                        return Ok(new { message = ItineraryConstants.BookingAddedSuccess });
                    return BadRequest(new { message = GeneralConstants.OperationFailed });
                }
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = GeneralConstants.InternalServerError, error = ex.Message });
            }
        }

        [HttpDelete("{itineraryId}/bookings/{bookingId}")]
        [Authorize(Roles = "Admin,Partner")]
        public async Task<IActionResult> RemoveBookingFromItinerary(long itineraryId, long bookingId)
        {
            try
            {
                var result = await _itineraryService.RemoveBookingFromItineraryAsync(itineraryId, bookingId);
                if (result)
                    return Ok(new { message = ItineraryConstants.BookingRemovedSuccess });
                return BadRequest(new { message = GeneralConstants.OperationFailed });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = GeneralConstants.InternalServerError, error = ex.Message });
            }
        }
    }
}
