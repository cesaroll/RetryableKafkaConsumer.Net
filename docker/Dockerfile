# Use official .NET SDK image for building the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

# ARG dotnet_build_log_level=q
ENV DOTNET_CLI_TELEMETRY_OPTOUT = 1

WORKDIR /app

# Copy project files and restore dependencies
COPY AppHost/*.csproj AppHost/
COPY aspire/ServiceDefaults/*.csproj aspire/ServiceDefaults/
COPY harness/Harness/*.csproj harness/Harness/
COPY lib/RetryableConsumer/*.csproj lib/RetryableConsumer/
COPY lib/RetryableConsumer.Abstractions/*.csproj lib/RetryableConsumer.Abstractions/

# Restore dependencies
WORKDIR /app/harness/Harness
RUN dotnet restore Harness.csproj

# Copy the rest of the source files
COPY . .

# Build the application
RUN dotnet publish harness/Harness/Harness.csproj -c Release -o /out

# Use a runtime image for final execution
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Harness.dll"]