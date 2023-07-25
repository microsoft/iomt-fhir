param 
(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$acrName
)
$normalizationImage = "normalization"
$fhirTransformationImage = "fhir-transformation"
$imageTag = "latest"
$normalizationDockerfile = '.\src\console\Microsoft.Health.Fhir.Ingest.Console.Normalization\Dockerfile'
$fhirTransformationDockerfile = '.\src\console\Microsoft.Health.Fhir.Ingest.Console.FhirTransformation\Dockerfile'

# Set-StrictMode -Version Latest
# $ErrorActionPreference = "Stop"

# # Get current Az context
# try {
#     Write-Host "Get current Az context..."
#     $azContext = Get-AzContext
# } 
# catch {
#     throw "Please log in to Azure RM with Login-AzAccount cmdlet before proceeding"
# }

# # Get current account context - User/Service Principal
# if ($azContext.Account.Type -eq "User") {
#     Write-Host "Current account context is user: $($azContext.Account.Id)"
    
#     $currentUser = Get-AzADUser -UserPrincipalName $azContext.Account.Id

#     if ($currentUser) {
#         $currentObjectId = $currentUser.Id
#     }

#     if (!$currentObjectId) {
#         throw "Failed to find objectId for signed in user"
#     }
# }
# elseif ($azContext.Account.Type -eq "ServicePrincipal") {
#     Write-Host "Current account context is service principal: $($azContext.Account.Id)"
#     $currentObjectId = (Get-AzADServicePrincipal -ServicePrincipalName $azContext.Account.Id).Id
# }
# else {
#     Write-Host "Current context is account of type '$($azContext.Account.Type)' with id of '$($azContext.Account.Id)"
#     throw "Running as an unsupported account type. Please use either a 'User' or 'Service Principal' to run this command"
# }

Write-Host "Building Normalization image..."
az acr build --registry $containerRegistry --image "$($normalizationImage):$($imageTag)" --file $normalizationDockerfile .

Write-Host "Building FHIR Transformation image..."
az acr build --registry $containerRegistry --image "$($fhirTransformationImage):$($imageTag)" --file $fhirTransformationDockerfile .