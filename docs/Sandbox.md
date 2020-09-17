# IoMT FHIR Connector for Azure Sandbox

You can deploy a sandbox application to see how [IoMT FHIR Connector for Azure](./ARMInstallation.md) can be used with [Azure API for FHIR](https://docs.microsoft.com/azure/healthcare-apis) and [Azure IoT Central](https://azure.microsoft.com/en-us/services/iot-central/). The script deploys all of these components with mock devices sending data through the IoMT FHIR Connector for Azure pipeline.

Once deployment is completed you should see the following Azure components:

- App Service
- App Service plan
- Application Insights
- Event Hubs Namespace
- Stream Analytics job
- Storage account
- Key vault (x2)
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

Connect to Azure AD with:

```PowerShell
Connect-AzureAD -TenantDomain <AAD TenantDomain>
```

**NOTE** The connection to Azure AD can be made using a different tenant domain than the one tied to your Azure subscription. If you don't have privileges to create app registrations, users, etc. in your Azure AD tenant, you can [create a new one](https://docs.microsoft.com/azure/active-directory/develop/quickstart-create-new-tenant), which will just be used for demo identities, etc.

If you have more than one subscription, you can choose which to deploy to with:

```PowerShell
Select-AzSubscription <SubscriptionID>
```

Then deploy the scenario with the Open Source IoMT FHIR Connector for Azure:

```PowerShell
.\Create-IomtFhirSandboxEnvironment.ps1 -EnvironmentName <ENVIRONMENTNAME>
```

## Post Deployment

**NOTE** The device conversion mapping template provided in this guide is designed to work with Data Export (legacy) within IoT Central.

After successful deployment, your IoT Central application must be connected to the IoMT FHIR Connector for Azure. To do so:

1. Navigate to your IoT Central app at \<ENVIRONMENTNAME\>.azureiotcentral.com
2. On the left panel, navigate to "Data export"
3. Click New > Azure Event Hubs
4. Under "Event Hubs namespace" choose your environment name.
5. Under "Event hub" choose "devicedata"
6. We only need to export "Telemetry", so you can turn off "Devices" and "Device Templates".
7. Click Save.

## Verification

Copy the FHIR server URL, client ID and client secret from the deployment output to query the FHIR server (NOTE: this client ID and secret are used by the IoMT FHIR Connector for Azure and shouldn't be used on any other production services.')

After a few minutes, you should begin to [see observations in the FHIR server](https://docs.microsoft.com/en-us/azure/healthcare-apis/access-fhir-postman-tutorial) from the simulated devices using the following GET URL

```
https://<ENVIRONMENTNAME>.azurehealthcareapis.com/Observation
```

If no data is flowing, you should [debug the environment](./Debugging.md)
