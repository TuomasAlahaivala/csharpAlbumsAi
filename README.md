# Music Albums Web Application

ASP.NET Core 10 JSON API with a React and TypeScript frontend for tracking music albums. Album data is stored in PostgreSQL, and pending Entity Framework Core migrations are applied when the API starts.

## Run locally

Install the .NET 10 SDK, Node.js, and Docker. Generate the admin credentials once, then start PostgreSQL and the API:

```powershell
dotnet run -- --initialize-admin
docker compose up -d
dotnet run
```

The initializer creates a random `admin-...@example.com` email and a strong password, then stores the email and an ASP.NET Core password hash in the git-ignored `.env` file. The password is printed only once; save it because it cannot be recovered from the hash. Keep the `.env` file private and do not commit or share it. Existing `.env` files are never overwritten by the initializer.

The app's album pages and API require sign-in. Use the generated credentials at the sign-in screen; the session is held in an HTTP-only cookie and expires after eight hours. In production, serve the app over HTTPS and provide `ADMIN_EMAIL` and `ADMIN_PASSWORD_HASH` through the deployment's secret configuration instead of a local `.env` file.

In a second terminal, install frontend dependencies and start Vite:

```powershell
cd ClientApp
npm install
npm run dev
```

Open `http://localhost:5173`. Vite proxies `/api` to `http://localhost:5000` by default. If ASP.NET uses another address, set `ASPNETCORE_URLS` to that address before starting Vite.

The default development database is `localhost:5433`, database `albums`, user `albums`, and password `albums_dev`. Override it with:

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

## Azure production deployment

The production deployment uses Azure Container Apps for the web application and Azure Database for PostgreSQL Flexible Server for data. The PostgreSQL server is reachable only through a private VNet. Azure Key Vault stores the database connection string and admin credentials; the app reads them through a user-assigned managed identity. Container images are stored in Azure Container Registry. Container Apps sends application logs to Log Analytics and exposes HTTPS ingress with startup, liveness, and database-readiness probes.

The included baseline is a single-region setup: PostgreSQL `Standard_D2s_v3`, 14-day backups, no database high availability, and one always-on Container Apps replica (scaling to three). Review Azure pricing and your availability/recovery requirements before using it for a critical service. The template creates the VNet address range `10.42.0.0/16`; choose a non-overlapping range if your Azure networks already use it.

### Provision Azure resources

Install and sign in to the [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli), select the subscription for this deployment, ensure it permits the Container Apps, PostgreSQL, Container Registry, Key Vault, Managed Identity, Network, and Log Analytics resource providers, then run these commands in PowerShell. Use an Azure region that supports all of these services:

```powershell
az login
az account set --subscription "<subscription-id>"

$resourceGroup = "rg-albums-prod"
$resourcePrefix = "albumsprod"
$location = "swedencentral"
$random = [Security.Cryptography.RandomNumberGenerator]::Create()
$databasePasswordBytes = New-Object "System.Byte[]" 36
try {
    $random.GetBytes($databasePasswordBytes)
} finally {
    $random.Dispose()
}
$databasePassword = [Convert]::ToBase64String($databasePasswordBytes).TrimEnd([char]"=").Replace("+", "a").Replace("/", "b")

az group create --name $resourceGroup --location $location
az deployment group create `
    --name albums-production `
    --resource-group $resourceGroup `
    --template-file infra/main.bicep `
    --parameters "resourcePrefix=$resourcePrefix" "location=$location" "databaseAdminPassword=$databasePassword"
```

Keep `$databasePassword` private and available until you have stored the connection string in Key Vault. The template also accepts `databaseSkuName`, `virtualNetworkAddressPrefix`, `containerAppsSubnetPrefix`, and `postgresSubnetPrefix` parameters; the Container Apps subnet must be at least `/23`. The deployment identity needs permission to create resources and role assignments in the resource group.

Give your signed-in Azure user the Key Vault Secrets Officer role on the new vault, then store the database connection string. The database is not publicly accessible:

```powershell
$outputs = az deployment group show `
    --name albums-production `
    --resource-group $resourceGroup `
    --query properties.outputs `
    --output json | ConvertFrom-Json
$vaultName = $outputs.keyVaultName.value
$vaultId = az keyvault show --name $vaultName --query id --output tsv
$userObjectId = az ad signed-in-user show --query id --output tsv

az role assignment create `
    --assignee-object-id $userObjectId `
    --assignee-principal-type User `
    --role "Key Vault Secrets Officer" `
    --scope $vaultId

$databaseHost = $outputs.postgresHostName.value
$connectionString = "Host=$databaseHost;Database=albums;Username=albumsadmin;Password=$databasePassword;SSL Mode=VerifyFull"
az keyvault secret set --vault-name $vaultName --name albums-db-connection-string --value $connectionString --query id --output tsv
Remove-Variable databasePassword, connectionString
Remove-Variable databasePasswordBytes, random
```

