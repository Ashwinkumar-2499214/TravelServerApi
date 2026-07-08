using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TravelEaseServer.Controllers;
using TravelEaseServer.Constant;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;
 
namespace TravelEaseServer.Tests.Controllers
{
    [TestFixture]
    public class PaymentsControllerTests
    {
        private Mock<IPaymentService> _mockPaymentService;
        private Mock<INotificationService> _mockNotificationService;
        private PaymentsController _controller;
 
        [SetUp]
        public void Setup()
        {
            _mockPaymentService = new Mock<IPaymentService>();
            _mockNotificationService = new Mock<INotificationService>();
            _controller = new PaymentsController(_mockPaymentService.Object, _mockNotificationService.Object);
            _controller.ModelState.Clear();
        }
 
        #region 1. GetAllPayments Tests (3 Cases)
 
        [Test]
        public async Task GetAllPayments_NullDto_ReturnsBadRequest()
        {
            // Case 1: Dto is completely null
            var result = await _controller.GetAllPayments(null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task GetAllPayments_InvalidModelState_ReturnsBadRequest()
        {
            // Case 2: Model state contains mapping or validation errors
            _controller.ModelState.AddModelError("PageNumber", "Invalid page value");
            var result = await _controller.GetAllPayments(new PaymentSearchDto());
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task GetAllPayments_Success_ReturnsOkWithData()
        {
            // Case 3: Valid input yields correct dataset payload
            var searchDto = new PaymentSearchDto { Status = 1 };
            var expectedList = new List<PaymentResponseDto> { new() { PaymentId = 1, Amount = 250.00m } };
            _mockPaymentService.Setup(s => s.GetAllPaymentsAsync(searchDto)).ReturnsAsync(expectedList);
 
            var result = await _controller.GetAllPayments(searchDto);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region 2. GetInvoicePayments Tests (3 Cases)
 
        [Test]
        public async Task GetInvoicePayments_NoDataFound_ReturnsOkWithEmptyList()
        {
            // Case 1: Active invoice ID but has no associated payments registered
            long invoiceId = 55;
            _mockPaymentService.Setup(s => s.GetInvoicePaymentsAsync(invoiceId))
                .ReturnsAsync(new List<PaymentResponseDto>());
 
            var result = await _controller.GetInvoicePayments(invoiceId);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        [Test]
        public async Task GetInvoicePayments_NullCollection_ReturnsOk()
        {
            // Case 2: Service yields a null collection response edge-case
            long invoiceId = 99;
            _mockPaymentService.Setup(s => s.GetInvoicePaymentsAsync(invoiceId))
                .ReturnsAsync((IEnumerable<PaymentResponseDto>?)null);
 
            var result = await _controller.GetInvoicePayments(invoiceId);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        [Test]
        public async Task GetInvoicePayments_Success_ReturnsOkWithData()
        {
            // Case 3: Returns matching structural payments array
            long invoiceId = 10;
            var payload = new List<PaymentResponseDto> { new() { PaymentId = 1, InvoiceId = invoiceId } };
            _mockPaymentService.Setup(s => s.GetInvoicePaymentsAsync(invoiceId)).ReturnsAsync(payload);
 
            var result = await _controller.GetInvoicePayments(invoiceId);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region 3. CreatePayment Tests (3 Cases)
 
        [Test]
        public async Task CreatePayment_NullDto_ReturnsBadRequest()
        {
            // Case 1: Payload payload is absent from contextual body
            var result = await _controller.CreatePayment(1, null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task CreatePayment_InvalidModelState_ReturnsBadRequest()
        {
            // Case 2: Validation constraints fail inside controller interceptor
            _controller.ModelState.AddModelError("Amount", "Amount required");
            var result = await _controller.CreatePayment(1, new PaymentRequestDto());
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task CreatePayment_Success_ReturnsOkWithCreatedPayload()
        {
            // Case 3: Smooth tracking assignment and generation path
            long invoiceId = 5;
            var requestDto = new PaymentRequestDto { Amount = 100m, Method = 1, TransactionReference = "TXN-100" };
            var expectedResponse = new PaymentResponseDto { PaymentId = 20, InvoiceId = invoiceId, Amount = 100m };
 
            _mockPaymentService.Setup(s => s.CreatePaymentAsync(It.IsAny<PaymentRequestDto>())).ReturnsAsync(expectedResponse);
 
            var result = await _controller.CreatePayment(invoiceId, requestDto);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region 4. GetPaymentById Tests (3 Cases)
 
        [Test]
        public async Task GetPaymentById_NotFound_ReturnsOkWithNullData()
        {
            // Case 1: Id is missing from DB (Controller returns Ok even if null based on codebase logic)
            long paymentId = 404;
            _mockPaymentService.Setup(s => s.GetPaymentByIdAsync(paymentId)).ReturnsAsync((PaymentResponseDto?)null);
 
            var result = await _controller.GetPaymentById(paymentId);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        [Test]
        public async Task GetPaymentById_Success_ReturnsOkWithPayment()
        {
            // Case 2: Target entry fetched clean from persistence engine
            long paymentId = 123;
            var mockPayment = new PaymentResponseDto { PaymentId = paymentId, Amount = 50.00m };
            _mockPaymentService.Setup(s => s.GetPaymentByIdAsync(paymentId)).ReturnsAsync(mockPayment);
 
            var result = await _controller.GetPaymentById(paymentId);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        [Test]
        public async Task GetPaymentById_BoundaryValue_ReturnsOk()
        {
            // Case 3: Handles large ID value boundaries gracefully
            long paymentId = long.MaxValue;
            var mockPayment = new PaymentResponseDto { PaymentId = paymentId };
            _mockPaymentService.Setup(s => s.GetPaymentByIdAsync(paymentId)).ReturnsAsync(mockPayment);
 
            var result = await _controller.GetPaymentById(paymentId);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region 5. UpdatePaymentStatus Tests (3 Cases)
 
        [Test]
        public async Task UpdatePaymentStatus_NullDto_ReturnsBadRequest()
        {
            // Case 1: Status payload is empty or missing completely
            var result = await _controller.UpdatePaymentStatus(1, null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task UpdatePaymentStatus_InvalidModelState_ReturnsBadRequest()
        {
            // Case 2: Model dictionary validation attributes fail
            _controller.ModelState.AddModelError("NewStatus", "Out of bounds range");
            var result = await _controller.UpdatePaymentStatus(1, new PaymentStatusUpdateDto());
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task UpdatePaymentStatus_Success_ReturnsOkWithUpdatedRecord()
        {
            // Case 3: Changes state flag successfully
            long paymentId = 10;
            var statusDto = new PaymentStatusUpdateDto { PaymentId = paymentId, NewStatus = 2 };
            var mockResponse = new PaymentResponseDto { PaymentId = paymentId, Status = 2 };
 
            _mockPaymentService.Setup(s => s.UpdatePaymentStatusAsync(paymentId, 2)).ReturnsAsync(mockResponse);
 
            var result = await _controller.UpdatePaymentStatus(paymentId, statusDto);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region 6. ProcessRefund Tests (3 Cases)
 
        [Test]
        public async Task ProcessRefund_NullDtoOrInvalidState_ReturnsBadRequest()
        {
            // Case 1: Initial payload input missing or structurally malformed
            var result = await _controller.ProcessRefund(1, null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task ProcessRefund_PaymentCannotBeRefunded_ReturnsBadRequest()
        {
            // Case 2: Service layer returns false (e.g., trying to refund a failed transaction)
            long paymentId = 15;
            var refundDto = new PaymentRefundDto { RefundAmount = 50m, Reason = "Customer cancel" };
           
            _mockPaymentService.Setup(s => s.ProcessRefundAsync(paymentId, 50m, "Customer cancel"))
                .ReturnsAsync(false);
 
            var result = await _controller.ProcessRefund(paymentId, refundDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task ProcessRefund_Success_ReturnsOk()
        {
            // Case 3: Complete operational confirmation validation mapping
            long paymentId = 15;
            var refundDto = new PaymentRefundDto { RefundAmount = 50m, Reason = "Customer cancel" };
 
            _mockPaymentService.Setup(s => s.ProcessRefundAsync(paymentId, 50m, "Customer cancel"))
                .ReturnsAsync(true);
 
            var result = await _controller.ProcessRefund(paymentId, refundDto);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
    }
}
 