# Use the official .NET 9.0 runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 10000

# Use the SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["PlaylistTransferAPI.csproj", "./"]
RUN dotnet restore "PlaylistTransferAPI.csproj"
COPY . .
WORKDIR "/src"
RUN dotnet build "PlaylistTransferAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlaylistTransferAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create logs directory
RUN mkdir -p /app/logs

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:10000

ENTRYPOINT ["dotnet", "PlaylistTransferAPI.dll"]