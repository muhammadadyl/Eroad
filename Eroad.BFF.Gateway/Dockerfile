FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Eroad.BFF.Gateway/Eroad.BFF.Gateway.csproj", "Eroad.BFF.Gateway/"]
COPY ["Eroad.DeliveryTracking.Contracts/Eroad.DeliveryTracking.Contracts.csproj", "Eroad.DeliveryTracking.Contracts/"]
COPY ["Eroad.FleetManagement.Contracts/Eroad.FleetManagement.Contracts.csproj", "Eroad.FleetManagement.Contracts/"]
COPY ["Eroad.RouteManagement.Contracts/Eroad.RouteManagement.Contracts.csproj", "Eroad.RouteManagement.Contracts/"]

# Restore dependencies
RUN dotnet restore "Eroad.BFF.Gateway/Eroad.BFF.Gateway.csproj"

# Copy source files
COPY ["Eroad.BFF.Gateway/", "Eroad.BFF.Gateway/"]
COPY ["Eroad.DeliveryTracking.Contracts/", "Eroad.DeliveryTracking.Contracts/"]
COPY ["Eroad.FleetManagement.Contracts/", "Eroad.FleetManagement.Contracts/"]
COPY ["Eroad.RouteManagement.Contracts/", "Eroad.RouteManagement.Contracts/"]

# Build the project
WORKDIR "/src/Eroad.BFF.Gateway"
RUN dotnet build "Eroad.BFF.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Eroad.BFF.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eroad.BFF.Gateway.dll"]
