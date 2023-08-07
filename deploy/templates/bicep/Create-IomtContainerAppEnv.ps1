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
    [Parameter(Mandatory = $true)]
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
    [string]$location,
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'Create',
        'Lookup',
        'LookupWithEncounter'
    )]
    [string]$resourceIdentityResolutionType,
    [Parameter(Mandatory = $true)]
    [ValidateSet(
        'R4'
    )]
    [string]$fhirVersion
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Get current Az context
try {
    Write-Host "Get current Az context..."
    az account show 
} 
catch {
    throw "Please log in with az login cmdlet before proceeding."
}

# Get current account context - User/Service Principal
$azAccountId = az account show --query user.name --output tsv
$azAccountType = az account show --query user.type --output tsv
if ($azAccountType -eq "user") {
    Write-Host "Current account context is user: $($azAccountId)."
    
    $currentUser = az ad user show --id $azAccountId

    if ($currentUser) {
        $currentObjectId = az ad user show --id $azAccountId --query id --output tsv
    }

    if (!$currentObjectId) {
        throw "Failed to find objectId for signed in user."
    }
}
elseif ($azAccountType -eq "servicePrincipal") {
    Write-Host "Current account context is service principal: $($azAccountId)."
}
else {
    Write-Host "Current context is account of type '$($azAccountType)' with id of '$($azAccountId)."
    throw "Running as an unsupported account type. Please use either a 'User' or 'Service Principal' to run this command."
}

# Create a resource group in provided subscription if it doesn't exist and deploy Azure resources needed to run IoMT Service.
$setupTemplate = "Main.bicep" 

Write-Host "Deploying Azure resources setup..."
az deployment sub create --location $location --template-file $setupTemplate --name "$($baseName)MainSetup" --parameters baseName=$baseName  location=$location resourceIdentityResolutionType=$resourceIdentityResolutionType fhirVersion=$fhirVersion

# Build and push container images to ACR
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
az deployment group create --resource-group $baseName --template-file $caSetupTemplate --name "$($baseName)ContainerAppSetup" --parameters baseName=$baseName location=$location resourceIdentityResolutionType=$resourceIdentityResolutionType fhirVersion=$fhirVersion

Write-Host "Deployment complete."