<#
.SYNOPSIS
Creates a new IoMT FHIR Connector for Azure without using Stream Analytics
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
    [ValidateSet('R4')]
    [string]$FhirVersion = "R4",

    [Parameter(Mandatory = $false)]
    [string]$SourceRepository = "https://github.com/microsoft/iomt-fhir",
  
    [Parameter(Mandatory = $false)]
    [string]$SourceRevision = "master",

    [Parameter(Mandatory = $true)]
    [string]$FhirServiceUrl,

    [Parameter(Mandatory = $true)]
    [string]$FhirServiceAudience,

    [Parameter(Mandatory = $false)]
    [string]$EnvironmentDeploy = $true
)

Function BuildPackage() {
    try {
        Push-Location $currentPath
        cd ../../src/console/
        dotnet restore
        dotnet build --output $buildPath /p:DeployOnBuild=true /p:DeployTarget=Package
    } catch {
        throw
    } finally {
        Pop-Location
    }
}

Function Deploy-WebJobs($DeviceDataWebJobName, $NormalizedDataWebJobName) {
    try {
        Clean-Path -WebJobName $DeviceDataWebJobName
        Clean-Path -WebJobName $NormalizedDataWebJobName

        $resourceGroupName = $EnvironmentName
        $webAppName = $EnvironmentName
        $webJobType = "Continuous"
        $buildPath = "$currentPath\OSS_Deployment"
        $tempPath = "$currentPath\Temp"

        $DeviceWebJobPath = "$tempPath\App_Data\jobs\$webJobType\$DeviceDataWebJobName"
        $NormalizedWebJobPath = "$tempPath\App_Data\jobs\$webJobType\$NormalizedDataWebJobName"
        Copy-Item "$buildPath\*" -Destination $DeviceWebJobPath -Recurse
        Copy-Item "$buildPath\*" -Destination $NormalizedWebJobPath -Recurse

        Compress-Archive -Path "$tempPath\*" -DestinationPath "$currentPath\iomtwebjobs.zip" -Force

        Publish-AzWebApp -ArchivePath "$currentPath\iomtwebjobs.zip" -ResourceGroupName $resourceGroupName -Name $webAppName
    } catch {
        throw
    } finally {
        Pop-Location
    }
}

Function Clean-Path($WebJobName) {
    $WebJobPath = "$tempPath\App_Data\jobs\$webJobType\$WebJobName"
    Get-ChildItem -Path $WebJobPath -Recurse | Remove-Item -Force -Recurse
    if( -Not (Test-Path -Path $WebJobPath ) )
    {
        New-Item $WebJobPath -ItemType Directory
    }
}

Set-StrictMode -Version Latest

# deploy event hubs, app service, key vaults, storage
if ($EnvironmentDeploy -eq $true) {
    Write-Host "Deploying environment resources..."
    $webjobTemplate = "..\templates\default-azuredeploy-webjobs.json"
    New-AzResourceGroupDeployment -TemplateFile $webjobTemplate -ResourceGroupName $EnvironmentName -ServiceName $EnvironmentName -FhirServiceUrl $fhirServerUrl -FhirServiceAudience $fhirServerUrl -RepositoryUrl $SourceRepository -RepositoryBranch $SourceRevision -ResourceLocation $EnvironmentLocation
}

# deploy the stream analytics replacement webjobs
Write-Host "Deploying WebJobs..."

$currentPath = (Get-Location).Path
BuildPackage
Deploy-WebJobs -DeviceDataWebJobName "devicedata" -NormalizedDataWebJobName "normalizeddata"

