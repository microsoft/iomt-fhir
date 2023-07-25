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

    # [Parameter(Mandatory = $false)]
    # [ValidateSet(
    #     'australiaeast',
    #     'canadacentral',
    #     'centralus',
    #     'eastus',
    #     'eastus2',
    #     'japaneast',
    #     'northeurope',
    #     'southeastasia',
    #     'southcentralus',
    #     'uksouth',
    #     'westcentralus',
    #     'westeurope',
    #     'westus'
    # )]
    # [string]$iotLocation = "westus"
)
Write-Host "Deploying Azure resources setup..."

$setupTemplate = "main.bicep" 

az deployment sub create --location $location --template-file $setupTemplate --name "${$baseName}MainSetup" --parameters baseName=$baseName  location=$location

Set-Location ..\..\..\

$acrName = "$($baseName)acr"
$normalizationImage = "normalization"
$fhirTransformationImage = "fhir-transformation"
$imageTag = "latest"
$normalizationDockerfile = "src\console\Microsoft.Health.Fhir.Ingest.Console.Normalization\Dockerfile"
$fhirTransformationDockerfile = "src\console\Microsoft.Health.Fhir.Ingest.Console.FhirTransformation\Dockerfile"

Write-Host "Building Normalization image..."
az acr build --registry $acrName --image "$($normalizationImage):$($imageTag)" --file $normalizationDockerfile . 
Write-Host "Normalization image created."

Write-Host "Building FHIR Transformation image..."
az acr build --registry $acrName --image "$($fhirTransformationImage):$($imageTag)" --file $fhirTransformationDockerfile . 
Write-Host "FHIR Transformation image created."

$caSetupTemplate = "deploy\templates\bicep\containerapp-setup.bicep"

Write-Host "Deploying Container Apps Setup..."
az deployment group create --resource-group $baseName --template-file $caSetupTemplate --name "${$baseName}ContainerAppSetup" --parameters baseName=$baseName location=$location

Write-Host "Deployment complete."