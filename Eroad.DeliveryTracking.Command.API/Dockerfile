FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Create non-root user for security
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore (layer caching optimization)
COPY ["Eroad.DeliveryTracking.Command.API/Eroad.DeliveryTracking.Command.API.csproj", "Eroad.DeliveryTracking.Command.API/"]
COPY ["Eroad.DeliveryTracking.Command.Domain/Eroad.DeliveryTracking.Command.Domain.csproj", "Eroad.DeliveryTracking.Command.Domain/"]
COPY ["Eroad.DeliveryTracking.Command.Infrastructure/Eroad.DeliveryTracking.Command.Infrastructure.csproj", "Eroad.DeliveryTracking.Command.Infrastructure/"]
COPY ["Eroad.DeliveryTracking.Common/Eroad.DeliveryTracking.Common.csproj", "Eroad.DeliveryTracking.Common/"]
COPY ["Eroad.Common/Eroad.Common.csproj", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/Eroad.CQRS.Core.csproj", "Eroad.CQRS.Core/"]

# Restore dependencies
RUN dotnet restore "Eroad.DeliveryTracking.Command.API/Eroad.DeliveryTracking.Command.API.csproj"

# Copy only the specific project directories needed
COPY ["Eroad.DeliveryTracking.Command.API/", "Eroad.DeliveryTracking.Command.API/"]
COPY ["Eroad.DeliveryTracking.Command.Domain/", "Eroad.DeliveryTracking.Command.Domain/"]
COPY ["Eroad.DeliveryTracking.Command.Infrastructure/", "Eroad.DeliveryTracking.Command.Infrastructure/"]
COPY ["Eroad.DeliveryTracking.Common/", "Eroad.DeliveryTracking.Common/"]
COPY ["Eroad.Common/", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/", "Eroad.CQRS.Core/"]

# Build the project
WORKDIR "/src/Eroad.DeliveryTracking.Command.API"
RUN dotnet build "Eroad.DeliveryTracking.Command.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Eroad.DeliveryTracking.Command.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run as non-root user
USER appuser

ENTRYPOINT ["dotnet", "Eroad.DeliveryTracking.Command.API.dll"]
