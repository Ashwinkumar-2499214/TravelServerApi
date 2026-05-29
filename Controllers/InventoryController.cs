using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // GET /inventory
        [HttpGet]
        public async Task<IActionResult> GetAllInventory([FromQuery] InventorySearchDto searchDto)
        {
            return searchDto == null || !ModelState.IsValid
                ? BadRequest(new { message = "Search filter cannot be null or model is invalid" })
                : Ok(new { message = GeneralConstants.OperationSuccess, data = await _inventoryService.GetAllInventoryAsync(searchDto) });
        }

        // GET /partners/{partnerId}/inventory
        [HttpGet("/api/v1/partners/{partnerId}/inventory")]
        public async Task<IActionResult> GetPartnerInventory(long partnerId)
        {
            return partnerId <= 0 
                ? BadRequest(new { message = "Partner ID must be greater than 0" })
                : Ok(new { message = GeneralConstants.OperationSuccess, data = await _inventoryService.GetPartnerInventoryAsync(partnerId) });
        }

        // POST /partners/{partnerId}/inventory
        [HttpPost("/api/v1/partners/{partnerId}/inventory")]
        public async Task<IActionResult> CreatePartnerInventory(long partnerId, [FromBody] InventoryRequestDto inventoryDto)
        {
            if (inventoryDto == null)
                return BadRequest(new { message = "Request body cannot be null" });

            if (partnerId <= 0)
                return BadRequest(new { message = "Partner ID must be greater than 0" });

            string validationError = ValidateInventoryRequest(inventoryDto);
            if (!string.IsNullOrEmpty(validationError))
                return BadRequest(new { message = validationError });

            if (!ModelState.IsValid)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            inventoryDto.PartnerId = partnerId;
            var result = await _inventoryService.CreateInventoryAsync(inventoryDto);
            return Ok(new { message = InventoryConstants.InventoryCreatedSuccess, data = result });
        }

        // PUT /partners/{partnerId}/inventory/{inventoryId}
        [HttpPut("/api/v1/partners/{partnerId}/inventory/{inventoryId}")]
        public async Task<IActionResult> UpdatePartnerInventory(long partnerId, long inventoryId, [FromBody] InventoryRequestDto inventoryDto)
        {
            if (inventoryDto == null)
                return BadRequest(new { message = "Request body cannot be null" });

            if (partnerId <= 0)
                return BadRequest(new { message = "Partner ID must be greater than 0" });

            if (inventoryId <= 0)
                return BadRequest(new { message = "Inventory ID must be greater than 0" });

            string validationError = ValidateInventoryRequest(inventoryDto);
            if (!string.IsNullOrEmpty(validationError))
                return BadRequest(new { message = validationError });

            if (!ModelState.IsValid)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            inventoryDto.PartnerId = partnerId;
            var result = await _inventoryService.UpdateInventoryAsync(inventoryId, inventoryDto);
            return result != null 
                ? Ok(new { message = InventoryConstants.InventoryUpdateSuccess, data = result })
                : NotFound(new { message = InventoryConstants.InventoryNotFound });
        }

        // DELETE /partners/{partnerId}/inventory/{inventoryId}
        [HttpDelete("/api/v1/partners/{partnerId}/inventory/{inventoryId}")]
        public async Task<IActionResult> DeletePartnerInventory(long partnerId, long inventoryId)
        {
            if (partnerId <= 0)
                return BadRequest(new { message = "Partner ID must be greater than 0" });

            if (inventoryId <= 0)
                return BadRequest(new { message = "Inventory ID must be greater than 0" });

            var ok = await _inventoryService.DeleteInventoryAsync(inventoryId);
            return ok 
                ? Ok(new { message = InventoryConstants.InventoryDeleteSuccess })
                : NotFound(new { message = InventoryConstants.InventoryNotFound });
        }

        // PATCH /inventory/{inventoryId}/availability
        [HttpPatch("/api/v1/inventory/{inventoryId}/availability")]
        public async Task<IActionResult> UpdateAvailability(long inventoryId, [FromBody] InventoryAvailabilityDto dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Request body cannot be null" });

            if (inventoryId <= 0)
                return BadRequest(new { message = "Inventory ID must be greater than 0" });

            if (dto.NewAvailability < 0)
                return BadRequest(new { message = "Availability cannot be negative" });

            if (dto.Status <= 0)
                return BadRequest(new { message = "Status must be a valid inventory status" });

            if (!ModelState.IsValid)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var result = await _inventoryService.UpdateAvailabilityAsync(inventoryId, dto.NewAvailability, dto.Status);
            return result != null 
                ? Ok(new { message = InventoryConstants.AvailabilityUpdateSuccess, data = result })
                : NotFound(new { message = InventoryConstants.InventoryNotFound });
        }

        private string ValidateInventoryRequest(InventoryRequestDto dto)
        {
            return string.IsNullOrWhiteSpace(dto.ItemType) ? "Item type is required and cannot be empty" :
                   string.IsNullOrWhiteSpace(dto.Description) ? "Description is required and cannot be empty" :
                   dto.PartnerId <= 0 ? "Partner ID must be greater than 0" :
                   dto.Availability < 0 ? "Availability cannot be negative" :
                   dto.Price <= 0 ? "Price must be greater than 0" :
                   string.Empty;
        }
    }
}
