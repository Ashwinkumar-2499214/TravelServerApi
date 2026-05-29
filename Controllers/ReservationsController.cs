using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly IReservationService _reservationService;

        public ReservationsController(IReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllReservations([FromQuery] ReservationSearchDto searchDto)
        {
            return (ModelState.IsValid && searchDto != null)
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = await _reservationService.GetAllReservationsAsync(searchDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpPost]
        public async Task<IActionResult> CreateReservation([FromBody] ReservationRequestDto reservationDto)
        {
            return (ModelState.IsValid && reservationDto != null)
                ? Ok(new { message = ReservationConstants.ReservationCreatedSuccess, data = await _reservationService.CreateReservationAsync(reservationDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpGet("{reservationId}")]
        public async Task<IActionResult> GetReservationById(long reservationId)
        {
            return Ok(new { message = GeneralConstants.OperationSuccess, data = await _reservationService.GetReservationByIdAsync(reservationId) });
        }

        [HttpGet("/api/v1/bookings/{bookingId}/reservations")]
        public async Task<IActionResult> GetBookingReservations(long bookingId)
        {
            return Ok(new { message = GeneralConstants.OperationSuccess, data = await _reservationService.GetBookingReservationsAsync(bookingId) });
        }

        [HttpPut("{reservationId}")]
        public async Task<IActionResult> UpdateReservation(long reservationId, [FromBody] ReservationRequestDto reservationDto)
        {
            return (ModelState.IsValid && reservationDto != null)
                ? Ok(new { message = ReservationConstants.ReservationUpdateSuccess, data = await _reservationService.UpdateReservationAsync(reservationId, reservationDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpDelete("{reservationId}")]
        public async Task<IActionResult> DeleteReservation(long reservationId)
        {
            return await _reservationService.DeleteReservationAsync(reservationId)
                ? Ok(new { message = ReservationConstants.ReservationDeleteSuccess })
                : BadRequest(new { message = ReservationConstants.ReservationNotFound });
        }

        [HttpPatch("{reservationId}/status")]
        public async Task<IActionResult> UpdateReservationStatus(long reservationId, [FromBody] ReservationStatusUpdateDto statusDto)
        {
            return (ModelState.IsValid && statusDto != null)
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = await _reservationService.UpdateReservationStatusAsync(reservationId, statusDto.NewStatus) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }
    }
}