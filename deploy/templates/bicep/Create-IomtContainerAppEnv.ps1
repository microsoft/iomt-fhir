param
(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [ValidateLength(3,16)]
    [ValidateScript({
        if ("$_" -cmatch "(^([a-z]|\d)+$)") {
            return $true
        }
        else {
			throw "Service name must be lowercase and numbers"
            return $false
        }
    })]
    [string]$baseName,
    
    [Parameter(Mandatory = $false)]
    [ValidateSet(
        'australiaeast', 
        'canadacentral', 
        'eastus', 
        'eastus2', 
        'japaneast', 
        'koreacentral', 
        'northcentralus',
        'northeurope',
        'qatarcentral',
        'southcentralus',
        'southeastasia',
        'swedencentral',
        'switzerlandnorth',
        'westcentralus',
        'westeurope',
        'westus2',
        'westus3',
        'uksouth'
    )]
    [string]$location = "westus2",

    [Parameter(Mandatory = $false)]
    [ValidateSet(
        'Create',
        'Lookup',
        'LookupWithEncounter'
    )]
    [string]$resourceIdentityResolutionType = "Create"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Get current Az context
try {
    Write-Host "Get current Az context..."
    $azContext = Get-AzContext
} 
catch {
    throw "Please log in to Azure RM with Login-AzAccount cmdlet before proceeding"
}

# Get current account context - User/Service Principal
if ($azContext.Account.Type -eq "User") {
    Write-Host "Current account context is user: $($azContext.Account.Id)"
    
    $currentUser = Get-AzADUser -UserPrincipalName $azContext.Account.Id

    if ($currentUser) {
        $currentObjectId = $currentUser.Id
    }

    if (!$currentObjectId) {
        throw "Failed to find objectId for signed in user"
    }
}
elseif ($azContext.Account.Type -eq "ServicePrincipal") {
    Write-Host "Current account context is service principal: $($azContext.Account.Id)"
    $currentObjectId = (Get-AzADServicePrincipal -ServicePrincipalName $azContext.Account.Id).Id
}
else {
    Write-Host "Current context is account of type '$($azContext.Account.Type)' with id of '$($azContext.Account.Id)"
    throw "Running as an unsupported account type. Please use either a 'User' or 'Service Principal' to run this command"
}

# Create a resource group in subscription if it doesn't exist and deploy Azure resources needed to run IoMT Service.
$setupTemplate = "Main.bicep" 

Write-Host "Deploying Azure resources setup..."
az deployment sub create --location $location --template-file $setupTemplate --name "$($baseName)MainSetup" --parameters baseName=$baseName  location=$location resourceIdentityResolutionType=$resourceIdentityResolutionType

# Build and push container images to ACR. Running command directly avoids the creation of a separate storage account and container instance to run the script. 
$acrName = "$($baseName)acr"
$normalizationImage = "normalization"
$fhirTransformationImage = "fhir-transformation"
$imageTag = "latest"
$gitRepositoryUrl = "https://github.com/microsoft/iomt-fhir.git"
$acrBuildPlatform = "linux"
$normalizationDockerfile = "src\console\Microsoft.Health.Fhir.Ingest.Console.Normalization\Dockerfile"
$fhirTransformationDockerfile = "src\console\Microsoft.Health.Fhir.Ingest.Console.FhirTransformation\Dockerfile"

Write-Host "Building Normalization image..."
az acr build --registry $acrName $gitRepositoryUrl --image "$($normalizationImage):$($imageTag)" --file $normalizationDockerfile --platform $acrBuildPlatform
Write-Host "Normalization image created."

Write-Host "Building FHIR Transformation image..."
az acr build --registry $acrName $gitRepositoryUrl --image "$($fhirTransformationImage):$($imageTag)" --file $fhirTransformationDockerfile --platform $acrBuildPlatform 
Write-Host "FHIR Transformation image created."

# Set up container apps and configure necessary permissions with other resources. 
$caSetupTemplate = "ContainerAppSetup.bicep"

Write-Host "Deploying Container Apps Setup..."
az deployment group create --resource-group $baseName --template-file $caSetupTemplate --name "$($baseName)ContainerAppSetup" --parameters baseName=$baseName location=$location resourceIdentityResolutionType=$resourceIdentityResolutionType

Write-Host "Deployment complete."