# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["HouseholdNeedsManager.csproj", "./"]
RUN dotnet restore "HouseholdNeedsManager.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "HouseholdNeedsManager.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "HouseholdNeedsManager.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HouseholdNeedsManager.dll"]
