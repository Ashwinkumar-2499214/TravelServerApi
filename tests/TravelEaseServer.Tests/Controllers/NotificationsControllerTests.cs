
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
using TravelEaseServer.Enum;
using TravelEaseServer.Service.Interface;

namespace TravelEaseServer.Tests.Controllers
{
    [TestFixture]
    public class NotificationsControllerTests
    {
        private Mock<INotificationService> _mockService;
        private NotificationsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockService = new Mock<INotificationService>(MockBehavior.Strict);
            _controller = new NotificationsController(_mockService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _mockService.VerifyAll();
        }

        #region 1. GetUserNotifications Endpoints

        [Test]
        public async Task GetUserNotifications_ReturnsOk_WithData()
        {
            var expected = new List<NotificationResponseDto>
            {
                new NotificationResponseDto
                {
                    NotificationId = 1,
                    UserId = 1,
                    Message = "hello",
                    Category = (NotificationCategory)0,
                    Status = (NotificationStatus)0,
                    CreatedDate = DateTime.UtcNow
                }
            };

            _mockService.Setup(s => s.GetUserNotificationsAsync(1)).ReturnsAsync(expected);

            var result = await _controller.GetUserNotifications(1) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            Assert.AreEqual(GeneralConstants.OperationSuccess, message);

            var data = value.GetType().GetProperty("data")!.GetValue(value) as IEnumerable<NotificationResponseDto>;
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data!.Count());
        }

        [Test]
        public async Task GetUserNotifications_NoNotificationsFound_ReturnsOkWithEmptyList()
        {
            _mockService.Setup(s => s.GetUserNotificationsAsync(999)).ReturnsAsync(new List<NotificationResponseDto>());

            var result = await _controller.GetUserNotifications(999) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as IEnumerable<NotificationResponseDto>;
            Assert.IsNotNull(data);
            Assert.IsEmpty(data);
        }

        [Test]
        public async Task GetUserNotifications_InvalidUserId_StillCallsServiceAndReturnsOk()
        {
            // The controller doesn't explicit check for userId <= 0 here, it lets the service handle it
            _mockService.Setup(s => s.GetUserNotificationsAsync(-1)).ReturnsAsync(new List<NotificationResponseDto>());

            var result = await _controller.GetUserNotifications(-1) as OkObjectResult;
            Assert.IsNotNull(result);
        }

        #endregion

        #region 2. CreateNotification Endpoints

