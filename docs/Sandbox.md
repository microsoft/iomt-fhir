# IoMT FHIR Connector for Azure Sandbox

You can deploy a sandbox application to see how [IoMT FHIR Connector for Azure](./ARMInstallation.md) can be used with [Azure API for FHIR](https://docs.microsoft.com/azure/healthcare-apis) and [Azure IoT Central](https://azure.microsoft.com/services/iot-central/). The script deploys all of these components with mock devices sending data through the IoMT FHIR Connector for Azure pipeline.

Once deployment is completed you should see the following Azure components:

- App Service
- App Service plan
- Application Insights
- Event Hubs Namespace
- Stream Analytics job
- Storage account
- Key vault
- Azure API for FHIR (R4)
- IoT Central Application

For the ease of using the sandbox, a few steps will be taken for you:

1. Simulated devices are set up in IoT Central to generate data.
2. Template files for those devices will be copied to the IoMT FHIR Connector for Azure storage account "Template" blob.
3. The IoMT FHIR Connector for Azure will be configured with the Resource Identity Resolution Type "Create" so that patients will automatically be created for each device.

## Prerequisites

Before deploying the samples scenario make sure that you have `Az` and `AzureAd` powershell modules installed (not required for Azure Cloud Shell):

```PowerShell
Install-Module Az
Install-Module AzureAd
```

## Deployment

To deploy the sample scenario, first clone this git repo and find the deployment scripts folder:

```PowerShell
git clone https://github.com/Microsoft/iomt-fhir
cd iomt-fhir/deploy/scripts
```

Log into your Azure subscription:

```PowerShell
Login-AzAccount
```

If you have more than one subscription, you can choose which to deploy to with:

```PowerShell
Select-AzSubscription <SubscriptionID>
```

Then deploy the scenario with the Open Source IoMT FHIR Connector for Azure:

```PowerShell
.\Create-IomtFhirSandboxEnvironment.ps1 -EnvironmentName <ENVIRONMENTNAME>
```

## Post Deployment

**NOTE** The device conversion mapping template provided in this guide is designed to work with the [Export Data](https://docs.microsoft.com/en-us/azure/iot-central/core/howto-export-data) feature of [Azure IoT Central](https://docs.microsoft.com/en-us/azure/iot-central/core/howto-export-data).

After successful deployment, your IoT Central application must be connected to the IoMT FHIR Connector for Azure. To do so:

1. Navigate to your IoT Central app at \<ENVIRONMENTNAME\>.azureiotcentral.com
2. On the left panel, navigate to "Data export".
3. Setup the destination to which the data has to be exported to:
    * Under the Destinations tab, click "Add a destination" or "+ New Destination".
    * Enter a name for this destination.
    * Select "Azure Event Hubs" as the Destination type.
    * [Get the connection string to the Event Hubs Namespace](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-get-connection-string) resource created in your environment and enter it in the Connection string field.
    * Enter "devicedata" for the Event Hub field.
    * Click Save.
4. Under the Exports tab, click "Add an export" or "+ New export".
5. Select "Telemetry" for type of data to export.
6. Select the name of the destination created in step 3.
7. Click Save.

## Verification

Copy the FHIR server URL from the deployment output to query the FHIR server.

After a few minutes, you should begin to [see observations in the FHIR server](https://docs.microsoft.com/azure/healthcare-apis/access-fhir-postman-tutorial) from the simulated devices using the following GET URL

```
https://<ENVIRONMENTNAME>.azurehealthcareapis.com/Observation
```

If no data is flowing, you should [debug the environment](./Debugging.md)
