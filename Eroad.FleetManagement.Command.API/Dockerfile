FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Eroad.FleetManagement.Command.API/Eroad.FleetManagement.Command.API.csproj", "Eroad.FleetManagement.Command.API/"]
COPY ["Eroad.FleetManagement.Command.Domain/Eroad.FleetManagement.Command.Domain.csproj", "Eroad.FleetManagement.Command.Domain/"]
COPY ["Eroad.FleetManagement.Command.Infrastructure/Eroad.FleetManagement.Command.Infrastructure.csproj", "Eroad.FleetManagement.Command.Infrastructure/"]
COPY ["Eroad.FleetManagement.Common/Eroad.FleetManagement.Common.csproj", "Eroad.FleetManagement.Common/"]
COPY ["Eroad.Common/Eroad.Common.csproj", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/Eroad.CQRS.Core.csproj", "Eroad.CQRS.Core/"]

# Restore dependencies
RUN dotnet restore "Eroad.FleetManagement.Command.API/Eroad.FleetManagement.Command.API.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/Eroad.FleetManagement.Command.API"
RUN dotnet build "Eroad.FleetManagement.Command.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Eroad.FleetManagement.Command.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eroad.FleetManagement.Command.API.dll"]