        [Test]
        public async Task CreateNotification_InvalidInput_ReturnsBadRequest()
        {
            // Null DTO
            var badResult = await _controller.CreateNotification(null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(badResult);

            // DTO with invalid fields (UserId = 0)
            var invalidDto = new NotificationRequestDto { UserId = 0, Message = "Valid message" };
            var badResult2 = await _controller.CreateNotification(invalidDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(badResult2);

            // DTO with empty message
            var emptyMessageDto = new NotificationRequestDto { UserId = 1, Message = "" };
            var badResult3 = await _controller.CreateNotification(emptyMessageDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(badResult3);
        }

        [Test]
        public async Task CreateNotification_Valid_ReturnsOk()
        {
            var request = new NotificationRequestDto { UserId = 2, Message = "Welcome", Category = 0 };
            var created = new NotificationResponseDto
            {
                NotificationId = 10,
                UserId = 2,
                Message = "Welcome",
                Category = (NotificationCategory)0,
                Status = (NotificationStatus)0,
                CreatedDate = DateTime.UtcNow
            };

            _mockService.Setup(s => s.CreateNotificationAsync(request)).ReturnsAsync(created);

            var result = await _controller.CreateNotification(request) as OkObjectResult;
            Assert.IsNotNull(result);

            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            Assert.AreEqual(NotificationConstants.NotificationCreatedSuccess, message);

            var data = value.GetType().GetProperty("data")!.GetValue(value) as NotificationResponseDto;
            Assert.IsNotNull(data);
            Assert.AreEqual(created.NotificationId, data!.NotificationId);
        }

        [Test]
        public async Task CreateNotification_WhitespaceMessage_ReturnsBadRequest()
        {
            var whitespaceDto = new NotificationRequestDto { UserId = 5, Message = "    " };
            var result = await _controller.CreateNotification(whitespaceDto);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        #endregion

        #region 3. GetNotificationById Endpoints

        [Test]
        public async Task GetNotificationById_ValidId_ReturnsOkWithData()
        {
            long targetId = 55;
            var expectedDto = new NotificationResponseDto { NotificationId = targetId, Message = "Target found" };
            _mockService.Setup(s => s.GetNotificationByIdAsync(targetId)).ReturnsAsync(expectedDto);

            var result = await _controller.GetNotificationById(targetId) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as NotificationResponseDto;

            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(targetId, data!.NotificationId);
        }

        [Test]
        public async Task GetNotificationById_NonExistentId_ReturnsOkWithNullData()
        {
            // Note: The controller endpoint does not have a null check validation for GetNotificationById, it passes whatever it gets to Ok()
            _mockService.Setup(s => s.GetNotificationByIdAsync(888)).ReturnsAsync((NotificationResponseDto?)null);

            var result = await _controller.GetNotificationById(888) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var data = value.GetType().GetProperty("data")!.GetValue(value);
            Assert.IsNull(data);
        }

        [Test]
        public async Task GetNotificationById_NegativeId_CallsServiceDirectly()
        {
            _mockService.Setup(s => s.GetNotificationByIdAsync(-10)).ReturnsAsync((NotificationResponseDto?)null);

            var result = await _controller.GetNotificationById(-10) as OkObjectResult;
            Assert.IsNotNull(result);
        }

        #endregion

        #region 4. DeleteNotification Endpoints

        [Test]
        public async Task DeleteNotification_NotFound_ReturnsBadRequest()
        {
            _mockService.Setup(s => s.DeleteNotificationAsync(99)).ReturnsAsync(false);

            var result = await _controller.DeleteNotification(99);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteNotification_Success_ReturnsOk()
        {
            long targetId = 44;
            _mockService.Setup(s => s.DeleteNotificationAsync(targetId)).ReturnsAsync(true);

            var result = await _controller.DeleteNotification(targetId) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            Assert.AreEqual(NotificationConstants.NotificationDeleteSuccess, message);
        }

        [Test]
        public async Task DeleteNotification_NegativeId_ReturnsBadRequestWhenServiceFails()
        {
            _mockService.Setup(s => s.DeleteNotificationAsync(-5)).ReturnsAsync(false);

            var result = await _controller.DeleteNotification(-5);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        #endregion

        #region 5. MarkAsRead Endpoints

        [Test]
        public async Task MarkAsRead_NotFound_ReturnsBadRequest()
        {
            _mockService.Setup(s => s.MarkAsReadAsync(123)).ReturnsAsync((NotificationResponseDto?)null);

            var result = await _controller.MarkAsRead(123);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task MarkAsRead_Success_ReturnsOkWithMessage()
        {
            long targetId = 77;
            var updatedDto = new NotificationResponseDto { NotificationId = targetId, Status = (NotificationStatus)1 };
            _mockService.Setup(s => s.MarkAsReadAsync(targetId)).ReturnsAsync(updatedDto);

            var result = await _controller.MarkAsRead(targetId) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            Assert.AreEqual(NotificationConstants.NotificationMarkedAsRead, message);
        }

        [Test]
        public async Task MarkAsRead_NegativeId_ReturnsBadRequestOnServiceNull()
        {
            _mockService.Setup(s => s.MarkAsReadAsync(-1)).ReturnsAsync((NotificationResponseDto?)null);

            var result = await _controller.MarkAsRead(-1);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        #endregion

        #region 6. MarkAllAsRead Endpoints

        [Test]
        public async Task MarkAllAsRead_Failure_ReturnsBadRequest()
        {
            _mockService.Setup(s => s.MarkAllAsReadAsync(5)).ReturnsAsync(false);

            var result = await _controller.MarkAllAsRead(5);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task MarkAllAsRead_Success_ReturnsOk()
        {
            long userId = 12;
            _mockService.Setup(s => s.MarkAllAsReadAsync(userId)).ReturnsAsync(true);

            var result = await _controller.MarkAllAsRead(userId) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            Assert.AreEqual(NotificationConstants.AllNotificationsMarkedAsRead, message);
        }

        [Test]
        public async Task MarkAllAsRead_ZeroUserId_ReturnsBadRequestWhenServiceFails()
        {
            _mockService.Setup(s => s.MarkAllAsReadAsync(0)).ReturnsAsync(false);

            var result = await _controller.MarkAllAsRead(0);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        #endregion
    }
}
