// Optional Azure API Management module (when useAPIM is true).
// Mediates frontend-to-backend API with rate limiting, CORS, and diagnostics.

@minLength(1)
param environmentName string
@minLength(1)
param location string
param tags object = {}

@description('APIM SKU: Consumption or Developer.')
param apimSku string = 'Consumption'

@description('APIM service name override.')
param apimServiceName string = ''

var abbrs = loadJsonContent('../abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var name = !empty(apimServiceName) ? apimServiceName : '${abbrs.apiManagementService}${resourceToken}'

resource apim 'Microsoft.ApiManagement/service@2022-08-01' = {
  name: name
  location: location
  tags: tags
  sku: { name: apimSku, capacity: apimSku == 'Developer' ? 1 : 0 }
  properties: {
    publisherEmail: 'noreply@example.com'
    publisherName: 'Contoso'
    notificationSenderEmail: 'noreply@example.com'
  }
}

output name string = apim.name
output gatewayUrl string = 'https://${apim.properties.gatewayUrl}'
output managementUrl string = apim.properties.managementApiUrl
output resourceId string = apim.id
