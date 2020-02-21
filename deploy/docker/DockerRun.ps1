<#
    This script can be used to build and run the IoMT Azure Function code in a docker container.
    It can be used for local development to build and run Docker images, and images can be used for cloud deployment.
    By default console logging is turned on, but can be turned off by removing AzureFunctionsJobHost__Logging__Console__IsEnabled
    One caveat with local development is that Docker cannot easily communicate with the Azure Storage Emulator, and it is easiest use a cloud storage container instead.
    Run this script from the project root .\deploy\docker\DockerRun.ps1
#>

if(-not (Test-Path .\src\func\Microsoft.Health.Fhir.Ingest.Host\local.settings.json)) {
    Write-Output 'Could not locate local.settings.json'
    Exit
}

$json = Get-Content .\src\func\Microsoft.Health.Fhir.Ingest.Host\local.settings.json | ConvertFrom-Json
$env = $json.Values

if($env.AzureWebJobsStorage -eq "UseDevelopmentStorage=true") {
    Write-Output 'Setting "AzureWebJobsStorage":"UseDevelopmentStorage=true" in local.settings.json not supported with Docker, must use connection string'
    Exit
}

docker build --tag iomt:v1.0.0 .
docker run -p 8080:80 `
    -e AzureFunctionsJobHost__Logging__Console__IsEnabled='true' `
    -e AzureWebJobsScriptRoot='/home/site/wwwroot' `
    -e AzureWebJobsStorage="$($env."AzureWebJobsStorage")" `
    -e AzureWebJobsSecretStorageType="$($env."AzureWebJobsSecretStorageType")" `
    -e FhirService:Authority="$($env."FhirService:Authority")" `
    -e FhirService:ClientId="$($env."FhirService:ClientId")" `
    -e FhirService:ClientSecret="$($env."FhirService:ClientSecret")" `
    -e FhirService:Resource="$($env."FhirService:Resource")" `
    -e FhirService:Url="$($env."FhirService:Url")" `
    -e FUNCTIONS_WORKER_RUNTIME="$($env."FUNCTIONS_WORKER_RUNTIME")" `
    -e InputEventHub="$($env."InputEventHub")" `
    -e OutputEventHub="$($env."OutputEventHub")" `
    -e ResourceIdentity:ResourceIdentityServiceType="$($env."ResourceIdentity:ResourceIdentityServiceType")" `
    -e ResourceIdentity:DefaultDeviceIdentifierSystem="$($env."ResourceIdentity:DefaultDeviceIdentifierSystem")" `
    -e Template:DeviceContent="$($env."Template:DeviceContent")" `
    -e Template:FhirMapping="$($env."Template:FhirMapping")" `
    -it iomt:v1.0.0