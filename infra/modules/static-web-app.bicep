// Azure Static Web Apps module for hosting the React frontend.
// Build configuration points to src/web; deployment token stored in Key Vault for CI/CD.

@minLength(1)
param environmentName string
@minLength(1)
param location string
param tags object = {}

@description('Source path for the app (e.g. src/web).')
param appLocation string = 'src/web'

@description('API location if applicable.')
param apiLocation string = ''

@description('Build output folder (e.g. dist).')
param outputLocation string = 'dist'

@description('SKU: Free or Standard.')
param sku string = 'Free'

@description('Static Web App name override.')
param staticWebAppName string = ''

var abbrs = loadJsonContent('../abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var name = !empty(staticWebAppName) ? staticWebAppName : '${abbrs.webStaticSites}${resourceToken}'

resource staticWebApp 'Microsoft.Web/staticSites@2022-09-01' = {
  name: name
  location: location
  tags: tags
  sku: { name: sku, tier: sku }
  properties: {
    repositoryUrl: ''
    branch: ''
    buildProperties: {
      appLocation: appLocation
      apiLocation: apiLocation
      outputLocation: outputLocation
    }
  }
}

// Deployment token available after creation via listSecrets(); retrieve with Azure CLI for CI/CD and store in Key Vault.
output name string = staticWebApp.name
output defaultHostname string = staticWebApp.properties.defaultHostname
output resourceId string = staticWebApp.id
