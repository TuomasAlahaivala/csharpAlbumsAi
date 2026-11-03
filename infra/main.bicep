@description('Short lowercase prefix used for Azure resource names.')
@minLength(3)
@maxLength(12)
param resourcePrefix string = 'albumsprod'

@description('Azure region for the application and database.')
param location string = resourceGroup().location

@description('PostgreSQL administrator login.')
param databaseAdminLogin string = 'albumsadmin'

@description('PostgreSQL administrator password. Keep this value out of source control.')
@secure()
param databaseAdminPassword string

@description('PostgreSQL Flexible Server SKU. Choose a production SKU appropriate for your workload.')
param databaseSkuName string = 'Standard_D2s_v3'

@description('Virtual network address space. Choose a range that does not overlap your existing networks.')
param virtualNetworkAddressPrefix string = '10.42.0.0/16'

@description('Container Apps infrastructure subnet. Container Apps requires a dedicated /23 or larger subnet.')
param containerAppsSubnetPrefix string = '10.42.0.0/23'

@description('Dedicated PostgreSQL delegated subnet.')
param postgresSubnetPrefix string = '10.42.2.0/24'

var suffix = uniqueString(subscription().id, resourceGroup().id)
var normalizedPrefix = replace(toLower(resourcePrefix), '-', '')
var databaseServerName = '${toLower(resourcePrefix)}-${suffix}'
var registryName = '${take(normalizedPrefix, 20)}${suffix}'
var keyVaultName = '${take(normalizedPrefix, 8)}${suffix}'
var privateDnsZoneName = 'private.${suffix}.postgres.database.azure.com'
var containerAppEnvironmentName = '${resourcePrefix}-env'
var workloadIdentityName = '${resourcePrefix}-identity'
var logAnalyticsName = '${resourcePrefix}-logs'
var acrPullRoleId = '7f951dda-4ed3-4680-a7ca-43fe172d538d'
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource virtualNetwork 'Microsoft.Network/virtualNetworks@2024-01-01' = {
  name: '${resourcePrefix}-vnet'
  location: location
  properties: {
    addressSpace: {
      addressPrefixes: [
        virtualNetworkAddressPrefix
      ]
    }
    subnets: [
      {
        name: 'container-apps'
        properties: {
          addressPrefix: containerAppsSubnetPrefix
          delegations: [
            {
              name: 'container-apps'
              properties: {
                serviceName: 'Microsoft.App/environments'
              }
            }
          ]
        }
      }
      {
        name: 'postgres'
        properties: {
          addressPrefix: postgresSubnetPrefix
          delegations: [
            {
              name: 'postgres'
              properties: {
                serviceName: 'Microsoft.DBforPostgreSQL/flexibleServers'
              }
            }
          ]
        }
      }
    ]
  }
}

resource privateDnsZone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: privateDnsZoneName
  location: 'global'
}

resource privateDnsLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  parent: privateDnsZone
  name: '${resourcePrefix}-vnet-link'
  location: 'global'
  properties: {
    registrationEnabled: false
    virtualNetwork: {
      id: virtualNetwork.id
    }
  }
}

resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: databaseServerName
  location: location
  sku: {
    name: databaseSkuName
    tier: startsWith(databaseSkuName, 'Standard_B') ? 'Burstable' : 'GeneralPurpose'
  }
  properties: {
    administratorLogin: databaseAdminLogin
    administratorLoginPassword: databaseAdminPassword
    version: '16'
    backup: {
      backupRetentionDays: 14
      geoRedundantBackup: 'Disabled'
    }
    highAvailability: {
      mode: 'Disabled'
    }
    network: {
      delegatedSubnetResourceId: resourceId('Microsoft.Network/virtualNetworks/subnets', virtualNetwork.name, 'postgres')
      privateDnsZoneArmResourceId: privateDnsZone.id
      publicNetworkAccess: 'Disabled'
    }
    storage: {
      storageSizeGB: 32
      autoGrow: 'Enabled'
    }
  }
  dependsOn: [
    privateDnsLink
  ]
}

resource albumsDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: postgres
  name: 'albums'
  properties: {
    charset: 'UTF8'
    collation: 'en_US.utf8'
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location
  properties: {
    tenantId: tenant().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enablePurgeProtection: true
    publicNetworkAccess: 'Enabled'
  }
}

resource registry 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: registryName
  location: location
  sku: {
    name: 'Basic'
  }
  properties: {
    adminUserEnabled: false
    publicNetworkAccess: 'Enabled'
  }
}

resource workloadIdentity 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: workloadIdentityName
  location: location
}

resource acrPullAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(registry.id, workloadIdentity.id, acrPullRoleId)
  scope: registry
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', acrPullRoleId)
    principalId: workloadIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource keyVaultSecretsUserAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, workloadIdentity.id, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: workloadIdentity.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    retentionInDays: 30
    sku: {
      name: 'PerGB2018'
    }
  }
}

resource containerAppEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: containerAppEnvironmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
    vnetConfiguration: {
      infrastructureSubnetId: resourceId('Microsoft.Network/virtualNetworks/subnets', virtualNetwork.name, 'container-apps')
    }
  }
}

output resourceGroupName string = resourceGroup().name
output containerRegistryName string = registry.name
output containerRegistryLoginServer string = registry.properties.loginServer
output keyVaultName string = keyVault.name
output containerAppEnvironmentName string = containerAppEnvironment.name
output containerAppName string = '${resourcePrefix}-app'
output workloadIdentityResourceId string = workloadIdentity.id
output postgresServerName string = postgres.name
output postgresHostName string = postgres.properties.fullyQualifiedDomainName
output containerAppsSubnetPrefix string = containerAppsSubnetPrefix
