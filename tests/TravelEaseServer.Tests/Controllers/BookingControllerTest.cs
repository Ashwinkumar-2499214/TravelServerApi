using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TravelEaseServer.Constant;
using TravelEaseServer.Controllers;
using TravelEaseServer.Dto;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Tests.Controllers
{
    [TestFixture]
    public class BookingsControllerTests
    {
        private Mock<IBookingService> _mockService;
        private BookingsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockService = new Mock<IBookingService>(MockBehavior.Strict);
            _controller = new BookingsController(_mockService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _mockService.VerifyAll();
        }

        #region 1. GetAllBookings Endpoints

        [Test]
        public async Task GetAllBookings_NullSearchDto_ReturnsBadRequest()
        {
            var result = await _controller.GetAllBookings(null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetAllBookings_InvalidModelState_ReturnsBadRequest()
        {
            var searchDto = new BookingSearchDto();
            _controller.ModelState.AddModelError("StartDate", "Invalid Date Range");

            var result = await _controller.GetAllBookings(searchDto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _controller.ModelState.Clear();
        }

        [Test]
        public async Task GetAllBookings_ValidSearch_ReturnsOkWithData()
        {
            var searchDto = new BookingSearchDto();
            var expectedList = new List<BookingResponseDto>
            {
                new() { BookingId = 101, UserId = 1, Amount = 250.00m }
            };

            _mockService.Setup(s => s.GetAllBookingsAsync(searchDto)).ReturnsAsync(expectedList);

            var result = await _controller.GetAllBookings(searchDto) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as IEnumerable<BookingResponseDto>;

            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data!.Count());
        }

        [Test]
        public async Task GetAllBookings_ServiceReturnsNull_ReturnsNotFound()
        {
            var searchDto = new BookingSearchDto();
            _mockService.Setup(s => s.GetAllBookingsAsync(searchDto)).ReturnsAsync((IEnumerable<BookingResponseDto>?)null);

            var result = await _controller.GetAllBookings(searchDto);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        #endregion

        #region 2. CreateBooking Endpoints

        [Test]
        public async Task CreateBooking_NullOrInvalidDtoFields_ReturnsBadRequest()
        {
            // Scenario A: Null Dto
            var resNull = await _controller.CreateBooking(null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(resNull);

            // Scenario B: Zero or Negative IDs / Amount
            var invalidDto = new BookingRequestDto { UserId = 0, PartnerId = 1, InventoryId = 1, Amount = 100 };
            var resInvalidId = await _controller.CreateBooking(invalidDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(resInvalidId);

            var negativeAmountDto = new BookingRequestDto { UserId = 1, PartnerId = 1, InventoryId = 1, Amount = -50 };
            var resInvalidAmount = await _controller.CreateBooking(negativeAmountDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(resInvalidAmount);
        }

        [Test]
        public async Task CreateBooking_InvalidModelState_ReturnsBadRequest()
        {
            var dto = new BookingRequestDto { UserId = 1, PartnerId = 1, InventoryId = 1, Amount = 100 };
            _controller.ModelState.AddModelError("Amount", "Amount limits exceeded");

            var result = await _controller.CreateBooking(dto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _controller.ModelState.Clear();
        }

        [Test]
        public async Task CreateBooking_ValidDto_ReturnsOkWithCreatedData()
        {
            var request = new BookingRequestDto { UserId = 1, PartnerId = 2, InventoryId = 3, Amount = 500.50m };
            var createdResponse = new BookingResponseDto { BookingId = 1, UserId = 1, Amount = 500.50m };

            _mockService.Setup(s => s.CreateBookingAsync(request)).ReturnsAsync(createdResponse);

            var result = await _controller.CreateBooking(request) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as BookingResponseDto;

            Assert.AreEqual(BookingConstants.BookingCreatedSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data!.BookingId);
        }

        [Test]
        public async Task CreateBooking_ServiceFailsToCreate_ReturnsBadRequest()
        {
            var request = new BookingRequestDto { UserId = 1, PartnerId = 2, InventoryId = 3, Amount = 500.50m };
            _mockService.Setup(s => s.CreateBookingAsync(request)).ReturnsAsync((BookingResponseDto?)null);

            var result = await _controller.CreateBooking(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        #endregion

        #region 3. GetBookingById Endpoints

        [Test]
        public async Task GetBookingById_InvalidId_ReturnsBadRequest()
        {
            var result = await _controller.GetBookingById(0);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetBookingById_NotFound_ReturnsNotFound()
        {
            long bookingId = 999;
            _mockService.Setup(s => s.GetBookingByIdAsync(bookingId)).ReturnsAsync((BookingResponseDto?)null);

            var result = await _controller.GetBookingById(bookingId);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetBookingById_ValidId_ReturnsOkWithData()
        {
            long bookingId = 45;
            var expectedBooking = new BookingResponseDto { BookingId = bookingId, UserId = 3, Amount = 120.00m };
            _mockService.Setup(s => s.GetBookingByIdAsync(bookingId)).ReturnsAsync(expectedBooking);

            var result = await _controller.GetBookingById(bookingId) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as BookingResponseDto;

            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(bookingId, data!.BookingId);
        }

        #endregion

        #region 4. UpdateBooking Endpoints

        [Test]
        public async Task UpdateBooking_InvalidParameters_ReturnsBadRequest()
        {
            var dto = new BookingRequestDto { UserId = 1, PartnerId = 1, InventoryId = 1, Amount = 100 };

            // Scenario A: Invalid Booking ID
            var resIdInvalid = await _controller.UpdateBooking(0, dto);
            Assert.IsInstanceOf<BadRequestObjectResult>(resIdInvalid);

            // Scenario B: Null Request DTO
            var resDtoNull = await _controller.UpdateBooking(5, null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(resDtoNull);

            // Scenario C: Invalid properties inside DTO
            var badDto = new BookingRequestDto { UserId = -1, PartnerId = 1, InventoryId = 1, Amount = 0 };
            var resFieldsInvalid = await _controller.UpdateBooking(5, badDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(resFieldsInvalid);
        }

        [Test]
        public async Task UpdateBooking_InvalidModelState_ReturnsBadRequest()
        {
            var dto = new BookingRequestDto { UserId = 1, PartnerId = 1, InventoryId = 1, Amount = 100 };
            _controller.ModelState.AddModelError("PartnerId", "Partner has been blacklisted");

            var result = await _controller.UpdateBooking(5, dto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _controller.ModelState.Clear();
        }

        [Test]
        public async Task UpdateBooking_NotFound_ReturnsNotFound()
        {
            long bookingId = 77;
            var dto = new BookingRequestDto { UserId = 1, PartnerId = 1, InventoryId = 1, Amount = 100 };
            _mockService.Setup(s => s.UpdateBookingAsync(bookingId, dto)).ReturnsAsync((BookingResponseDto?)null);

            var result = await _controller.UpdateBooking(bookingId, dto);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task UpdateBooking_Success_ReturnsOkWithUpdatedData()
        {
            long bookingId = 77;
            var dto = new BookingRequestDto { UserId = 1, PartnerId = 1, InventoryId = 1, Amount = 150.00m };
            var updatedResponse = new BookingResponseDto { BookingId = bookingId, UserId = 1, Amount = 150.00m };

            _mockService.Setup(s => s.UpdateBookingAsync(bookingId, dto)).ReturnsAsync(updatedResponse);

            var result = await _controller.UpdateBooking(bookingId, dto) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as BookingResponseDto;

            Assert.AreEqual(BookingConstants.BookingUpdateSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(150.00m, data!.Amount);
        }

        #endregion

        #region 5. DeleteBooking Endpoints

        [Test]
        public async Task DeleteBooking_InvalidId_ReturnsBadRequest()
        {
            var result = await _controller.DeleteBooking(-1);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteBooking_NotFound_ReturnsNotFound()
        {
            long bookingId = 88;
            _mockService.Setup(s => s.DeleteBookingAsync(bookingId)).ReturnsAsync(false);

            var result = await _controller.DeleteBooking(bookingId);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task DeleteBooking_Success_ReturnsOk()
        {
            long bookingId = 88;
            _mockService.Setup(s => s.DeleteBookingAsync(bookingId)).ReturnsAsync(true);

            var result = await _controller.DeleteBooking(bookingId) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;

            Assert.AreEqual(BookingConstants.BookingDeleteSuccess, message);
        }

        #endregion

        #region 6. UpdateBookingStatus Endpoints

        [Test]
        public async Task UpdateBookingStatus_InvalidInput_ReturnsBadRequest()
        {
            var statusDto = new BookingStatusUpdateDto { NewStatus = 1 };

            // Scenario A: Invalid Booking ID
            var resIdInvalid = await _controller.UpdateBookingStatus(0, statusDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(resIdInvalid);

            // Scenario B: Null DTO payload
            var resDtoNull = await _controller.UpdateBookingStatus(10, null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(resDtoNull);

            // Scenario C: Negative Status index boundary
            var badStatusDto = new BookingStatusUpdateDto { NewStatus = -1 };
            var resStatusInvalid = await _controller.UpdateBookingStatus(10, badStatusDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(resStatusInvalid);
        }

        [Test]
        public async Task UpdateBookingStatus_InvalidModelState_ReturnsBadRequest()
        {
            var statusDto = new BookingStatusUpdateDto { NewStatus = 2 };
            _controller.ModelState.AddModelError("NewStatus", "Status step skipped transition validation logic");

            var result = await _controller.UpdateBookingStatus(10, statusDto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _controller.ModelState.Clear();
        }

        [Test]
        public async Task UpdateBookingStatus_NotFound_ReturnsNotFound()
        {
            long bookingId = 505;
            var statusDto = new BookingStatusUpdateDto { NewStatus = 2 };
            _mockService.Setup(s => s.UpdateBookingStatusAsync(bookingId, statusDto.NewStatus)).ReturnsAsync((BookingResponseDto?)null);

            var result = await _controller.UpdateBookingStatus(bookingId, statusDto);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task UpdateBookingStatus_Success_ReturnsOkWithUpdatedData()
        {
            long bookingId = 505;
            var statusDto = new BookingStatusUpdateDto { NewStatus = 3 };
            var updatedResponse = new BookingResponseDto { BookingId = bookingId, UserId = 4, Amount = 300.00m };

            _mockService.Setup(s => s.UpdateBookingStatusAsync(bookingId, statusDto.NewStatus)).ReturnsAsync(updatedResponse);

            var result = await _controller.UpdateBookingStatus(bookingId, statusDto) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as BookingResponseDto;

            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(bookingId, data!.BookingId);
        }

        #endregion
    }
}