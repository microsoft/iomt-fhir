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
    [string]$location = "westus2"
)
Write-Host "Deploying Azure resources setup..."

$setupTemplate = "Main.bicep" 

az deployment sub create --location $location --template-file $setupTemplate --name "${$baseName}MainSetup" --parameters baseName=$baseName  location=$location

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

$caSetupTemplate = "ContainerAppSetup.bicep"

Write-Host "Deploying Container Apps Setup..."
az deployment group create --resource-group $baseName --template-file $caSetupTemplate --name "${$baseName}ContainerAppSetup" --parameters baseName=$baseName location=$location

Write-Host "Deployment complete."