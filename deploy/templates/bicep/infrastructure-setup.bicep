param baseName string 
param location string 

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${baseName}sa'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity__Create'
    IomtFhirVersion: 'R4'
  }
  kind: 'StorageV2'
  sku: {
    name: 'Standard_RAGRS'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2022-09-01' = {
    name: 'default'
    parent: storageAccount
}

resource checkpointContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
    parent: blobService
    name: 'checkpoint'
    properties: {
        publicAccess: 'None'
        metadata: {}
    }
}

resource templateContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2022-09-01' = {
    parent: blobService
    name: 'template'
    properties: {
        publicAccess: 'None'
        metadata: {}
    }
}

resource eventhubNamespace 'Microsoft.EventHub/namespaces@2021-11-01' = {
  name: 'en-${baseName}'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity__Create'
    IomtFhirVersion: 'R4'
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
}

resource fhirService 'Microsoft.HealthcareApis/workspaces/fhirservices@2023-02-28' = {
  name: 'fs-${baseName}'
  parent: healthWorkspace
  location: location
  kind: 'fhir-R4'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    authenticationConfiguration: {
      authority: '${environment().authentication.loginEndpoint}${subscription().tenantId}'
      audience: 'https://fs-${baseName}.fhir.azurehealthcareapis.com'
      smartProxyEnabled: false
    }
  }
}

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' = {
  name: '${baseName}acr'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity__Create'
    IomtFhirVersion: 'R4'
  }
  identity: {
    type: 'SystemAssigned' //maybe? 
  }
  sku: {
    name:'Standard'
  }
  properties: {
    adminUserEnabled: true
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'law-${baseName}'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity__Create'
    IomtFhirVersion: 'R4'
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
  name: 'ai-${baseName}'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity__Create'
    IomtFhirVersion: 'R4'
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

var acrPushRoleId = resourceId('Microsoft.Authorization/roleDefinitions', '8311e382-0749-4cb8-b61a-304f252e45ec')

resource acrPushRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(acrPushRoleId, userAssignedMI.id, containerRegistry.id)
  properties: {
    roleDefinitionId: acrPushRoleId
    principalId: userAssignedMI.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

var acrPullRoleId = resourceId('Microsoft.Authorization/roleDefinitions', '7f951dda-4ed3-4680-a7ca-43fe172d538d')

resource acrPullRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(acrPullRoleId, userAssignedMI.id, containerRegistry.id)
  properties: {
    roleDefinitionId: acrPullRoleId
    principalId: userAssignedMI.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

var readerId = resourceId('Microsoft.Authorization/roleDefinitions', 'acdd72a7-3385-48ef-bd42-f606fba81ae7')

resource readerRole 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: containerRegistry
  name: guid(readerId, userAssignedMI.id, containerRegistry.id)
  properties: {
    roleDefinitionId: readerId
    principalId: userAssignedMI.properties.principalId
    principalType: 'ServicePrincipal'
  }
}
