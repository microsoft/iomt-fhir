# Configuration
This article details how to configure your instance of the IoMT FHIR Connector for Azure.

The IoMT FHIR Connector for Azure requires two JSON configuration files.  The first, device content, is responsible for mapping the payloads sent to the Event Hub end point and extracting types, device identifiers, measurement date time, and the measurement value(s).  The second template controls the FHIR mapping.  The FHIR mapping allows configuration of the length of the observation period, FHIR data type used to store the values, and code(s).  The two configuration files should be uploaded to the storage container "template" created under the blob storage account provisioned during the [ARM template deployment](ARMInstallation.md). The device content mapping file should be call `devicecontent.json` and the FHIR mapping file should be called `fhirmapping.json`. Full examples can be found in the repository under [/sample/templates](../sample/templates).  Configuration files are loaded from blob per compute execution.  Once updated they should take effect immediately. 

# Device Content Mapping
The IoMT FHIR Connector for Azure provides mapping functionality to extract device content into a common format for further evaluation.  Each event hub message received is evaluated against all templates. This allows a single inbound message to be projected to multiple outbound messages and subsequently mapped to different observations in FHIR.  The result is a normalized data object representing the value or values parsed by the templates.  The normalized data model has a few required properties that must be found and extracted: 

| Property | Description |
| - | - |
|**Type**|The name/type to classify the measurement.  This is used to bind to the desired FHIR mapping template.  Multiple templates can output to the same type allowing you to map different representations across multiple devices to a single common output.|
|**OccurenceTimeUtc**|The time the measurement occurred.|
|**DeviceId**|The identifier for the device.  This should match an identifier on the device resource that resides on the destination FHIR server.|
 |**Properties**|Extract at least one property so the value can be saved in the observation created.  Properties are a collection of key value pairs extracted during normalization.|

The full normalized model is defined by the [IMeasurement](../src/lib/Microsoft.Health.Fhir.Ingest/Data/IMeasurement.cs) interface.

Below is a conceptual example of what happens during normalization.

 ![alt text](../images/normalizationexample.png "Normalization Example")

 The content payload itself is an event hub message which is composed of three parts: Body, Properties, and SystemProperties.  The `Body` is a byte array representing an UTF-8 encoded string.  During template evaluation the byte array is automatically converted into the string value. `Properties` is a key value collection for use by the message creator.  `SystemProperties` is also a key value collection reserved by the EventHub framework with entries automatically populated by EventHub.

 ```json
 {
     "Body" :
     {
         "content" : "value"
     },
     "Properties" :
     {
         "key1" : "value1",
         "key2" : "value2"
     },
     "SystemProperties" :
     {
         "x-opt-sequence-number" : 1,
         "x-opt-enqueued-time" : "2019-02-01T22:46:01.8750000Z",
         "x-opt-offset" : 1,
         "x-opt-partition-key" : "1"
     }
 }
 ```

