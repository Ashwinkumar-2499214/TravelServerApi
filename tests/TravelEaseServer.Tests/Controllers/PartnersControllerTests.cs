using System;
using System.Collections.Generic;
using System.Reflection;
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
    public class PartnersControllerTests
    {
        private Mock<IPartnerService> _mockPartnerService = null!;
        private PartnersController _controller = null!;
 
        [SetUp]
        public void SetUp()
        {
            _mockPartnerService = new Mock<IPartnerService>();
            _controller = new PartnersController(_mockPartnerService.Object);
        }
 
        #region Helper Methods for Anonymous Objects Extraction
 
        /// <summary>
        /// Safely extracts properties from anonymous objects returned by endpoints.
        /// </summary>
        private static object? GetPropertyValue(object? obj, string propertyName)
        {
            if (obj == null) return null;
            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj, null);
        }
 
        #endregion
 
        #region GetAllPartners Tests
 
        [Test]
        public async Task GetAllPartners_ValidSearchDto_Returns200OkWithData()
        {
            // Arrange
            var searchDto = new PartnerSearchDto { SearchTerm = "Hotel" };
            var expectedPartners = new List<PartnerResponseDto>
            {
                new() { PartnerId = 1, Name = "Grand Stay Hotel", Type = 1, ContactEmail = "info@grandstay.com", ContactPhone = "12345", Address = "Main St", Status = 0 }
            };
 
            _mockPartnerService.Setup(s => s.GetAllPartnersAsync(searchDto))
                .ReturnsAsync(expectedPartners);
 
            // Act
            var result = await _controller.GetAllPartners(searchDto);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            var data = GetPropertyValue(okResult.Value, "data");
 
            Assert.That(message, Is.EqualTo(GeneralConstants.OperationSuccess));
            Assert.That(data, Is.EqualTo(expectedPartners));
        }
 
        [Test]
        public async Task GetAllPartners_NullSearchDto_Returns400BadRequest()
        {
            // Act
            var result = await _controller.GetAllPartners(null!);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task GetAllPartners_ServiceReturnsNull_Returns404NotFound()
        {
            // Arrange
            var searchDto = new PartnerSearchDto { SearchTerm = "NonExistent" };
            _mockPartnerService.Setup(s => s.GetAllPartnersAsync(searchDto))
                .ReturnsAsync((IEnumerable<PartnerResponseDto>?)null);
 
            // Act
            var result = await _controller.GetAllPartners(searchDto);
 
            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
 
            var message = GetPropertyValue(notFoundResult.Value, "message");
            Assert.That(message, Is.EqualTo(PartnerConstants.PartnerNotFound));
        }
 
        #endregion
 
        #region CreatePartner Tests
 
        [Test]
        public async Task CreatePartner_ValidPartnerDto_Returns200OkWithCreatedPartner()
        {
            // Arrange
            var requestDto = new PartnerRequestDto { Name = "FlyHigh Travels", Type = 1, ContactEmail = "contact@flyhigh.com", ContactPhone = "999", Address = "Airport Rd" };
            var responseDto = new PartnerResponseDto { PartnerId = 2, Name = "FlyHigh Travels", Type = 1, ContactEmail = "contact@flyhigh.com", ContactPhone = "999", Address = "Airport Rd", Status = 0 };
 
            _mockPartnerService.Setup(s => s.CreatePartnerAsync(requestDto))
                .ReturnsAsync(responseDto);
 
            // Act
            var result = await _controller.CreatePartner(requestDto);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            var data = GetPropertyValue(okResult.Value, "data");
 
            Assert.That(message, Is.EqualTo(PartnerConstants.PartnerCreatedSuccess));
            Assert.That(data, Is.EqualTo(responseDto));
        }
 
        [Test]
        public async Task CreatePartner_NullPartnerDto_Returns400BadRequest()
        {
            // Act
            var result = await _controller.CreatePartner(null!);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task CreatePartner_ServiceFailsToCreate_Returns400BadRequest()
        {
            // Arrange
            var requestDto = new PartnerRequestDto { Name = "Invalid Business" };
            _mockPartnerService.Setup(s => s.CreatePartnerAsync(requestDto))
                .ReturnsAsync((PartnerResponseDto?)null);
 
            // Act
            var result = await _controller.CreatePartner(requestDto);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        #endregion
 
        #region GetPartnerById Tests
 
        [Test]
        public async Task GetPartnerById_ValidId_Returns200OkWithPartner()
        {
            // Arrange
            long partnerId = 10;
            var responseDto = new PartnerResponseDto { PartnerId = partnerId, Name = "Partner 10", Type = 1, ContactEmail = "p10@test.com", ContactPhone = "111", Address = "Hub 1", Status = 0 };
 
            _mockPartnerService.Setup(s => s.GetPartnerByIdAsync(partnerId))
                .ReturnsAsync(responseDto);
 
            // Act
            var result = await _controller.GetPartnerById(partnerId);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            var data = GetPropertyValue(okResult.Value, "data");
 
            Assert.That(message, Is.EqualTo(GeneralConstants.OperationSuccess));
            Assert.That(data, Is.EqualTo(responseDto));
        }
 
        [TestCase(0)]
        [TestCase(-5)]
        public async Task GetPartnerById_InvalidId_Returns400BadRequest(long partnerId)
        {
            // Act
            var result = await _controller.GetPartnerById(partnerId);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task GetPartnerById_PartnerDoesNotExist_Returns404NotFound()
        {
            // Arrange
            long partnerId = 999;
            _mockPartnerService.Setup(s => s.GetPartnerByIdAsync(partnerId))
                .ReturnsAsync((PartnerResponseDto?)null);
 
            // Act
            var result = await _controller.GetPartnerById(partnerId);
 
            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
 
            var message = GetPropertyValue(notFoundResult.Value, "message");
            Assert.That(message, Is.EqualTo(PartnerConstants.PartnerNotFound));
        }
 
        #endregion
 
        #region UpdatePartner Tests
 
        [Test]
        public async Task UpdatePartner_ValidRequest_Returns200OkWithUpdatedPartner()
        {
            // Arrange
            long partnerId = 1;
            var requestDto = new PartnerRequestDto { Name = "Updated Name", Type = 2, ContactEmail = "up@ex.com", ContactPhone = "777", Address = "New Address" };
            var responseDto = new PartnerResponseDto { PartnerId = partnerId, Name = "Updated Name", Type = 2, ContactEmail = "up@ex.com", ContactPhone = "777", Address = "New Address", Status = 0 };
 
            _mockPartnerService.Setup(s => s.UpdatePartnerAsync(partnerId, requestDto))
                .ReturnsAsync(responseDto);
 
            // Act
            var result = await _controller.UpdatePartner(partnerId, requestDto);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            var data = GetPropertyValue(okResult.Value, "data");
 
            Assert.That(message, Is.EqualTo(PartnerConstants.PartnerUpdateSuccess));
            Assert.That(data, Is.EqualTo(responseDto));
        }
 
        [TestCase(0, false)] // Invalid ID, valid DTO
        [TestCase(1, true)]  // Valid ID, null DTO
        [TestCase(-1, true)] // Invalid ID, null DTO
        public async Task UpdatePartner_InvalidInputs_Returns400BadRequest(long partnerId, bool isDtoNull)
        {
            // Arrange
            var requestDto = isDtoNull ? null : new PartnerRequestDto { Name = "Valid Name" };
 
            // Act
            var result = await _controller.UpdatePartner(partnerId, requestDto!);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task UpdatePartner_PartnerNotFound_Returns404NotFound()
        {
            // Arrange
            long partnerId = 1;
            var requestDto = new PartnerRequestDto { Name = "Name" };
            _mockPartnerService.Setup(s => s.UpdatePartnerAsync(partnerId, requestDto))
                .ReturnsAsync((PartnerResponseDto?)null);
 
            // Act
            var result = await _controller.UpdatePartner(partnerId, requestDto);
 
            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
 
            var message = GetPropertyValue(notFoundResult.Value, "message");
            Assert.That(message, Is.EqualTo(PartnerConstants.PartnerNotFound));
        }
 
        #endregion
 
        #region DeletePartner Tests
 
        [Test]
        public async Task DeletePartner_ValidId_Returns200Ok()
        {
            // Arrange
            long partnerId = 5;
            _mockPartnerService.Setup(s => s.DeletePartnerAsync(partnerId))
                .ReturnsAsync(true);
 
            // Act
            var result = await _controller.DeletePartner(partnerId);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            Assert.That(message, Is.EqualTo(PartnerConstants.PartnerDeleteSuccess));
        }
 
        [TestCase(0)]
        [TestCase(-120)]
        public async Task DeletePartner_InvalidId_Returns400BadRequest(long partnerId)
        {
            // Act
            var result = await _controller.DeletePartner(partnerId);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task DeletePartner_PartnerNotFound_Returns404NotFound()
        {
            // Arrange
            long partnerId = 404;
            _mockPartnerService.Setup(s => s.DeletePartnerAsync(partnerId))
                .ReturnsAsync(false);
 
            // Act
            var result = await _controller.DeletePartner(partnerId);
 
            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
 
            var message = GetPropertyValue(notFoundResult.Value, "message");
            Assert.That(message, Is.EqualTo(PartnerConstants.PartnerNotFound));
        }
 
        #endregion
 
        #region UpdatePartnerStatus Tests
 
        [Test]
        public async Task UpdatePartnerStatus_ValidRequest_Returns200OkWithUpdatedData()
        {
            // Arrange
            long partnerId = 1;
            var statusDto = new PartnerStatusUpdateDto { PartnerId = partnerId, NewStatus = 2 };
            var responseDto = new PartnerResponseDto { PartnerId = partnerId, Name = "Partner 1", Type = 1, ContactEmail = "e@e.com", ContactPhone = "1", Address = "A", Status = 2 };
 
            _mockPartnerService.Setup(s => s.UpdatePartnerStatusAsync(partnerId, statusDto.NewStatus))
                .ReturnsAsync(responseDto);
 
            // Act
            var result = await _controller.UpdatePartnerStatus(partnerId, statusDto);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            var data = GetPropertyValue(okResult.Value, "data");
 
            Assert.That(message, Is.EqualTo(PartnerConstants.PartnerStatusUpdateSuccess));
            Assert.That(data, Is.EqualTo(responseDto));
        }
 
        [TestCase(0, false)] // Invalid ID, valid DTO
        [TestCase(1, true)]  // Valid ID, null DTO
        [TestCase(-9, true)] // Invalid ID, null DTO
        public async Task UpdatePartnerStatus_InvalidInputs_Returns400BadRequest(long partnerId, bool isDtoNull)
        {
            // Arrange
            var statusDto = isDtoNull ? null : new PartnerStatusUpdateDto { PartnerId = partnerId, NewStatus = 1 };
 
            // Act
            var result = await _controller.UpdatePartnerStatus(partnerId, statusDto!);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task UpdatePartnerStatus_PartnerNotFound_Returns404NotFound()
        {
            // Arrange
            long partnerId = 88;
            var statusDto = new PartnerStatusUpdateDto { PartnerId = partnerId, NewStatus = 1 };
            _mockPartnerService.Setup(s => s.UpdatePartnerStatusAsync(partnerId, statusDto.NewStatus))
                .ReturnsAsync((PartnerResponseDto?)null);
 
            // Act
            var result = await _controller.UpdatePartnerStatus(partnerId, statusDto);
 
            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
 
            var message = GetPropertyValue(notFoundResult.Value, "message");
            Assert.That(message, Is.EqualTo(PartnerConstants.PartnerNotFound));
        }
 
        #endregion
    }
}
 