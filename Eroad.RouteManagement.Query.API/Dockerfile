FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Create non-root user for security
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore (layer caching optimization)
COPY ["Eroad.RouteManagement.Query.API/Eroad.RouteManagement.Query.API.csproj", "Eroad.RouteManagement.Query.API/"]
COPY ["Eroad.RouteManagement.Query.Domain/Eroad.RouteManagement.Query.Domain.csproj", "Eroad.RouteManagement.Query.Domain/"]
COPY ["Eroad.RouteManagement.Query.Infrastructure/Eroad.RouteManagement.Query.Infrastructure.csproj", "Eroad.RouteManagement.Query.Infrastructure/"]
COPY ["Eroad.RouteManagement.Common/Eroad.RouteManagement.Common.csproj", "Eroad.RouteManagement.Common/"]
COPY ["Eroad.Common/Eroad.Common.csproj", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/Eroad.CQRS.Core.csproj", "Eroad.CQRS.Core/"]

# Restore dependencies
RUN dotnet restore "Eroad.RouteManagement.Query.API/Eroad.RouteManagement.Query.API.csproj"

# Copy only the specific project directories needed
COPY ["Eroad.RouteManagement.Query.API/", "Eroad.RouteManagement.Query.API/"]
COPY ["Eroad.RouteManagement.Query.Domain/", "Eroad.RouteManagement.Query.Domain/"]
COPY ["Eroad.RouteManagement.Query.Infrastructure/", "Eroad.RouteManagement.Query.Infrastructure/"]
COPY ["Eroad.RouteManagement.Common/", "Eroad.RouteManagement.Common/"]
COPY ["Eroad.Common/", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/", "Eroad.CQRS.Core/"]

# Build the project
WORKDIR "/src/Eroad.RouteManagement.Query.API"
RUN dotnet build "Eroad.RouteManagement.Query.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Eroad.RouteManagement.Query.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run as non-root user
USER appuser

ENTRYPOINT ["dotnet", "Eroad.RouteManagement.Query.API.dll"]
