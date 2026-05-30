using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
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
        if (searchDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _partnerService.GetAllPartnersAsync(searchDto);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = PartnerConstants.PartnerNotFound });
    }

    [HttpPost]
    public async Task<IActionResult> CreatePartner([FromBody] PartnerRequestDto partnerDto)
    {
        if (partnerDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _partnerService.CreatePartnerAsync(partnerDto);

        return data != null
            ? Ok(new { message = PartnerConstants.PartnerCreatedSuccess, data })
            : BadRequest(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("{partnerId}")]
    public async Task<IActionResult> GetPartnerById(long partnerId)
    {
        if (partnerId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _partnerService.GetPartnerByIdAsync(partnerId);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = PartnerConstants.PartnerNotFound });
    }

    [HttpPut("{partnerId}")]
    public async Task<IActionResult> UpdatePartner(long partnerId, [FromBody] PartnerRequestDto partnerDto)
    {
        if (partnerId <= 0 || partnerDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _partnerService.UpdatePartnerAsync(partnerId, partnerDto);

        return data != null
            ? Ok(new { message = PartnerConstants.PartnerUpdateSuccess, data })
            : NotFound(new { message = PartnerConstants.PartnerNotFound });
    }

    [HttpDelete("{partnerId}")]
    public async Task<IActionResult> DeletePartner(long partnerId)
    {
        if (partnerId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var result = await _partnerService.DeletePartnerAsync(partnerId);

        return result
            ? Ok(new { message = PartnerConstants.PartnerDeleteSuccess })
            : NotFound(new { message = PartnerConstants.PartnerNotFound });
    }

    [HttpPatch("{partnerId}/status")]
    public async Task<IActionResult> UpdatePartnerStatus(long partnerId, [FromBody] PartnerStatusUpdateDto statusDto)
    {
        if (partnerId <= 0 || statusDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _partnerService.UpdatePartnerStatusAsync(partnerId, statusDto.NewStatus);

        return data != null
            ? Ok(new { message = PartnerConstants.PartnerStatusUpdateSuccess, data })
            : NotFound(new { message = PartnerConstants.PartnerNotFound });
    }
}