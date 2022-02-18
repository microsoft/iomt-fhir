# Installation via ARM Template for use with Azure API for FHIR and Azure Active Directory
This article details provisioning and installation of the IoMT FHIR Connector for Azure and connecting to Azure API for FHIR with a managed identity in the same subscription using an ARM template.

## ARM Template Provisioning
<a href="https://portal.azure.com/#create/Microsoft.Template/uri/https%3A%2F%2Fraw.githubusercontent.com%2FMicrosoft%2Fiomt-fhir%2Fmain%2Fdeploy%2Ftemplates%2Fdefault-managed-identity-azuredeploy.json" target="_blank">
    <img src="https://azuredeploy.net/deploybutton.png"/>
</a>

An [ARM Template](../deploy/templates/default-managed-identity-azuredeploy.json) is provided for easy provisioning of an environment within Azure with the Azure API for FHIR. When executed, the ARM template will provision the following:

* App Service Plan - The service plan for used for hosting the Azure Functions Web app.
* Azure Web App - The web app running the Azure Functions responsible for normalization and FHIR conversion.
* Azure Event Hubs - Two Event Hubs are deployed. One is the initial ingestion point for device data. The second receives normalized device data for further processing.
* Azure Stream Analytics - Used to group and buffer the normalized data stream. Controls the end to end latency between device data ingested and landing the data in the configured FHIR server.
* Azure Key Vault - Used for secret storage.  Event Hub Shared Access Keys and the OAuth client credentials are stored here.
* Azure Storage - Used by the Azure Functions to track Event Hub processing watermark and also hosts the configuration files for device normalization mapping and FHIR conversion mapping.
* App Insights - Used to record telemetry.
* Managed Identity - an Azure Active Directory service identity for the IoMT FHIR Connector for Azure to use to connect to the Azure API for FHIR

### Prerequisites
To run this ARM template the following additional items must be set up before execution:

* FHIR Server - An Azure API for FHIR instance using FHIR version R4

### Parameters
The following parameters are provided by the ARM template:

|Parameter|Use
|---|---
|**Service Name**|Name for the service(s) being deployed.  Name will applied to all relevant services being created.
|**Repository URL**|Repository to pull source code from. If blank, source code will not be deployed.
|**Repository Branch**|Source code branch to deploy.
|**Job Window Unit**|The time period to collect events before sending them to the FHIR server.
|**Job Window Magnitude**|The magnitude of time period to collect events before sending them to the FHIR server.
|**Streaming Units**|Number of Streaming Units for the Stream Analytics job processing device events. For more information see [understanding Streaming Units](https://docs.microsoft.com/en-us/azure/stream-analytics/stream-analytics-streaming-unit-consumption) in the Stream Analytics documentation.
|**Throughput Units**| The throughput units reserved for the Event Hubs created. For more information see [Throughput units FAQ](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-faq#throughput-units) in the Event Hubs documentation.
|**App Service Plan SKU**|The app service plan tier to use for hosting the required Azure Functions.
|**Resource Location**|The location of the deployed resources.
|**FHIR Version**|The FHIR version used in the connector. It should match the version used in the FHIR service.
|**FHIR Service URL**|URL of the FHIR server that IoMT data will be written to.
|**Resource Identity Resolution Type**|Configures how patient, device, and other FHIR resource identities are resolved from the ingested data stream. The different supported modes are further documented below.
|**Default Device Identifier System**|Default system to use when searching for device identities. If empty system is not used in the search.

### Resource Identity Resolution Type
**Note** all identity look ups are cached once resolved to decrease load on the FHIR server.  If you plan on reusing devices with multiple patients it is advised you create a *virtual device* resource that is specific to the patient and the virtual device identifier is what is sent in the message payload. The virtual device can be linked to the actual device resource as a parent.

|Type|Behavior
|---|---
|**Lookup**|Default setting.  Device identifier from ingested messages is retrieved from the FHIR server. Patient is expected to be linked to the device.
|**Create**|System attempts to retrieve the device identifier and associated patient from the FHIR server. If either isn't found a shell resource with just the identity will be created. Requires a patient identifier be mapped in the device content configuration template.
|**LookupWithEncounter**|Like the first setting but allows you to include an encounter identifier with the message to associate with the device/patient.  The encounter is looked up during processing and any observations created are linked to the encounter. The association here is assumed to be one encounter per device.

## Post Deployment
After the ARM template is successfully deployed, [configure Azure RBAC for your Azure API for FHIR](https://docs.microsoft.com/en-us/azure/healthcare-apis/fhir/configure-azure-rbac) using the App Service name when searching for the entity to assign the role to, in order to assign access for the IoMT FHIR Connector for Azure to the Azure API for FHIR data plane. Also, the Authority on the Authentication page of your Azure API for FHIR should NOT be changed from the Azure Active Directory in your subscription, or this connection will be broken.

 Also, the mapping configurations for device content and converting to FHIR need to be added to the template container in the deployed Azure Storage blob.  You can use a tool like [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/) to easily upload and update the configurations. Navigate to the Azure Storage account deployed by the ARM template (it will be service name you selected) and select the template storage to container.  From there upload the configurations and you are done.

More information on mapping templates can be found [here](https://github.com/microsoft/iomt-fhir/blob/7794cbcc463e8d26c3097cd5e2243d770f26fe45/docs/Configuration.md).
Full examples can be found in the repository under [/sample/templates](https://github.com/microsoft/iomt-fhir/tree/7794cbcc463e8d26c3097cd5e2243d770f26fe45/sample/templates)