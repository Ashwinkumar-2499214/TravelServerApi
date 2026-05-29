using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPayments([FromQuery] PaymentSearchDto searchDto)
        {
            return ModelState.IsValid && searchDto != null
                ? Ok(new { message = GeneralConstants.OperationSuccess, data = await _paymentService.GetAllPaymentsAsync(searchDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpGet("/api/v1/invoices/{invoiceId}/payments")]
        public async Task<IActionResult> GetInvoicePayments(long invoiceId)
        {
            var payments = await _paymentService.GetInvoicePaymentsAsync(invoiceId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = payments });
        }

        [HttpPost("/api/v1/invoices/{invoiceId}/payments")]
        public async Task<IActionResult> CreatePayment(long invoiceId, [FromBody] PaymentRequestDto paymentDto)
        {
            return ModelState.IsValid && paymentDto != null
                ? Ok(new { message = PaymentConstants.PaymentCreatedSuccess, data = await _paymentService.CreatePaymentAsync(paymentDto) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpGet("{paymentId}")]
        public async Task<IActionResult> GetPaymentById(long paymentId)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
            return Ok(new { message = GeneralConstants.OperationSuccess, data = payment });
        }

        [HttpPatch("{paymentId}/status")]
        public async Task<IActionResult> UpdatePaymentStatus(long paymentId, [FromBody] PaymentStatusUpdateDto statusDto)
        {
            return ModelState.IsValid && statusDto != null
                ? Ok(new { message = PaymentConstants.PaymentStatusUpdateSuccess, data = await _paymentService.UpdatePaymentStatusAsync(paymentId, statusDto.NewStatus) })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpPost("{paymentId}/refund")]
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
