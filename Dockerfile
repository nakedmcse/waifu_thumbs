FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS base
USER root
WORKDIR /app
EXPOSE 5030:8080

# Required OS packages
RUN apk add --no-cache git pkgconf vips-dev vips ffmpeg

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Thumbnails.csproj", "./"]
RUN dotnet restore "Thumbnails.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "Thumbnails.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Thumbnails.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Thumbnails.dll"]
