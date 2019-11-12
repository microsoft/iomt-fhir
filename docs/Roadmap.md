# Roadmap
This document contains a list of Microsoft IoMT FHIR Connector for Azure features that are in various stages of consideration or implementation.  Feedback on the features presented below is appreciated.
 
## New Operation for Updating Observations
When the IoMT FHIR Connector stream analytics job buffering data is set to a small period of time (under 15 minutes) high frequency data can cause the history trail for the observation being updated to become quite large.  The proposal is to add an operation to the FHIR observation resource to support updates with out generating history records.  Using the operation would keep the observation in a preliminary status.  Once the observation is complete its status would be changed to final and updates past that stage would generate history records as normal.

## Configurable Caching Layer
Currently resource identities and observations are cached in memory of the App Service running the FHIR conversion service. This feature would add support for a configurable caching layer, i.e. [Azure Cache for Redis](https://azure.microsoft.com/en-us/services/cache/) or other distributed caching options.

## Support Multiple Device Identifier Systems
Currently look ups on device identifiers are limited to no system or at most one system.  This feature would allow users of the system to classify devices and associate a system with the classification which then would be used during device lookup or creation on the FHIR server (depending on the operating mode selected). The exact nature of this classification and association is under discussion and input as to what would be useful is welcome.

## Support Binding Content to DocumentReference Resources
In some IoMT scenarios the data being generated is either too large or not appropriate to store directly in an observation (images, video, etc.).  In order to support these scenarios the feature being considered would add a new FHIR mapping template that would allow the creation of DocumentReference resource linked to the patient and device in place of an observation.  The expectation is the value mapped in the normalization step would be a URI to the storage location that the content was uploaded to (i.e. Blob storage).

## Alternatives to SampledData Data Type
Currently the IoMT FHIR Connecter for Azure represents high frequency time series data using the [SampledData](https://www.hl7.org/fhir/datatypes.html#SampledData) data type in FHIR.  The SampledData type works great for high precision devices recording data with a constant period.  Problems arise when the device sending data transmit values intermittently or have variable period.  These can be represented with the SampledData type but either require the period to be set very precise resulting in a value stream filled predominantly with "E"'s (used to represent missing or non present values).

One potential alternative is introducing a new data type (or modifying SampledData) to include a second dimension for the time component which represents the offset in milliseconds for the corresponding ordinal position in the value dimension stream.  The millisecond value would be added to the start value for the observation's effective period.  You could still do this with SampledData using a 2+ dimensions.  The limitation with this approach is two fold.  The period in SampledData is required and including a period.  Including a period and also a second dimension representing the time offset could lead to confusion in implementers reading the value. The other problem is the identity of the ordinal positions in the stream has to be implicitly known.  Is the time value the first dimension, the second, or 3+.  It would need to be established by convention.  At that point explicitly defining it in the data type seems like a better approach.

## Operation to Return SampledData Observations in a Tabular Format
One downside of the SampledData format is an implementer reading observations needs to project the values using the defined period to represent the values with their corresponding date time value.  The process becomes more cumbersome when you have several observations in the time period you want to observe.  The proposal is to add an operation to the FHIR observation resource to allow matching observations with the SampledData data type to project the values in single tabular representation allowing for easier processing and display.

## Create Provenance Resources for Observations Created
This feature would allow creating Provenance resources linked to the observations created by the IoMT FHIR Connector for Azure.
