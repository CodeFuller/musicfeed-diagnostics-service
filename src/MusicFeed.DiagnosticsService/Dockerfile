# docker pull codefuller/musicfeed-updates-service:latest
# docker pull codefuller/musicfeed-api-service:latest
# docker-compose -f "../../tests/MusicFeed.DiagnosticsService.IntegrationTests/docker-compose.yml" up --no-build --detach
# docker build --network=musicfeed-test-network -t musicfeed/diagnostics-service:latest -f "Dockerfile" ../..
# docker-compose -f "../../tests/MusicFeed.DiagnosticsService.IntegrationTests/docker-compose.yml" down

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

COPY ["src/MusicFeed.DiagnosticsService/MusicFeed.DiagnosticsService.csproj", "/project/src/MusicFeed.DiagnosticsService/"]
COPY ["tests/MusicFeed.DiagnosticsService.IntegrationTests/MusicFeed.DiagnosticsService.IntegrationTests.csproj", "/project/tests/MusicFeed.DiagnosticsService.IntegrationTests/"]
COPY ["tests/MusicFeed.DiagnosticsService.UnitTests/MusicFeed.DiagnosticsService.UnitTests.csproj", "/project/tests/MusicFeed.DiagnosticsService.UnitTests/"]
COPY ["MusicFeed.DiagnosticsService.sln", "/project/"]
RUN dotnet restore "/project/MusicFeed.DiagnosticsService.sln"

COPY . "/project/"

RUN dotnet build "/project/MusicFeed.DiagnosticsService.sln" --no-restore -c Release

RUN dotnet test "/project/MusicFeed.DiagnosticsService.sln" --no-build -c Release --logger "trx;LogFileName=MusicFeed.DiagnosticsService.trx" 

FROM build AS publish
RUN dotnet publish "/project/src/MusicFeed.DiagnosticsService/MusicFeed.DiagnosticsService.csproj" --no-restore -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish /app

ENTRYPOINT ["dotnet", "MusicFeed.DiagnosticsService.dll"]
