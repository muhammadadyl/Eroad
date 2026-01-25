FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Create non-root user for security
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore (layer caching optimization)
COPY ["Eroad.DeliveryTracking.Query.API/Eroad.DeliveryTracking.Query.API.csproj", "Eroad.DeliveryTracking.Query.API/"]
COPY ["Eroad.DeliveryTracking.Query.Domain/Eroad.DeliveryTracking.Query.Domain.csproj", "Eroad.DeliveryTracking.Query.Domain/"]
COPY ["Eroad.DeliveryTracking.Query.Infrastructure/Eroad.DeliveryTracking.Query.Infrastructure.csproj", "Eroad.DeliveryTracking.Query.Infrastructure/"]
COPY ["Eroad.DeliveryTracking.Common/Eroad.DeliveryTracking.Common.csproj", "Eroad.DeliveryTracking.Common/"]
COPY ["Eroad.Common/Eroad.Common.csproj", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/Eroad.CQRS.Core.csproj", "Eroad.CQRS.Core/"]

# Restore dependencies
RUN dotnet restore "Eroad.DeliveryTracking.Query.API/Eroad.DeliveryTracking.Query.API.csproj"

# Copy only the specific project directories needed
COPY ["Eroad.DeliveryTracking.Query.API/", "Eroad.DeliveryTracking.Query.API/"]
COPY ["Eroad.DeliveryTracking.Query.Domain/", "Eroad.DeliveryTracking.Query.Domain/"]
COPY ["Eroad.DeliveryTracking.Query.Infrastructure/", "Eroad.DeliveryTracking.Query.Infrastructure/"]
COPY ["Eroad.DeliveryTracking.Common/", "Eroad.DeliveryTracking.Common/"]
COPY ["Eroad.Common/", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/", "Eroad.CQRS.Core/"]

# Build the project
WORKDIR "/src/Eroad.DeliveryTracking.Query.API"
RUN dotnet build "Eroad.DeliveryTracking.Query.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Eroad.DeliveryTracking.Query.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run as non-root user
USER appuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "Eroad.DeliveryTracking.Query.API.dll"]
