# Connection to Azure IoT
This article details connecting the IoMT FHIR Connector for Azure to IoT Hub or IoT Central.

## Connect to IoT Hub
Connecting to IoT Hub is done by using the [message routing](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-messages-d2c#routing-endpoints) feature.  You will want to create a [custom endpoint](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-endpoints#custom-endpoints
) pointing to the `devicedata` Event Hub.

## Connect to IoT Central
Connecting IoT Central is done by using the [continuous data export](https://docs.microsoft.com/en-us/azure/iot-central/core/howto-export-data-pnp) feature.  Please follow the instructions for exporting to Event Hub.  The target endpoint will be the `devicedata` Event Hub.  When selecting the types of data to export only *Telemetry* is needed.  Device and templates can be left unchecked.