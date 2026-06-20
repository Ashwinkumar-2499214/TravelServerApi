using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TravelEaseServer.Controllers;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Tests
{
    [TestFixture]
    public class InvoicesControllerTests
    {
        private Mock<IInvoiceService> _mockInvoiceService;
        private InvoicesController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockInvoiceService = new Mock<IInvoiceService>();
            _controller = new InvoicesController(_mockInvoiceService.Object);
        }

        #region GetAllInvoices Tests

        [Test]
        public async Task GetAllInvoices_NullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.GetAllInvoices(null!);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetAllInvoices_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("PageNumber", "Required");
            var searchDto = new InvoiceSearchDto();

            // Act
            var result = await _controller.GetAllInvoices(searchDto);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetAllInvoices_ValidSearch_ReturnsOkWithData()
        {
            // Arrange
            var searchDto = new InvoiceSearchDto { PageNumber = 1, PageSize = 10 };
            var expectedResponse = new List<InvoiceResponseDto>
            {
                new() { InvoiceId = 1, BookingId = 100, Amount = 150.00m, Description = "Flight Invoice" }
            };

            _mockInvoiceService
                .Setup(s => s.GetAllInvoicesAsync(searchDto))
                .ReturnsAsync(expectedResponse);

            // Act
            var result = await _controller.GetAllInvoices(searchDto);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetAllInvoices_NoDataFound_ReturnsNotFound()
        {
            // Arrange
            var searchDto = new InvoiceSearchDto();
            _mockInvoiceService
                .Setup(s => s.GetAllInvoicesAsync(searchDto))
                .ReturnsAsync((IEnumerable<InvoiceResponseDto>?)null);

            // Act
            var result = await _controller.GetAllInvoices(searchDto);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        #endregion

        #region CreateInvoice Tests

        [Test]
        public async Task CreateInvoice_NullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.CreateInvoice(null!);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task CreateInvoice_Success_ReturnsOkWithData()
        {
            // Arrange
            var requestDto = new InvoiceRequestDto { BookingId = 100, Amount = 250.50m, Description = "Hotel Accommodation" };
            var responseDto = new InvoiceResponseDto { InvoiceId = 1, BookingId = 100, Amount = 250.50m };

            _mockInvoiceService
                .Setup(s => s.CreateInvoiceAsync(requestDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.CreateInvoice(requestDto);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task CreateInvoice_Failure_ReturnsBadRequest()
        {
            // Arrange
            var requestDto = new InvoiceRequestDto { BookingId = 100 };
            _mockInvoiceService
                .Setup(s => s.CreateInvoiceAsync(requestDto))
                .ReturnsAsync((InvoiceResponseDto?)null);

            // Act
            var result = await _controller.CreateInvoice(requestDto);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        #endregion

        #region GetInvoiceById Tests

        [TestCase(0)]
        [TestCase(-1)]
        public async Task GetInvoiceById_InvalidId_ReturnsBadRequest(long invalidId)
        {
            // Act
            var result = await _controller.GetInvoiceById(invalidId);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetInvoiceById_ExistingId_ReturnsOkWithData()
        {
            // Arrange
            long invoiceId = 5;
            var responseDto = new InvoiceResponseDto { InvoiceId = invoiceId, BookingId = 200, Amount = 99.99m };

            _mockInvoiceService
                .Setup(s => s.GetInvoiceByIdAsync(invoiceId))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.GetInvoiceById(invoiceId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task GetInvoiceById_NonExistingId_ReturnsNotFound()
        {
            // Arrange
            long invoiceId = 999;
            _mockInvoiceService
                .Setup(s => s.GetInvoiceByIdAsync(invoiceId))
                .ReturnsAsync((InvoiceResponseDto?)null);

            // Act
            var result = await _controller.GetInvoiceById(invoiceId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        #endregion

        #region UpdateInvoice Tests

        [Test]
        public async Task UpdateInvoice_InvalidIdOrNullDto_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.UpdateInvoice(0, null!);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task UpdateInvoice_ExistingInvoiceSuccess_ReturnsOkWithData()
        {
            // Arrange
            long invoiceId = 10;
            var requestDto = new InvoiceRequestDto { BookingId = 101, Amount = 300m };
            var responseDto = new InvoiceResponseDto { InvoiceId = invoiceId, Amount = 300m };

            _mockInvoiceService
                .Setup(s => s.UpdateInvoiceAsync(invoiceId, requestDto))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.UpdateInvoice(invoiceId, requestDto);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task UpdateInvoice_NonExistingInvoice_ReturnsNotFound()
        {
            // Arrange
            long invoiceId = 999;
            var requestDto = new InvoiceRequestDto { BookingId = 101 };

            _mockInvoiceService
                .Setup(s => s.UpdateInvoiceAsync(invoiceId, requestDto))
                .ReturnsAsync((InvoiceResponseDto?)null);

            // Act
            var result = await _controller.UpdateInvoice(invoiceId, requestDto);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        #endregion

        #region DeleteInvoice Tests

        [TestCase(0)]
        [TestCase(-5)]
        public async Task DeleteInvoice_InvalidId_ReturnsBadRequest(long invalidId)
        {
            // Act
            var result = await _controller.DeleteInvoice(invalidId);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteInvoice_ExistingInvoice_ReturnsOk()
        {
            // Arrange
            long invoiceId = 12;
            _mockInvoiceService
                .Setup(s => s.DeleteInvoiceAsync(invoiceId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteInvoice(invoiceId);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task DeleteInvoice_NonExistingInvoice_ReturnsNotFound()
        {
            // Arrange
            long invoiceId = 999;
            _mockInvoiceService
                .Setup(s => s.DeleteInvoiceAsync(invoiceId))
                .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteInvoice(invoiceId);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        #endregion

        #region UpdateInvoiceStatus Tests

        [Test]
        public async Task UpdateInvoiceStatus_InvalidInputs_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.UpdateInvoiceStatus(0, null!);

            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task UpdateInvoiceStatus_ExistingInvoice_ReturnsOkWithData()
        {
            // Arrange
            long invoiceId = 45;
            var statusDto = new InvoiceStatusUpdateDto { NewStatus = 2 };
            var responseDto = new InvoiceResponseDto { InvoiceId = invoiceId, Status = 2 };

            _mockInvoiceService
                .Setup(s => s.UpdateInvoiceStatusAsync(invoiceId, statusDto.NewStatus))
                .ReturnsAsync(responseDto);

            // Act
            var result = await _controller.UpdateInvoiceStatus(invoiceId, statusDto);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }

        [Test]
        public async Task UpdateInvoiceStatus_NonExistingInvoice_ReturnsNotFound()
        {
            // Arrange
            long invoiceId = 999;
            var statusDto = new InvoiceStatusUpdateDto { NewStatus = 3 };

            _mockInvoiceService
                .Setup(s => s.UpdateInvoiceStatusAsync(invoiceId, statusDto.NewStatus))
                .ReturnsAsync((InvoiceResponseDto?)null);

            // Act
            var result = await _controller.UpdateInvoiceStatus(invoiceId, statusDto);

            // Assert
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        #endregion
    }
}