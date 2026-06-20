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
    public class ComplianceControllerTests
    {
        private Mock<IComplianceService> _mockService;
        private ComplianceController _controller;

        [SetUp]
        public void SetUp()
        {
            // Strict behavior requires setting up every mock call explicitly
            _mockService = new Mock<IComplianceService>(MockBehavior.Strict);
            _controller = new ComplianceController(_mockService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _mockService.VerifyAll();
        }

        #region 1. GetAllReports Endpoints

        [Test]
        public async Task GetAllReports_NullSearch_ReturnsBadRequest()
        {
            var result = await _controller.GetAllReports(null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetAllReports_InvalidModelState_ReturnsBadRequest()
        {
            var searchDto = new ComplianceReportSearchDto();
            _controller.ModelState.AddModelError("Scope", "Scope is required");

            var result = await _controller.GetAllReports(searchDto);
            
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _controller.ModelState.Clear(); // Clean up state
        }

        [Test]
        public async Task GetAllReports_ValidSearch_ReturnsOkWithData()
        {
            var searchDto = new ComplianceReportSearchDto();
            var expectedReports = new List<ComplianceReportResponseDto>
            {
                new() { ComplianceReportId = 1, Title = "Q1 Report" }
            };

            _mockService.Setup(s => s.GetAllReportsAsync(searchDto)).ReturnsAsync(expectedReports);

            var result = await _controller.GetAllReports(searchDto) as OkObjectResult;
            
            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as IEnumerable<ComplianceReportResponseDto>;

            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data!.Count());
        }

        #endregion

        #region 2. CreateReport Endpoints

        [Test]
        public async Task CreateReport_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.CreateReport(null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task CreateReport_InvalidModelState_ReturnsBadRequest()
        {
            var request = new ComplianceReportRequestDto();
            _controller.ModelState.AddModelError("Title", "Title is required");

            var result = await _controller.CreateReport(request);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _controller.ModelState.Clear();
        }

        [Test]
        public async Task CreateReport_Valid_ReturnsOk()
        {
            var request = new ComplianceReportRequestDto { Title = "Monthly" };
            var created = new ComplianceReportResponseDto
            {
                ComplianceReportId = 1,
                Title = "Monthly",
                Scope = "All",
                Metrics = "{}",
                GeneratedDate = DateTime.UtcNow,
                ReportContent = "content"
            };

            _mockService.Setup(s => s.CreateReportAsync(request)).ReturnsAsync(created);

            var result = await _controller.CreateReport(request) as OkObjectResult;
            Assert.IsNotNull(result);

            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            Assert.AreEqual(ComplianceConstants.ReportGeneratedSuccess, message);

            var data = value.GetType().GetProperty("data")!.GetValue(value) as ComplianceReportResponseDto;
            Assert.IsNotNull(data);
            Assert.AreEqual(created.ComplianceReportId, data!.ComplianceReportId);
        }

        #endregion

        #region 3. GetReportById Endpoints

        [Test]
        public async Task GetReportById_InvalidId_ReturnsBadRequest()
        {
            var result = await _controller.GetReportById(0);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetReportById_NotFound_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetReportByIdAsync(50)).ReturnsAsync((ComplianceReportResponseDto?)null);

            var result = await _controller.GetReportById(50);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetReportById_ValidId_ReturnsOkWithData()
        {
            long reportId = 10;
            var expectedReport = new ComplianceReportResponseDto { ComplianceReportId = reportId, Title = "Specific Report" };
            _mockService.Setup(s => s.GetReportByIdAsync(reportId)).ReturnsAsync(expectedReport);

            var result = await _controller.GetReportById(reportId) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as ComplianceReportResponseDto;

            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(reportId, data!.ComplianceReportId);
        }

        #endregion

        #region 4. DeleteReport Endpoints

        [Test]
        public async Task DeleteReport_InvalidId_ReturnsBadRequest()
        {
            var result = await _controller.DeleteReport(-5);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteReport_NotFound_ReturnsNotFound()
        {
            long reportId = 99;
            _mockService.Setup(s => s.DeleteReportAsync(reportId)).ReturnsAsync(false);

            var result = await _controller.DeleteReport(reportId);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task DeleteReport_ValidId_ReturnsOk()
        {
            long reportId = 42;
            _mockService.Setup(s => s.DeleteReportAsync(reportId)).ReturnsAsync(true);

            var result = await _controller.DeleteReport(reportId) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            Assert.AreEqual(ComplianceConstants.ReportDeletedSuccess, message);
        }

        #endregion

        #region 5. GetAuditLogs Endpoints

        [Test]
        public async Task GetAuditLogs_NullSearch_ReturnsBadRequest()
        {
            var result = await _controller.GetAuditLogs(null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetAuditLogs_InvalidModelState_ReturnsBadRequest()
        {
            var searchDto = new AuditLogSearchDto();
            _controller.ModelState.AddModelError("UserId", "UserId format is invalid");

            var result = await _controller.GetAuditLogs(searchDto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _controller.ModelState.Clear();
        }

        [Test]
        public async Task GetAuditLogs_ValidSearch_ReturnsOkWithLogs()
        {
            var searchDto = new AuditLogSearchDto();
            var expectedLogs = new List<AuditLogResponseDto> 
            { 
                new() { AuditLogId = 1, Action = "DELETE" } 
            };
            _mockService.Setup(s => s.GetAuditLogsAsync(searchDto)).ReturnsAsync(expectedLogs);

            var result = await _controller.GetAuditLogs(searchDto) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as IEnumerable<AuditLogResponseDto>;

            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data!.Count());
        }

        #endregion

        #region 6. GetAllPolicies Endpoints

        [Test]
        public async Task GetAllPolicies_ReturnsOkWithPoliciesList()
        {
            var expectedPolicies = new List<RetentionPolicyDto>
            {
                new() { RetentionPolicyId = 1, DataType = "Logs", RetentionDays = 365 }
            };
            _mockService.Setup(s => s.GetAllRetentionPoliciesAsync()).ReturnsAsync(expectedPolicies);

            var result = await _controller.GetAllPolicies() as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as IEnumerable<RetentionPolicyDto>;

            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data!.Count());
        }

        [Test]
        public async Task GetAllPolicies_EmptyList_ReturnsOkWithEmptyCollection()
        {
            _mockService.Setup(s => s.GetAllRetentionPoliciesAsync()).ReturnsAsync(new List<RetentionPolicyDto>());

            var result = await _controller.GetAllPolicies() as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as IEnumerable<RetentionPolicyDto>;
            Assert.IsNotNull(data);
            Assert.IsEmpty(data);
        }

        [Test]
        public async Task GetAllPolicies_ServiceThrowsException_PropagatesException()
        {
            _mockService.Setup(s => s.GetAllRetentionPoliciesAsync()).ThrowsAsync(new Exception("Database down"));

            Assert.ThrowsAsync<Exception>(async () => await _controller.GetAllPolicies());
        }

        #endregion

        #region 7. UpdatePolicy Endpoints

        [Test]
        public async Task UpdatePolicy_InvalidIdOrNullDto_ReturnsBadRequest()
        {
            var dto = new RetentionPolicyDto();
            
            // Scenario A: Invalid Policy ID
            var resultIdInvalid = await _controller.UpdatePolicy(0, dto);
            Assert.IsInstanceOf<BadRequestObjectResult>(resultIdInvalid);

            // Scenario B: Null DTO
            var resultDtoNull = await _controller.UpdatePolicy(1, null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(resultDtoNull);
        }

        [Test]
        public async Task UpdatePolicy_InvalidModelState_ReturnsBadRequest()
        {
            var dto = new RetentionPolicyDto();
            _controller.ModelState.AddModelError("RetentionDays", "Range must be positive");

            var result = await _controller.UpdatePolicy(1, dto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _controller.ModelState.Clear();
        }

        [Test]
        public async Task UpdatePolicy_NotFound_ReturnsNotFound()
        {
            var dto = new RetentionPolicyDto { DataType = "X", RetentionDays = 30, Description = "d", IsActive = true };
            _mockService.Setup(s => s.UpdateRetentionPolicyAsync(999, dto)).ReturnsAsync((RetentionPolicyDto?)null);

            var result = await _controller.UpdatePolicy(999, dto);
            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task UpdatePolicy_Valid_ReturnsOk()
        {
            var dto = new RetentionPolicyDto { RetentionPolicyId = 2, DataType = "X", RetentionDays = 90, Description = "d", IsActive = true };
            _mockService.Setup(s => s.UpdateRetentionPolicyAsync(2, dto)).ReturnsAsync(dto);

            var result = await _controller.UpdatePolicy(2, dto) as OkObjectResult;
            Assert.IsNotNull(result);

            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            Assert.AreEqual(ComplianceConstants.PolicyUpdateSuccess, message);

            var data = value.GetType().GetProperty("data")!.GetValue(value) as RetentionPolicyDto;
            Assert.IsNotNull(data);
            Assert.AreEqual(dto.RetentionPolicyId, data!.RetentionPolicyId);
        }

        #endregion
    }
}