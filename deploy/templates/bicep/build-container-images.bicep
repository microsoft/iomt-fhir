param baseName string 
param location string 

resource userAssignedMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: '${baseName}UAMI'
}

var normalizationImage = 'normalization'
var fhirTransformationImage = 'fhir-transformation'
var imageTag = 'latest'
var normalizationDockerfile = 'src/console/Microsoft.Health.Fhir.Ingest.Console.Normalization/Dockerfile'
var fhirTransformationDockerfile = 'src/console/Microsoft.Health.Fhir.Ingest.Console.FhirTransformation/Dockerfile'

// az acr build --registry wotest4acr https://github.com/microsoft/iomt-fhir.git --image normalization:latest --file src/console/Microsoft.Health.Fhir.Ingest.Console.Normalization/Dockerfile --platform linux

param gitRepositoryUrl string = 'https://github.com/microsoft/iomt-fhir.git'
param acrBuildPlatform string = 'linux'

resource containerRegistry 'Microsoft.ContainerRegistry/registries@2022-12-01' existing = {
  name: '${baseName}acr'
}

// https://github.com/Azure/azure-quickstart-templates/blob/master/quickstarts/microsoft.resources/deployment-script-azcli-acr-build/main.bicep
// module buildNormalizationImage 'br/public:deployment-scripts/build-acr:1.0.1' = {
//   name: 'buildNormalizationImage'
//   params: {
//     AcrName: containerRegistry.name
//     location: location
//     gitRepositoryUrl: gitRepositoryUrl
//     gitBranch: gitBranch
//     gitRepoDirectory: gitRepoDirectory
//     imageName: normalizationImage
//     imageTag: imageTag
//     acrBuildPlatform: acrBuildPlatform
//     useExistingManagedIdentity: true
//     managedIdentityName: userAssignedMI.name
//   }
// }

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
      containerGroupName: 'buildContainerImages'
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
