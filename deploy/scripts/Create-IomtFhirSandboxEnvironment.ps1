<#
.SYNOPSIS
Creates a new IoMT FHIR Connector for Azure sandbox  environment.
.DESCRIPTION
#>
param
(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [ValidateLength(5,12)]
	[ValidateScript({
        if ("$_" -cmatch "(^([a-z]|\d)+$)") {
            return $true
        }
        else {
			throw "Environment name must be lowercase and numbers"
            return $false
        }
    })]
    [string]$EnvironmentName,

    [Parameter(Mandatory = $false)]
    [ValidateSet('Australia East','East US','East US 2', 'West US', 'West US 2','North Central US','South Central US','Southeast Asia','North Europe','West Europe','UK West','UK South')]
    [string]$EnvironmentLocation = "North Central US",

    [Parameter(Mandatory = $false)]
    [ValidateSet('North Europe', 'Central US')]
    [string]$IotCentralLocation = "Central US",

    [Parameter(Mandatory = $false)]
    [ValidateSet('R4')]
    [string]$FhirVersion = "R4",

    [Parameter(Mandatory = $false)]
    [string]$FhirApiLocation = "northcentralus",

    [Parameter(Mandatory = $false)]
    [string]$SourceRepository = "https://github.com/microsoft/iomt-fhir",
  
    [Parameter(Mandatory = $false)]
    [string]$SourceRevision = "main",

	[parameter(Mandatory = $false)]
    [SecureString]$AdminPassword,

    [parameter(Mandatory = $false)]
    [ValidateSet('Lookup', 'Create', 'LookupWithEncounter')]
    [string]$ResourceIdentityResolutionType = "Create"
)

Set-StrictMode -Version Latest

# Get current Az context
try {
    $azContext = Get-AzContext
} 
catch {
    throw "Please log in to Azure RM with Login-AzAccount cmdlet before proceeding"
}

# Get current account context - User/Service Principal
if ($azContext.Account.Type -eq "User") {
    Write-Host "Current context is user: $($azContext.Account.Id)"
    
    $currentUser = Get-AzADUser -UserPrincipalName $azContext.Account.Id

    #If this is guest account, we will try a search instead
    if (!$currentUser) {
        # External user accounts have UserPrincipalNames of the form:
        # myuser_outlook.com#EXT#@mytenant.onmicrosoft.com for a user with username myuser@outlook.com
        $tmpUserName = $azContext.Account.Id.Replace("@", "_")
        $currentUser = Get-AzureADUser -Filter "startswith(UserPrincipalName, '${tmpUserName}')"
        $currentObjectId = $currentUser.ObjectId
    } else {
        $currentObjectId = $currentUser.Id
    }

    if (!$currentObjectId) {
        throw "Failed to find objectId for signed in user"
    }
}
elseif ($azContext.Account.Type -eq "ServicePrincipal") {
    Write-Host "Current context is service principal: $($azContext.Account.Id)"
    $currentObjectId = (Get-AzADServicePrincipal -ServicePrincipalName $azContext.Account.Id).Id
}
else {
    Write-Host "Current context is account of type '$($azContext.Account.Type)' with id of '$($azContext.Account.Id)"
    throw "Running as an unsupported account type. Please use either a 'User' or 'Service Principal' to run this command"
}

# Creating Resource Group
Write-Host "Creating resource group with the name $EnvironmentName in the subscription $($azContext.Subscription.Name)"
$resourceGroup = Get-AzResourceGroup -Name $EnvironmentName -ErrorAction SilentlyContinue
if (!$resourceGroup) {
    New-AzResourceGroup -Name $EnvironmentName -Location $EnvironmentLocation | Out-Null
}

# Deploy the template
$githubRawBaseUrl = $SourceRepository.Replace("github.com","raw.githubusercontent.com").TrimEnd('/')
$sandboxTemplate = "..\templates\default-azuredeploy-sandbox.json"
$iomtConnectorTemplate = "${githubRawBaseUrl}/${SourceRevision}/deploy/templates/default-managed-identity-azuredeploy.json"
$fhirServerUrl = "https://${EnvironmentName}.azurehealthcareapis.com"

Write-Host "Deploying resources..."
New-AzResourceGroupDeployment -TemplateFile $sandboxTemplate -ResourceGroupName $EnvironmentName -ServiceName $EnvironmentName -FhirServiceLocation $FhirApiLocation -FhirVersion $FhirVersion -RepositoryUrl $SourceRepository -RepositoryBranch $SourceRevision -FhirServiceUrl $fhirServerUrl -ResourceLocation $EnvironmentLocation -IomtConnectorTemplateUrl $iomtConnectorTemplate -ResourceIdentityResolutionType $ResourceIdentityResolutionType

# Copy the config templates to storage
Write-Host "Copying templates to storage..."
$storageAcct = Get-AzStorageAccount -ResourceGroupName $EnvironmentName -Name $EnvironmentName
Get-ChildItem -Path "../../sample/templates/sandbox" -File | Set-AzStorageBlobContent -Context $storageAcct.Context -Container "template" -Force

Write-Host "Warming up site..."
Invoke-WebRequest -Uri "${fhirServerUrl}/metadata" | Out-Null

@{
    fhirServerUrl             = $fhirServerUrl	
}
