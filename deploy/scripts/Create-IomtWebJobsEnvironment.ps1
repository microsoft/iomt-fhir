<#
.SYNOPSIS
Creates a new IoMT FHIR Connector for Azure without using Stream Analytics
.DESCRIPTION
#>
param
(
    [Parameter(Mandatory = $true)]
    [string]$ResourceGroup,

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
    [ValidateSet('South Africa North', 'South Africa West', 'East Asia', 'Southeast Asia', 'Australia Central', 'Australia Central 2', 'Australia East', 'Australia Southeast', 'Brazil South', 'Brazil Southeast', 'Canada Central', 'Canada East', 'China East', 'China East 2', 'China North', 'China North 2', 'North Europe', 'West Europe', 'France Central', 'France South', 'Germany Central', 'Germany Northeast', 'Germany West Central', 'Central India', 'South India', 'West India', 'Japan East', 'Japan West', 'Korea Central', 'Korea South', 'Norway East', 'Switzerland North', 'Switzerland West', 'UAE Central', 'UAE North', 'UK West', 'UK South', 'Central US', 'East US', 'East US 2', 'North Central US', 'South Central US', 'West Central US', 'West US', 'West US 2')]
    [string]$EnvironmentLocation = "North Central US",
    [Parameter(Mandatory = $false)]
    [ValidateSet('R4')]
    [string]$FhirVersion = "R4",

    [Parameter(Mandatory = $false)]
    [string]$SourceRepository = "https://github.com/microsoft/iomt-fhir",
  
    [Parameter(Mandatory = $false)]
    [string]$SourceRevision = "main",

    [Parameter(Mandatory = $true)]
    [string]$FhirServiceUrl,

    [Parameter(Mandatory = $false)]
    [string]$EnvironmentDeploy = $true,

    [Parameter(Mandatory = $false)]
    [string]$UseManagedIdentity = $true
)

Function BuildPackage() {
    try {
        Push-Location $currentPath
        cd ../../src/console/
        dotnet restore
        dotnet build --output $buildPath /p:DeployOnBuild=true /p:DeployTarget=Package
    } finally {
        Pop-Location
    }
}

Function Deploy-WebJobs($NormalizationWebJobName, $MeasurementToFhirWebJobName) {
    try {
        $tempPath = "$currentPath\Temp"
        $webAppName = $EnvironmentName
        $webJobType = "Continuous"

        Clear-Path -WebJobName $NormalizationWebJobName
        Clear-Path -WebJobName $MeasurementToFhirWebJobName

        $NormalizationWebJobPath = "$tempPath\App_Data\jobs\$webJobType\$NormalizationWebJobName"
        $MeasurementToFhirWebJobPath = "$tempPath\App_Data\jobs\$webJobType\$MeasurementToFhirWebJobName"
        Copy-Item "$buildPath\*" -Destination $NormalizationWebJobPath -Recurse
        Copy-Item "$buildPath\*" -Destination $MeasurementToFhirWebJobPath -Recurse

        Compress-Archive -Path "$tempPath\*" -DestinationPath "$currentPath\iomtwebjobs.zip" -Force

        Publish-AzWebApp -ArchivePath "$currentPath\iomtwebjobs.zip" -ResourceGroupName $ResourceGroup -Name $webAppName
    } finally {
        Pop-Location
    }
}

Function Clear-Path($WebJobName) {
    $WebJobPath = "$tempPath\App_Data\jobs\$webJobType\$WebJobName"
    Get-ChildItem -Path $WebJobPath -Recurse -ErrorAction Ignore | Remove-Item -Force -Recurse -ErrorAction Ignore
    if( -Not (Test-Path -Path $WebJobPath ) )
    {
        New-Item $WebJobPath -ItemType Directory
    }
}

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# deploy event hubs, app service, key vaults, storage
if ($EnvironmentDeploy -eq $true) {
    $webjobTemplate = "..\templates\default-azuredeploy-webjobs.json"

    if ($UseManagedIdentity -eq $true) {
        Write-Host "Deploying environment resources..."
        New-AzResourceGroupDeployment -TemplateFile $webjobTemplate -ResourceGroupName $ResourceGroup -ServiceName $EnvironmentName -FhirServiceUrl $fhirServiceUrl -RepositoryUrl $SourceRepository -RepositoryBranch $SourceRevision -ResourceLocation $EnvironmentLocation
    }
    else {
        $FhirServiceAuthority = Read-Host -Prompt 'Input your fhir service authority'
        $FhirServiceClientId = Read-Host -Prompt 'Input your fhir service client id'
        $FhirServiceSecret = Read-Host -Prompt 'Input your fhir service sercret'

        Write-Host "Deploying environment resources..."
        New-AzResourceGroupDeployment -TemplateFile $webjobTemplate -ResourceGroupName $ResourceGroup -ServiceName $EnvironmentName -FhirClientUseManagedIdentity $false -FhirServiceUrl $fhirServiceUrl -FhirServiceAuthority $FhirServiceAuthority -FhirServiceResource $fhirServiceUrl -FhirServiceClientId $FhirServiceClientId -FhirServiceClientSecret (ConvertTo-SecureString -String $FhirServiceSecret -AsPlainText -Force) -RepositoryUrl $SourceRepository -RepositoryBranch $SourceRevision -ResourceLocation $EnvironmentLocation
    }
}

# deploy the stream analytics replacement webjobs
Write-Host "Deploying WebJobs..."

$currentPath = (Get-Location).Path
$buildPath = "$currentPath\OSS_Deployment"
BuildPackage
Deploy-WebJobs -NormalizationWebJobName "Normalization" -MeasurementToFhirWebJobName "MeasurementToFhir"

