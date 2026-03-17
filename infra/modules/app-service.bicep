// App Service Plan and App Service module for the backend API.
// Configures system-assigned managed identity, HTTPS only, TLS 1.2, App Insights, Key Vault references.

@minLength(1)
param environmentName string
@minLength(1)
param location string
param tags object = {}

@description('Application Insights resource ID for diagnostics.')
param applicationInsightsResourceId string

@description('Key Vault name for Key Vault endpoint in app settings.')
param keyVaultName string = ''

@description('App Service Plan SKU: B1 for dev, S1 for staging/prod.')
param appServicePlanSku object = { name: 'B1', tier: 'Basic', capacity: 1 }

@description('App Service name override.')
param appServiceName string = ''

@description('Additional app settings (Key Vault references can use @Microsoft.KeyVault(VaultName=...)).')
param additionalAppSettings object = {}

param allowedOrigins array = []

var abbrs = loadJsonContent('../abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var planName = '${abbrs.webServerFarms}${resourceToken}'
var siteName = !empty(appServiceName) ? appServiceName : '${abbrs.webSitesAppService}api-${resourceToken}'

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
  name: planName
  location: location
  tags: tags
  sku: appServicePlanSku
  kind: 'linux'
  properties: {
    reserved: true
  }
}

resource appService 'Microsoft.Web/sites@2022-09-01' = {
  name: siteName
  location: location
  tags: union(tags, { 'azd-service-name': 'api' })
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|8.0'
      alwaysOn: true
      minTlsVersion: '1.2'
      ftpsState: 'Disabled'
      cors: {
        allowedOrigins: union(['https://portal.azure.com', 'https://ms.portal.azure.com'], allowedOrigins)
        supportCredentials: true
      }
    }
  }
}

// All app settings (Application Insights + Key Vault endpoint + parent-provided settings)
resource appSettingsResource 'Microsoft.Web/sites/config@2022-09-01' = {
  parent: appService
  name: 'appsettings'
  properties: union(
    {
      APPLICATIONINSIGHTS_CONNECTION_STRING: reference(applicationInsightsResourceId, '2020-02-02').ConnectionString
      APPINSIGHTS_INSTRUMENTATIONKEY: reference(applicationInsightsResourceId, '2020-02-02').InstrumentationKey
      ApplicationInsightsAgent_EXTENSION_VERSION: '~3'
      AZURE_KEY_VAULT_ENDPOINT: 'https://${keyVaultName}.${environment().suffixes.keyvaultDns}'
      ENABLE_ORYX_BUILD: 'true'
      SCM_DO_BUILD_DURING_DEPLOYMENT: 'false'
    },
    additionalAppSettings
  )
}

output appServiceName string = appService.name
output appServicePlanName string = appServicePlan.name
output principalId string = appService.identity.principalId
output defaultHostname string = appService.properties.defaultHostName
output resourceId string = appService.id
