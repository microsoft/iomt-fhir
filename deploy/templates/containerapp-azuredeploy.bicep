// subscription 
// resource group 
// app insights 
// storage account
    // blob
    // template - devicemapping.json fhirdesintation.json
// keyvault
// devicedata and normalizeddata event hubs 
// fhir api (r4)
// iot central 
// smart rule 

// _________________________

// uami
// log analytics 
// acr 
// script 
    // build and deploy image to acr
// container app env
// container apps (2) 
    // normalization 
        // normalization image 
    // fhir transformation 
        // fhir transformation image
    // permissions? 

@description('Subscription ')
param subscriptionId string
@description('Name used for provisioned resources.')
param serviceName string

param environmentName string = '${serviceName}-env'
param appName string = serviceName
param logAnalyticsWorkspaceName string = '${serviceName}-logsanalyticsws'
param appInsightsName string = '${serviceName}-appinsights'
param location string = resourceGroup().location

param userAssignedMIACR string = '/subscriptions/${subscriptionId}/resourcegroups/converter-test-infra/providers/Microsoft.ManagedIdentity/userAssignedIdentities/${serviceName}-uami'
param converterTestACR string = '${serviceName}acr'
param normalizationTestImage string = 'normalization'
param fhirTransformationTestImage string = 'fhir-transformation'
param converterTestImageTag string = 'latest'

resource logAnalyticsWorkspace 'Microsoft.OperationalInsights/workspaces@2020-10-01' = {
  name: logAnalyticsWorkspaceName
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
  name: appInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId:logAnalyticsWorkspace.id
  }
}

// https://github.com/Azure/azure-rest-api-specs/blob/Microsoft.App-2022-03-01/specification/app/resource-manager/Microsoft.App/preview/2022-01-01-preview/ManagedEnvironments.json
resource environment 'Microsoft.App/managedEnvironments@2022-03-01' = {
  name: environmentName
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

param timestamp string = utcNow('yyyyMMddHHmmss')
// https://github.com/Azure/azure-rest-api-specs/blob/Microsoft.App-2022-01-01-preview/specification/app/resource-manager/Microsoft.App/preview/2022-01-01-preview/ContainerApps.json
resource containerApp 'Microsoft.App/containerApps@2022-03-01' ={
  name: appName
  location: location
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedMIACR}': {}
    }
  }
  properties:{
    managedEnvironmentId: environment.id
    configuration: {
      ingress: {
        targetPort: 80
        external: true
      }
      registries: [
        {
          server: '${converterTestACR}.azurecr.io'
          identity: userAssignedMIACR
        }
      ]      
    }
    template: {
      containers: [
        {
          image: '${converterTestACR}.azurecr.io/${converterTestImage}:${converterTestImageTag}'
          name: 'converter-${timestamp}'
          env: [
            {
              name: 'AppInsightsConnectionString'
              value: appInsights.properties.ConnectionString
            }
          ]
        }
      ]
    }
  }
}

output location string = location
output environmentId string = environment.id
output appUrl string = containerApp.properties.configuration.ingress.fqdn
output appInsightsConnectionString string = appInsights.properties.ConnectionString
