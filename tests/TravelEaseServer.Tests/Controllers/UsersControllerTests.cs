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
    public class UsersControllerTests
    {
        private Mock<IUserService> _mockUserService = null!;
        private UsersController _controller = null!;
 
        [SetUp]
        public void SetUp()
        {
            _mockUserService = new Mock<IUserService>();
            _controller = new UsersController(_mockUserService.Object);
        }
 
        #region Helper Methods for Anonymous Objects Extraction
 
        /// <summary>
        /// Safely extracts a property from an anonymous object without triggering nullability warnings.
        /// </summary>
        private static object? GetPropertyValue(object? obj, string propertyName)
        {
            if (obj == null) return null;
            var property = obj.GetType().GetProperty(propertyName);
            return property?.GetValue(obj, null);
        }
 
        #endregion
 
        #region GetAllUsers Tests
 
        [Test]
        public async Task GetAllUsers_ValidSearchDto_Returns200OkWithData()
        {
            // Arrange
            var searchDto = new UserSearchDto { SearchTerm = "John" };
var expectedUsers = new List<UserResponseDto>
            {
                new UserResponseDto { UserId = 1, Name = "John Doe", Email = "john@example.com", Phone = "12345", Role = "Traveler" }
            };
 
            _mockUserService.Setup(s => s.GetAllUsersAsync(searchDto))
                .ReturnsAsync(expectedUsers);
 
            // Act
            var result = await _controller.GetAllUsers(searchDto);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            var data = GetPropertyValue(okResult.Value, "data");
 
            Assert.That(message, Is.EqualTo(GeneralConstants.OperationSuccess));
            Assert.That(data, Is.EqualTo(expectedUsers));
        }
 
        [Test]
        public async Task GetAllUsers_NullSearchDto_Returns400BadRequest()
        {
            // Act
            var result = await _controller.GetAllUsers(null!);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task GetAllUsers_ServiceReturnsNull_Returns404NotFound()
        {
            // Arrange
            var searchDto = new UserSearchDto { SearchTerm = "NonExistent" };
            _mockUserService.Setup(s => s.GetAllUsersAsync(searchDto))
                .ReturnsAsync((IEnumerable<UserResponseDto>?)null);
 
            // Act
            var result = await _controller.GetAllUsers(searchDto);
 
            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
 
            var message = GetPropertyValue(notFoundResult.Value, "message");
            Assert.That(message, Is.EqualTo(UserConstants.UserNotFound));
        }
 
        #endregion
 
        #region RegisterUser Tests
 
        [Test]
        public async Task RegisterUser_ValidUserDto_Returns200OkWithCreatedUser()
        {
            // Arrange
            var requestDto = new UserRequestDto { Name = "Jane", Email = "jane@example.com", Phone = "54321" };
var responseDto = new UserResponseDto { UserId = 2, Name = "Jane", Email = "jane@example.com", Phone = "54321", Role = "Traveler" };
 
            _mockUserService.Setup(s => s.CreateUserAsync(requestDto))
                .ReturnsAsync(responseDto);
 
            // Act
            var result = await _controller.RegisterUser(requestDto);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            var data = GetPropertyValue(okResult.Value, "data");
 
            Assert.That(message, Is.EqualTo(UserConstants.UserCreatedSuccess));
            Assert.That(data, Is.EqualTo(responseDto));
        }
 
        [Test]
        public async Task RegisterUser_NullUserDto_Returns400BadRequest()
        {
            // Act
            var result = await _controller.RegisterUser(null!);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task RegisterUser_ServiceFailsToCreate_Returns400BadRequest()
        {
            // Arrange
            var requestDto = new UserRequestDto { Name = "Invalid", Email = "bad@example.com", Phone = "0000" };
            _mockUserService.Setup(s => s.CreateUserAsync(requestDto))
                .ReturnsAsync((UserResponseDto?)null);
 
            // Act
            var result = await _controller.RegisterUser(requestDto);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        #endregion
 
        #region GetUserById Tests
 
        [Test]
        public async Task GetUserById_ValidId_Returns200OkWithUser()
        {
            // Arrange
            long userId = 1;
var responseDto = new UserResponseDto { UserId = userId, Name = "John", Email = "john@example.com", Phone = "123", Role = "Traveler" };
 
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync(responseDto);
 
            // Act
            var result = await _controller.GetUserById(userId);
 
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
        [TestCase(-1)]
        public async Task GetUserById_InvalidId_Returns400BadRequest(long userId)
        {
            // Act
            var result = await _controller.GetUserById(userId);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task GetUserById_UserDoesNotExist_Returns404NotFound()
        {
            // Arrange
            long userId = 99;
            _mockUserService.Setup(s => s.GetUserByIdAsync(userId))
                .ReturnsAsync((UserResponseDto?)null);
 
            // Act
            var result = await _controller.GetUserById(userId);
 
            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
 
            var message = GetPropertyValue(notFoundResult.Value, "message");
            Assert.That(message, Is.EqualTo(UserConstants.UserNotFound));
        }
 
        #endregion
 
        #region UpdateUser Tests
 
        [Test]
        public async Task UpdateUser_ValidRequest_Returns200OkWithUpdatedUser()
        {
            // Arrange
            long userId = 1;
            var requestDto = new UserRequestDto { Name = "Updated Name", Email = "up@ex.com", Phone = "111" };
var responseDto = new UserResponseDto { UserId = userId, Name = "Updated Name", Email = "up@ex.com", Phone = "111", Role = "Traveler" };
 
            _mockUserService.Setup(s => s.UpdateUserAsync(userId, requestDto))
                .ReturnsAsync(responseDto);
 
            // Act
            var result = await _controller.UpdateUser(userId, requestDto);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            var data = GetPropertyValue(okResult.Value, "data");
 
            Assert.That(message, Is.EqualTo(UserConstants.UserUpdateSuccess));
            Assert.That(data, Is.EqualTo(responseDto));
        }
 
        [TestCase(0, false)] // Invalid ID, valid DTO
        [TestCase(1, true)]  // Valid ID, null DTO
        [TestCase(-5, true)] // Invalid ID, null DTO
        public async Task UpdateUser_InvalidInputs_Returns400BadRequest(long userId, bool isDtoNull)
        {
            // Arrange
            var requestDto = isDtoNull ? null : new UserRequestDto { Name = "Test", Email = "t@t.com", Phone = "1" };
 
            // Act
            var result = await _controller.UpdateUser(userId, requestDto!);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task UpdateUser_UserNotFoundInService_Returns404NotFound()
        {
            // Arrange
            long userId = 1;
            var requestDto = new UserRequestDto { Name = "Name", Email = "e@e.com", Phone = "1" };
            _mockUserService.Setup(s => s.UpdateUserAsync(userId, requestDto))
                .ReturnsAsync((UserResponseDto?)null);
 
            // Act
            var result = await _controller.UpdateUser(userId, requestDto);
 
            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
 
            var message = GetPropertyValue(notFoundResult.Value, "message");
            Assert.That(message, Is.EqualTo(UserConstants.UserNotFound));
        }
 
        #endregion
 
        #region DeleteUser Tests
 
        [Test]
        public async Task DeleteUser_ValidId_Returns200Ok()
        {
            // Arrange
            long userId = 1;
            _mockUserService.Setup(s => s.DeleteUserAsync(userId))
                .ReturnsAsync(true);
 
            // Act
            var result = await _controller.DeleteUser(userId);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            Assert.That(message, Is.EqualTo(UserConstants.UserDeleteSuccess));
        }
 
        [TestCase(0)]
        [TestCase(-10)]
        public async Task DeleteUser_InvalidId_Returns400BadRequest(long userId)
        {
            // Act
            var result = await _controller.DeleteUser(userId);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task DeleteUser_UserDoesNotExist_Returns404NotFound()
        {
            // Arrange
            long userId = 50;
            _mockUserService.Setup(s => s.DeleteUserAsync(userId))
                .ReturnsAsync(false);
 
            // Act
            var result = await _controller.DeleteUser(userId);
 
            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
 
            var message = GetPropertyValue(notFoundResult.Value, "message");
            Assert.That(message, Is.EqualTo(UserConstants.UserNotFound));
        }
 
        #endregion
 
        #region AssignUserRole Tests
 
        [Test]
        public async Task AssignUserRole_ValidRequest_Returns200OkWithUpdatedDetails()
        {
            // Arrange
            long userId = 1;
            var roleDto = new UserRoleAssignmentDto { UserId = userId, NewRole = 2 };
var responseDto = new UserResponseDto { UserId = userId, Name = "User", Email = "u@u.com", Phone = "1", Role = "Traveler" };
 
            _mockUserService.Setup(s => s.AssignRoleAsync(userId, roleDto.NewRole))
                .ReturnsAsync(responseDto);
 
            // Act
            var result = await _controller.AssignUserRole(userId, roleDto);
 
            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            Assert.That(okResult!.StatusCode, Is.EqualTo(200));
 
            var message = GetPropertyValue(okResult.Value, "message");
            var data = GetPropertyValue(okResult.Value, "data");
 
            Assert.That(message, Is.EqualTo(UserConstants.RoleAssignmentSuccess));
            Assert.That(data, Is.EqualTo(responseDto));
        }
 
        [TestCase(0, false)] // Invalid ID, valid DTO
        [TestCase(1, true)]  // Valid ID, null DTO
        [TestCase(-1, true)] // Invalid ID, null DTO
        public async Task AssignUserRole_InvalidInputs_Returns400BadRequest(long userId, bool isDtoNull)
        {
            // Arrange
            var roleDto = isDtoNull ? null : new UserRoleAssignmentDto { UserId = userId, NewRole = 1 };
 
            // Act
            var result = await _controller.AssignUserRole(userId, roleDto!);
 
            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult!.StatusCode, Is.EqualTo(400));
 
            var message = GetPropertyValue(badRequestResult.Value, "message");
            Assert.That(message, Is.EqualTo(GeneralConstants.InvalidInput));
        }
 
        [Test]
        public async Task AssignUserRole_UserNotFound_Returns404NotFound()
        {
            // Arrange
            long userId = 5;
            var roleDto = new UserRoleAssignmentDto { UserId = userId, NewRole = 3 };
            _mockUserService.Setup(s => s.AssignRoleAsync(userId, roleDto.NewRole))
                .ReturnsAsync((UserResponseDto?)null);
 
            // Act
            var result = await _controller.AssignUserRole(userId, roleDto);
 
            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            Assert.That(notFoundResult!.StatusCode, Is.EqualTo(404));
 
            var message = GetPropertyValue(notFoundResult.Value, "message");
            Assert.That(message, Is.EqualTo(UserConstants.UserNotFound));
        }
 
        #endregion
    }
}
 