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
    public class ReservationsControllerTests
    {
        private Mock<IReservationService> _mockReservationService;
        private Mock<INotificationService> _mockNotificationService;
        private ReservationsController _controller;
 
        [SetUp]
        public void Setup()
        {
            _mockReservationService = new Mock<IReservationService>();
            _mockNotificationService = new Mock<INotificationService>();
            _controller = new ReservationsController(_mockReservationService.Object, _mockNotificationService.Object);
        }
 
        #region Helper Methods
 
        /// <summary>
        /// Safely extracts property values from anonymous objects returned in OkObjectResult/BadRequestObjectResult.
        /// </summary>
        private static object GetAnonymousPropertyValue(object obj, string propertyName)
        {
            return obj?.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }
 
        #endregion
 
        #region GetAllReservations Tests
 
        [Test]
        public async Task GetAllReservations_ValidRequest_Returns200OkWithData()
        {
            // Arrange
            var searchDto = new ReservationSearchDto();
            var expectedData = new System.Collections.Generic.List<ReservationResponseDto>
            {
                new ReservationResponseDto
                {
                    ReservationId = 1,
                    BookingId = 1,
                    Details = "Test",
                    StartDate = System.DateTime.UtcNow,
                    EndDate = System.DateTime.UtcNow.AddDays(1),
                    Status = 1,
                    CreatedDate = System.DateTime.UtcNow
                }
            } as System.Collections.Generic.IEnumerable<ReservationResponseDto>;
            _mockReservationService
                .Setup(s => s.GetAllReservationsAsync(searchDto))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.GetAllReservations(searchDto);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
           
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        public async Task GetAllReservations_NullDto_Returns400BadRequest()
        {
            // Act
            var result = await _controller.GetAllReservations(null!);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(GeneralConstants.InvalidInput, message);
        }
 
        [Test]
        public async Task GetAllReservations_InvalidModelState_Returns400BadRequest()
        {
            // Arrange
            var searchDto = new ReservationSearchDto();
            _controller.ModelState.AddModelError("SearchKey", "Invalid criteria");
 
            // Act
            var result = await _controller.GetAllReservations(searchDto);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(GeneralConstants.InvalidInput, message);
        }
 
        #endregion
 
        #region CreateReservation Tests
 
        [Test]
        public async Task CreateReservation_ValidRequest_Returns200OkWithCreatedData()
        {
            // Arrange
            var requestDto = new ReservationRequestDto
            {
                BookingId = 1,
                Details = "Details for reservation",
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow.AddDays(1)
            };
            var expectedData = new ReservationResponseDto
            {
                ReservationId = 2,
                BookingId = requestDto.BookingId,
                Details = requestDto.Details,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                Status = 1,
                CreatedDate = System.DateTime.UtcNow
            };
            _mockReservationService
                .Setup(s => s.CreateReservationAsync(requestDto))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.CreateReservation(requestDto);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(ReservationConstants.ReservationCreatedSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        public async Task CreateReservation_NullDto_Returns400BadRequest()
        {
            // Act
            var result = await _controller.CreateReservation(null!);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(GeneralConstants.InvalidInput, message);
        }
 
        [Test]
        public async Task CreateReservation_InvalidModelState_Returns400BadRequest()
        {
            // Arrange
            var requestDto = new ReservationRequestDto
            {
                BookingId = 1,
                Details = "Edited details",
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow.AddDays(1)
            };
            _controller.ModelState.AddModelError("RoomNumber", "Required");
 
            // Act
            var result = await _controller.CreateReservation(requestDto);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        #endregion
 
        #region GetReservationById Tests
 
        [Test]
        public async Task GetReservationById_ValidId_Returns200OkWithData()
        {
            // Arrange
            long reservationId = 101;
            var expectedData = new ReservationResponseDto
            {
                ReservationId = reservationId,
                BookingId = 1,
                Details = "Existing",
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow.AddDays(1),
                Status = 1,
                CreatedDate = System.DateTime.UtcNow
            };
            _mockReservationService
                .Setup(s => s.GetReservationByIdAsync(reservationId))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.GetReservationById(reservationId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        public async Task GetReservationById_IdDoesNotExist_Returns200OkWithNullData()
        {
            // Note: Per controller implementation, missing IDs still return 200 with null data
            // Arrange
            long reservationId = 999;
            _mockReservationService
                .Setup(s => s.GetReservationByIdAsync(reservationId))
                .Returns(System.Threading.Tasks.Task.FromResult<ReservationResponseDto>(default!));
 
            // Act
            var result = await _controller.GetReservationById(reservationId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
            Assert.IsNull(data);
        }
 
        [Test]
        [TestCase(-1)]
        [TestCase(0)]
        public async Task GetReservationById_EdgeCaseIds_Returns200OkWithNullOrMockedBehavior(long boundaryId)
        {
            // Note: The controller provides no route guards for negative IDs on this endpoint
            // Arrange
            _mockReservationService
                .Setup(s => s.GetReservationByIdAsync(boundaryId))
                .Returns(System.Threading.Tasks.Task.FromResult<ReservationResponseDto>(default!));
 
            // Act
            var result = await _controller.GetReservationById(boundaryId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region GetBookingReservations Tests
 
        [Test]
        public async Task GetBookingReservations_ValidBookingId_Returns200OkWithData()
        {
            // Arrange
            long bookingId = 55;
            var expectedData = new System.Collections.Generic.List<ReservationResponseDto>
            {
                new ReservationResponseDto
                {
                    ReservationId = 3,
                    BookingId = bookingId,
                    Details = "Booking reservation",
                    StartDate = System.DateTime.UtcNow,
                    EndDate = System.DateTime.UtcNow.AddDays(1),
                    Status = 1,
                    CreatedDate = System.DateTime.UtcNow
                }
            } as System.Collections.Generic.IEnumerable<ReservationResponseDto>;
            _mockReservationService
                .Setup(s => s.GetBookingReservationsAsync(bookingId))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.GetBookingReservations(bookingId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        public async Task GetBookingReservations_NoReservationsFound_Returns200OkWithNullOrEmpty()
        {
            // Arrange
            long bookingId = 55;
            _mockReservationService
                .Setup(s => s.GetBookingReservationsAsync(bookingId))
                .Returns(System.Threading.Tasks.Task.FromResult<System.Collections.Generic.IEnumerable<ReservationResponseDto>>(default!));
 
            // Act
            var result = await _controller.GetBookingReservations(bookingId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
            Assert.IsNull(data);
        }
 
        [Test]
        [TestCase(0)]
        [TestCase(-100)]
        public async Task GetBookingReservations_BoundaryBookingIds_Returns200Ok(long edgeBookingId)
        {
            // Act
            var result = await _controller.GetBookingReservations(edgeBookingId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region UpdateReservation Tests
 
        [Test]
        public async Task UpdateReservation_ValidRequest_Returns200OkWithUpdatedData()
        {
            // Arrange
            long reservationId = 10;
            var requestDto = new ReservationRequestDto
            {
                BookingId = 1,
                Details = "Updated details",
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow.AddDays(1)
            };
            var expectedData = new ReservationResponseDto
            {
                ReservationId = reservationId,
                BookingId = requestDto.BookingId,
                Details = requestDto.Details,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                Status = 1,
                CreatedDate = System.DateTime.UtcNow
            };
            _mockReservationService
                .Setup(s => s.UpdateReservationAsync(reservationId, requestDto))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.UpdateReservation(reservationId, requestDto);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(ReservationConstants.ReservationUpdateSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        public async Task UpdateReservation_NullDto_Returns400BadRequest()
        {
            // Act
            var result = await _controller.UpdateReservation(10, null!);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(GeneralConstants.InvalidInput, message);
        }
 
        [Test]
        public async Task UpdateReservation_InvalidModelState_Returns400BadRequest()
        {
            // Arrange
            var requestDto = new ReservationRequestDto
            {
                Details = "Test details",
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow.AddDays(1)
            };
            _controller.ModelState.AddModelError("IdMismatch", "Payload values do not align");
 
            // Act
            var result = await _controller.UpdateReservation(10, requestDto);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        #endregion
 
        #region DeleteReservation Tests
 
        [Test]
        public async Task DeleteReservation_SuccessPath_Returns200Ok()
        {
            // Arrange
            long reservationId = 500;
            _mockReservationService
                .Setup(s => s.DeleteReservationAsync(reservationId))
                .Returns(System.Threading.Tasks.Task.FromResult(true));
 
            // Act
            var result = await _controller.DeleteReservation(reservationId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            Assert.AreEqual(ReservationConstants.ReservationDeleteSuccess, message);
        }
 
        [Test]
        public async Task DeleteReservation_ResourceMissing_Returns400BadRequest()
        {
            // Arrange
            long reservationId = 500;
            _mockReservationService
                .Setup(s => s.DeleteReservationAsync(reservationId))
                .Returns(System.Threading.Tasks.Task.FromResult(false));
 
            // Act
            var result = await _controller.DeleteReservation(reservationId);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(ReservationConstants.ReservationNotFound, message);
        }
 
        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task DeleteReservation_InvalidOrEdgeCaseIds_ReturnsExpectedServiceBehavior(long edgeId)
        {
            // Arrange
            _mockReservationService
                .Setup(s => s.DeleteReservationAsync(edgeId))
                .ReturnsAsync(false);
 
            // Act
            var result = await _controller.DeleteReservation(edgeId);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        #endregion
 
        #region UpdateReservationStatus Tests
 
        [Test]
        public async Task UpdateReservationStatus_ValidRequest_Returns200OkWithData()
        {
            // Arrange
            long reservationId = 202;
            var statusDto = new ReservationStatusUpdateDto { NewStatus = 3 };
            var expectedData = new ReservationResponseDto
            {
                ReservationId = reservationId,
                BookingId = 1,
                Details = "Status change",
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow.AddDays(1),
                Status = statusDto.NewStatus,
                CreatedDate = System.DateTime.UtcNow
            };
            _mockReservationService
                .Setup(s => s.UpdateReservationStatusAsync(reservationId, statusDto.NewStatus))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.UpdateReservationStatus(reservationId, statusDto);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        public async Task UpdateReservationStatus_NullDto_Returns400BadRequest()
        {
            // Act
            var result = await _controller.UpdateReservationStatus(202, null!);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(GeneralConstants.InvalidInput, message);
        }
 
        [Test]
        public async Task UpdateReservationStatus_InvalidModelState_Returns400BadRequest()
        {
            // Arrange
            var statusDto = new ReservationStatusUpdateDto();
            _controller.ModelState.AddModelError("NewStatus", "Out of enum range");
 
            // Act
            var result = await _controller.UpdateReservationStatus(202, statusDto);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        #endregion
    }
}
 