using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq.AutoMock;

namespace MusicFeed.DiagnosticsService.UnitTests
{
	[TestClass]
	public class OverallHealthCheckTests
	{
		[TestMethod]
		public async Task CheckHealthAsync_NoExecutionsExist_ReturnsUnhealthyResult()
		{
			// Arrange

			var executions = Enumerable.Empty<HealthCheckExecution>();
			await using var dbContext = StubDbContext(executions);

			var mocker = new AutoMocker();
			mocker.Use(dbContext);

			var target = mocker.CreateInstance<OverallHealthCheck>();

			// Act

			var result = await target.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

			// Assert

			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		}

		[DataTestMethod]
		[DataRow(UIHealthStatus.Degraded)]
		[DataRow(UIHealthStatus.Unhealthy)]
		public async Task CheckHealthAsync_SomeExecutionIsNotHealthy_ReturnsUnhealthyResult(UIHealthStatus unhealthyStatus)
		{
			// Arrange

			var executions = new[]
			{
				new HealthCheckExecution { Status = UIHealthStatus.Healthy },
				new HealthCheckExecution { Status = unhealthyStatus },
				new HealthCheckExecution { Status = UIHealthStatus.Healthy },
			};

			await using var dbContext = StubDbContext(executions);

			var mocker = new AutoMocker();
			mocker.Use(dbContext);

			var target = mocker.CreateInstance<OverallHealthCheck>();

			// Act

			var result = await target.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

			// Assert

			Assert.AreEqual(HealthStatus.Unhealthy, result.Status);
		}

		[TestMethod]
		public async Task CheckHealthAsync_AllExecutionsAreHealthy_ReturnsHealthyResult()
		{
			// Arrange

			var executions = new[]
			{
				new HealthCheckExecution { Status = UIHealthStatus.Healthy },
				new HealthCheckExecution { Status = UIHealthStatus.Healthy },
			};

			await using var dbContext = StubDbContext(executions);

			var mocker = new AutoMocker();
			mocker.Use(dbContext);

			var target = mocker.CreateInstance<OverallHealthCheck>();

			// Act

			var result = await target.CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

			// Assert

			Assert.AreEqual(HealthStatus.Healthy, result.Status);
		}

		private static HealthChecksDb StubDbContext(IEnumerable<HealthCheckExecution> executions)
		{
			var options = new DbContextOptionsBuilder<HealthChecksDb>()
				.UseInMemoryDatabase(databaseName: "HealthChecksDatabase")
				.Options;

			var context = new HealthChecksDb(options);

			context.Executions.RemoveRange(context.Executions);
			context.Executions.AddRange(executions);
			context.SaveChanges();

			return context;
		}
	}
}
