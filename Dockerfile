# syntax=docker/dockerfile:1
FROM node:22-alpine AS web
WORKDIR /src/frontend
COPY frontend/package.json ./
RUN npm install
COPY frontend/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS api-build
WORKDIR /src
COPY backend/CellErp.Api/CellErp.Api.csproj backend/CellErp.Api/
RUN dotnet restore backend/CellErp.Api/CellErp.Api.csproj
COPY backend/CellErp.Api/ backend/CellErp.Api/
COPY --from=web /src/frontend/dist/ backend/CellErp.Api/wwwroot/
RUN dotnet publish backend/CellErp.Api/CellErp.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=api-build /app/publish .
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet","CellErp.Api.dll"]
