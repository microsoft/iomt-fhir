param baseName string
param location string

param normalizationImage string = 'normalization'
param fhirTransformationImage string = 'fhir-transformation'
param imageTag string = 'latest'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: '${baseName}acr'
}

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2022-10-01' existing = {
  name: 'law-${baseName}'
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: 'ai-${baseName}'
}

resource eventhubNamespace 'Microsoft.EventHub/namespaces@2021-11-01' existing = {
  name: 'en-${baseName}'
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

resource fhirService 'Microsoft.HealthcareApis/workspaces/fhirservices@2023-02-28' existing = {
  name: 'fs-${baseName}'
}

// // param userAssignedMIName string = '${baseName}UAMI'

resource userAssignedMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: '${baseName}UAMI'
  location: location
}

// resource userAssignedMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
//   name: '${baseName}UAMI'
// }

// param userAssignedMIPath string = 'subscriptions/${subscription().subscriptionId}/resourceGroups/${baseName}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/${userAssignedMIName}'

// var managedIdentityOperatorRoleId = resourceId('Microsoft.Authorization/roleDefinitions', 'f1a07417-d97a-45cb-824c-7a7467783830')

// resource managedI 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   scope: containerRegistry
//   name: guid(managedIdentityOperatorRoleId, userAssignedMI.id, containerRegistry.id)
//   properties: {
//     roleDefinitionId: managedIdentityOperatorRoleId
//     principalId: userAssignedMI.properties.principalId
//     principalType: 'ServicePrincipal'
//   }
// }

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

// // @description('Arry of actions for the the custom deployment principal role.')
// // param actions array = [
// //   'Microsoft.Resources/deployments/*'
// //   'Microsoft.Resources/deploymentScripts/*'
// //   'Microsoft.ContainerRegistry/registries/push/write'
// // ]

// // @description('Array of notActions for the custom deployment principal role.')
// // param notActions array = []

// // param deploymentPrincipalRoleName string = 'Custom Role - Deployment Principal'

// // param deploymentPrincipalRoleDefName string = guid(subscription, string(actions), string(notActions))

// // resource deploymentPrincipalRoleDef 'Microsoft.Authorization/roleDefinitions@2022-04-01' = {
// //   name: deploymentPrincipalRoleDefName
// //   properties: {
// //     roleName: deploymentPrincipalRoleName
// //     description: 'Configure least privilege for the deployment principal in deployment script'
// //     type: 'customRole'
// //     permissions: [
// //       {
// //         actions: actions 
// //         notActions: notActions
// //       }
// //     ]
// //     assignableScopes: [
// //       '${subscription}' // may change to provider later 
// //     ]
// //   }
// // }

// // // Build and deploy Normalization and FHIR Transformation container images 
// // resource containerAppScript 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
// //   name: 'containerAppScript'
// //   location: location
// //   kind: 'AzurePowerShell'
// //   // tags: {
// //   //   tagName1: 'tagValue1'
// //   //   tagName2: 'tagValue2'
// //   // }
// //   identity: {
// //     type: 'UserAssigned'
// //     userAssignedIdentities: {
// //       '${userAssignedMI.id}': {}
// //     }
// //   }
// //   properties: {
// //     containerSettings: {
// //       containerGroupName: 'deployment'
// //     }
// //     azPowerShellVersion: '9.7' 
// //     arguments: '-containerRegistry ${containerRegistry.name}'
// //     scriptContent: loadTextContent('build-container-images.ps1')
// //     retentionInterval: 'P1D'
// //   }
// // }

// resource containerAppEnv 'Microsoft.App/managedEnvironments@2022-10-01' existing = {
//   name: '${baseName}env'
// }

