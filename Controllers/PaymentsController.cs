using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,FinanceOfficer,CorporateTravelManager")]
        public async Task<IActionResult> GetAllPayments([FromQuery] PaymentSearchDto searchDto)
        {
            return ModelState.IsValid && searchDto != null
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = await _paymentService.GetAllPaymentsAsync(searchDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpGet("/api/v1/invoices/{invoiceId}/payments")]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager,FinanceOfficer")]
        public async Task<IActionResult> GetInvoicePayments(long invoiceId)
        {
            var payments = await _paymentService.GetInvoicePaymentsAsync(invoiceId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = payments });
        }

        [HttpPost("/api/v1/invoices/{invoiceId}/payments")]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager")]
        public async Task<IActionResult> CreatePayment(long invoiceId, [FromBody] PaymentRequestDto paymentDto)
        {
            if (!ModelState.IsValid || paymentDto == null)
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            paymentDto.InvoiceId = invoiceId; 

            var data = await _paymentService.CreatePaymentAsync(paymentDto);
            return Ok(new { message = PaymentConstants.PaymentCreatedSuccess, data });
        }

        [HttpGet("{paymentId}")]
        [Authorize(Roles = "Admin,Traveler,TravelAgent,CorporateTravelManager,FinanceOfficer")]
        public async Task<IActionResult> GetPaymentById(long paymentId)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = payment });
        }

        [HttpPatch("{paymentId}/status")]
        [Authorize(Roles = "Admin,FinanceOfficer")]
        public async Task<IActionResult> UpdatePaymentStatus(long paymentId, [FromBody] PaymentStatusUpdateDto statusDto)
        {
            return ModelState.IsValid && statusDto != null
                ? Ok(new { message = PaymentConstants.PaymentStatusUpdateSuccess, data = await _paymentService.UpdatePaymentStatusAsync(paymentId, statusDto.NewStatus) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpPost("{paymentId}/refund")]
        [Authorize(Roles = "Admin,FinanceOfficer")]
        public async Task<IActionResult> ProcessRefund(long paymentId, [FromBody] PaymentRefundDto refundDto)
        {
            return ModelState.IsValid && refundDto != null
                ? (await _paymentService.ProcessRefundAsync(paymentId, refundDto.RefundAmount, refundDto.Reason))
                    ? Ok(new { message = PaymentConstants.RefundProcessedSuccess })
                    : BadRequest(new { message = PaymentConstants.CannotRefundFailedPayment })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }
    }
}