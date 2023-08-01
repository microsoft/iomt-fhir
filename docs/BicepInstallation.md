# Installation options via Bicep Templates for use with FHIR service in Azure Health Data Services 
This article details three deployment options for provisioning and installation of the IoMT FHIR Connector for Azure and [FHIR service in Azure Health Data Services](https://learn.microsoft.com/en-us/azure/healthcare-apis/fhir/overview) using Bicep templates. 

The following Azure components will be provisioned once deployment has completed:

* Storage Account 
* Two Blob Containers
* Event Hubs Namespace  
* Two Event Hubs 
* Azure Health Data Services workspace
* FHIR service* 
* Azure Container Registry 
* Log Analytics Workspace 
* App Insights 
* User-Assigned Managed Identity 
* Container Apps Environment
* Two Container Apps  

An additional Storage Account and Container Instances will be created if the 'deploymentScripts' Bicep resource type is used in the chosen deployment option (Options 1 and 2). 

\* NOTE: These deployment options use the FHIR service in Azure Health Data Services as opposed to the Azure API for FHIR. For more information on the differences between these services, please see this [page](https://learn.microsoft.com/en-us/azure/healthcare-apis/fhir/overview). 

### Prerequisites
To run any of these deployment options, the following items must be set up before execution:

* Contributor and User Access Administrator OR Owner permissions on your Azure subscription 

For local deployments (Options 2 and 3), the following additional steps must be performed:
* Install the [Azure CLI](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli) module 

* Log into your Azure account and select the subscription you want to deploy the resources in: 
```PowerShell
az login 
```

* If you have more than one subscription, select the subscription you would like to deploy to: 
```PowerShell
az account set --subscription <SubscriptionId>
```

* Clone this repo and navigate to the Bicep deployment folder: 
```PowerShell
git clone https://github.com/Microsoft/iomt-fhir
cd iomt-fhir/deploy/templates/bicep 
```

### Parameters
The following parameters are provided by the Bicep template:

|Parameter|Use
|---|---
|**Service Name**|Name for the service(s) being deployed. Name will be applied to all relevant services being created.
|**Resource Location**|The location of the deployed resources.
|**Resource Identity Resolution Type**|Configures how patient, device, and other FHIR resource identities are resolved from the ingested data stream. The different supported modes are further documented below.

### Resource Identity Resolution Type
NOTE: All identity look ups are cached once resolved to decrease load on the FHIR server. If you plan on reusing devices with multiple patients it is advised you create a *virtual device* resource that is specific to the patient and the virtual device identifier is what is sent in the message payload. The virtual device can be linked to the actual device resource as a parent.

|Type|Behavior
|---|---
|**Create**|System attempts to retrieve the device identifier and associated patient from the FHIR server. If either isn't found a shell resource with just the identity will be created. Requires a patient identifier be mapped in the device content configuration template.
|**Lookup**|Device identifier from ingested messages is retrieved from the FHIR server. Patient is expected to be linked to the device.
|**LookupWithEncounter**|Similar to the 'Lookup' setting but allows you to include an encounter identifier with the message to associate with the device/patient. The encounter is looked up during processing and any observations created are linked to the encounter. The association here is assumed to be one encounter per device.

## Deployment 
### Option 1: Single-click Deploy to Azure via ARM template generated from Bicep Template

### Option 2: Deploy a single Bicep file locally 
Deploy the [Bicep template](../deploy/templates/bicep/ContainerApp-SingleAzureDeploy.bicep) by running the following command: 

```PowerShell
az deployment sub create --location <Location> --template-file ContainerApp-SingleAzureDeploy.bicep
```

Example: 
```PowerShell
az deployment sub create --location westus2 --template-file ContainerApp-SingleAzureDeploy.bicep
```

NOTE: See [region availability](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=health-data-services) for Azure Health Data Servicces to select a valid location for the resources to be deployed in. 

You will need to provide a *baseName* to name the services that will be provisioned. The valid *location* and *resourceIdentityResolutionType* options are presented as an enumerated list. To select an option, type the number corresponding to your desired selection. For help, type '?' to see a description of a parameter. 

This option deploys the Bicep file that was used to generate the ARM template in Option 1. This Bicep template serves as a single entry point for setting up all necessary Azure resources and role assignments. Sample configuration templates, [devicecontent.json](../sample/templates/basic/devicecontent.json) and [fhirmapping.json](../sample/templates/basic/fhirmapping.json) are also uploaded to the 'template' container in the Storage Account using a User-Assigned Managed Identity. 

The 'deploymentScripts' resource type in Bicep is used to (1) upload the sample mapping templates and (2) build and push container container images to the ACR. An additional Storage Account is provisioned to execute these deployment scripts. A Container Instance is also created for each 'deploymentScripts' resource instance and is deleted upon successful deployment. 

### Option 3: Execute a single PowerShell deployment script locally
Run the following command to run the PowerShell deployment script: 

```PowerShell
./Create-IomtContainerAppEnv.ps1
```

Values for *baseName*, *location*, and *resourceIdentityResolutionType* will need to be provided. Please note the valid options listed in the PowerShell deployment script file for each parameter before deployment. Be careful to provide these values exactly, otherwise the deployment will fail. 

This [PowerShell deployment script](../deploy/templates/bicep/Create-IomtContainerAppEnv.ps1) sets up all necessary Azure resources for running the IoMT Service by deploying Bicep templates via Azure CLI commands. Azure CLI commands are also used to build and push container images to the ACR. The 'deploymentScripts' resource type is not used in this option and the commands are instead invoked locally via the PowerShell script. Therefore, no additional Storage Account or Container Instances are created. The duration of running these commands locally is also shorter than running the commands within the 'deploymentScripts' resource in Bicep. 

The mapping configurations for device content and converting to FHIR need to be added to the 'template' container in the deployed Azure Storage blob. Navigate to the  Storage Account and select the 'template' storage container. From there, upload the configurations to complete set up of the IoMT FHIR Connector.

More information on mapping templates can be found [here](./Configuration.md). Full examples can be found in the repository under [../sample/templates](../sample/templates/)

### Additional Deployment Notes
To view the progress of a deployment, navigate to the resource group in Azure Portal and select the 'Deployments' tab under 'Settings' in the left panel.

All deployment options reference separate Bicep files that are used to provision a set of resources. To redeploy a specific supporting Bicep file, run the following command: 
```PowerShell
az deployment group create --resource-group <baseName> --template-file <File.bicep>
```

Example: 
```PowerShell
az deployment group create --resource-group testdeployment --template-file BuildContainerImages.bicep
```

NOTE: If you wish to redeploy Main.bicep, run the following command: 
```PowerShell
az deployment sub create --location <Location> --template-file Main.bicep
```

Example: 
```PowerShell
az deployment sub create --location westus2 --template-file Main.bicep
```