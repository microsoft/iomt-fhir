# Installation via ARM Template for use with Azure API for FHIR and Azure Active Directory
This article details three deployment options for provisioning and installation of the IoMT FHIR Connector for Azure and [FHIR service in Azure Health Data Services](https://learn.microsoft.com/en-us/azure/healthcare-apis/fhir/overview) using Bicep templates. 

The following Azure components will be provisioned once deployment has completed:

* Storage Account - Used to track Event Hub processing watermark and host the configuration files for device normalization mapping and FHIR conversion mapping.
* Event Hubs Namespace - Hosts the two Event Hubs, 'devicedata' and 'normalizeddata'. 
* Event Hubs - Two Event Hubs are deployed. One is the initial ingestion point for device data. The second receives normalized device data for further processing.
* Azure Health Data Services Workspace - Logical container to host the FHIR Service.
* FHIR Service - A FHIR Service in Azure Health Data Services workspace instance using FHIR version R4
* Azure Container Registry - Stores two container images, 'normalization' and 'fhir-transformation'. 
* Log Analytics Workspace - 
* App Insights - Records telemetry.
* Managed Identity - An Azure Active Directory service identity is created to connect the Container Apps to the ACR where they pull the container images to run. 
* Container Apps Environment - Hosts the two Container Apps, 'normalization' and 'fhir-transformation'. 
* Container Apps - Two Container Apps are deployed. One performs normalization of the device data. The second executes the FHIR conversion logic and sends the results to the FHIR service. 

### Prerequisites
To run any of these deployment options, the following items must be set up before execution:

* Contributor and User Access Administrator OR Owner permissions on your Azure subscription 

For local deployments (Options 2 and 3), the following additional steps must be performed:
Install the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) module 

Log into your Azure account and select the subscription you want to deploy the resources in: 
```PowerShell
az login 
```

If you have more than one subscription, select the subscription you would like to deploy to: 
```PowerShell
az account set --subscription <SubscriptionId>
```

Clone this repo and navigate to the Bicep deployment folder: 
```PowerShell
git clone https://github.com/Microsoft/iomt-fhir
cd iomt-fhir/deploy/templates/bicep 
```

### Parameters
The following parameters are provided by the Bicep template:

|Parameter|Use
|---|---
|**Service Name**|Name for the service(s) being deployed. Name will applied to all relevant services being created.
|**Resource Location**|The location of the deployed resources.
|**Resource Identity Resolution Type**|Configures how patient, device, and other FHIR resource identities are resolved from the ingested data stream. The different supported modes are further documented below.

### Resource Identity Resolution Type
**Note** all identity look ups are cached once resolved to decrease load on the FHIR server. If you plan on reusing devices with multiple patients it is advised you create a *virtual device* resource that is specific to the patient and the virtual device identifier is what is sent in the message payload. The virtual device can be linked to the actual device resource as a parent.

|Type|Behavior
|---|---
|**Lookup**|Default setting.  Device identifier from ingested messages is retrieved from the FHIR server. Patient is expected to be linked to the device.
|**Create**|System attempts to retrieve the device identifier and associated patient from the FHIR server. If either isn't found a shell resource with just the identity will be created. Requires a patient identifier be mapped in the device content configuration template.
|**LookupWithEncounter**|Like the first setting but allows you to include an encounter identifier with the message to associate with the device/patient.  The encounter is looked up during processing and any observations created are linked to the encounter. The association here is assumed to be one encounter per device.

## Deployment 
## Option 1: Single-click Deploy to Azure via ARM template generated from [Bicep Template](../deploy/templates/bicep/ContainerApp-SingleAzureDeploy.bicep)

## Option 2: Deploy a single [Bicep file](../deploy/templates/bicep/ContainerApp-SingleAzureDeploy.bicep) locally 
Deploy the Bicep template by running the following command: 

```PowerShell
az deployment sub create --
```

This option deploys the [Bicep template](../deploy/templates/bicep/ContainerApp-SingleAzureDeploy.bicep) that was used to generate the ARM template in Option 1. This Bicep template serves as a single entry point for provisioning all necessary resources and configuring permissions. 

## Option 3: Execute a single [PowerShell deployment script](../deploy/templates/bicep/Create-IomtContainerAppEnv.ps1) locally
Run the following command to run the PowerShell script: 

```PowerShell
./Create-IomtContainerAppEnv.ps1
```



## Post Deployment