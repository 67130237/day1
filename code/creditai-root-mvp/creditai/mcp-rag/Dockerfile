FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY src/Mcp.Rag.Core/Mcp.Rag.Core.csproj src/Mcp.Rag.Core/
COPY src/Mcp.Rag.Api/Mcp.Rag.Api.csproj src/Mcp.Rag.Api/
RUN dotnet restore src/Mcp.Rag.Api/Mcp.Rag.Api.csproj
COPY . .
RUN dotnet publish src/Mcp.Rag.Api/Mcp.Rag.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Mcp.Rag.Api.dll"]
