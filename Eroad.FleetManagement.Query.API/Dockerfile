FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Eroad.FleetManagement.Query.API/Eroad.FleetManagement.Query.API.csproj", "Eroad.FleetManagement.Query.API/"]
COPY ["Eroad.FleetManagement.Query.Domain/Eroad.FleetManagement.Query.Domain.csproj", "Eroad.FleetManagement.Query.Domain/"]
COPY ["Eroad.FleetManagement.Query.Infrastructure/Eroad.FleetManagement.Query.Infrastructure.csproj", "Eroad.FleetManagement.Query.Infrastructure/"]
COPY ["Eroad.FleetManagement.Common/Eroad.FleetManagement.Common.csproj", "Eroad.FleetManagement.Common/"]
COPY ["Eroad.Common/Eroad.Common.csproj", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/Eroad.CQRS.Core.csproj", "Eroad.CQRS.Core/"]

# Restore dependencies
RUN dotnet restore "Eroad.FleetManagement.Query.API/Eroad.FleetManagement.Query.API.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/Eroad.FleetManagement.Query.API"
RUN dotnet build "Eroad.FleetManagement.Query.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Eroad.FleetManagement.Query.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eroad.FleetManagement.Query.API.dll"]
