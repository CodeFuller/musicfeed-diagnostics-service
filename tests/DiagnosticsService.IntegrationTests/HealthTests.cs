﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DiagnosticsService.IntegrationTests
{
	[TestClass]
	public class HealthTests
	{
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

			using var factory = new CustomWebApplicationFactory(configBuilder =>
			{
				configBuilder.AddInMemoryCollection(new[] { new KeyValuePair<string, string>("healthChecksUI:evaluationTimeInSeconds", "1") });
			});

			using var client = factory.CreateClient();

			// Act

			await Task.Delay(TimeSpan.FromSeconds(3), CancellationToken.None);
			var response = await client.GetAsync(new Uri("/health/overall", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

			await StopHostedServices(factory);
		}

		[TestMethod]
		public async Task HealthOverallRequest_TheStatusIsUnhealthy_ReturnsServiceUnavailableError()
		{
			// Arrange

			using var factory = new CustomWebApplicationFactory(configBuilder =>
			{
				configBuilder.AddInMemoryCollection(new[]
				{
					new KeyValuePair<string, string>("healthChecksUI:evaluationTimeInSeconds", "1"),
					new KeyValuePair<string, string>("healthChecksUI:healthChecks:0:uri", "http://localhost/no-such-service/health/ready"),
				});
			});

			using var client = factory.CreateClient();

			// Act

			await Task.Delay(TimeSpan.FromSeconds(3), CancellationToken.None);
			var response = await client.GetAsync(new Uri("/health/overall", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.ServiceUnavailable, response.StatusCode);

			await StopHostedServices(factory);
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
