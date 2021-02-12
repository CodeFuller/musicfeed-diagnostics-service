# docker pull codefuller/musicfeed-updates-service:latest
# docker pull codefuller/musicfeed-api-service:latest
# docker-compose -f "../../tests/DiagnosticsService.IntegrationTests/docker-compose.yml" up --no-build --detach
# docker build --network=musicfeed-test-network -t musicfeed/diagnostics-service:latest -f "Dockerfile" ../..
# docker-compose -f "../../tests/DiagnosticsService.IntegrationTests/docker-compose.yml" down

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

COPY ["src/DiagnosticsService/DiagnosticsService.csproj", "/project/src/DiagnosticsService/"]
COPY ["tests/DiagnosticsService.IntegrationTests/DiagnosticsService.IntegrationTests.csproj", "/project/tests/DiagnosticsService.IntegrationTests/"]
COPY ["tests/DiagnosticsService.UnitTests/DiagnosticsService.UnitTests.csproj", "/project/tests/DiagnosticsService.UnitTests/"]
COPY ["DiagnosticsService.sln", "/project/"]
RUN dotnet restore "/project/DiagnosticsService.sln"

COPY . "/project/"

RUN dotnet build "/project/DiagnosticsService.sln" --no-restore -c Release

RUN dotnet test "/project/DiagnosticsService.sln" --no-build -c Release --logger "trx;LogFileName=DiagnosticsService.trx" 

FROM build AS publish
RUN dotnet publish "/project/src/DiagnosticsService/DiagnosticsService.csproj" --no-restore -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish /app

ENTRYPOINT ["dotnet", "DiagnosticsService.dll"]
