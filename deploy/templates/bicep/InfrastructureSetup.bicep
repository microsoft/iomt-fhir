@minLength(6)
@maxLength(16)
@description('Basename that is used to name provisioned resources. Should be alphanumeric, at least 6 characters and less than 16 characters.')
param baseName string

@description('Location where the resources are deployed. For a list of Azure regions where Azure Health Data Services are available, see [Products available by regions](https://azure.microsoft.com/explore/global-infrastructure/products-by-region/?products=health-data-services)')
@allowed([
  'australiaeast'
  'canadacentral'
  'centralindia'
  'eastus'
  'eastus2'
  'francecentral'
  'japaneast'
  'koreacentral'
  'northcentralus'
  'northeurope'
  'qatarcentral'
  'southcentralus'
  'southeastasia'
  'swedencentral'
  'switzerlandnorth'
  'westcentralus'
  'westeurope'
  'westus2'
  'westus3'
  'uksouth'
])
param location string 

@description('Configures how patient, device, and other FHIR resource identities are resolved from the ingested data stream.')
@allowed([
  'Create'
  'Lookup'
  'LookupWithEncounter'
])
param resourceIdentityResolutionType string 

@description('FHIR version that the FHIR Server supports')
@allowed([
  'R4'
])
param fhirVersion string

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${baseName}sa'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
  }
  kind: 'StorageV2'
  sku: {
    name: 'Standard_RAGRS'
  }
}

// A Blob Service resource (default) is not explicitly provisioned to allow direct dependency of Container creation on the completion of tbe Storage Account deployment.
resource checkpointContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: '${baseName}sa/default/checkpoint'
  properties: {
      publicAccess: 'None'
      metadata: {}
  }
  dependsOn: [
    storageAccount
  ]
}

resource templateContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
  name: '${baseName}sa/default/template'
  properties: {
      publicAccess: 'None'
      metadata: {}
  }
  dependsOn: [
    storageAccount
  ]
}

resource eventhubNamespace 'Microsoft.EventHub/namespaces@2021-11-01' = {
  name: '${baseName}en'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
  }
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 2
  }
  properties: {
    zoneRedundant: false
    isAutoInflateEnabled: false
    kafkaEnabled: true
    disableLocalAuth: false
  }
}

resource devicedataEH 'Microsoft.EventHub/namespaces/eventhubs@2021-11-01' = {
  name: 'devicedata'
  parent: eventhubNamespace
  properties: {
    messageRetentionInDays: 1
    partitionCount: 32
    status: 'Active'
  }
}

resource normalizeddataEH 'Microsoft.EventHub/namespaces/eventhubs@2021-11-01' = {
  name: 'normalizeddata'
  parent: eventhubNamespace
  properties: {
    messageRetentionInDays: 1
    partitionCount: 32
    status: 'Active'
  }
}

resource devicedataEHAuthRule 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2021-11-01' = {
  name: 'devicedataSendAndListen'
  parent: devicedataEH
  properties: {
    rights: [
      'Send'
      'Listen'
    ]
  }
}

resource normalizeddataEHAuthRule 'Microsoft.EventHub/namespaces/eventhubs/authorizationRules@2021-11-01' = {
  name: 'normalizeddataSendAndListen'
  parent: normalizeddataEH
  properties: {
    rights: [
      'Send'
      'Listen'
    ]
  }
}

resource healthWorkspace 'Microsoft.HealthcareApis/workspaces@2023-02-28' = {
  name: '${baseName}hw'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
  }
}

resource fhirService 'Microsoft.HealthcareApis/workspaces/fhirservices@2023-02-28' = {
  name: '${baseName}fs'
  parent: healthWorkspace
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
  }
  kind: 'fhir-${fhirVersion}'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    authenticationConfiguration: {
      authority: '${environment().authentication.loginEndpoint}${subscription().tenantId}'
      audience: 'https://${healthWorkspace.name}-${baseName}fs.fhir.azurehealthcareapis.com'
      smartProxyEnabled: false
    }
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' = {
  name: '${baseName}acr'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
  }
  identity: {
    type: 'SystemAssigned' 
  }
  sku: {
    name:'Standard'
  }
  properties: {
    adminUserEnabled: true
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: '${baseName}law'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
  }
  properties: any({
    retentionInDays: 30
    features: {
      searchVersion: 1
    }
    sku: {
      name: 'PerGB2018'
    }
  })
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${baseName}ai'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
  }
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId:logAnalyticsWorkspace.id
  }
}

resource userAssignedMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${baseName}UAMI'
  location: location
}

var contributorId = resourceId('Microsoft.Authorization/roleDefinitions', 'b24988ac-6180-42a0-ab88-20f7382dd24c')
var acrPushRoleId = resourceId('Microsoft.Authorization/roleDefinitions', '8311e382-0749-4cb8-b61a-304f252e45ec')

resource contributorRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(contributorId, userAssignedMI.id, containerRegistry.id)
  properties: {
    roleDefinitionId: contributorId
    principalId: userAssignedMI.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

resource acrPushRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(acrPushRoleId, userAssignedMI.id, containerRegistry.id)
  properties: {
    roleDefinitionId: acrPushRoleId
    principalId: userAssignedMI.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
