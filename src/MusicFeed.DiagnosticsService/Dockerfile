# docker pull codefuller/musicfeed-updates-service:latest
# docker pull codefuller/musicfeed-api-service:latest
# docker-compose -f "../../tests/MusicFeed.DiagnosticsService.IntegrationTests/docker-compose.yaml" up --no-build --detach
# docker build --network=host -t musicfeed/diagnostics-service:latest -f "../../src/MusicFeed.DiagnosticsService/Dockerfile" ../..
# docker-compose -f "../../tests/MusicFeed.DiagnosticsService.IntegrationTests/docker-compose.yaml" down

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /build

COPY ["src/MusicFeed.DiagnosticsService/MusicFeed.DiagnosticsService.csproj", "src/MusicFeed.DiagnosticsService/"]
COPY ["tests/MusicFeed.DiagnosticsService.IntegrationTests/MusicFeed.DiagnosticsService.IntegrationTests.csproj", "tests/MusicFeed.DiagnosticsService.IntegrationTests/"]
COPY ["tests/MusicFeed.DiagnosticsService.UnitTests/MusicFeed.DiagnosticsService.UnitTests.csproj", "tests/MusicFeed.DiagnosticsService.UnitTests/"]
COPY ["MusicFeed.DiagnosticsService.Build.sln", "./"]
COPY ["MusicFeed.DiagnosticsService.src.ruleset", "./"]
COPY ["MusicFeed.DiagnosticsService.tests.ruleset", "./"]

RUN dotnet restore "MusicFeed.DiagnosticsService.Build.sln"

COPY . .

RUN dotnet restore "MusicFeed.DiagnosticsService.Build.sln"

RUN dotnet build "MusicFeed.DiagnosticsService.Build.sln" --no-restore --configuration Release

FROM build AS test

# We don't let dotnet test command to fail build if some tests are failed.
# This will prevent publish of tests results (trx file).
# If some tests are failed, the task PublishTestResults will fail due to failTaskOnFailedTests parameter.
RUN dotnet test "MusicFeed.DiagnosticsService.Build.sln" --no-build --configuration Release --logger "trx" --collect:"XPlat Code Coverage"; exit 0

RUN mkdir TestResults
RUN cp -r --target-directory=TestResults --parents tests/*/TestResults

FROM test AS publish
RUN dotnet publish "src/MusicFeed.DiagnosticsService/MusicFeed.DiagnosticsService.csproj" --no-build --configuration Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MusicFeed.DiagnosticsService.dll"]
