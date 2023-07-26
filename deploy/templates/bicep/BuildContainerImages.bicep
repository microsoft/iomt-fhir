param baseName string 
param location string 

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: '${baseName}acr'
}
resource userAssignedMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: '${baseName}UAMI'
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

var normalizationImage = 'normalization'
var fhirTransformationImage = 'fhir-transformation'
var imageTag = 'latest'
var normalizationDockerfile = 'src/console/Microsoft.Health.Fhir.Ingest.Console.Normalization/Dockerfile'
var fhirTransformationDockerfile = 'src/console/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation/Dockerfile'

param gitRepositoryUrl string = 'https://github.com/microsoft/iomt-fhir.git'
param acrBuildPlatform string = 'linux'

resource buildNormalizationImage 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'buildNormalizationImage'
  location: location
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedMI.id}': {}
    }
  }
  properties: {
    containerSettings: {
      containerGroupName: '${baseName}buildContainerImages'
    }
    azCliVersion: '2.50.0'
    arguments: '${containerRegistry.name} ${gitRepositoryUrl} ${normalizationImage} ${imageTag} ${normalizationDockerfile} ${acrBuildPlatform}'
    scriptContent: '''
      az acr build --registry $1 $2 --image $3:$4 --file $5 --platform $6
    '''
    retentionInterval: 'P1D'
  }
}

resource buildFhirTransformationImage 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'buildFhirTransformationImage'
  location: location
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedMI.id}': {}
    }
  }
  properties: {
    containerSettings: {
      containerGroupName: 'buildContainerImages'
    }
    azCliVersion: '2.50.0'
    arguments: '${containerRegistry.name} ${gitRepositoryUrl} ${fhirTransformationImage} ${imageTag} ${fhirTransformationDockerfile} ${acrBuildPlatform}'
    scriptContent: '''
      az acr build --registry $1 $2 --image $3:$4 --file $5 --platform $6
    '''
    retentionInterval: 'P1D'
  }
}
