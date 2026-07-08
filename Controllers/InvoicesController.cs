using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Enum;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _invoiceService;
    private readonly INotificationService _notificationService;

    public InvoicesController(IInvoiceService invoiceService, INotificationService notificationService)
    {
        _invoiceService = invoiceService;
        _notificationService = notificationService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,FinanceOfficer,CorporateTravelManager,Traveler")]
    public async Task<IActionResult> GetAllInvoices([FromQuery] InvoiceSearchDto searchDto)
    {
        if (!ModelState.IsValid || searchDto == null)
            return BadRequest(new { message = GeneralConstants.InvalidInput });

        var data = await _invoiceService.GetAllInvoicesAsync(searchDto);
        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = InvoiceConstants.InvoiceNotFound });
    }

    [HttpPost]
    [Authorize(Roles = "Admin,FinanceOfficer,CorporateTravelManager,Traveler")]
    public async Task<IActionResult> CreateInvoice([FromBody] InvoiceRequestDto invoiceDto)
    {
        if (!ModelState.IsValid || invoiceDto == null)
            return BadRequest(new { message = GeneralConstants.InvalidInput });

        var data = await _invoiceService.CreateInvoiceAsync(invoiceDto);

        if (data != null && data.UserId > 0)
        {
            try
            {
                await _notificationService.TriggerInvoiceNotificationAsync(
                    data.UserId,
                    $"A new invoice has been created for your booking. Amount: ${data.Amount}, Due: {data.DueDate:yyyy-MM-dd}.",
                    NotificationCategory.PaymentReminder
                );
            }
            catch (Exception ex) { Console.WriteLine($"Notification failed for invoice creation: {ex.Message}"); }

            return Ok(new { message = InvoiceConstants.InvoiceCreatedSuccess, data });
        }

        return BadRequest(new { message = GeneralConstants.InvalidInput });
    }

    [HttpGet("{invoiceId}")]
    [Authorize(Roles = "Admin,FinanceOfficer,CorporateTravelManager,Traveler")]
    public async Task<IActionResult> GetInvoiceById(long invoiceId)
    {
        if (invoiceId <= 0)
            return BadRequest(new { message = GeneralConstants.InvalidInput });

        var data = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        return data != null
            ? Ok(new { message = GeneralConstants.OperationSuccess, data })
            : NotFound(new { message = InvoiceConstants.InvoiceNotFound });
    }

    [HttpPut("{invoiceId}")]
    [Authorize(Roles = "Admin,FinanceOfficer")]
    public async Task<IActionResult> UpdateInvoice(long invoiceId, [FromBody] InvoiceRequestDto invoiceDto)
    {
        if (invoiceId <= 0 || !ModelState.IsValid || invoiceDto == null)
            return BadRequest(new { message = GeneralConstants.InvalidInput });

        var data = await _invoiceService.UpdateInvoiceAsync(invoiceId, invoiceDto);

        if (data != null && data.UserId > 0)
        {
            try
            {
                await _notificationService.TriggerInvoiceNotificationAsync(
                    data.UserId,
                    $"Your invoice has been updated. New Amount: ${data.Amount}.",
                    NotificationCategory.SystemAlert
                );
            }
            catch (Exception ex) { Console.WriteLine($"Notification failed for invoice update: {ex.Message}"); }

            return Ok(new { message = InvoiceConstants.InvoiceUpdateSuccess, data });
        }

        return NotFound(new { message = InvoiceConstants.InvoiceNotFound });
    }

    [HttpDelete("{invoiceId}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteInvoice(long invoiceId)
    {
        if (invoiceId <= 0)
            return BadRequest(new { message = GeneralConstants.InvalidInput });

        var existing = await _invoiceService.GetInvoiceByIdAsync(invoiceId);
        var result = await _invoiceService.DeleteInvoiceAsync(invoiceId);

        if (result)
        {
            if (existing?.UserId > 0)
            {
                try
                {
                    await _notificationService.TriggerInvoiceNotificationAsync(
                        existing.UserId,
                        $"Your invoice for booking has been deleted.",
                        NotificationCategory.SystemAlert
                    );
                }
                catch (Exception ex) { Console.WriteLine($"Notification failed for invoice deletion: {ex.Message}"); }
            }
            return Ok(new { message = InvoiceConstants.InvoiceDeleteSuccess });
        }

        return NotFound(new { message = InvoiceConstants.InvoiceNotFound });
    }

    [HttpPatch("{invoiceId}/status")]
    [Authorize(Roles = "Admin,FinanceOfficer,CorporateTravelManager")]
    public async Task<IActionResult> UpdateInvoiceStatus(long invoiceId, [FromBody] InvoiceStatusUpdateDto statusDto)
    {
        if (invoiceId <= 0 || !ModelState.IsValid || statusDto == null)
            return BadRequest(new { message = GeneralConstants.InvalidInput });

        var data = await _invoiceService.UpdateInvoiceStatusAsync(invoiceId, statusDto.NewStatus);

        if (data != null && data.UserId > 0)
        {
            try
            {
                await _notificationService.TriggerInvoiceNotificationAsync(
                    data.UserId,
                    $"Your invoice status has been updated to: {(InvoiceStatus)statusDto.NewStatus}.",
                    NotificationCategory.PaymentConfirmation
                );
            }
            catch (Exception ex) { Console.WriteLine($"Notification failed for invoice status update: {ex.Message}"); }

            return Ok(new { message = InvoiceConstants.InvoiceStatusUpdateSuccess, data });
        }

        return NotFound(new { message = InvoiceConstants.InvoiceNotFound });
    }
}
