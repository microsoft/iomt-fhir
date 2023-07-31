# Installation options via Bicep Templates for use with FHIR service in Azure Health Data Services 
This article details three deployment options for provisioning and installation of the IoMT FHIR Connector for Azure and [FHIR service in Azure Health Data Services](https://learn.microsoft.com/en-us/azure/healthcare-apis/fhir/overview) using Bicep templates. 

The following Azure components will be provisioned once deployment has completed:

* Storage Account 
* Event Hubs Namespace  
* Event Hubs 
* Azure Health Data Services Workspace
* FHIR Service
* Azure Container Registry 
* Log Analytics Workspace 
* App Insights 
* Managed Identity 
* Container Apps Environment
* Container Apps  

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
## Option 1: Single-click Deploy to Azure via ARM template generated from Bicep Template

## Option 2: Deploy a single Bicep file locally 
Deploy the [Bicep template](../deploy/templates/bicep/ContainerApp-SingleAzureDeploy.bicep) by running the following command: 

```PowerShell
az deployment sub create --location <Location> --template-file ContainerApp-SingleAzureDeploy.bicep
```

NOTE: See [region availability](https://azure.microsoft.com/en-us/explore/global-infrastructure/products-by-region/?products=health-data-services) to select a location for the resources to be deployed in. 

This option deploys the Bicep template that was used to generate the ARM template in Option 1. This Bicep template serves as a single entry point for provisioning all necessary Azure resources and role assignments. Sample configuration templates, [devicecontent.json](../sample/templates/basic/devicecontent.json) and [fhirmapping.json](../sample/templates/basic/fhirmapping.json) are also uploaded to the 'template' blob container in the storage account using a User-Assigned Managed Identity. 

The 'deploymentScripts' resource is used to upload the sample mapping templates and build and push container images to the ACR. An additional Storage Account is provisioned to run these deployment scripts. A Container Instance is also created for each 'deploymentScripts' resource instance and is deleted upon successful deployment. 

To view the progress of the deployment, navigate to the resource group in Azure Portal and select the 'Deployments' tab under 'Settings' in the left panel. 

## Option 3: Execute a single PowerShell deployment script locally
Run the following command to run the PowerShell script: 

```PowerShell
./Create-IomtContainerAppEnv.ps1
```

This [PowerShell deployment script](../deploy/templates/bicep/Create-IomtContainerAppEnv.ps1) sets up all necessary Azure resources for running the IoMT Service by deploying Bicep templates. The 'deploymentScripts' resource is not used in this option and the commands are instead invoked locally via the PowerShell script. Therefore, no additional Storage Account or Container Instances are created.

The mapping configurations for device content and converting to FHIR need to be added to the template container in the deployed Azure Storage blob. Navigate to the Azure Storage account and select the template storage container. From there, upload the configurations and you are done.

More information on mapping templates can be found [here](https://github.com/microsoft/iomt-fhir/blob/7794cbcc463e8d26c3097cd5e2243d770f26fe45/docs/Configuration.md).
Full examples can be found in the repository under [/sample/templates](https://github.com/microsoft/iomt-fhir/tree/7794cbcc463e8d26c3097cd5e2243d770f26fe45/sample/templates)

To view the progress of the deployment, navigate to the resource group in Azure Portal and select the 'Deployments' tab under 'Settings' in the left panel. Outputs from each deployment step are visible in the terminal following completion. 