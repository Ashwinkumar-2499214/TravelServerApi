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
    public class ItinerariesControllerTests
    {
        private Mock<IItineraryService> _mockItineraryService;
        private Mock<INotificationService> _mockNotificationService;
        private ItinerariesController _controller;
        [SetUp]
        public void Setup()
        {
            _mockItineraryService = new Mock<IItineraryService>();
            _mockNotificationService = new Mock<INotificationService>();
            _controller = new ItinerariesController(_mockItineraryService.Object, _mockNotificationService.Object);
        }
 
        #region Helper Methods
 
        /// <summary>
        /// Extracts property values from compiled anonymous objects returned inside response results.
        /// </summary>
        private static object GetAnonymousPropertyValue(object obj, string propertyName)
        {
            return obj?.GetType().GetProperty(propertyName)?.GetValue(obj, null);
        }
 
        #endregion
 
        #region GetAllItineraries Tests
 
        [Test]
        public async Task GetAllItineraries_ValidRequest_Returns200OkWithData()
        {
            // Arrange
            var searchDto = new ItinerarySearchDto();
            var expectedData = new System.Collections.Generic.List<ItineraryResponseDto>
            {
                new ItineraryResponseDto
                {
                    ItineraryId = 1,
                    UserId = 1,
                    Title = "Test Itinerary",
                    StartDate = System.DateTime.UtcNow,
                    EndDate = System.DateTime.UtcNow.AddDays(1),
                    Status = 1,
                    CreatedDate = System.DateTime.UtcNow
                }
            } as System.Collections.Generic.IEnumerable<ItineraryResponseDto>;
            _mockItineraryService
                .Setup(s => s.GetAllItinerariesAsync(searchDto))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.GetAllItineraries(searchDto);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        public async Task GetAllItineraries_NullDto_Returns400BadRequest()
        {
            // Act
            var result = await _controller.GetAllItineraries(null!);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(GeneralConstants.InvalidInput, message);
        }
 
        [Test]
        public async Task GetAllItineraries_InvalidModelState_Returns400BadRequest()
        {
            // Arrange
            var searchDto = new ItinerarySearchDto();
            _controller.ModelState.AddModelError("SearchField", "Invalid sorting value");
 
            // Act
            var result = await _controller.GetAllItineraries(searchDto);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        #endregion
 
        #region CreateItinerary Tests
 
        [Test]
        public async Task CreateItinerary_ValidRequest_Returns200OkWithCreatedData()
        {
            // Arrange
            var requestDto = new ItineraryRequestDto
            {
                UserId = 1,
                Title = "New Itinerary",
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow.AddDays(5)
            };
            var expectedData = new ItineraryResponseDto
            {
                ItineraryId = 2,
                UserId = requestDto.UserId,
                Title = requestDto.Title,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                Status = 1,
                CreatedDate = System.DateTime.UtcNow
            };
            _mockItineraryService
                .Setup(s => s.CreateItineraryAsync(requestDto))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.CreateItinerary(requestDto);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(ItineraryConstants.ItineraryCreatedSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        public async Task CreateItinerary_NullDto_Returns400BadRequest()
        {
            // Act
            var result = await _controller.CreateItinerary(null!);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task CreateItinerary_InvalidModelState_Returns400BadRequest()
        {
            // Arrange
            var requestDto = new ItineraryRequestDto { Title = string.Empty };
            _controller.ModelState.AddModelError("Title", "Title field is mandatory");
 
            // Act
            var result = await _controller.CreateItinerary(requestDto);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        #endregion
 
        #region GetItineraryById Tests
 
        [Test]
        public async Task GetItineraryById_ValidId_Returns200OkWithData()
        {
            // Arrange
            long itineraryId = 123;
            var expectedData = new ItineraryResponseDto
            {
                ItineraryId = itineraryId,
                UserId = 1,
                Title = "Existing",
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow.AddDays(2),
                Status = 1,
                CreatedDate = System.DateTime.UtcNow
            };
            _mockItineraryService
                .Setup(s => s.GetItineraryByIdAsync(itineraryId))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.GetItineraryById(itineraryId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task GetItineraryById_InvalidIds_Returns400BadRequest(long invalidId)
        {
            // Act
            var result = await _controller.GetItineraryById(invalidId);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(GeneralConstants.InvalidInput, message);
        }
 
        [Test]
        public async Task GetItineraryById_ResourceMissing_Returns200OkWithNullData()
        {
            // Note: Per explicit controller logic, if service returns null, it still issues Ok()
            // Arrange
            long missingId = 999;
            _mockItineraryService
                .Setup(s => s.GetItineraryByIdAsync(missingId))
                .Returns(System.Threading.Tasks.Task.FromResult<ItineraryResponseDto>(default!));
 
            // Act
            var result = await _controller.GetItineraryById(missingId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
            Assert.IsNull(data);
        }
 
        #endregion
 
        #region UpdateItinerary Tests
 
        [Test]
        public async Task UpdateItinerary_ValidRequest_Returns200OkWithUpdatedData()
        {
            // Arrange
            long itineraryId = 45;
            var requestDto = new ItineraryRequestDto
            {
                UserId = 1,
                Title = "Updated Title",
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow.AddDays(3)
            };
            var expectedData = new ItineraryResponseDto
            {
                ItineraryId = itineraryId,
                UserId = requestDto.UserId,
                Title = requestDto.Title,
                StartDate = requestDto.StartDate,
                EndDate = requestDto.EndDate,
                Status = 1,
                CreatedDate = System.DateTime.UtcNow
            };
            _mockItineraryService
                .Setup(s => s.UpdateItineraryAsync(itineraryId, requestDto))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.UpdateItinerary(itineraryId, requestDto);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(ItineraryConstants.ItineraryUpdateSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        [TestCase(0)]
        [TestCase(-5)]
        public async Task UpdateItinerary_InvalidIdInputs_Returns400BadRequest(long invalidId)
        {
            // Act
            var result = await _controller.UpdateItinerary(invalidId, new ItineraryRequestDto { Title = string.Empty });
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task UpdateItinerary_NullDtoOrInvalidModelState_Returns400BadRequest()
        {
            // Act 1: Null Dto
            var resultNull = await _controller.UpdateItinerary(45, null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(resultNull);
 
            // Act 2: Invalid Model State
            _controller.ModelState.AddModelError("UpdateErr", "Error details");
            var resultInvalidState = await _controller.UpdateItinerary(45, new ItineraryRequestDto { Title = string.Empty });
            Assert.IsInstanceOf<BadRequestObjectResult>(resultInvalidState);
        }
 
        #endregion
 
        #region DeleteItinerary Tests
 
        [Test]
        public async Task DeleteItinerary_SuccessPath_Returns200Ok()
        {
            // Arrange
            long itineraryId = 10;
            _mockItineraryService
                .Setup(s => s.DeleteItineraryAsync(itineraryId))
                .Returns(System.Threading.Tasks.Task.FromResult(true));
 
            // Act
            var result = await _controller.DeleteItinerary(itineraryId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            Assert.AreEqual(ItineraryConstants.ItineraryDeleteSuccess, message);
        }
 
        [Test]
        [TestCase(0)]
        [TestCase(-10)]
        public async Task DeleteItinerary_InvalidIds_Returns400BadRequest(long invalidId)
        {
            // Act
            var result = await _controller.DeleteItinerary(invalidId);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(GeneralConstants.InvalidInput, message);
        }
 
        [Test]
        public async Task DeleteItinerary_ResourceMissingInService_Returns400BadRequest()
        {
            // Arrange
            long nonExistentId = 88;
            _mockItineraryService
                .Setup(s => s.DeleteItineraryAsync(nonExistentId))
                .Returns(System.Threading.Tasks.Task.FromResult(false));
 
            // Act
            var result = await _controller.DeleteItinerary(nonExistentId);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(ItineraryConstants.ItineraryNotFound, message);
        }
 
        #endregion
 
        #region UpdateItineraryStatus Tests
 
        [Test]
        public async Task UpdateItineraryStatus_ValidRequest_Returns200OkWithData()
        {
            // Arrange
            long itineraryId = 22;
            var statusDto = new ItineraryStatusUpdateDto { NewStatus = 1 };
            var expectedData = new ItineraryResponseDto
            {
                ItineraryId = itineraryId,
                UserId = 1,
                Title = "Status Updated",
                StartDate = System.DateTime.UtcNow,
                EndDate = System.DateTime.UtcNow.AddDays(1),
                Status = statusDto.NewStatus,
                CreatedDate = System.DateTime.UtcNow
            };
            _mockItineraryService
                .Setup(s => s.UpdateItineraryStatusAsync(itineraryId, statusDto.NewStatus))
                .Returns(System.Threading.Tasks.Task.FromResult(expectedData));
 
            // Act
            var result = await _controller.UpdateItineraryStatus(itineraryId, statusDto);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            var data = GetAnonymousPropertyValue(okResult.Value, "data");
 
            Assert.AreEqual(ItineraryConstants.ItineraryStatusUpdateSuccess, message);
            Assert.AreEqual(expectedData, data);
        }
 
        [Test]
        [TestCase(0)]
        [TestCase(-1)]
        public async Task UpdateItineraryStatus_InvalidId_Returns400BadRequest(long invalidId)
        {
            // Act
            var result = await _controller.UpdateItineraryStatus(invalidId, new ItineraryStatusUpdateDto());
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task UpdateItineraryStatus_NullDtoOrBadModelState_Returns400BadRequest()
        {
            // Act 1: Null Payload
            var resultNull = await _controller.UpdateItineraryStatus(22, null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(resultNull);
 
            // Act 2: Bad ModelState
            _controller.ModelState.AddModelError("Status", "Invalid value conversion");
            var resultState = await _controller.UpdateItineraryStatus(22, new ItineraryStatusUpdateDto());
            Assert.IsInstanceOf<BadRequestObjectResult>(resultState);
        }
 
        #endregion
 
        #region AddBookingToItinerary Tests
 
        [Test]
        public async Task AddBookingToItinerary_SuccessPath_Returns200Ok()
        {
            // Arrange
            long itineraryId = 5;
            var bookingDto = new ItineraryBookingAddDto { BookingId = 99 };
            _mockItineraryService
                .Setup(s => s.AddBookingToItineraryAsync(itineraryId, bookingDto.BookingId))
                .Returns(System.Threading.Tasks.Task.FromResult(true));
 
            // Act
            var result = await _controller.AddBookingToItinerary(itineraryId, bookingDto);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            Assert.AreEqual(ItineraryConstants.BookingAddedSuccess, message);
        }
 
        [Test]
        [TestCase(0)]
        [TestCase(-2)]
        public async Task AddBookingToItinerary_InvalidId_Returns400BadRequest(long invalidId)
        {
            // Act
            var result = await _controller.AddBookingToItinerary(invalidId, new ItineraryBookingAddDto());
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task AddBookingToItinerary_ServiceExecutionFails_Returns400BadRequest()
        {
            // Arrange
            long itineraryId = 5;
            var bookingDto = new ItineraryBookingAddDto { BookingId = 99 };
            _mockItineraryService
                .Setup(s => s.AddBookingToItineraryAsync(itineraryId, bookingDto.BookingId))
                .Returns(System.Threading.Tasks.Task.FromResult(false));
 
            // Act
            var result = await _controller.AddBookingToItinerary(itineraryId, bookingDto);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(GeneralConstants.OperationFailed, message);
        }
 
        #endregion
 
        #region RemoveBookingFromItinerary Tests
 
        [Test]
        public async Task RemoveBookingFromItinerary_SuccessPath_Returns200Ok()
        {
            // Arrange
            long itineraryId = 14;
            long bookingId = 88;
            _mockItineraryService
                .Setup(s => s.RemoveBookingFromItineraryAsync(itineraryId, bookingId))
                .Returns(System.Threading.Tasks.Task.FromResult(true));
 
            // Act
            var result = await _controller.RemoveBookingFromItinerary(itineraryId, bookingId);
 
            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            var message = GetAnonymousPropertyValue(okResult.Value, "message") as string;
            Assert.AreEqual(ItineraryConstants.BookingRemovedSuccess, message);
        }
 
        [Test]
        [TestCase(0, 88)]
        [TestCase(14, 0)]
        [TestCase(-1, -1)]
        public async Task RemoveBookingFromItinerary_InvalidInputs_Returns400BadRequest(long itineraryId, long bookingId)
        {
            // Act
            var result = await _controller.RemoveBookingFromItinerary(itineraryId, bookingId);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task RemoveBookingFromItinerary_ServiceExecutionFails_Returns400BadRequest()
        {
            // Arrange
            long itineraryId = 14;
            long bookingId = 88;
            _mockItineraryService
                .Setup(s => s.RemoveBookingFromItineraryAsync(itineraryId, bookingId))
                .Returns(System.Threading.Tasks.Task.FromResult(false));
 
            // Act
            var result = await _controller.RemoveBookingFromItinerary(itineraryId, bookingId);
 
            // Assert
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            var message = GetAnonymousPropertyValue(badRequestResult.Value, "message") as string;
            Assert.AreEqual(GeneralConstants.OperationFailed, message);
        }
 
        #endregion
    }
}
 