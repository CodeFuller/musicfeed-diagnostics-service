using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicFeed.DiagnosticsService.IntegrationTests
{
	[TestClass]
	public class HealthTests
	{
		private class HealthCheckCollectorInterceptor : IHealthCheckCollectorInterceptor
		{
			private readonly ILogger<HealthCheckCollectorInterceptor> logger;

			private readonly CancellationTokenSource cancellationTokenSource;

			private readonly UIHealthStatus desiredStatus;

			private bool TokenWacCancelled { get; set; }

			public HealthCheckCollectorInterceptor(ILogger<HealthCheckCollectorInterceptor> logger, CancellationTokenSource cancellationTokenSource, UIHealthStatus desiredStatus)
			{
				this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
				this.cancellationTokenSource = cancellationTokenSource ?? throw new ArgumentNullException(nameof(cancellationTokenSource));
				this.desiredStatus = desiredStatus;
			}

			public ValueTask OnCollectExecuting(HealthCheckConfiguration healthCheck)
			{
				return ValueTask.CompletedTask;
			}

			public ValueTask OnCollectExecuted(UIHealthReport report)
			{
				logger.LogInformation("OnCollectExecuted: invoked for {UIHealthStatus}", report.Status);

				if (report.Status != desiredStatus)
				{
					return ValueTask.CompletedTask;
				}

				lock (cancellationTokenSource)
				{
					if (TokenWacCancelled)
					{
						logger.LogInformation("Token was already cancelled");
					}
					else
					{
						logger.LogInformation("OnCollectExecuted: cancelling token");
						TokenWacCancelled = true;
						cancellationTokenSource.Cancel();
					}
				}

				return ValueTask.CompletedTask;
			}
		}

		[TestMethod]
		public async Task HealthLiveRequest_ReturnsHealthyResponse()
		{
			// Arrange

			using var factory = new CustomWebApplicationFactory();
			using var client = factory.CreateClient();

			// Act

			var response = await client.GetAsync(new Uri("/health/live", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			await StopHostedServices(factory);
		}

		[TestMethod]
		public async Task HealthReadyRequest_ReturnsHealthyResponse()
		{
			// Arrange

			using var factory = new CustomWebApplicationFactory();
			using var client = factory.CreateClient();

			// Act

			var response = await client.GetAsync(new Uri("/health/ready", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			await StopHostedServices(factory);
		}

		[TestMethod]
		public async Task HealthOverallRequest_AllServicesAreHealthy_ReturnsHealthyResponse()
		{
			// Arrange

			using var cts = new CancellationTokenSource();

			void SetupServices(IServiceCollection services) => services.AddSingleton<IHealthCheckCollectorInterceptor>(
				sp => ActivatorUtilities.CreateInstance<HealthCheckCollectorInterceptor>(sp, cts, UIHealthStatus.Healthy));

			using var factory = new CustomWebApplicationFactory(SetupServices);

			using var client = factory.CreateClient();

			// Act

			await WaitForHealthCheckCollector(cts.Token);
			var response = await client.GetAsync(new Uri("/health/overall", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			await StopHostedServices(factory);
		}

		[TestMethod]
		public async Task HealthOverallRequest_TheStatusIsUnhealthy_ReturnsServiceUnavailableError()
		{
			// Arrange

			using var cts = new CancellationTokenSource();

			void SetupServices(IServiceCollection services)
			{
				services.AddSingleton<IHealthCheckCollectorInterceptor>(
					sp => ActivatorUtilities.CreateInstance<HealthCheckCollectorInterceptor>(sp, cts, UIHealthStatus.Unhealthy));
			}

			static void SetupConfiguration(IConfigurationBuilder configBuilder)
			{
				configBuilder.AddInMemoryCollection(new[]
				{
					// We add new test service with incorrect answer.
					// We cannot override existing service configuration (e.g. healthChecksUI:healthChecks:0),
					// because HealthChecksUI contains static mapping of configuration id (from the database) to endpoint address (endpointAddresses in HealthCheckReportCollector).
					new KeyValuePair<string, string>("healthChecksUI:healthChecks:2:name", "Test Service"),
					new KeyValuePair<string, string>("healthChecksUI:healthChecks:2:uri", "http://localhost/no-such-service/health/ready"),
				});
			}

			using var factory = new CustomWebApplicationFactory(SetupServices, SetupConfiguration);

			using var client = factory.CreateClient();

			// Act

			await WaitForHealthCheckCollector(cts.Token);
			var response = await client.GetAsync(new Uri("/health/overall", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

			await StopHostedServices(factory);
		}

		private static async Task WaitForHealthCheckCollector(CancellationToken cancellationToken)
		{
			try
			{
				var delay = TimeSpan.FromSeconds(5);
				await Task.Delay(delay, cancellationToken);

				Assert.Fail($"Health check was not executed within {delay}");
			}
			catch (TaskCanceledException)
			{
				Console.WriteLine("Health check was executed");
			}
		}

		// Without explicit stop of hosted services, the tests fail sporadically due to ObjectDisposedException,
		// because HealthCheckCollectorHostedService tries to log message on disposed logger.
		private static async Task StopHostedServices(CustomWebApplicationFactory webApplicationFactory)
		{
			var hostedServices = webApplicationFactory.Services.GetRequiredService<IEnumerable<IHostedService>>();

			foreach (var hostedService in hostedServices)
			{
				Console.WriteLine($"Stopping hosted service {hostedService.GetType()} ...");
				await hostedService.StopAsync(CancellationToken.None);
			}
		}
	}
}
