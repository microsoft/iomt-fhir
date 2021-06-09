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

# Get current Az context
try {
    $azContext = Get-AzContext
} 
catch {
    throw "Please log in to Azure RM with Login-AzAccount cmdlet before proceeding"
}

# Wipe out the environment
Get-AzResourceGroup -Name $EnvironmentName | Remove-AzResourceGroup -Verbose -Force
