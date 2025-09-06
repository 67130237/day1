FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/Mcp.Sql.Core/Mcp.Sql.Core.csproj src/Mcp.Sql.Core/
COPY src/Mcp.Sql.Api/Mcp.Sql.Api.csproj src/Mcp.Sql.Api/
RUN dotnet restore src/Mcp.Sql.Api/Mcp.Sql.Api.csproj
COPY . .
RUN dotnet publish src/Mcp.Sql.Api/Mcp.Sql.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Mcp.Sql.Api.dll"]
