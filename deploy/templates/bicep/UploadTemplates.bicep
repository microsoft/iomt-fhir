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
    IomtFhirVersion: fhirVersion
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
