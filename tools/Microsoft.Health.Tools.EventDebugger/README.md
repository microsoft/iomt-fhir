# IoT Connector Event Debugger

## Introduction

The IoT Connector Event Debugger allows local verificaiton of the normalization and transformation functions provided by the IoT Connector. This is done by processing sample device events against IoT Connector mapping files. The results of the processing are then immediately made available to the developer. This provides the developer an innerloop experience in which mapping file adjusted are made and the debugging tool can be run again. 

The following details are relayed to the developer after running the tool:

- The sample device event
- Errors within the template itself
- Issues between the templates
- The measurements that were projected from the sample device data
- FHIR Observations that were created from the measurements.
  - **NOTE**: The debugging tool does NOT connect to a FHIR Server. Data required from a FHIR Server (i.e. Resource Identifiers) will not appear inside of Observations.

## Setup and Requirement

### .Net Core
The application requires .Net Core 3.1 and above. This can be found [here](https://dotnet.microsoft.com/en-us/download/dotnet/3.1).

## Getting Started

IoT Connector Event Debugger is a console app and can be executed via `dotnet run`

1. Clone the repository. Go to the directory of this tool from the root of the repository.

   ```console
   cd tools/tools/Microsoft.Health.Tools.EventDebugger
   ```

2. Run the application.

   ```console
   dotnet run 
   ```

## Commands
The following commands are available to run from within the debugger

### Validate
Runs the debugger against a specific device event and mapping files.

#### Required Parameters
##### --deviceMapping
The path to the device mapping template file

#### Optional Parameters
##### --fhirMapping
The path to the fhir mapping template file

##### --deviceData
The path to the file containing sample device data

Example:
```console
dotnet run validate --deviceData devicecontent.json --deviceMapping devicecontent.json --fhirMapping fhirmapping.json
```
### Replay
Connects to an Azure EventHub, pulls down events and runs them through the debugger

Azure EventHub can retain data for a [certain period of time](https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-faq#what-is-the-maximum-retention-period-for-events-). The use of this command allows developers to replay recent events that have already been processed by the system to help debug issues.
 
#### Required Parameters
##### --deviceMapping
The path to the device mapping template file

##### --connectionString
The connection string to the EventHub

##### --consumerGroup
The EventHub consumer group

#### Optional Parameters
##### --fhirMapping
The path to the fhir mapping template file

##### --totalEventsToProcess
Total number of events that should be replayed

##### --eventReadTimeout
The amount of time to wait for new messages to appear. Specified as a .Net Timespan. Application will end if this timeout is reached

##### --outputDirectory
The directory to write debugging results

##### --enqueuedTime
A specific date and time from which events will begin to be read from the EventHub. If not supplied events will be read from the beginning of the EventHub. Example: `2021-12-29T18:19:18.000-08:00`

Example:
```console
dotnet run replay 
    --consumerGroup "\$Default"
    --connectionString "<a connection string>" 
    --deviceMapping devicecontent.json
    --fhirMapping fhirmapping.json
    --enqueuedTime "2021-12-29T18:19:18.000-08:00"
```

## Example Debugging Output
Given the followng [sample data](sampleData/deviceData.json), [device mapping](sampleData/devicecontent.json) and [fhir mapping](sampleData/fhirmapping.json), the following debug output is produced.

```json
{
  "TemplateDetails": {
    "Exceptions": [
      {
        "Message": "The value [HeartRate] in Device Mapping [heartrate] is not represented within the Fhir Template of type [heartrate]. Available values are: [hr]. No value will appear inside of Observations.",
        "Level": "WARN"
      },
      {
        "Message": "No matching Fhir Template exists for Device Mapping [bodyweight]. Ensure case matches. Available Fhir Templates: [heartrate ,bloodpressure].",
        "Level": "WARN"
      },
      {
        "Message": "The value [systolic] in Device Mapping [bloodpressure] is not represented within the Fhir Template of type [bloodpressure]. Available values are: [diastolic ,systolicFake]. No value will appear inside of Observations.",
        "Level": "WARN"
      }
    ]
  },
  "DeviceDetails": {
    "DeviceEvent": {
      "Body": {
        "patientid": "patient_1_717654",
        "deviceid": "device_1_717654",
        "systolic": "149",
        "diastolic": "102",
        "measurementdatetime": "2022-01-03T10:28:01Z"
      },
      "Properties": {},
      "SystemProperties": {
        "x-opt-sequence-number": 1674844,
        "x-opt-offset": 64424509440,
        "x-opt-enqueued-time": "2022-01-03T18:28:02.535+00:00"
      }
    },
    "Measurements": [
      {
        "Type": "bloodpressure",
        "OccurrenceTimeUtc": "2022-01-03T10:28:01Z",
        "DeviceId": "device_1_717654",
        "PatientId": "patient_1_717654",
        "Properties": [
          {
            "Name": "systolic",
            "Value": "149"
          },
          {
            "Name": "diastolic",
            "Value": "102"
          }
        ]
      }
    ],
    "Observations": [
      {
        "StatusElement": {
          "Value": "Final"
        },
        "Code": {
          "Coding": [
            {
              "SystemElement": {
                "Value": "http://loinc.org"
              },
              "CodeElement": {
                "Value": "85354-9"
              },
              "DisplayElement": {
                "Value": "Blood pressure panel"
              }
            },
            {
              "SystemElement": {
                "Value": "https://azure.microsoft.com/en-us/services/iomt-fhir-connector/"
              },
              "CodeElement": {
                "Value": "bloodpressure"
              },
              "DisplayElement": {
                "Value": "bloodpressure"
              }
            }
          ],
          "TextElement": {
            "Value": "bloodpressure"
          }
        },
        "Effective": {
          "StartElement": {
            "Value": "2022-01-03T10:00:00.0000000Z"
          },
          "EndElement": {
            "Value": "2022-01-03T10:59:59.9999999Z"
          }
        },
        "IssuedElement": {
          "Value": "2022-01-03T18:28:42.327441+00:00"
        },
        "Component": [
          {
            "Code": {
              "Coding": [
                {
                  "SystemElement": {
                    "Value": "http://loinc.org"
                  },
                  "CodeElement": {
                    "Value": "8867-4"
                  },
                  "DisplayElement": {
                    "Value": "Diastolic blood pressure"
                  }
                },
                {
                  "SystemElement": {
                    "Value": "https://azure.microsoft.com/en-us/services/iomt-fhir-connector/"
                  },
                  "CodeElement": {
                    "Value": "diastolic"
                  },
                  "DisplayElement": {
                    "Value": "diastolic"
                  }
                }
              ],
              "TextElement": {
                "Value": "diastolic"
              }
            },
            "Value": {
              "Origin": {
                "ValueElement": {
                  "Value": 0.0
                },
                "UnitElement": {
                  "Value": "mmHg"
                }
              },
              "PeriodElement": {
                "Value": 1000.0
              },
              "DimensionsElement": {
                "Value": 1
              },
              "DataElement": {
                "Value": "E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E 102 E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E E"
              }
            }
          }
        ]
      }
    ]
  }
}
```