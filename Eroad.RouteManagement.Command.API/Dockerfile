FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Eroad.RouteManagement.Command.API/Eroad.RouteManagement.Command.API.csproj", "Eroad.RouteManagement.Command.API/"]
COPY ["Eroad.RouteManagement.Command.Domain/Eroad.RouteManagement.Command.Domain.csproj", "Eroad.RouteManagement.Command.Domain/"]
COPY ["Eroad.RouteManagement.Command.Infrastructure/Eroad.RouteManagement.Command.Infrastructure.csproj", "Eroad.RouteManagement.Command.Infrastructure/"]
COPY ["Eroad.RouteManagement.Common/Eroad.RouteManagement.Common.csproj", "Eroad.RouteManagement.Common/"]
COPY ["Eroad.Common/Eroad.Common.csproj", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/Eroad.CQRS.Core.csproj", "Eroad.CQRS.Core/"]

# Restore dependencies
RUN dotnet restore "Eroad.RouteManagement.Command.API/Eroad.RouteManagement.Command.API.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/Eroad.RouteManagement.Command.API"
RUN dotnet build "Eroad.RouteManagement.Command.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Eroad.RouteManagement.Command.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eroad.RouteManagement.Command.API.dll"]
