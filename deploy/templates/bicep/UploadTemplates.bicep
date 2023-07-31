param baseName string 
param location string 
param resourceIdentityResolutionType string 

resource storageAccount 'Microsoft.Storage/storageAccounts@2022-09-01' existing = {
  name: '${baseName}sa'
}

resource userAssignedMI 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' existing = {
  name: '${baseName}UAMI'
}

var storageBlobDataOwnerRoleId = resourceId('Microsoft.Authorization/roleDefinitions', 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b')

resource storageBlobDataOwnerDeployment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageBlobDataOwnerRoleId, userAssignedMI.id, storageAccount.id)
  properties: {
    roleDefinitionId: storageBlobDataOwnerRoleId
    principalId: userAssignedMI.properties.principalId
    principalType: 'ServicePrincipal'
  }
}

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

param blobContainerName string = 'template'
param devicecontentFile string = 'devicecontent.json'
param fhirmappingFile string = 'fhirmapping.json'
param devicecontentURL string = 'https://raw.githubusercontent.com/microsoft/iomt-fhir/main/sample/templates/basic/devicecontent.json'
param fhirmappingURL string = 'https://raw.githubusercontent.com/microsoft/iomt-fhir/main/sample/templates/basic/fhirmapping.json'

resource uploadDeviceContentTemplate 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'uploadDeviceContentTemplate'
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
    arguments: '${devicecontentFile} ${devicecontentURL} ${storageAccount.name} ${blobContainerName}'
    scriptContent: '''
      wget -O $1 $2
      az storage blob upload --account-name $3 --container-name $4 --name $1 --file $1 --auth-mode login 
    '''
    retentionInterval: 'P1D'
    cleanupPreference: 'OnSuccess'
  }
}

resource uploadFhirMappingTemplate 'Microsoft.Resources/deploymentScripts@2020-10-01' = {
  name: 'uploadFhirMappingTemplate'
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
    arguments: '${fhirmappingFile} ${fhirmappingURL} ${storageAccount.name} ${blobContainerName}'
    scriptContent: '''
      wget -O $1 $2
      az storage blob upload --account-name $3 --container-name $4 --name $1 --file $1 --auth-mode login
    '''
    retentionInterval: 'P1D'
    cleanupPreference: 'OnSuccess'
  }
  dependsOn: [
    uploadDeviceContentTemplate
  ]
}
