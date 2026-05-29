using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class InvoicesController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInvoices([FromQuery] InvoiceSearchDto searchDto)
        {
            return (ModelState.IsValid && searchDto != null)
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = await _invoiceService.GetAllInvoicesAsync(searchDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpPost]
        public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequestDto invoiceDto)
        {
            return (ModelState.IsValid && invoiceDto != null)
                ? Ok(new { message = InvoiceConstants.InvoiceCreatedSuccess, data = await _invoiceService.CreateInvoiceAsync(invoiceDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpGet("{invoiceId}")]
        public async Task<IActionResult> GetInvoiceById(long invoiceId)
        {
            return Ok(new { message = GeneralConstants.OperationSuccess, data = await _invoiceService.GetInvoiceByIdAsync(invoiceId) });
        }

        [HttpPut("{invoiceId}")]
        public async Task<IActionResult> UpdateInvoice(long invoiceId, [FromBody] InvoiceRequestDto invoiceDto)
        {
            return (ModelState.IsValid && invoiceDto != null)
                ? Ok(new { message = InvoiceConstants.InvoiceUpdateSuccess, data = await _invoiceService.UpdateInvoiceAsync(invoiceId, invoiceDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpDelete("{invoiceId}")]
        public async Task<IActionResult> DeleteInvoice(long invoiceId)
        {
            return await _invoiceService.DeleteInvoiceAsync(invoiceId)
                ? Ok(new { message = InvoiceConstants.InvoiceDeleteSuccess })
                : BadRequest(new { message = InvoiceConstants.InvoiceNotFound });
        }

        [HttpPatch("{invoiceId}/status")]
        public async Task<IActionResult> UpdateInvoiceStatus(long invoiceId, [FromBody] InvoiceStatusUpdateDto statusDto)
        {
            return (ModelState.IsValid && statusDto != null)
                ? Ok(new { message = InvoiceConstants.InvoiceStatusUpdateSuccess, data = await _invoiceService.UpdateInvoiceStatusAsync(invoiceId, statusDto.NewStatus) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }
    }
}