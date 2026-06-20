using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAllInventory([FromQuery] InventorySearchDto searchDto)
    {
        if (!ModelState.IsValid || searchDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _inventoryService.GetAllInventoryAsync(searchDto);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = InventoryConstants.InventoryNotFound });
    }

    [HttpGet("/api/v1/partners/{partnerId}/inventory")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPartnerInventory(long partnerId)
    {
        if (partnerId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _inventoryService.GetPartnerInventoryAsync(partnerId);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = InventoryConstants.InventoryNotFound });
    }

    [HttpPost("/api/v1/partners/{partnerId}/inventory")]
    [Authorize(Roles = "Admin,TravelAgent")]
    public async Task<IActionResult> CreatePartnerInventory(long partnerId, [FromBody] InventoryRequestDto inventoryDto)
    {
        if (partnerId <= 0 || !ModelState.IsValid || inventoryDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        inventoryDto.PartnerId = partnerId;
        var data = await _inventoryService.CreateInventoryAsync(inventoryDto);

        return data != null
            ? Ok(new { message = InventoryConstants.InventoryCreatedSuccess, data })
            : BadRequest(new { message = GeneralConstants.InvalidInput });
    }

    [HttpPut("/api/v1/partners/{partnerId}/inventory/{inventoryId}")]
    [Authorize(Roles = "Admin,TravelAgent")]
    public async Task<IActionResult> UpdatePartnerInventory(long partnerId, long inventoryId, [FromBody] InventoryRequestDto inventoryDto)
    {
        if (partnerId <= 0 || inventoryId <= 0 || !ModelState.IsValid || inventoryDto == null || 
            string.IsNullOrWhiteSpace(inventoryDto.ItemType) || string.IsNullOrWhiteSpace(inventoryDto.Description) || 
            inventoryDto.Availability < 0 || inventoryDto.Price <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        inventoryDto.PartnerId = partnerId;
        var data = await _inventoryService.UpdateInventoryAsync(inventoryId, inventoryDto);

        return data != null
            ? Ok(new { message = InventoryConstants.InventoryUpdateSuccess, data })
            : NotFound(new { message = InventoryConstants.InventoryNotFound });
    }

    [HttpDelete("/api/v1/partners/{partnerId}/inventory/{inventoryId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePartnerInventory(long partnerId, long inventoryId)
    {
        if (partnerId <= 0 || inventoryId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var result = await _inventoryService.DeleteInventoryAsync(inventoryId);

        return result
            ? Ok(new { message = InventoryConstants.InventoryDeleteSuccess })
            : NotFound(new { message = InventoryConstants.InventoryNotFound });
    }

    [HttpPatch("/api/v1/inventory/{inventoryId}/availability")]
    [Authorize(Roles = "Admin,TravelAgent")]
    public async Task<IActionResult> UpdateAvailability(long inventoryId, [FromBody] InventoryAvailabilityDto dto)
    {
        if (inventoryId <= 0 || !ModelState.IsValid || dto == null || dto.NewAvailability < 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        int status;
        if (dto.Status.HasValue)
        {
            status = dto.Status.Value;
            if (status <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }
        }
        else
        {
            var existing = await _inventoryService.GetInventoryByIdAsync(inventoryId);
            if (existing == null)
            {
                return NotFound(new { message = InventoryConstants.InventoryNotFound });
            }

            status = existing.Status;
            if (status <= 0)
            {
                return BadRequest(new { message = GeneralConstants.InvalidInput });
            }
        }

        var data = await _inventoryService.UpdateAvailabilityAsync(inventoryId, dto.NewAvailability, status);

        return data != null
            ? Ok(new { message = InventoryConstants.AvailabilityUpdateSuccess, data })
            : NotFound(new { message = InventoryConstants.InventoryNotFound });
    }
}