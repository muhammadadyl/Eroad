FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Eroad.RouteManagement.Query.API/Eroad.RouteManagement.Query.API.csproj", "Eroad.RouteManagement.Query.API/"]
COPY ["Eroad.RouteManagement.Query.Domain/Eroad.RouteManagement.Query.Domain.csproj", "Eroad.RouteManagement.Query.Domain/"]
COPY ["Eroad.RouteManagement.Query.Infrastructure/Eroad.RouteManagement.Query.Infrastructure.csproj", "Eroad.RouteManagement.Query.Infrastructure/"]
COPY ["Eroad.RouteManagement.Common/Eroad.RouteManagement.Common.csproj", "Eroad.RouteManagement.Common/"]
COPY ["Eroad.Common/Eroad.Common.csproj", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/Eroad.CQRS.Core.csproj", "Eroad.CQRS.Core/"]

# Restore dependencies
RUN dotnet restore "Eroad.RouteManagement.Query.API/Eroad.RouteManagement.Query.API.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/Eroad.RouteManagement.Query.API"
RUN dotnet build "Eroad.RouteManagement.Query.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Eroad.RouteManagement.Query.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eroad.RouteManagement.Query.API.dll"]
