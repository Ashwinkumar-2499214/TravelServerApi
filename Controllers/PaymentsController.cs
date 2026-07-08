using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Enum;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly INotificationService _notificationService;

        public PaymentsController(
            IPaymentService paymentService,
            INotificationService notificationService)
        {
            _paymentService = paymentService;
            _notificationService = notificationService;
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

            if (data != null)
            {
                // Trigger payment confirmation notification
                try
                {
                    await _notificationService.TriggerPaymentNotificationAsync(
                        paymentDto.UserId,
                        $"Your payment of ${data.Amount} has been successfully processed.",
                        NotificationCategory.PaymentConfirmation
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification trigger failed for payment creation: {ex.Message}");
                }
            }

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
            if (!(ModelState.IsValid && statusDto != null))
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var data = await _paymentService.UpdatePaymentStatusAsync(paymentId, statusDto.NewStatus);

            if (data != null)
            {
                // Trigger payment status update notification
                try
                {
                    var statusMessage = GetPaymentStatusMessage(statusDto.NewStatus);
                    await _notificationService.TriggerPaymentNotificationAsync(
                        data.UserId,
                        $"Your payment of ${data.Amount} status has been updated to: {statusMessage}.",
                        NotificationCategory.SystemAlert
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification trigger failed for payment status update: {ex.Message}");
                }
            }

            return ModelState.IsValid && statusDto != null
                ? Ok(new { message = PaymentConstants.PaymentStatusUpdateSuccess, data })
                : BadRequest(new { message = GeneralConstants.InvalidInput });
        }

        [HttpPost("{paymentId}/refund")]
        [Authorize(Roles = "Admin,FinanceOfficer")]
        public async Task<IActionResult> ProcessRefund(long paymentId, [FromBody] PaymentRefundDto refundDto)
        {
            if (!(ModelState.IsValid && refundDto != null))
                return BadRequest(new { message = GeneralConstants.InvalidInput });

            var result = await _paymentService.ProcessRefundAsync(paymentId, refundDto.RefundAmount, refundDto.Reason);

            if (result)
            {
                // Trigger refund notification
                try
                {
                    var payment = await _paymentService.GetPaymentByIdAsync(paymentId);
                    if (payment != null)
                    {
                        await _notificationService.TriggerPaymentNotificationAsync(
                            payment.UserId,
                            $"A refund of ${refundDto.RefundAmount} has been processed for your payment of ${payment.Amount}. Reason: {refundDto.Reason}.",
                            NotificationCategory.PaymentReminder
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification trigger failed for refund processing: {ex.Message}");
                }

                return Ok(new { message = PaymentConstants.RefundProcessedSuccess });
            }

            return BadRequest(new { message = PaymentConstants.CannotRefundFailedPayment });
        }

        /// <summary>
        /// Helper method to convert payment status code to readable message
        /// </summary>
        private static string GetPaymentStatusMessage(int statusCode)
        {
            return statusCode switch
            {
                0 => "Pending",
                1 => "Completed",
                2 => "Failed",
                3 => "Refunded",
                _ => "Unknown"
            };
        }
    }
}