FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# Create non-root user for security
RUN addgroup --system --gid 1000 appgroup && \
    adduser --system --uid 1000 --ingroup appgroup appuser

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore (layer caching optimization)
COPY ["Eroad.FleetManagement.Command.API/Eroad.FleetManagement.Command.API.csproj", "Eroad.FleetManagement.Command.API/"]
COPY ["Eroad.FleetManagement.Command.Domain/Eroad.FleetManagement.Command.Domain.csproj", "Eroad.FleetManagement.Command.Domain/"]
COPY ["Eroad.FleetManagement.Command.Infrastructure/Eroad.FleetManagement.Command.Infrastructure.csproj", "Eroad.FleetManagement.Command.Infrastructure/"]
COPY ["Eroad.FleetManagement.Common/Eroad.FleetManagement.Common.csproj", "Eroad.FleetManagement.Common/"]
COPY ["Eroad.Common/Eroad.Common.csproj", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/Eroad.CQRS.Core.csproj", "Eroad.CQRS.Core/"]

# Restore dependencies
RUN dotnet restore "Eroad.FleetManagement.Command.API/Eroad.FleetManagement.Command.API.csproj"

# Copy only the specific project directories needed
COPY ["Eroad.FleetManagement.Command.API/", "Eroad.FleetManagement.Command.API/"]
COPY ["Eroad.FleetManagement.Command.Domain/", "Eroad.FleetManagement.Command.Domain/"]
COPY ["Eroad.FleetManagement.Command.Infrastructure/", "Eroad.FleetManagement.Command.Infrastructure/"]
COPY ["Eroad.FleetManagement.Common/", "Eroad.FleetManagement.Common/"]
COPY ["Eroad.Common/", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/", "Eroad.CQRS.Core/"]

# Build the project
WORKDIR "/src/Eroad.FleetManagement.Command.API"
RUN dotnet build "Eroad.FleetManagement.Command.API.csproj" -c Release -o /app/build --no-restore

FROM build AS publish
RUN dotnet publish "Eroad.FleetManagement.Command.API.csproj" -c Release -o /app/publish --no-restore

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Run as non-root user
USER appuser

ENTRYPOINT ["dotnet", "Eroad.FleetManagement.Command.API.dll"]
