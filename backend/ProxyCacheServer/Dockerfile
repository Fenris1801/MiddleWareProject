# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ./ProxyCacheServer.csproj ProxyCacheServer/
RUN dotnet restore ProxyCacheServer/ProxyCacheServer.csproj

COPY . ProxyCacheServer/

RUN dotnet publish ProxyCacheServer/ProxyCacheServer.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8733
EXPOSE 8733

ENTRYPOINT ["dotnet", "ProxyCacheServer.dll"]
