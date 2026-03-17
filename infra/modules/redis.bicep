// Azure Cache for Redis module for distributed caching and rate limiting.
// Connection string should be stored in Key Vault via init-secrets or post-deploy.

@minLength(1)
param environmentName string
@minLength(1)
param location string
param tags object = {}

@description('SKU: Basic for dev, Standard for staging/prod.')
param sku string = 'Basic'
@description('Family: C for Basic/Standard.')
param family string = 'C'
@description('Capacity: 0 for Basic C0, 1+ for Standard.')
param capacity int = 0

@description('Redis name override.')
param redisName string = ''

var abbrs = loadJsonContent('../abbreviations.json')
var resourceToken = toLower(uniqueString(subscription().id, environmentName, location))
var name = !empty(redisName) ? redisName : '${abbrs.cacheRedis}${resourceToken}'

resource redis 'Microsoft.Cache/redis@2023-08-01' = {
  name: name
  location: location
  tags: tags
  properties: {
    sku: { name: sku, family: family, capacity: capacity }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
    publicNetworkAccess: 'Enabled'
    redisConfiguration: {}
  }
}

output name string = redis.name
output hostName string = redis.properties.hostName
output port int = redis.properties.port
output sslPort int = redis.properties.sslPort
@secure()
output primaryKey string = redis.listKeys().primaryKey
@secure()
output connectionString string = '${redis.properties.hostName}:${redis.properties.sslPort},password=${redis.listKeys().primaryKey},ssl=True,abortConnect=False'
output resourceId string = redis.id
