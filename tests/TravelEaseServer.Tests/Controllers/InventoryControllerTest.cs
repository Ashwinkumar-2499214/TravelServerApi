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
    public class InventoryControllerTest
    {
        private Mock<IInventoryService> _mockInventoryService;
        private InventoryController _controller;
 
        [SetUp]
        public void Setup()
        {
            _mockInventoryService = new Mock<IInventoryService>();
            _controller = new InventoryController(_mockInventoryService.Object);
            _controller.ModelState.Clear();
        }
 
        #region 1. GetAllInventory Tests
 
        [Test]
        public async Task GetAllInventory_NullDto_ReturnsBadRequest()
        {
            // Case 1: Dto is completely null
            var result = await _controller.GetAllInventory(null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task GetAllInventory_InvalidModelState_ReturnsBadRequest()
        {
            // Case 2: Query binding or validation fails on ModelState
            _controller.ModelState.AddModelError("PageNumber", "Must be positive");
            var result = await _controller.GetAllInventory(new InventorySearchDto());
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task GetAllInventory_NoDataFound_ReturnsNotFound()
        {
            // Case 3: Inputs valid, but underlying service yields nothing
            var searchDto = new InventorySearchDto { ItemType = "Flight" };
            _mockInventoryService.Setup(s => s.GetAllInventoryAsync(searchDto))
                .ReturnsAsync((IEnumerable<InventoryResponseDto>?)null);
 
            var result = await _controller.GetAllInventory(searchDto);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
 
        [Test]
        public async Task GetAllInventory_Success_ReturnsOkWithData()
        {
            // Case 4: Smooth operational success path
            var searchDto = new InventorySearchDto { ItemType = "Hotel" };
            var expectedData = new List<InventoryResponseDto> { new() { InventoryId = 1, ItemType = "Hotel" } };
            _mockInventoryService.Setup(s => s.GetAllInventoryAsync(searchDto)).ReturnsAsync(expectedData);
 
            var result = await _controller.GetAllInventory(searchDto);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region 2. GetPartnerInventory Tests
 
        [TestCase(0)]
        [TestCase(-10)]
        public async Task GetPartnerInventory_InvalidPartnerId_ReturnsBadRequest(long partnerId)
        {
            // Case 1: Testing bound or negative boundary constraints
            var result = await _controller.GetPartnerInventory(partnerId);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task GetPartnerInventory_NoRecordsFound_ReturnsNotFound()
        {
            // Case 2: Service yields null values for explicit partner
            long partnerId = 99;
            _mockInventoryService.Setup(s => s.GetPartnerInventoryAsync(partnerId))
                .ReturnsAsync((IEnumerable<InventoryResponseDto>?)null);
 
            var result = await _controller.GetPartnerInventory(partnerId);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
 
        [Test]
        public async Task GetPartnerInventory_Success_ReturnsOk()
        {
            // Case 3: Returns records successfully
            long partnerId = 5;
            var dataList = new List<InventoryResponseDto> { new() { InventoryId = 12, PartnerId = partnerId } };
            _mockInventoryService.Setup(s => s.GetPartnerInventoryAsync(partnerId)).ReturnsAsync(dataList);
 
            var result = await _controller.GetPartnerInventory(partnerId);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region 3. CreatePartnerInventory Tests
 
        [Test]
        public async Task CreatePartnerInventory_InvalidArgsOrModelState_ReturnsBadRequest()
        {
            // Case 1: Bad validation parameters or zeroed ID route logic
            var dto = new InventoryRequestDto { ItemType = "Hotel" };
            var result = await _controller.CreatePartnerInventory(0, dto);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task CreatePartnerInventory_ServiceFailsToPersist_ReturnsBadRequest()
        {
            // Case 2: Service fails backend operations and hands back a null record
            long partnerId = 10;
            var dto = new InventoryRequestDto { ItemType = "Flight", Description = "Business Class" };
            _mockInventoryService.Setup(s => s.CreateInventoryAsync(It.IsAny<InventoryRequestDto>()))
                .ReturnsAsync((InventoryResponseDto?)null);
 
            var result = await _controller.CreatePartnerInventory(partnerId, dto);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task CreatePartnerInventory_Success_ReturnsOk()
        {
            // Case 3: Seamlessly generates record
            long partnerId = 10;
            var dto = new InventoryRequestDto { ItemType = "Flight", Description = "Business Class" };
            var mockResponse = new InventoryResponseDto { InventoryId = 500, PartnerId = partnerId, ItemType = "Flight" };
            _mockInventoryService.Setup(s => s.CreateInventoryAsync(It.IsAny<InventoryRequestDto>())).ReturnsAsync(mockResponse);
 
            var result = await _controller.CreatePartnerInventory(partnerId, dto);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region 4. UpdatePartnerInventory Tests
 
        [TestCase(0, 1, "Hotel", "Desc", 5, 100)]  // Bad Partner ID
        [TestCase(1, 0, "Hotel", "Desc", 5, 100)]  // Bad Inventory ID
        [TestCase(1, 1, " ", "Desc", 5, 100)]      // Empty/White space type
        [TestCase(1, 1, "Hotel", "Desc", -2, 100)] // Out-of-bounds Availability
        [TestCase(1, 1, "Hotel", "Desc", 5, 0)]    // Price at zero or under
        public async Task UpdatePartnerInventory_InlineValidationChecks_ReturnsBadRequest(long pId, long iId, string type, string desc, int avail, decimal price)
        {
            // Case 1: Multi-conditional input failures matched via controller constraints
            var badDto = new InventoryRequestDto { ItemType = type, Description = desc, Availability = avail, Price = price };
            var result = await _controller.UpdatePartnerInventory(pId, iId, badDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task UpdatePartnerInventory_RecordMissingFromDb_ReturnsNotFound()
        {
            // Case 2: Parameters check out, but updating resource returns null (Missing)
            var validDto = new InventoryRequestDto { ItemType = "Train", Description = "Sleeper", Availability = 2, Price = 45m };
            _mockInventoryService.Setup(s => s.UpdateInventoryAsync(5, It.IsAny<InventoryRequestDto>()))
                .ReturnsAsync((InventoryResponseDto?)null);
 
            var result = await _controller.UpdatePartnerInventory(1, 5, validDto);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
 
        [Test]
        public async Task UpdatePartnerInventory_Success_ReturnsOk()
        {
            // Case 3: Update successfully transforms entity database state
            var validDto = new InventoryRequestDto { ItemType = "Train", Description = "Sleeper", Availability = 2, Price = 45m };
            var mockResponse = new InventoryResponseDto { InventoryId = 5, ItemType = "Train" };
            _mockInventoryService.Setup(s => s.UpdateInventoryAsync(5, It.IsAny<InventoryRequestDto>())).ReturnsAsync(mockResponse);
 
            var result = await _controller.UpdatePartnerInventory(1, 5, validDto);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region 5. DeletePartnerInventory Tests
 
        [TestCase(0, 1)]
        [TestCase(1, -5)]
        public async Task DeletePartnerInventory_BadRouteIds_ReturnsBadRequest(long partnerId, long inventoryId)
        {
            // Case 1: Zeroed or invalid route params
            var result = await _controller.DeletePartnerInventory(partnerId, inventoryId);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task DeletePartnerInventory_TargetDoesNotExist_ReturnsNotFound()
        {
            // Case 2: Service yields false execution signaling absence
            _mockInventoryService.Setup(s => s.DeleteInventoryAsync(404)).ReturnsAsync(false);
 
            var result = await _controller.DeletePartnerInventory(1, 404);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
 
        [Test]
        public async Task DeletePartnerInventory_Success_ReturnsOk()
        {
            // Case 3: Complete operational drop sequence execution
            _mockInventoryService.Setup(s => s.DeleteInventoryAsync(200)).ReturnsAsync(true);
 
            var result = await _controller.DeletePartnerInventory(1, 200);
            Assert.IsInstanceOf<OkObjectResult>(result);
        }
 
        #endregion
 
        #region 6. UpdateAvailability (HttpPatch) Tests
 
        [Test]
        public async Task UpdateAvailability_NegativeAvailabilityOrBadId_ReturnsBadRequest()
        {
            // Case 1: Request payload provides unacceptable numbers
            var invalidDto = new InventoryAvailabilityDto { InventoryId = 1, NewAvailability = -10, Status = 1 };
            var result = await _controller.UpdateAvailability(1, invalidDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task UpdateAvailability_ExplicitStatusValidationFails_ReturnsBadRequest()
        {
            // Case 2: Caller sets explicit status parameter but it is <= 0
            var badStatusDto = new InventoryAvailabilityDto { InventoryId = 1, NewAvailability = 5, Status = 0 };
            var result = await _controller.UpdateAvailability(1, badStatusDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }
 
        [Test]
        public async Task UpdateAvailability_OmittedStatus_ExistingTargetMissing_ReturnsNotFound()
        {
            // Case 3: Status is null -> attempts lookup -> target missing from DB
            var patchDto = new InventoryAvailabilityDto { InventoryId = 55, NewAvailability = 10, Status = null };
            _mockInventoryService.Setup(s => s.GetInventoryByIdAsync(55)).ReturnsAsync((InventoryResponseDto?)null);
 
            var result = await _controller.UpdateAvailability(55, patchDto);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }
 
        [Test]
        public async Task UpdateAvailability_OmittedStatus_ValidTarget_UpdatesUsingDbStatus()
        {
            // Case 4: Status is null -> lookup succeeds -> patch runs with db status
            var patchDto = new InventoryAvailabilityDto { InventoryId = 12, NewAvailability = 4, Status = null };
            var dbRecord = new InventoryResponseDto { InventoryId = 12, Status = 2 }; // DB status is 2
            var updatedRecord = new InventoryResponseDto { InventoryId = 12, Availability = 4, Status = 2 };
 
            _mockInventoryService.Setup(s => s.GetInventoryByIdAsync(12)).ReturnsAsync(dbRecord);
            _mockInventoryService.Setup(s => s.UpdateAvailabilityAsync(12, 4, 2)).ReturnsAsync(updatedRecord);
 
            var result = await _controller.UpdateAvailability(12, patchDto);
            Assert.IsInstanceOf<OkObjectResult>(result);
            _mockInventoryService.Verify(s => s.GetInventoryByIdAsync(12), Times.Once);
        }
 
        [Test]
        public async Task UpdateAvailability_WithStatusProvided_SavesDirectly()
        {
            // Case 5: Direct Status provided -> bypasses query fetch flow
            var patchDto = new InventoryAvailabilityDto { InventoryId = 12, NewAvailability = 8, Status = 3 };
            var updatedRecord = new InventoryResponseDto { InventoryId = 12, Availability = 8, Status = 3 };
 
            _mockInventoryService.Setup(s => s.UpdateAvailabilityAsync(12, 8, 3)).ReturnsAsync(updatedRecord);
 
            var result = await _controller.UpdateAvailability(12, patchDto);
            Assert.IsInstanceOf<OkObjectResult>(result);
            _mockInventoryService.Verify(s => s.GetInventoryByIdAsync(It.IsAny<long>()), Times.Never);
        }
 
        #endregion
    }
}
 