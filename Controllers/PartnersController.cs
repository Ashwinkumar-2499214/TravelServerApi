using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PartnersController : ControllerBase
    {
        private readonly IPartnerService _partnerService;

        public PartnersController(IPartnerService partnerService)
        {
            _partnerService = partnerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPartners([FromQuery] PartnerSearchDto searchDto)
        {
            if (!ModelState.IsValid || searchDto == null)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var partners = await _partnerService.GetAllPartnersAsync(searchDto);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = partners });
        }

        [HttpPost]
        public async Task<IActionResult> CreatePartner([FromBody] PartnerRequestDto partnerDto)
        {
            if (!ModelState.IsValid || partnerDto == null)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            if (string.IsNullOrWhiteSpace(partnerDto.Name))
                return BadRequest(new { message = "Partner name is required and cannot be empty" });

            if (partnerDto.Type <= 0)
                return BadRequest(new { message = "Partner type must be a valid positive integer" });

            if (string.IsNullOrWhiteSpace(partnerDto.ContactEmail))
                return BadRequest(new { message = "Partner contact email is required" });

            if (!partnerDto.ContactEmail.Contains("@"))
                return BadRequest(new { message = "Partner contact email must be a valid email address" });

            if (string.IsNullOrWhiteSpace(partnerDto.ContactPhone))
                return BadRequest(new { message = "Partner contact phone is required" });

            if (string.IsNullOrWhiteSpace(partnerDto.Address))
                return BadRequest(new { message = "Partner address is required" });

            var result = await _partnerService.CreatePartnerAsync(partnerDto);
            return Ok(new { message = PartnerConstants.PartnerCreatedSuccess, data = result });
        }

        [HttpGet("{partnerId}")]
        public async Task<IActionResult> GetPartnerById(long partnerId)
        {
            if (partnerId <= 0)
                return BadRequest(new { message = "Partner ID must be a valid positive integer" });

            var partner = await _partnerService.GetPartnerByIdAsync(partnerId);
            return partner == null
                ? NotFound(new { message = PartnerConstants.PartnerNotFound })
                : Ok(new { message = GeneralConstants.OperationSuccess, data = partner });
        }

        [HttpPut("{partnerId}")]
        public async Task<IActionResult> UpdatePartner(long partnerId, [FromBody] PartnerRequestDto partnerDto)
        {
            if (partnerId <= 0)
                return BadRequest(new { message = "Partner ID must be a valid positive integer" });

            if (!ModelState.IsValid || partnerDto == null)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            if (string.IsNullOrWhiteSpace(partnerDto.Name))
                return BadRequest(new { message = "Partner name is required and cannot be empty" });

            if (partnerDto.Type <= 0)
                return BadRequest(new { message = "Partner type must be a valid positive integer" });

            if (string.IsNullOrWhiteSpace(partnerDto.ContactEmail))
                return BadRequest(new { message = "Partner contact email is required" });

            if (!partnerDto.ContactEmail.Contains("@"))
                return BadRequest(new { message = "Partner contact email must be a valid email address" });

            if (string.IsNullOrWhiteSpace(partnerDto.ContactPhone))
                return BadRequest(new { message = "Partner contact phone is required" });

            if (string.IsNullOrWhiteSpace(partnerDto.Address))
                return BadRequest(new { message = "Partner address is required" });

            var result = await _partnerService.UpdatePartnerAsync(partnerId, partnerDto);
            return result == null
                ? NotFound(new { message = PartnerConstants.PartnerNotFound })
                : Ok(new { message = PartnerConstants.PartnerUpdateSuccess, data = result });
        }

        [HttpDelete("{partnerId}")]
        public async Task<IActionResult> DeletePartner(long partnerId)
        {
            if (partnerId <= 0)
                return BadRequest(new { message = "Partner ID must be a valid positive integer" });

            var result = await _partnerService.DeletePartnerAsync(partnerId);
            return result
                ? Ok(new { message = PartnerConstants.PartnerDeleteSuccess })
                : BadRequest(new { message = PartnerConstants.PartnerNotFound });
        }

        [HttpPatch("{partnerId}/status")]
        public async Task<IActionResult> UpdatePartnerStatus(long partnerId, [FromBody] PartnerStatusUpdateDto statusDto)
        {
            if (partnerId <= 0)
                return BadRequest(new { message = "Partner ID must be a valid positive integer" });

            if (!ModelState.IsValid || statusDto == null)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            if (statusDto.NewStatus <= 0)
                return BadRequest(new { message = "Partner status must be a valid positive integer" });

            var result = await _partnerService.UpdatePartnerStatusAsync(partnerId, statusDto.NewStatus);
            return result == null
                ? NotFound(new { message = PartnerConstants.PartnerNotFound })
                : Ok(new { message = PartnerConstants.PartnerStatusUpdateSuccess, data = result });
        }
    }
}
