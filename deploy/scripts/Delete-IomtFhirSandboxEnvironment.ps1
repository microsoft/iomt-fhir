<#
.SYNOPSIS
Removes a IoMT FHIR Connector for Azure sandbox environment
.DESCRIPTION
#>
param
(
    [Parameter(Mandatory = $true)]
    [ValidateNotNullOrEmpty()]
    [string]$EnvironmentName
)

Set-StrictMode -Version Latest

# Get current AzureAd context
try {
    $tenantInfo = Get-AzureADCurrentSessionInfo -ErrorAction Stop
} 
catch {
    throw "Please log in to Azure AD with Connect-AzureAD cmdlet before proceeding"
}

# Get current Az context
try {
    $azContext = Get-AzContext
} 
catch {
    throw "Please log in to Azure RM with Login-AzAccount cmdlet before proceeding"
}

# Set up Auth Configuration and Resource Group
./Delete-IomtFhirSandboxAuthConfig.ps1 -EnvironmentName $EnvironmentName 

# Wipe out the environment
Get-AzResourceGroup -Name $EnvironmentName | Remove-AzResourceGroup -Verbose -Force
