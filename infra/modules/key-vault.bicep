// Azure Key Vault module with RBAC authorization.
// AC-FOUNDATION-019.3/019.4: Managed identities and RBAC role assignments for secret access.

@minLength(1)
param environmentName string
@minLength(1)
param location string
param tags object = {}

@description('Key Vault name override. If empty, derived from abbreviations.')
param keyVaultName string = ''

@description('Enable purge protection (recommended for production).')
param enablePurgeProtection bool = false

var abbrs = loadJsonContent('../abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var vaultName = !empty(keyVaultName) ? keyVaultName : '${abbrs.keyVaultVaults}${resourceToken}'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: vaultName
  location: location
  tags: tags
  properties: {
    sku: { family: 'A', name: 'standard' }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enablePurgeProtection: enablePurgeProtection
    enabledForDeployment: false
    enabledForTemplateDeployment: false
    networkAcls: {
      defaultAction: 'Allow'
      bypass: 'AzureServices'
    }
  }
}

// Grant Key Vault Secrets User to the provided principal (e.g. App Service) so it can read secrets at runtime.
// Deployment principal should get Key Vault Secrets Officer via rbac.bicep for initial secret seeding.
output name string = keyVault.name
output uri string = keyVault.properties.vaultUri
output resourceId string = keyVault.id
