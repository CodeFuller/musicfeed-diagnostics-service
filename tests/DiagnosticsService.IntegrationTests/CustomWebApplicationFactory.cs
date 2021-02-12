using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace DiagnosticsService.IntegrationTests
{
	public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
	{
		private readonly Action<IConfigurationBuilder> setupConfiguration;

		public CustomWebApplicationFactory(Action<IConfigurationBuilder> setupConfiguration = null)
		{
			this.setupConfiguration = setupConfiguration ?? (_ => { });
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			base.ConfigureWebHost(builder);

			builder.ConfigureAppConfiguration(setupConfiguration);
		}
	}
}
