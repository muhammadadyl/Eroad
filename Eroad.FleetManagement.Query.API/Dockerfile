FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Create non-root user for security
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore (layer caching optimization)
COPY ["Eroad.FleetManagement.Query.API/Eroad.FleetManagement.Query.API.csproj", "Eroad.FleetManagement.Query.API/"]
COPY ["Eroad.FleetManagement.Query.Domain/Eroad.FleetManagement.Query.Domain.csproj", "Eroad.FleetManagement.Query.Domain/"]
COPY ["Eroad.FleetManagement.Query.Infrastructure/Eroad.FleetManagement.Query.Infrastructure.csproj", "Eroad.FleetManagement.Query.Infrastructure/"]
COPY ["Eroad.FleetManagement.Common/Eroad.FleetManagement.Common.csproj", "Eroad.FleetManagement.Common/"]
COPY ["Eroad.FleetManagement.Contracts/Eroad.FleetManagement.Contracts.csproj", "Eroad.FleetManagement.Contracts/"]
COPY ["Eroad.CQRS.Core/Eroad.CQRS.Core.csproj", "Eroad.CQRS.Core/"]

# Restore dependencies
RUN dotnet restore "Eroad.FleetManagement.Query.API/Eroad.FleetManagement.Query.API.csproj"

# Copy only the specific project directories needed
COPY ["Eroad.FleetManagement.Query.API/", "Eroad.FleetManagement.Query.API/"]
COPY ["Eroad.FleetManagement.Query.Domain/", "Eroad.FleetManagement.Query.Domain/"]
COPY ["Eroad.FleetManagement.Query.Infrastructure/", "Eroad.FleetManagement.Query.Infrastructure/"]
COPY ["Eroad.FleetManagement.Common/", "Eroad.FleetManagement.Common/"]
COPY ["Eroad.FleetManagement.Contracts/", "Eroad.FleetManagement.Contracts/"]
COPY ["Eroad.CQRS.Core/", "Eroad.CQRS.Core/"]

# Build the project
WORKDIR "/src/Eroad.FleetManagement.Query.API"
RUN dotnet build "Eroad.FleetManagement.Query.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Eroad.FleetManagement.Query.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run as non-root user
USER appuser

ENTRYPOINT ["dotnet", "Eroad.FleetManagement.Query.API.dll"]
