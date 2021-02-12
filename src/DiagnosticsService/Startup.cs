using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;

namespace DiagnosticsService
{
	public class Startup
	{
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
