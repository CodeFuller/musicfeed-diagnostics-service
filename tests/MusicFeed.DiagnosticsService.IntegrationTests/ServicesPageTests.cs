using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MusicFeed.DiagnosticsService.IntegrationTests
{
	[TestClass]
	public class ServicesPageTests
	{
		[TestMethod]
		public async Task ServicesPage_IsLoadedCorrectly()
		{
			// Arrange

			using var factory = new WebApplicationFactory<Startup>();
			using var client = factory.CreateClient();

			// Act

			var response = await client.GetAsync(new Uri("/diagnostics/services", UriKind.Relative));

			// Assert

			Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
		}
	}
}