// https://github.com/Azure/azure-rest-api-specs/blob/Microsoft.App-2022-03-01/specification/app/resource-manager/Microsoft.App/preview/2022-01-01-preview/ManagedEnvironments.json
resource containerAppEnv 'Microsoft.App/managedEnvironments@2022-10-01' = {
  name: '${baseName}env'
  location: location
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

// // resource normalizationContainerApp 'Microsoft.App/containerApps@2022-03-01' existing = {
// //   name: 'normalization'
// // }

// // resource fhirTransformationContainerApp 'Microsoft.App/containerApps@2022-03-01' existing = {
// //   name: 'fhir-transformation'
// // }

param timestamp string = utcNow('yyyyMMddHHmmss')
// https://github.com/Azure/azure-rest-api-specs/blob/Microsoft.App-2022-01-01-preview/specification/app/resource-manager/Microsoft.App/preview/2022-01-01-preview/ContainerApps.json
resource normalizationContainerApp 'Microsoft.App/containerApps@2022-03-01' ={
  name: 'normalization'
  location: location
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
              name: 'CheckpointStorage_BlobStorageContainerUri'
              value: '${storageAccount.properties.primaryEndpoints.blob}checkpoint'
            }
            {
              name: 'TemplateStorage_BlobStorageContainerUri'
              value: '${storageAccount.properties.primaryEndpoints.blob}template'
            }
            {
              name: 'InputEventHub_EventHubNamespaceFQDN'
              value: eventhubNamespace.properties.serviceBusEndpoint
            }
            {
              name: 'InputEventHub_EventHubName'
              value: 'devicedata'
            }
            {
              name: 'NormalizationEventHub_EventHubNamespaceFQDN'
              value: eventhubNamespace.properties.serviceBusEndpoint
            }
            {
              name: 'NormalizationEventHub_EventHubName'
              value: 'normalizeddata'
            }
          ]
        }
      ]
    }
  }
}

resource fhirTransformationContainerApp 'Microsoft.App/containerApps@2022-03-01' ={
  name: 'fhir-transformation'
  location: location
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
              name: 'CheckpointStorage_BlobStorageContainerUri'
              value: '${storageAccount.properties.primaryEndpoints.blob}checkpoint'
            }
            {
              name: 'TemplateStorage_BlobStorageContainerUri'
              value: '${storageAccount.properties.primaryEndpoints.blob}template'
            }
            {
              name: 'FhirService_Url'
              value: 'https://fs-${baseName}.fhir.azurehealthcareapis.com'
            }
            {
              name: 'InputEventHub_EventHubNamespaceFQDN'
              value: eventhubNamespace.properties.serviceBusEndpoint
            }
            {
              name: 'InputEventHub_EventHubName'
              value: 'devicedata'
            }
            {
              name: 'NormalizationEventHub_EventHubNamespaceFQDN'
              value: eventhubNamespace.properties.serviceBusEndpoint
            }
            {
              name: 'NormalizationEventHub_EventHubName'
              value: 'normalizeddata'
            }
          ]
        }
      ]
    }
  }
}

var eventHubReceiverRoleId = resourceId('Microsoft.Authorization/roleDefinitions', 'a638d3c7-ab3a-418d-83e6-5f17a39d4fde')
var eventHubOwnerRoleId = resourceId('Microsoft.Authorization/roleDefinitions', 'f526a384-b230-433a-b45c-95f59c4a2dec')
var storageBlobDataOwnerRoleId = resourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')
var fhirDataContributorRoleId = resourceId('Microsoft.Authorization/roleDefinitions', '5a1fc7df-4bf1-4951-a576-89034ee01acd')

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

// may not be needed 
// resource fhirDataContributorFhirTransformation 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
//   scope: fhirService
//   name: guid(fhirDataContributorRoleId, fhirTransformationContainerApp.id, fhirService.identity.principalId)
//   properties: {
//     roleDefinitionId: fhirDataContributorRoleId
//     principalId: fhirTransformationContainerApp.identity.principalId
//     principalType: 'ServicePrincipal'
//   }
// }

// // // output location string = location
// // // output environmentId string = environment.id
// // // output appUrl string = containerApp.properties.configuration.ingress.fqdn
// // // output appInsightsConnectionString string = appInsights.properties.ConnectionString
