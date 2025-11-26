# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ./RoutingServer.csproj RoutingServer/
RUN dotnet restore RoutingServer/RoutingServer.csproj

COPY . RoutingServer/

RUN dotnet publish RoutingServer/RoutingServer.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "RoutingServer.dll"]
