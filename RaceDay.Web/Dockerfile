# syntax=docker/dockerfile:1

# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project file first so the restore layer is cached until dependencies change.
COPY RaceDay.Web.csproj ./
RUN dotnet restore RaceDay.Web.csproj

# Copy the rest of the source and publish a release build.
COPY . .
RUN dotnet publish RaceDay.Web.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Only the published output is copied: no SDK, no source code.
COPY --from=build /app/publish .

# Listen on 8080 (the .NET 8 container default) and run as the image's built-in non-root user.
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    ApiSettings__BaseUrl=http://host.docker.internal:5243/
EXPOSE 8080
USER $APP_UID

ENTRYPOINT ["dotnet", "RaceDay.Web.dll"]
