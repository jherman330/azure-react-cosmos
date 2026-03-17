// RBAC role assignments module. Centralizes Key Vault role assignments (AC-FOUNDATION-019.4).
// Cosmos DB Data Contributor is assigned in app/cosmos-role-assignment.bicep to avoid circular dependency.

@description('Principal ID of the App Service managed identity.')
param appServicePrincipalId string = ''

@description('Principal ID of the deployment principal (for initial Key Vault secret seeding).')
param deploymentPrincipalId string = ''

@description('Key Vault name (must exist in same resource group; used for scope via existing reference).')
param keyVaultName string = ''

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource appServiceKeyVaultSecretsUser 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(appServicePrincipalId) && !empty(keyVaultName)) {
  name: guid(keyVault.id, appServicePrincipalId, '4633458b-17de-408a-b874-0445c86b69e6')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
    principalId: appServicePrincipalId
    principalType: 'ServicePrincipal'
  }
}

resource deploymentKeyVaultSecretsOfficer 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (!empty(deploymentPrincipalId) && !empty(keyVaultName)) {
  name: guid(keyVault.id, deploymentPrincipalId, 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'b86a8fe4-44ce-4948-aee5-eccb2c155cd7')
    principalId: deploymentPrincipalId
    principalType: 'ServicePrincipal'
  }
}

output appServiceKeyVaultAssignmentId string = appServiceKeyVaultSecretsUser.id
output deploymentOfficerAssignmentId string = deploymentKeyVaultSecretsOfficer.id
