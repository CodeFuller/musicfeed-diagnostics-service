using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HealthChecks.UI.Core;
using HealthChecks.UI.Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace DiagnosticsService
{
	public class OverallHealthCheck : IHealthCheck
	{
		private readonly HealthChecksDb healthChecksDb;

		public OverallHealthCheck(HealthChecksDb healthChecksDb)
		{
			this.healthChecksDb = healthChecksDb ?? throw new ArgumentNullException(nameof(healthChecksDb));
		}

		public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
		{
			var executions = await healthChecksDb.Executions.ToListAsync(cancellationToken);

			if (executions.Any() && executions.All(x => x.Status == UIHealthStatus.Healthy))
			{
				return HealthCheckResult.Healthy();
			}

			return HealthCheckResult.Unhealthy();
		}
	}
}
