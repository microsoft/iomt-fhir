param baseName string
param location string 

resource storageAccount 'Microsoft.Storage/storageAccounts@2021-02-01' = {
  name: '${baseName}sa'
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_RAGRS'
  }
  properties: {}
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
  sku: {
    name: 'Standard'
    tier: 'Standard'
    capacity: 2
  }
  properties: {
    zoneRedundant: true
    isAutoInflateEnabled: true
    maximumThroughputUnits: 8
    kafkaEnabled: false
    disableLocalAuth: true
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
// resource fhirService 'Microsoft.HealthcareApis/services@2021-11-01' = {
//   name: 'fs-${baseName}'
//   location: location
//   kind: 'fhir-R4'
//   identity: {
//     type: 'SystemAssigned'
//   }
//   properties: {
//     authenticationConfiguration: {
//       audience: 'https://${baseName}.azurehealthcareapis.com'
//       authority: uri(environment().authentication.loginEndpoint, subscription().tenantId)
//       smartProxyEnabled: false
//     }
//   }
// }

// var iotCentralName = 'iotc-${baseName}'
// param iotLocation string 

// resource iotCentral 'Microsoft.IoTCentral/iotApps@2021-06-01' = {
//   name: iotCentralName
//   location: iotLocation
//   sku: {
//     name: 'ST2'
//   }
//   properties: {
//     displayName: iotCentralName
//     subdomain: iotCentralName
//     template: 'iotc-patient@1.0.0'
//   }
// }

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' = {
  name: '${baseName}acr'
  location: location
  sku: {
    name:'Standard'
  }
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'law-${baseName}'
  location: location
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
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId:logAnalyticsWorkspace.id
  }
}
