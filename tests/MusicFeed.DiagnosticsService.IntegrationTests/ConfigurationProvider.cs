using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace MusicFeed.DiagnosticsService.IntegrationTests
{
	public static class ConfigurationProvider
	{
		private static bool RunsInsideContainer => Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

		public static void ApplyConfiguration(IConfigurationBuilder configBuilder)
		{
			if (RunsInsideContainer)
			{
				configBuilder
					.AddInMemoryCollection(new[]
					{
						new KeyValuePair<string, string>("healthChecksUI:healthChecks:0:uri", "http://api-service/health/ready"),
						new KeyValuePair<string, string>("healthChecksUI:healthChecks:1:uri", "http://updates-service/health/ready"),
					});
			}
		}
	}
}
