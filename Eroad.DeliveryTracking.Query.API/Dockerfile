FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["Eroad.DeliveryTracking.Query.API/Eroad.DeliveryTracking.Query.API.csproj", "Eroad.DeliveryTracking.Query.API/"]
COPY ["Eroad.DeliveryTracking.Query.Domain/Eroad.DeliveryTracking.Query.Domain.csproj", "Eroad.DeliveryTracking.Query.Domain/"]
COPY ["Eroad.DeliveryTracking.Query.Infrastructure/Eroad.DeliveryTracking.Query.Infrastructure.csproj", "Eroad.DeliveryTracking.Query.Infrastructure/"]
COPY ["Eroad.DeliveryTracking.Common/Eroad.DeliveryTracking.Common.csproj", "Eroad.DeliveryTracking.Common/"]
COPY ["Eroad.Common/Eroad.Common.csproj", "Eroad.Common/"]
COPY ["Eroad.CQRS.Core/Eroad.CQRS.Core.csproj", "Eroad.CQRS.Core/"]

# Restore dependencies
RUN dotnet restore "Eroad.DeliveryTracking.Query.API/Eroad.DeliveryTracking.Query.API.csproj"

# Copy all source files
COPY . .

# Build the project
WORKDIR "/src/Eroad.DeliveryTracking.Query.API"
RUN dotnet build "Eroad.DeliveryTracking.Query.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Eroad.DeliveryTracking.Query.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Eroad.DeliveryTracking.Query.API.dll"]
