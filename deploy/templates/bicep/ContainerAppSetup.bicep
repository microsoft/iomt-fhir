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

param normalizationImage string = 'normalization'
param fhirTransformationImage string = 'fhir-transformation'
param imageTag string = 'latest'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: '${baseName}acr'
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: '${baseName}law'
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: '${baseName}ai'
}

resource eventhubNamespace 'Microsoft.EventHub/namespaces@2021-11-01' existing = {
  name: '${baseName}en'
}

resource devicedataEH 'Microsoft.EventHub/namespaces/eventhubs@2021-11-01' existing = {
  name: 'devicedata'
  parent: eventhubNamespace
}

resource normalizeddataEH 'Microsoft.EventHub/namespaces/eventhubs@2021-11-01' existing = {
  name: 'normalizeddata'
  parent: eventhubNamespace
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: '${baseName}sa'
}

resource healthWorkspace 'Microsoft.HealthcareApis/workspaces@2023-02-28' existing = {
  name: '${baseName}hw'
}

resource fhirService 'Microsoft.HealthcareApis/workspaces/fhirservices@2023-02-28' existing = {
  name: '${baseName}fs'
  parent: healthWorkspace
}

resource userAssignedMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: '${baseName}UAMI'
}

var fhirURL = 'https://${healthWorkspace.name}-${fhirService.name}.fhir.azurehealthcareapis.com'

resource containerAppEnv 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: '${baseName}env'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
  }
  properties: {
    daprAIInstrumentationKey:appInsights.properties.InstrumentationKey
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalyticsWorkspace.properties.customerId
        sharedKey: logAnalyticsWorkspace.listKeys().primarySharedKey
      }
    }
  }
}

// The UserAssigned identity is used by both Container Apps to pull the container image from ACR. 
// This cannot be done with the Container App's SystemAssigned Identity since the image needs to be pulled in order to create the Container App. 
// The SystemAssigned identities for each Container App is used to assign the necessary permissions for running each service. 
param timestamp string = utcNow('yyyyMMddHHmmss')
resource normalizationContainerApp 'Microsoft.App/containerApps@2022-03-01' ={
  name: 'normalization'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
  }
  identity: {
    type: 'SystemAssigned,UserAssigned'
    userAssignedIdentities: {
      '${userAssignedMI.id}': {}
    }
  }
  properties:{
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      registries: [
        {
          server: '${containerRegistry.name}.azurecr.io'
          identity: userAssignedMI.id
        }
      ]      
    }
    template: {
      containers: [
        {
          image: '${containerRegistry.name}.azurecr.io/${normalizationImage}:${imageTag}'
          name: 'normalization-${timestamp}'
          env: [
            {
              name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
              value: appInsights.properties.InstrumentationKey
            }
            {
              name: 'EventBatching__FlushTimespan'
              value: '30'
            }
            {
              name: 'EventBatching__MaxEvents'
              value: '20'
            }
            {
              name: 'Checkpoint__BlobPrefix'
              value: 'Normalization'
            }
            {
              name: 'CheckpointStorage__AuthenticationType'
              value: 'ManagedIdentity'
            }
            {
              name: 'CheckpointStorage__BlobStorageContainerUri'
              value: '${storageAccount.properties.primaryEndpoints.blob}checkpoint'
            }
            {
              name: 'CheckpointStorage__BlobContainerName'
              value: 'checkpoint'
            } 
            {
              name: 'TemplateStorage__AuthenticationType'
              value: 'ManagedIdentity'
            } 
            {
              name: 'TemplateStorage__BlobStorageContainerUri'
              value: '${storageAccount.properties.primaryEndpoints.blob}template'
            }
            {
              name: 'TemplateStorage__BlobContainerName'
              value: 'template'
            }
            {
              name: 'InputEventHub__AuthenticationType'
              value: 'ManagedIdentity'
            }
            {
              name: 'InputEventHub__EventHubNamespaceFQDN'
              value: eventhubNamespace.properties.serviceBusEndpoint
            }
            {
              name: 'InputEventHub__EventHubConsumerGroup'
              value: '$Default'
            }
            {
              name: 'InputEventHub__EventHubName'
              value: 'devicedata'
            }
            {
              name: 'NormalizationEventHub__AuthenticationType'
              value: 'ManagedIdentity'
            }
            {
              name: 'NormalizationEventHub__EventHubNamespaceFQDN'
              value: eventhubNamespace.properties.serviceBusEndpoint
            }
            {
              name: 'NormalizationEventHub__EventHubConsumerGroup'
              value: '$Default'
            }
            {
              name: 'NormalizationEventHub__EventHubName'
              value: 'normalizeddata'
            } 
            {
              name: 'Template__DeviceContent'
              value: 'devicecontent.json'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
      }
    }
  }
}

resource fhirTransformationContainerApp 'Microsoft.App/containerApps@2022-03-01' ={
  name: 'fhir-transformation'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
  }
  identity: {
    type: 'SystemAssigned,UserAssigned'
    userAssignedIdentities: {
      '${userAssignedMI.id}': {}
    }
  }
  properties:{
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      registries: [
        {
          server: '${containerRegistry.name}.azurecr.io'
          identity: userAssignedMI.id
        }
      ]      
    }
    template: {
      containers: [
        {
          image: '${containerRegistry.name}.azurecr.io/${fhirTransformationImage}:${imageTag}'
          name: 'fhir-transformation-${timestamp}'
          env: [
            {
              name: 'APPINSIGHTS_INSTRUMENTATIONKEY'
              value: appInsights.properties.InstrumentationKey
            }
            {
              name: 'EventBatching__FlushTimespan'
              value: '300'
            }
            {
              name: 'EventBatching__MaxEvents'
              value: '300'
            }
            {
              name: 'Checkpoint__BlobPrefix'
              value: 'MeasurementToFhir'
            }
            {
              name: 'CheckpointStorage__AuthenticationType'
              value: 'ManagedIdentity'
            }
            {
              name: 'CheckpointStorage__BlobStorageContainerUri'
              value: '${storageAccount.properties.primaryEndpoints.blob}checkpoint'
            }
            {
              name: 'CheckpointStorage__BlobContainerName'
              value: 'checkpoint'
            }
            {
              name: 'TemplateStorage__AuthenticationType'
              value: 'ManagedIdentity'
            }
            {
              name: 'TemplateStorage__BlobStorageContainerUri'
              value: '${storageAccount.properties.primaryEndpoints.blob}template'
            }
            {
              name: 'TemplateStorage__BlobContainerName'
              value: 'template'
            }
            {
              name: 'FhirClient__UseManagedIdentity'
              value: 'true'
            }
            {
              name: 'FhirService__Url'
              value: fhirURL
            }
            {
              name: 'FhirService__Resource'
              value: fhirURL
            }
            {
              name: 'InputEventHub__AuthenticationType'
              value: 'ManagedIdentity'
            }
            {
              name: 'InputEventHub__EventHubNamespaceFQDN'
              value: eventhubNamespace.properties.serviceBusEndpoint
            }
            {
              name: 'InputEventHub__EventHubConsumerGroup'
              value: '$Default'
            }
            {
              name: 'InputEventHub__EventHubName'
              value: 'devicedata'
            }
            {
              name: 'NormalizationEventHub__AuthenticationType'
              value: 'ManagedIdentity'
            }
            {
              name: 'NormalizationEventHub__EventHubNamespaceFQDN'
              value: eventhubNamespace.properties.serviceBusEndpoint
            } 
            {
              name: 'NormalizationEventHub__EventHubConsumerGroup'
              value: '$Default'
            }
            {
              name: 'NormalizationEventHub__EventHubName'
              value: 'normalizeddata'
            }
            {
              name: 'ResourceIdentity__ResourceIdentityServiceType'
              value: resourceIdentityResolutionType
            }
            {
              name: 'ResourceIdentity__DefaultDeviceIdentifierSystem'
              value: ''
            }
            {
              name: 'Template__FhirMapping'
              value: 'fhirmapping.json'
            }
          ]
        }
      ]
      scale: {
        minReplicas: 1
        maxReplicas: 10
      }
    }
  }
}

