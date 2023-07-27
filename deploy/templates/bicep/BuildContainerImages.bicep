param baseName string 
param location string 
param resourceIdentityResolutionType string 

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: '${baseName}acr'
}
resource userAssignedMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: '${baseName}UAMI'
}

var normalizationImage = 'normalization'
var fhirTransformationImage = 'fhir-transformation'
var imageTag = 'latest'
var normalizationDockerfile = 'src/console/Microsoft.Health.Fhir.Ingest.Console.Normalization/Dockerfile'
var fhirTransformationDockerfile = 'src/console/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation/Dockerfile'

param gitRepositoryUrl string = 'https://github.com/microsoft/iomt-fhir.git'
param acrBuildPlatform string = 'linux'

resource deploymentStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' = {
  name: '${baseName}deploysa'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: 'R4'
  }
  kind: 'StorageV2'
  sku: {
    name: 'Standard_RAGRS'
  }
}

resource buildNormalizationImage 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'buildNormalizationImage'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: 'R4'
  }
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedMI.id}': {}
    }
  }
  properties: {
    storageAccountSettings: {
      storageAccountName: '${baseName}deploysa'
      storageAccountKey: listkeys(resourceId('Microsoft.Storage/storageAccounts', deploymentStorageAccount.name), '2022-09-01').keys[0].value
    }
    containerSettings: {
      containerGroupName: '${baseName}deployContainer'
    }
    azCliVersion: '2.50.0'
    arguments: '${containerRegistry.name} ${gitRepositoryUrl} ${normalizationImage} ${imageTag} ${normalizationDockerfile} ${acrBuildPlatform}'
    scriptContent: '''
      az acr build --registry $1 $2 --image $3:$4 --file $5 --platform $6
    '''
    retentionInterval: 'P1D'
    cleanupPreference: 'OnSuccess'
  }
}

resource buildFhirTransformationImage 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'buildFhirTransformationImage'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: 'R4'
  }
  kind: 'AzureCLI'
  identity: {
    type: 'UserAssigned'
    userAssignedIdentities: {
      '${userAssignedMI.id}': {}
    }
  }
  properties: {
    storageAccountSettings: {
      storageAccountName: '${baseName}deploysa'
      storageAccountKey: listkeys(resourceId('Microsoft.Storage/storageAccounts', deploymentStorageAccount.name), '2022-09-01').keys[0].value
    }
    containerSettings: {
      containerGroupName: '${baseName}deployContainer'
    }
    azCliVersion: '2.50.0'
    arguments: '${containerRegistry.name} ${gitRepositoryUrl} ${fhirTransformationImage} ${imageTag} ${fhirTransformationDockerfile} ${acrBuildPlatform}'
    scriptContent: '''
      az acr build --registry $1 $2 --image $3:$4 --file $5 --platform $6
    '''
    retentionInterval: 'P1D'
    cleanupPreference: 'OnSuccess'
  }
  dependsOn: [
    buildNormalizationImage
  ]
}
