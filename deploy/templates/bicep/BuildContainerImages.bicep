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

@description('The URL for the repository where the container code resides.')
param gitRepositoryUrl string = 'https://github.com/microsoft/iomt-fhir.git'

@description('The branch name or commit hash to be used when building the containers, defaults to main.')
param gitBranch string = 'main'

@description('FHIR version that the FHIR Server supports')
@allowed([
  'R4'
])
param fhirVersion string

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: '${baseName}acr'
}

resource userAssignedMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: '${baseName}UAMI'
}

var gitRepositoryAndBranch = '${gitRepositoryUrl}#${gitBranch}'
var normalizationImage = 'normalization'
var fhirTransformationImage = 'fhir-transformation'
var imageTag = 'latest'
var normalizationDockerfile = 'src/console/Microsoft.Health.Fhir.Ingest.Console.Normalization/Dockerfile'
var fhirTransformationDockerfile = 'src/console/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation/Dockerfile'

param acrBuildPlatform string = 'linux'

resource deploymentStorageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: '${baseName}deploysa'
}

resource buildNormalizationImage 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'buildNormalizationImage'
  location: location
  tags: {
    IomtFhirConnector: 'ResourceIdentity:${resourceIdentityResolutionType}'
    IomtFhirVersion: fhirVersion
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
    arguments: '${containerRegistry.name} ${gitRepositoryAndBranch} ${normalizationImage} ${imageTag} ${normalizationDockerfile} ${acrBuildPlatform}'
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
    IomtFhirVersion: fhirVersion
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
    arguments: '${containerRegistry.name} ${gitRepositoryAndBranch} ${fhirTransformationImage} ${imageTag} ${fhirTransformationDockerfile} ${acrBuildPlatform}'
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
