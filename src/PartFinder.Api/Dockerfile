# Build from the repository root:
#   docker build -f src/PartFinder.Api/Dockerfile -t partfinder-api .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Directory.Build.props Directory.Packages.props ./
COPY src/PartFinder.Core/PartFinder.Core.csproj src/PartFinder.Core/
COPY src/PartFinder.Infrastructure/PartFinder.Infrastructure.csproj src/PartFinder.Infrastructure/
COPY src/PartFinder.Api/PartFinder.Api.csproj src/PartFinder.Api/
RUN dotnet restore src/PartFinder.Api/PartFinder.Api.csproj
COPY src/ src/
RUN dotnet publish src/PartFinder.Api/PartFinder.Api.csproj -c Release --no-restore -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "PartFinder.Api.dll"]
