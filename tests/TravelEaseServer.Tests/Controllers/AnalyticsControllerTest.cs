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
    public class AnalyticsControllerTests
    {
        private Mock<IAnalyticsService> _mockService;
        private AnalyticsController _controller;

        [SetUp]
        public void SetUp()
        {
            _mockService = new Mock<IAnalyticsService>(MockBehavior.Strict);
            _controller = new AnalyticsController(_mockService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _mockService.VerifyAll();
        }

        #region 1. GetAllKPIReports Endpoints

        [Test]
        public async Task GetAllKPIReports_NullSearchDto_ReturnsBadRequest()
        {
            var result = await _controller.GetAllKPIReports(null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetAllKPIReports_InvalidModelState_ReturnsBadRequest()
        {
            var searchDto = new KPIReportSearchDto();
            _controller.ModelState.AddModelError("SearchTerm", "SearchTerm cannot contain symbols");

            var result = await _controller.GetAllKPIReports(searchDto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _controller.ModelState.Clear();
        }

        [Test]
        public async Task GetAllKPIReports_ValidSearch_ReturnsOkWithData()
        {
            var searchDto = new KPIReportSearchDto { SearchTerm = "Q2" };
            var expectedData = new List<KPIReportResponseDto>
            {
                new() { KPIReportId = 1, Title = "Q2 Revenue KPI", Scope = "Global", Metrics = "{}", ReportContent = "Content" }
            };

            _mockService.Setup(s => s.GetAllKPIReportsAsync(searchDto)).ReturnsAsync(expectedData);

            var result = await _controller.GetAllKPIReports(searchDto) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as IEnumerable<KPIReportResponseDto>;

            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(1, data!.Count());
            Assert.AreEqual("Q2 Revenue KPI", data!.First().Title);
        }

        [Test]
        public async Task GetAllKPIReports_ServiceReturnsNull_ReturnsNotFound()
        {
            var searchDto = new KPIReportSearchDto();
            _mockService.Setup(s => s.GetAllKPIReportsAsync(searchDto)).ReturnsAsync((IEnumerable<KPIReportResponseDto>?)null);

            var result = await _controller.GetAllKPIReports(searchDto);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        #endregion

        #region 2. CreateKPIReport Endpoints

        [Test]
        public async Task CreateKPIReport_NullDto_ReturnsBadRequest()
        {
            var result = await _controller.CreateKPIReport(null!);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task CreateKPIReport_InvalidModelState_ReturnsBadRequest()
        {
            var dto = new KPIReportRequestDto();
            _controller.ModelState.AddModelError("Title", "Title cannot be blank");

            var result = await _controller.CreateKPIReport(dto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
            _controller.ModelState.Clear();
        }

        [Test]
        public async Task CreateKPIReport_ValidRequest_ReturnsOkWithData()
        {
            var dto = new KPIReportRequestDto { Title = "Annual Optimization", Scope = "Company", Metrics = "[]", ReportContent = "Data" };
            var responseDto = new KPIReportResponseDto { KPIReportId = 12, Title = "Annual Optimization", GeneratedDate = DateTime.UtcNow };

            _mockService.Setup(s => s.CreateKPIReportAsync(dto)).ReturnsAsync(responseDto);

            var result = await _controller.CreateKPIReport(dto) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as KPIReportResponseDto;

            Assert.AreEqual(AnalyticsConstants.KPIReportGeneratedSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(12, data!.KPIReportId);
        }

        [Test]
        public async Task CreateKPIReport_ServiceFailsToBuildReport_ReturnsBadRequest()
        {
            var dto = new KPIReportRequestDto { Title = "Failed Generation" };
            _mockService.Setup(s => s.CreateKPIReportAsync(dto)).ReturnsAsync((KPIReportResponseDto?)null);

            var result = await _controller.CreateKPIReport(dto);

            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        #endregion

        #region 3. GetKPIReportById Endpoints

        [Test]
        public async Task GetKPIReportById_InvalidId_ReturnsBadRequest()
        {
            var result = await _controller.GetKPIReportById(0);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task GetKPIReportById_NotFound_ReturnsNotFound()
        {
            long targetId = 404;
            _mockService.Setup(s => s.GetKPIReportByIdAsync(targetId)).ReturnsAsync((KPIReportResponseDto?)null);

            var result = await _controller.GetKPIReportById(targetId);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetKPIReportById_Success_ReturnsOkWithReport()
        {
            long targetId = 25;
            var expectedReport = new KPIReportResponseDto { KPIReportId = targetId, Title = "Efficiency Metrics" };
            _mockService.Setup(s => s.GetKPIReportByIdAsync(targetId)).ReturnsAsync(expectedReport);

            var result = await _controller.GetKPIReportById(targetId) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as KPIReportResponseDto;

            Assert.AreEqual(GeneralConstants.OperationSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(targetId, data!.KPIReportId);
        }

        #endregion

        #region 4. DeleteKPIReport Endpoints

        [Test]
        public async Task DeleteKPIReport_InvalidId_ReturnsBadRequest()
        {
            var result = await _controller.DeleteKPIReport(-1);
            Assert.IsInstanceOf<BadRequestObjectResult>(result);
        }

        [Test]
        public async Task DeleteKPIReport_NotFound_ReturnsNotFound()
        {
            long targetId = 99;
            _mockService.Setup(s => s.DeleteKPIReportAsync(targetId)).ReturnsAsync(false);

            var result = await _controller.DeleteKPIReport(targetId);

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task DeleteKPIReport_Success_ReturnsOk()
        {
            long targetId = 99;
            _mockService.Setup(s => s.DeleteKPIReportAsync(targetId)).ReturnsAsync(true);

            var result = await _controller.DeleteKPIReport(targetId) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;

            Assert.AreEqual(AnalyticsConstants.KPIReportDeletedSuccess, message);
        }

        #endregion

        #region 5. GetTravelSpendDashboard Endpoints

        [Test]
        public async Task GetTravelSpendDashboard_Success_ReturnsOkWithDashboardData()
        {
            var expectedDashboard = new DashboardDataDto { Title = "Travel Spend", TotalAmount = 150000.75m, TotalCount = 350, Period = "Monthly" };
            _mockService.Setup(s => s.GetTravelSpendDashboardAsync("month")).ReturnsAsync(expectedDashboard);

            var result = await _controller.GetTravelSpendDashboard(new DashboardFilterDto { Filter = "month" }) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as DashboardDataDto;

            Assert.AreEqual(AnalyticsConstants.DashboardDataRetrievedSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(150000.75m, data!.TotalAmount);
        }

        [Test]
        public async Task GetTravelSpendDashboard_NotFound_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetTravelSpendDashboardAsync("month")).ReturnsAsync((DashboardDataDto?)null);

            var result = await _controller.GetTravelSpendDashboard(new DashboardFilterDto { Filter = "month" });

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetTravelSpendDashboard_ServiceThrows_PropagatesException()
        {
            _mockService.Setup(s => s.GetTravelSpendDashboardAsync("month")).ThrowsAsync(new Exception("Timeout"));

            Assert.ThrowsAsync<Exception>(async () => await _controller.GetTravelSpendDashboard(new DashboardFilterDto { Filter = "month" }));
        }

        #endregion

        #region 6. GetBookingVolumeDashboard Endpoints

        [Test]
        public async Task GetBookingVolumeDashboard_Success_ReturnsOkWithData()
        {
            var expectedDashboard = new DashboardDataDto { Title = "Booking Volume", TotalCount = 842, TotalAmount = 0m };
            _mockService.Setup(s => s.GetBookingVolumeDashboardAsync("month")).ReturnsAsync(expectedDashboard);

            var result = await _controller.GetBookingVolumeDashboard(new DashboardFilterDto { Filter = "month" }) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as DashboardDataDto;

            Assert.AreEqual(AnalyticsConstants.DashboardDataRetrievedSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(842, data!.TotalCount);
        }

        [Test]
        public async Task GetBookingVolumeDashboard_NotFound_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetBookingVolumeDashboardAsync("month")).ReturnsAsync((DashboardDataDto?)null);

            var result = await _controller.GetBookingVolumeDashboard(new DashboardFilterDto { Filter = "month" });

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetBookingVolumeDashboard_EmptyDashboardObject_StillReturnsOk()
        {
            var emptyDashboard = new DashboardDataDto { TotalCount = 0, Title = "Empty Dashboard" };
            _mockService.Setup(s => s.GetBookingVolumeDashboardAsync("month")).ReturnsAsync(emptyDashboard);

            var result = await _controller.GetBookingVolumeDashboard(new DashboardFilterDto { Filter = "month" }) as OkObjectResult;
            Assert.IsNotNull(result);
        }

        #endregion

        #region 7. GetCancellationDashboard Endpoints

        [Test]
        public async Task GetCancellationDashboard_Success_ReturnsOkWithData()
        {
            var expectedDashboard = new DashboardDataDto { Title = "Cancellations Overview", TotalCount = 14 };
            _mockService.Setup(s => s.GetCancellationDashboardAsync("month")).ReturnsAsync(expectedDashboard);

            var result = await _controller.GetCancellationDashboard(new DashboardFilterDto { Filter = "month" }) as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as DashboardDataDto;

            Assert.AreEqual(AnalyticsConstants.DashboardDataRetrievedSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual(14, data!.TotalCount);
        }

        [Test]
        public async Task GetCancellationDashboard_NotFound_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetCancellationDashboardAsync("month")).ReturnsAsync((DashboardDataDto?)null);

            var result = await _controller.GetCancellationDashboard(new DashboardFilterDto { Filter = "month" });

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetCancellationDashboard_ServiceFailsWithError_PropagatesException()
        {
            _mockService.Setup(s => s.GetCancellationDashboardAsync("month")).ThrowsAsync(new InvalidOperationException("Connection Broken"));

            Assert.ThrowsAsync<InvalidOperationException>(async () => await _controller.GetCancellationDashboard(new DashboardFilterDto { Filter = "month" }));
        }

        #endregion

        #region 8. GetSpendPerTravelerTrend Endpoints

        [Test]
        public async Task GetSpendPerTravelerTrend_Success_ReturnsOkWithTrends()
        {
            var expectedTrend = new TrendAnalysisDto { Title = "Spend Per Traveler Trend", TrendType = "Ascending", Period = new DateTime(2026, 1, 1) };
            _mockService.Setup(s => s.GetSpendPerTravelerTrendAsync()).ReturnsAsync(expectedTrend);

            var result = await _controller.GetSpendPerTravelerTrend() as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as TrendAnalysisDto;

            Assert.AreEqual(AnalyticsConstants.TrendAnalysisSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual("Spend Per Traveler Trend", data!.Title);
        }

        [Test]
        public async Task GetSpendPerTravelerTrend_NotFound_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetSpendPerTravelerTrendAsync()).ReturnsAsync((TrendAnalysisDto?)null);

            var result = await _controller.GetSpendPerTravelerTrend();

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetSpendPerTravelerTrend_EmptyDataFields_ReturnsOk()
        {
            var emptyTrend = new TrendAnalysisDto { Title = string.Empty, TrendType = "None" };
            _mockService.Setup(s => s.GetSpendPerTravelerTrendAsync()).ReturnsAsync(emptyTrend);

            var result = await _controller.GetSpendPerTravelerTrend() as OkObjectResult;
            Assert.IsNotNull(result);
        }

        #endregion

        #region 9. GetDestinationTrend Endpoints

        [Test]
        public async Task GetDestinationTrend_Success_ReturnsOkWithTrends()
        {
            var expectedTrend = new TrendAnalysisDto { Title = "Top Destinations", TrendType = "VolumeBased" };
            _mockService.Setup(s => s.GetDestinationTrendAsync()).ReturnsAsync(expectedTrend);

            var result = await _controller.GetDestinationTrend() as OkObjectResult;

            Assert.IsNotNull(result);
            var value = result!.Value!;
            var message = value.GetType().GetProperty("message")!.GetValue(value) as string;
            var data = value.GetType().GetProperty("data")!.GetValue(value) as TrendAnalysisDto;

            Assert.AreEqual(AnalyticsConstants.TrendAnalysisSuccess, message);
            Assert.IsNotNull(data);
            Assert.AreEqual("Top Destinations", data!.Title);
        }

        [Test]
        public async Task GetDestinationTrend_NotFound_ReturnsNotFound()
        {
            _mockService.Setup(s => s.GetDestinationTrendAsync()).ReturnsAsync((TrendAnalysisDto?)null);

            var result = await _controller.GetDestinationTrend();

            Assert.IsInstanceOf<NotFoundObjectResult>(result);
        }

        [Test]
        public async Task GetDestinationTrend_ServiceFails_ThrowsException()
        {
            _mockService.Setup(s => s.GetDestinationTrendAsync()).ThrowsAsync(new Exception("Engine failure"));

            Assert.ThrowsAsync<Exception>(async () => await _controller.GetDestinationTrend());
        }

        #endregion
    }
}
