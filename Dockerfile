# One container for the whole site: the Angular app is built and served by the ASP.NET Core API
# from wwwroot, so the browser and API share one origin (no CORS needed). Used by render.yaml.

# 1. Build the Angular app
FROM node:22-alpine AS frontend
WORKDIR /src/frontend
COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci --no-audit --no-fund
COPY frontend/ ./
RUN npx ng build --configuration production

# 2. Publish the API (the legacy .NET Framework projects are not part of SkirtPling.sln)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS backend
WORKDIR /src/backend
COPY backend/ ./
RUN dotnet publish SkirtPling.Api/SkirtPling.Api.csproj -c Release -o /app/publish

# 3. Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=backend /app/publish ./
COPY --from=frontend /src/frontend/dist/frontend/browser ./wwwroot
ENV ASPNETCORE_ENVIRONMENT=Production \
    ASPNETCORE_HTTP_PORTS=10000
EXPOSE 10000
USER $APP_UID
ENTRYPOINT ["dotnet", "SkirtPling.Api.dll"]
