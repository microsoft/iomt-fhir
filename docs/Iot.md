# Connection to Azure IoT Hub

Connecting to IoT Hub is done by using the [message routing](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-messages-d2c#routing-endpoints) feature.  You will want to create a [custom endpoint](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-endpoints#custom-endpoints
) pointing to the `devicedata` Event Hub.
