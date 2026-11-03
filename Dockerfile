FROM node:24-bookworm-slim AS frontend
WORKDIR /src/ClientApp
COPY ClientApp/package.json ClientApp/package-lock.json ./
RUN npm ci
COPY ClientApp/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS publish
WORKDIR /src
COPY AlbumsWebApp.csproj version.txt ./
RUN dotnet restore AlbumsWebApp.csproj
COPY . ./
COPY --from=frontend /src/wwwroot ./wwwroot
RUN dotnet publish AlbumsWebApp.csproj --configuration Release --output /app/publish --no-restore -p:SkipClientBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
COPY --from=publish /app/publish ./
USER $APP_UID
ENTRYPOINT ["dotnet", "AlbumsWebApp.dll"]