var eventHubReceiverRoleId = resourceId('Microsoft.Authorization/roleDefinitions', 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde')
var eventHubOwnerRoleId = resourceId('Microsoft.Authorization/roleDefinitions', 'f526a384-b230-433a-b45c-95f59c4a2dec')
var storageBlobDataOwnerRoleId = resourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
var fhirContributorRoleId = resourceId('Microsoft.Authorization/roleDefinitions', '5a1fc7df-4bf1-4951-a576-89034ee01acd')

// Assign roles to Normalization Container App 
resource eventHubReceiverNormalization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: devicedataEH
  name: guid(eventHubReceiverRoleId, normalizationContainerApp.id, devicedataEH.id)
  properties: {
    roleDefinitionId: eventHubReceiverRoleId
    principalId: normalizationContainerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource eventHubOwnerNormalization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: normalizeddataEH
  name: guid(eventHubOwnerRoleId, normalizationContainerApp.id, normalizeddataEH.id)
  properties: {
    roleDefinitionId: eventHubOwnerRoleId
    principalId: normalizationContainerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageBlobDataOwnerNormalization 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageBlobDataOwnerRoleId, normalizationContainerApp.id, storageAccount.id)
  properties: {
    roleDefinitionId: storageBlobDataOwnerRoleId
    principalId: normalizationContainerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Assign roles to FHIR Transformation Container App
resource eventHubOwnerFhirTransformation 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: normalizeddataEH
  name: guid(eventHubOwnerRoleId, fhirTransformationContainerApp.id, normalizeddataEH.id)
  properties: {
    roleDefinitionId: eventHubOwnerRoleId
    principalId: fhirTransformationContainerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageBlobDataOwnerFhirTransformation 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageBlobDataOwnerRoleId, fhirTransformationContainerApp.id, storageAccount.id)
  properties: {
    roleDefinitionId:storageBlobDataOwnerRoleId
    principalId: fhirTransformationContainerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource fhirContributorFhirTransformation 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: fhirService
  name: guid(fhirContributorRoleId, fhirTransformationContainerApp.id, fhirService.id)
  properties: {
    roleDefinitionId: fhirContributorRoleId
    principalId: fhirTransformationContainerApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}