## Mapping with JSON Path
The two device content template types supported today rely on JSON Path to both match the desired template and extract values.  
More information on JSON Path can be found [here](https://goessner.net/articles/JsonPath/). Both template types use the [JSON .NET implementation](https://www.newtonsoft.com/json/help/html/QueryJsonSelectTokenJsonPath.htm) for resolving JSON Path expressions. Additional examples can be found in the [unit tests](../test/Microsoft.Health.Fhir.Ingest.UnitTests/Template/JsonPathContentTemplateTests.cs).

### **JsonPathContentTemplate**
The JsonPathContentTemplate allows matching on and extracting values from an EventHub message using JSON Path.

| Property | Description |<div style="width:150px">Example</div>
| --- | --- | --- 
|**TypeName**|The type to associate with measurements that match the template.|`heartrate`
|**TypeMatchExpression**|The JSON Path expression that is evaluated against the EventData payload. If a matching JToken is found the template is considered a match. All subsequent expressions are evaluated against the extracted JToken matched here.|`$..[?(@heartRate)]`
|**TimestampExpression**|The JSON Path expression to extract the timestamp value for the measurement's OccurenceTimeUtc.|`$.endDate`
|**DeviceIdExpression**|The JSON Path expression to extract the device identifier.|`$.deviceId`
|**PatientIdExpression**|*Optional*: The JSON Path expression to extract the patient identifier.|`$.patientId`
|**EncounterIdExpression**|*Optional*: The JSON Path expression to extract the encounter identifier.|`$.encounterId`
|**Values[].ValueName**|The name to associate with the value extracted by the subsequent expression. Used to bind the desired value/component in the FHIR mapping template. |`hr`
|**Values[].ValueExpression**|The JSON Path expression to extract the desired value.|`$.heartRate`
|**Values[].Required**|Will require the value to be present in the payload.  If not found a measurement will not be generated and an InvalidOperationException will be thrown.|`true`

#### Examples
---
**Heart Rate**

*Message*
```json
{
    "Body": {
        "heartRate": "78",
        "endDate": "2019-02-01T22:46:01.8750000Z",
        "deviceId": "device123"
    },
    "Properties": {},
    "SystemProperties": {}
}
```
*Template*
```json
{
    "templateType": "JsonPathContent",
    "template": {
        "typeName": "heartrate",
        "typeMatchExpression": "$..[?(@heartRate)]",
        "deviceIdExpression": "$.deviceId",
        "timestampExpression": "$.endDate",
        "values": [
            {
                "required": "true",
                "valueExpression": "$.heartRate",
                "valueName": "hr"
            }
        ]
    }
}
```
---
**Blood Pressure**

*Message*
```json
{
    "Body": {
        "systolic": "123",
        "diastolic" : "87",
        "endDate": "2019-02-01T22:46:01.8750000Z",
        "deviceId": "device123"
    },
    "Properties": {},
    "SystemProperties": {}
}
```
*Template*
```json
{
    "typeName": "bloodpressure",
    "typeMatchExpression": "$..[?(@systolic && @diastolic)]",
    "deviceIdExpression": "$.deviceid",
    "timestampExpression": "$.endDate",
    "values": [
        {
            "required": "true",
            "valueExpression": "$.systolic",
            "valueName": "systolic"
        },
        {
            "required": "true",
            "valueExpression": "$.diastolic",
            "valueName": "diastolic"
        }
    ]
}
```
**Project Multiple Measurements from Single Message**

*Message*
```json
{
    "Body": {
        "heartRate": "78",
        "steps": "2",
        "endDate": "2019-02-01T22:46:01.8750000Z",
        "deviceId": "device123"
    },
    "Properties": {},
    "SystemProperties": {}
}
```
*Template 1*
```json
{
    "templateType": "JsonPathContent",
    "template": {
        "typeName": "heartrate",
        "typeMatchExpression": "$..[?(@heartRate)]",
        "deviceIdExpression": "$.deviceId",
        "timestampExpression": "$.endDate",
        "values": [
            {
                "required": "true",
                "valueExpression": "$.heartRate",
                "valueName": "hr"
            }
        ]
    }
}
```
*Template 2*
```json
{
    "templateType": "JsonPathContent",
    "template": {
        "typeName": "stepcount",
        "typeMatchExpression": "$..[?(@steps)]",
        "deviceIdExpression": "$.deviceId",
        "timestampExpression": "$.endDate",
        "values": [
            {
                "required": "true",
                "valueExpression": "$.steps",
                "valueName": "steps"
            }
        ]
    }
}
```
**Project Multiple Measurements from Array in Message**

*Message*
```json
{
    "Body": [
        {
            "heartRate": "78",
            "endDate": "2019-02-01T22:46:01.8750000Z",
            "deviceId": "device123"
        },
        {
            "heartRate": "81",
            "endDate": "2019-02-01T23:46:01.8750000Z",
            "deviceId": "device123"
        },
        {
            "heartRate": "72",
            "endDate": "2019-02-01T24:46:01.8750000Z",
            "deviceId": "device123"
        }
    ],
    "Properties": {},
    "SystemProperties": {}
}
```
*Template*
```json
{
    "templateType": "JsonPathContent",
    "template": {
        "typeName": "heartrate",
        "typeMatchExpression": "$..[?(@heartRate)]",
        "deviceIdExpression": "$.deviceId",
        "timestampExpression": "$.endDate",
        "values": [
            {
                "required": "true",
                "valueExpression": "$.heartRate",
                "valueName": "hr"
            }
        ]
    }
}
```
### **IotJsonPathContentTemplate**
The IotJsonPathContentTemplate is similar to the JsonPathContentTemplate except the DeviceIdExpression and TimestampExpression are not required.

The assumption when using this template is the messages being evaluated were sent using the [Azure IoT Hub Device SDKs](https://docs.microsoft.com/en-us/azure/iot-hub/iot-hub-devguide-sdks#azure-iot-hub-device-sdks).  When using these SDKs the device identity (assuming the device id from Iot Hub/Central is registered as an identifer for a device resource on the destination FHIR server) is known as well as the timestamp of the message.  If you are using Azure IoT Hub Device SDKs but are using custom properties in the message body for the device identity or measurement timestamp you can still use the JsonPathContentTemplate.

*Note: When using the IotJsonPathContentTemplate the TypeMatchExpression should resolve to the entire message as a JToken.  Please see the examples below.* 
#### Examples
---
**Heart Rate**

*Message*
```json
{
    "Body": {
        "heartRate": "78"        
    },
    "Properties": {
        "iothub-creation-time-utc" : "2019-02-01T22:46:01.8750000Z"
    },
    "SystemProperties": {
        "iothub-connection-device-id" : "device123"
    }
}
```
*Template*
```json
{
    "templateType": "JsonPathContent",
    "template": {
        "typeName": "heartrate",
        "typeMatchExpression": "$..[?(@Body.heartRate)]",
        "deviceIdExpression": "$.deviceId",
        "timestampExpression": "$.endDate",
        "values": [
            {
                "required": "true",
                "valueExpression": "$.Body.heartRate",
                "valueName": "hr"
            }
        ]
    }
}
```
---
**Blood Pressure**

*Message*
```json
{
    "Body": {
        "systolic": "123",
        "diastolic" : "87"
    },
    "Properties": {
        "iothub-creation-time-utc" : "2019-02-01T22:46:01.8750000Z"
    },
    "SystemProperties": {
        "iothub-connection-device-id" : "device123"
    }
}
```
*Template*
```json
{
    "typeName": "bloodpressure",
    "typeMatchExpression": "$..[?(@Body.systolic && @Body.diastolic)]",
    "values": [
        {
            "required": "true",
            "valueExpression": "$.Body.systolic",
            "valueName": "systolic"
        },
        {
            "required": "true",
            "valueExpression": "$.Body.diastolic",
            "valueName": "diastolic"
        }
    ]
}
```

# FHIR Mapping
Once the device content is extracted into [Measurement](../src/lib/Microsoft.Health.Fhir.Ingest/Data/Measurement.cs) definitions the data is collected and grouped according to a window of time (set during deployment), device id, and type.  The output of this grouping is sent to be converted into a FHIR resource (observation currently). Here the FHIR mapping controls how the data is mapped into a FHIR observation. Should an observation be created for a point in time or over a period of an hour? What codes should be added to the observation? Should be value be represented as SampledData or a Quantity? These are all options the FHIR mapping configuration controls.

## CodeValueFhirTemplate
The CodeValueFhirTemplate is currently the only template supported in FHIR mapping at this time.  It allows you defined codes, the effective period, and value of the observation. Multiple value types are supported: SampledData, CodeableConcept, and Quantity.  In addition to these configurable values the identifier for the observation, along with linking to the proper device and patient are handled automatically. An additional code used by IoMT FHIR Connector for Azure is also added.

| Property | Description 
| --- | ---
|**TypeName**| The type of measurement this template should bind to. There should be at least one DeviceContent template that outputs this type.
|**PeriodInterval**|The period of time the observation created should represent. Supported values are 0 (an instance), 60 (an hour), 1440 (a day).
|**Category**|Any number of [CodeableConcepts](http://hl7.org/fhir/datatypes-definitions.html#codeableconcept) to classify the type of observation created.
|**Codes**|One or more [Codings](http://hl7.org/fhir/datatypes-definitions.html#coding) to apply to the observation created.
|**Codes[].Code**|The code for the [Coding](http://hl7.org/fhir/datatypes-definitions.html#coding).
|**Codes[].System**|The system for the [Coding](http://hl7.org/fhir/datatypes-definitions.html#coding).
|**Codes[].Display**|The display for the [Coding](http://hl7.org/fhir/datatypes-definitions.html#coding).
|**Value**|The value to extract and represent in the observation. See [Value Type Templates](#valuetypes) for more information.
|**Components**|*Optional:* One or more components to create on the observation.
|**Components[].Codes**|One or more [Codings](http://hl7.org/fhir/datatypes-definitions.html#coding) to apply to the component.
|**Components[].Value**|The value to extract and represent in the component. See [Value Type Templates](#valuetypes) for more information.

## Value Type Templates <a name="valuetypes"></a>
Below are the currently supported value type templates. In the future further templates may be added.
### SampledData
Represents the [SampledData](http://hl7.org/fhir/datatypes.html#SampledData) FHIR data type. Measurements are written to value stream starting with start of the observations and incrementing forward using the period defined.  If no value is present an `E` will be written into the data stream.  If the period is such that two more values occupy the same position in the data stream the latest value is used.  The same logic is applied when an observation using the SampledData is updated.

| Property | Description 
| --- | ---
|**DefaultPeriod**|The default period in milliseconds to use. 
|**Unit**|The unit to set on the origin of the SampledData. 

### Quantity
Represents the [Quantity](http://hl7.org/fhir/datatypes.html#Quantity) FHIR data type.  If more than one value is present in the grouping only the first value is used.  If new value arrives that maps to the same observation it will overwrite the old value.

| Property | Description 
| --- | --- 
|**Unit**| Unit representation.
|**Code**| Coded form of the unit.
|**System**| System that defines the coded unit form.

### CodeableConcept
Represents the [CodeableConcept](http://hl7.org/fhir/datatypes.html#CodeableConcept) FHIR data type. The actual value isn't used.

| Property | Description 
| --- | --- 
|**Text**|Plain text representation. 
|**Codes**|One or more [Codings](http://hl7.org/fhir/datatypes-definitions.html#coding) to apply to the observation created.
|**Codes[].Code**|The code for the [Coding](http://hl7.org/fhir/datatypes-definitions.html#coding).
|**Codes[].System**|The system for the [Coding](http://hl7.org/fhir/datatypes-definitions.html#coding).
|**Codes[].Display**|The display for the [Coding](http://hl7.org/fhir/datatypes-definitions.html#coding).

## Examples
**Heart Rate - Sampled Data**
```json
{
    "templateType": "CodeValueFhir",
    "template": {
        "codes": [
            {
                "code": "8867-4",
                "system": "http://loinc.org",
                "display": "Heart rate"
            }
        ],
        "periodInterval": 60,
        "typeName": "heartrate",
        "value": {
            "defaultPeriod": 5000,
            "unit": "count/min",
            "valueName": "hr",
            "valueType": "SampledData"
        }
    }
}
```
---
**Steps - Sampled Data**
```json
{
    "templateType": "CodeValueFhir",
    "template": {
        "codes": [
            {
                "code": "55423-8",
                "system": "http://loinc.org",
                "display": "Number of steps"
            }
        ],        
        "periodInterval": 60,
        "typeName": "stepsCount",
        "value": {
            "defaultPeriod": 5000,
            "unit": "",
            "valueName": "steps",
            "valueType": "SampledData"
        }
    }
}
```
---
**Blood Pressure - Sampled Data**
```json
{
    "templateType": "CodeValueFhir",
    "template": {
        "codes": [
            {
                "code": "85354-9",
                "display": "Blood pressure panel with all children optional",
                "system": "http://loinc.org"
            }
        ],
        "periodInterval": 60,
        "typeName": "bloodpressure",
        "components": [
            {
                "codes": [
                    {
                        "code": "8867-4",
                        "display": "Diastolic blood pressure",
                        "system": "http://loinc.org"
                    }
                ],
                "value": {
                    "defaultPeriod": 5000,
                    "unit": "mmHg",
                    "valueName": "diastolic",
                    "valueType": "sampledData"
                }
            },
            {
                "codes": [
                    {
                        "code": "8480-6",
                        "display": "Systolic blood pressure",
                        "system": "http://loinc.org"
                    }
                ],
                "value": {
                    "defaultPeriod": 5000,
                    "unit": "mmHg",
                    "valueName": "systolic",
                    "valueType": "sampledData"
                }
            }
        ]
    }
}
```
---
**Blood Pressure - Quantity**
```json
{
    "templateType": "CodeValueFhir",
    "template": {
        "codes": [
            {
                "code": "85354-9",
                "display": "Blood pressure panel with all children optional",
                "system": "http://loinc.org"
            }
        ],
        "periodInterval": 0,
        "typeName": "bloodpressure",
        "components": [
            {
                "codes": [
                    {
                        "code": "8867-4",
                        "display": "Diastolic blood pressure",
                        "system": "http://loinc.org"
                    }
                ],
                "value": {
                    "unit": "mmHg",
                    "valueName": "diastolic",
                    "valueType": "quantity"
                }
            },
            {
                "codes": [
                    {
                        "code": "8480-6",
                        "display": "Systolic blood pressure",
                        "system": "http://loinc.org"
                    }
                ],
                "value": {
                    "unit": "mmHg",
                    "valueName": "systolic",
                    "valueType": "quantity"
                }
            }
        ]
    }
}
```
---
**Device Removed - Codeable Concept**
```json
{
    "templateType": "CodeValueFhir",
    "template": {
        "codes": [
            {
                "code": "deviceEvent",
                "system": "https://www.mydevice.com/v1",
                "display": "Device Event"
            }
        ],
        "periodInterval": 0,
        "typeName": "deviceRemoved",
        "value": {
            "text": "Device Removed",
            "codes": [
                {
                    "code": "deviceRemoved",
                    "system": "https://www.mydevice.com/v1",
                    "display": "Device Removed"
                }
            ],
            "valueName": "deviceRemoved",
            "valueType": "codeableConcept"
        }
    }
}
```
---
FHIR&reg; is the registered trademark of HL7 and is used with the permission of HL7. 