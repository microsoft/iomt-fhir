# IoMT for FHIR Sandbox

You can deploy a sandbox application to see how [IoMT FHIR Connector for Azure](./ARMInstallation.md) can be used with [Azure API for FHIR](https://docs.microsoft.com/azure/healthcare-apis) and [Azure IoT Central](https://azure.microsoft.com/en-us/services/iot-central/). The script deploys all of these components with mock devices sending data through the IoMT for FHIR pipeline.

## Prerequisites

Before deploying the samples scenario make sure that you have `Az` and `AzureAd` powershell modules installed:

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

Then deploy the scenario with the Open Source IoMT FHIR Connector for Azure:

```PowerShell
.\Create-IomtFhirSandboxEnvironment.ps1 -EnvironmentName <ENVIRONMENTNAME>
```
## Post Deployment
After successful deployment, your IoT Central application must be connected to the IoMT FHIR Connector for Azure. To do so:

1. Navigate to your IOT Central app at \<ENVIRONMENTNAME\>.azureiotcentral.com
2. On the left panel, natigate to "Data export"
3. Click New > Azure Event Hubs
4. Under "Event Hubs namespace" choose your environment name.
5. Under "Event hub" choose "devicedata"
6. We only need to export "Telemetry", so you can turn off "Devices" and "Device Templates".
7. Click Save.


