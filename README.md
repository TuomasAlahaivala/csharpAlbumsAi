# Music Albums Web Application

ASP.NET Core 8 JSON API with a React and TypeScript frontend for tracking music albums. Album data is stored in PostgreSQL, and pending Entity Framework Core migrations are applied when the API starts.

## Run locally

Install the .NET 8 SDK, Node.js, and Docker. Start PostgreSQL and the API:

```powershell
docker compose up -d
dotnet run
```

In a second terminal, install frontend dependencies and start Vite:

```powershell
cd ClientApp
npm install
npm run dev
```

Open `http://localhost:5173`. Vite proxies `/api` to `http://localhost:5000` by default. If ASP.NET uses another address, set `ASPNETCORE_URLS` to that address before starting Vite.

The default development database is `localhost:5432`, database `albums`, user `albums`, and password `albums_dev`. Override it with:

```powershell
$env:ConnectionStrings__AlbumsDb = "Host=server;Port=5432;Database=albums;Username=user;Password=password"
dotnet run
```

## Checks

```powershell
dotnet test AlbumsWebApp.Tests
cd ClientApp
npm run lint
npm test
npm run build
```

The frontend production build is emitted to `wwwroot`. Publishing the ASP.NET application automatically runs `npm ci` and `npm run build` first:

```powershell
dotnet publish -c Release
```

The published ASP.NET application serves both `/api/albums` and the React SPA.

## Multi-agent development workflow

This workspace includes a Copilot workflow for implementing a feature or bug fix with separate coding, review, and testing roles. In VS Code Chat, run `/albums-workflow` and describe the task and acceptance criteria. The **Albums Workflow** coordinator delegates implementation to **Albums Coder**, requests an independent read-only review from **Albums Reviewer**, and then asks **Albums Tester** to run the relevant checks. Review findings or change-related test failures are sent back to the coder before the workflow completes.

The shared agent definitions and prompt are in `.github/agents/` and `.github/prompts/`. You can also select **Albums Workflow** directly from the agent picker.

## Database migrations

```powershell
dotnet ef migrations add MigrationName
dotnet ef database update
```

To reset local data and restore the two seeded albums:

```powershell
docker compose down --volumes
docker compose up -d
```
