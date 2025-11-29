# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ./NotificationPublisher.csproj NotificationPublisher/
RUN dotnet restore NotificationPublisher/NotificationPublisher.csproj

COPY . NotificationPublisher/

RUN dotnet publish NotificationPublisher/NotificationPublisher.csproj -c Release -o /app/publish

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .
ENV DOTNET_RUNNING_IN_CONTAINER=true

ENTRYPOINT ["dotnet", "NotificationPublisher.dll"]
