# build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ../../../.. ./
RUN dotnet restore ./src/system/appsettings/AppSettingsService/AppSettingsService.csproj
RUN dotnet publish ./src/system/appsettings/AppSettingsService/AppSettingsService.csproj -c Release -o /out

# runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out ./
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AppSettingsService.dll"]