Allow time for Azure role assignments to propagate before continuing.

### Store the admin credentials

Generate the admin credentials locally if you do not already have the git-ignored `.env` file:

```powershell
dotnet run -- --initialize-admin
```

Store the email and password hash in Key Vault; the plaintext password is only printed by the initializer and should be saved securely:

```powershell
$adminCredentials = @{}
Get-Content .env | ForEach-Object {
    if ($_ -match "^([^=]+)=(.*)$") {
        $adminCredentials[$matches[1]] = $matches[2]
    }
}

$adminEmail = $adminCredentials["ADMIN_EMAIL"]
$adminPasswordHash = $adminCredentials["ADMIN_PASSWORD_HASH"]
az keyvault secret set --vault-name $vaultName --name admin-email --value $adminEmail --query id --output tsv
az keyvault secret set --vault-name $vaultName --name admin-password-hash --value $adminPasswordHash --query id --output tsv
Remove-Variable adminCredentials, adminEmail, adminPasswordHash
```

The Container Apps identity receives read access to these secrets and pull access to the private ACR. Do not put the database password, admin password, or password hash in GitHub variables or source control.

### Configure GitHub Actions deployment

The `Deploy to Azure` workflow tests the .NET and frontend projects, builds and pushes a container image, then creates or updates the Container App when code is pushed to `master`. It authenticates to Azure using GitHub OIDC rather than a stored Azure password.

1. In Microsoft Entra ID, create an app registration and service principal for GitHub Actions. Add a federated credential with issuer `https://token.actions.githubusercontent.com`, audience `api://AzureADTokenExchange`, and subject `repo:OWNER/REPOSITORY:environment:production` (replace the owner and repository).
2. Assign that service principal the **Container Apps Contributor** role on the deployment resource group and **AcrPush** on the new ACR. The deployment identity is privileged; protect the GitHub environment and limit who can deploy.

   ```powershell
   $githubClientId = "<app-registration-client-id>"
   $githubPrincipalId = az ad sp show --id $githubClientId --query id --output tsv
   $resourceGroupId = az group show --name $resourceGroup --query id --output tsv
   $registryId = az acr show --name $outputs.containerRegistryName.value --query id --output tsv

   az role assignment create `
       --assignee-object-id $githubPrincipalId `
       --assignee-principal-type ServicePrincipal `
       --role "Container Apps Contributor" `
       --scope $resourceGroupId
   az role assignment create `
       --assignee-object-id $githubPrincipalId `
       --assignee-principal-type ServicePrincipal `
       --role AcrPush `
       --scope $registryId
   ```

3. In GitHub, create a `production` environment restricted to the `master` branch. Add these environment secrets: `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, and `AZURE_SUBSCRIPTION_ID`.
4. Add these environment variables, using the Bicep outputs where indicated:

   | Variable | Value |
   | --- | --- |
   | `AZURE_RESOURCE_GROUP` | `rg-albums-prod` |
   | `AZURE_CONTAINER_REGISTRY` | Bicep output `containerRegistryName` |
   | `AZURE_CONTAINER_APP` | Bicep output `containerAppName` |
   | `AZURE_CONTAINER_APP_ENVIRONMENT` | Bicep output `containerAppEnvironmentName` |
   | `AZURE_KEY_VAULT` | Bicep output `keyVaultName` |
   | `AZURE_WORKLOAD_IDENTITY_RESOURCE_ID` | Bicep output `workloadIdentityResourceId` |
   | `AZURE_CONTAINER_APPS_SUBNET_PREFIX` | Bicep output `containerAppsSubnetPrefix` |

After role assignments have propagated, run the workflow from the `master` branch or push a change to it. The resulting app URL is available with:

```powershell
az containerapp show `
    --name $outputs.containerAppName.value `
    --resource-group $resourceGroup `
    --query properties.configuration.ingress.fqdn `
    --output tsv
```

Azure provides HTTPS for the default Container Apps domain. For a custom domain, configure and validate the domain and certificate in Container Apps before switching DNS. Review the configured backup and high-availability settings against your recovery objectives; test database restore procedures before production use.

## Releases

Releases use [Conventional Commits](https://www.conventionalcommits.org/). Push or merge commits to `master` using `feat:` for features, `fix:` for fixes, and `perf:` for performance improvements. These produce minor or patch version bumps; a `!` after the type or a `BREAKING CHANGE:` footer produces a major bump. Other commit types such as `docs:` and `chore:` do not trigger a release.

The Release Please workflow opens or updates a release pull request with the next semantic version and generated `CHANGELOG.md` entry. Review and merge that pull request to create the corresponding GitHub Release and version tag. The current version is recorded in `version.txt` and is used by the .NET build.

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
