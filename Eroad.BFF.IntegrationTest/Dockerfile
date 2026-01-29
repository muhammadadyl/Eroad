# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy only the test project file
COPY ["Eroad.BFF.IntegrationTest/Eroad.BFF.IntegrationTest.csproj", "Eroad.BFF.IntegrationTest/"]

# Restore dependencies
RUN dotnet restore "Eroad.BFF.IntegrationTest/Eroad.BFF.IntegrationTest.csproj"

# Copy only the test source code
COPY ["Eroad.BFF.IntegrationTest/", "Eroad.BFF.IntegrationTest/"]

# Set working directory and run tests directly (no separate build needed)
WORKDIR "/src/Eroad.BFF.IntegrationTest"

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Entry point runs the tests
ENTRYPOINT ["dotnet", "test", "Eroad.BFF.IntegrationTest.csproj", "-c", "Release", "--logger:console;verbosity=detailed"]
