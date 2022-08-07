using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace MusicFeed.DiagnosticsService
{
	public class Startup
	{
		private readonly IConfiguration configuration;

		public Startup(IConfiguration configuration)
		{
			this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			// We use random database name for a sake of integration tests.
			// If same database is reused in scope of the same process (test runner),
			// tests produce sporadic errors in log: InvalidOperationException: Sequence contains more than one element
			services.AddHealthChecksUI()
				.AddInMemoryStorage(databaseName: $"HealthChecksUI-{Guid.NewGuid():N}");

			services.AddHealthChecks()
				.AddCheck<OverallHealthCheck>("Overall", failureStatus: HealthStatus.Unhealthy, tags: new[] { "overall" });
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// We make service available at sub-path /diagnostics as a workaround for missing URL rewrite in ALB controller.
			// This could be removed when URL rewrite is supported by ALB controller.
			// Tracking issue: https://github.com/kubernetes-sigs/aws-load-balancer-controller/issues/1571
			// The service is still available at root path / - https://stackoverflow.com/questions/53429942/
			app.UsePathBase("/diagnostics");

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();

				endpoints.MapHealthChecksUI(o => o.UIPath = "/services");

				endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
				{
					Predicate = check => check.Tags.Contains("ready"),
				});

				endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
				{
					Predicate = check => check.Tags.Contains("live"),
				});

				endpoints.MapHealthChecks("/health/overall", new HealthCheckOptions
				{
					Predicate = check => check.Tags.Contains("overall"),
				});
			});
		}
	}
}
