// Application Insights and Log Analytics workspace module
// Provides centralized observability for all services per Infrastructure blueprint.

@minLength(1)
param environmentName string
@minLength(1)
param location string
param tags object = {}

@description('Retention in days for Log Analytics. Use 90 for non-prod, 365 for prod.')
param logAnalyticsRetentionDays int = 90

@description('Application Insights name override. If empty, derived from abbreviations.')
param applicationInsightsName string = ''
@description('Log Analytics workspace name override.')
param logAnalyticsName string = ''

var abbrs = loadJsonContent('../abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var appInsightsName = !empty(applicationInsightsName) ? applicationInsightsName : '${abbrs.insightsComponents}${resourceToken}'
var workspaceName = !empty(logAnalyticsName) ? logAnalyticsName : '${abbrs.operationalInsightsWorkspaces}${resourceToken}'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: workspaceName
  location: location
  tags: tags
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: logAnalyticsRetentionDays
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: appInsightsName
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
    IngestionMode: 'LogAnalytics'
    SamplingPercentage: 100
  }
}

output applicationInsightsName string = appInsights.name
output applicationInsightsResourceId string = appInsights.id
output applicationInsightsConnectionString string = appInsights.properties.ConnectionString
output applicationInsightsInstrumentationKey string = appInsights.properties.InstrumentationKey
output workspaceId string = logAnalytics.properties.customerId
output workspaceName string = logAnalytics.name
