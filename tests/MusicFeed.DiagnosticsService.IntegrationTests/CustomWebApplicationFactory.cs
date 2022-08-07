using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MusicFeed.DiagnosticsService.IntegrationTests
{
	public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
	{
		private readonly Action<IServiceCollection> setupServices;

		private readonly Action<IConfigurationBuilder> setupConfiguration;

		public CustomWebApplicationFactory(Action<IServiceCollection> setupServices = null, Action<IConfigurationBuilder> setupConfiguration = null)
		{
			this.setupServices = setupServices ?? (_ => { });
			this.setupConfiguration = setupConfiguration ?? (_ => { });
		}

		protected override void ConfigureWebHost(IWebHostBuilder builder)
		{
			base.ConfigureWebHost(builder);

			builder.ConfigureAppConfiguration(configBuilder =>
			{
				ConfigurationProvider.ApplyConfiguration(configBuilder);
				setupConfiguration(configBuilder);
			});

			builder.ConfigureServices(setupServices);
		}
	}
}
