using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

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
        if (!ModelState.IsValid || searchDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _invoiceService.GetAllInvoicesAsync(searchDto);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = InvoiceConstants.InvoiceNotFound });
    }

    [HttpPost]
    public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequestDto invoiceDto)
    {
        if (!ModelState.IsValid || invoiceDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _invoiceService.CreateInvoiceAsync(invoiceDto);

        return data != null
            ? Ok(new { message = InvoiceConstants.InvoiceCreatedSuccess, data })
            : BadRequest(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("{invoiceId}")]
    public async Task<IActionResult> GetInvoiceById(long invoiceId)
    {
        if (invoiceId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _invoiceService.GetInvoiceByIdAsync(invoiceId);

        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = InvoiceConstants.InvoiceNotFound });
    }

    [HttpPut("{invoiceId}")]
    public async Task<IActionResult> UpdateInvoice(long invoiceId, [FromBody] InvoiceRequestDto invoiceDto)
    {
        if (invoiceId <= 0 || !ModelState.IsValid || invoiceDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _invoiceService.UpdateInvoiceAsync(invoiceId, invoiceDto);

        return data != null
            ? Ok(new { message = InvoiceConstants.InvoiceUpdateSuccess, data })
            : NotFound(new { message = InvoiceConstants.InvoiceNotFound });
    }

    [HttpDelete("{invoiceId}")]
    public async Task<IActionResult> DeleteInvoice(long invoiceId)
    {
        if (invoiceId <= 0)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var result = await _invoiceService.DeleteInvoiceAsync(invoiceId);

        return result
            ? Ok(new { message = InvoiceConstants.InvoiceDeleteSuccess })
            : NotFound(new { message = InvoiceConstants.InvoiceNotFound });
    }

    [HttpPatch("{invoiceId}/status")]
    public async Task<IActionResult> UpdateInvoiceStatus(long invoiceId, [FromBody] InvoiceStatusUpdateDto statusDto)
    {
        if (invoiceId <= 0 || !ModelState.IsValid || statusDto == null)
        {
            return BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        var data = await _invoiceService.UpdateInvoiceStatusAsync(invoiceId, statusDto.NewStatus);

        return data != null
            ? Ok(new { message = InvoiceConstants.InvoiceStatusUpdateSuccess, data })
            : NotFound(new { message = InvoiceConstants.InvoiceNotFound });
    }
}