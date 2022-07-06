# Mapping Generator Classes

The Microsoft.Health.Fhir.Ingest.Template.Generator project contains abstract classes that can be used to develop solutions for generating Device Content Mappings and FHIR Mappings. The most basic implementation would involve creating an implementation of one of the *TemplateGenerator classes for a specific data model and overriding the required methods. When `GenerateTemplate()` is called on the template generator instance, a Template will be generated and returned as a JObject. The JObject can be serialized and used for IoMT Connector or [MedTech services](https://docs.microsoft.com/en-us/azure/healthcare-apis/iot/iot-connector-overview).

## Template Generator Abstract Classes

There are 2 abstract classes that can be used a starting point for developing a template generator solution, [CalculatedContentTemplateGenerator](./Microsoft.Health.Fhir.Ingest.Template.Generator/CalculatedContentTemplateGenerator.cs) and [CodeValueFhirTemplateGenerator](./Microsoft.Health.Fhir.Ingest.Template.Generator/CodeValueFhirTemplateGenerator.cs).

### **[CalculatedContentTemplateGenerator](./Microsoft.Health.Fhir.Ingest.Template.Generator/CalculatedContentTemplateGenerator.cs)**

This abstract class can be used to generate templates for Device Content Mappings. The output of CalculatedContentTemplateGenerators is a serialized [CalculatedFunctionContentTemplate](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/CalculatedFunctionContentTemplate.cs) which can use both JSONPath and JMESPath to evaluate a JSON payload and "extract" values.

While there are only 4 methods that require implementation (`GetTypeName()`, `GetTypeMatchExpression()`, `GetDeviceIdExpression()` and `GetTimestampExpression()`), it is recommended that the `GetValues()` method is also implemented because this method provides the Template with the expressions required to "extract" the measurements from a JSON payload.

`GetTypeName(TModel model, CancellationToken cancellationToken)` - Must return a string that will populate the TypeName property of the Template. The TypeName property is used to correlate the Device Content Mapping templates with FHIR Mapping templates.

`GetTypeMatchExpression(TModel model, CancellationToken cancellationToken)` - Must return a [TemplateExpression](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/TemplateExpression.cs). The TypeMatchExpression property is used to identify JSON data that should be processed using the given template. Expressions can be provided in JSONPath or JMESPath formats.

`GetDeviceIdExpression(TModel model, CancellationToken cancellationToken)` - Must return a [TemplateExpression](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/TemplateExpression.cs). The DeviceIdExpression property is used to identify where the device id can be found in a JSON payload. Device Ids are used to lookup and/or create Device Resources in FHIR and the Device Resource is linked to the resulting Observation Resource.

`GetTimestampExpression(TModel model, CancellationToken cancellationToken)` - Must return a [TemplateExpression](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/TemplateExpression.cs). The TimestampExpression property is used to identify where the timestamp can be found in a JSON payload. Timestamps are used to set the time that a device measurement occurred. The timestamp is recorded to the Observation Resource.

`GetValues(TModel model, CancellationToken cancellationToken)` - Must return a list of [CalculatedFunctionValueExpressions](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/CalculatedFunctionValueExpression.cs).

**Note - If your IoMT Connector or [MedTech service](https://docs.microsoft.com/en-us/azure/healthcare-apis/iot/iot-connector-overview) has the Identity Resolution configured to [Create](../../docs/Configuration.md#configuration), the `GetPatientIdExpression()` method should also be implemented so the resulting Template contains an expression to "extract" patient ids from a JSON payload.

### **[CodeValueFhirTemplateGenerator](./Microsoft.Health.Fhir.Ingest.Template.Generator/CodeValueFhirTemplateGenerator.cs)**

This abstract class can be used to generate templates for FHIR Mappings. The output of CodeValueFhirTemplateGenerators is a serialized [CodeValueFhirTemplate](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/CodeValueFhirTemplate.cs). The `TModel` type for this generator must derive from the [Template](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/Template.cs) class. Because the `TModel` type is a Template, the CodeValueFhirTemplateGenerator can take the output of CalculatedContentTemplateGenerator to generate a FHIR Mapping template.

While there is only 1 method that requires implementation (`GetCodes()`), it is recommended that at least one other method be implemented.`GetValue()` or `GetComponents()` are used to express how the Observation Resource value is "extracted". `GetValue()` would be used if a single value is needed, while `GetComponents()` would be used when multiple related values would be saved to an Observation Resource (e.g. blood pressure might contain both diastolic and systolic measurements).

`GetCodes(TModel model, CancellationToken cancellationToken)` - Must return a list of [FhirCodes](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/FhirCode.cs). At least 1 code is required for the CodeValueFhirTemplate to pass validation. Codes are used to define the Observation Resource type in the FHIR service.

`GetValue(TModel model, CancellationToken cancellationToken)` - Should return one of the following types [CodeableConceptFhirValueType](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/CodeableConceptFhirValueType.cs), [QuantityFhirValueType](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/QuantityFhirValueType.cs), [SampledDataFhirValueType](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/SampledDataFhirValueType.cs) or [StringFhirValueType](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/StringFhirValueType.cs). This method is used to provide a value type that determines the value to "extract" and save to the Observation Resource.

`GetComponents(TModel model, CancellationToken cancellationToken)` - Must return a list of [CodeValueMappings](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/CodeValueMapping.cs). This method is used to provide a list of value mappings that determines the values to "extract" and save to the Observation Resource as components.

## Template Collection Generator

When templates are configured for use in an IoMT Connector or [MedTech service](https://docs.microsoft.com/en-us/azure/healthcare-apis/iot/iot-connector-overview), they are added to a template collection or [TemplateContainer](../../src/lib/Microsoft.Health.Fhir.Ingest.Template/TemplateContainer.cs). An implementation of [TemplateCollectionGenerator](../mapping-generator/Microsoft.Health.Fhir.Ingest.Template.Generator/TemplateCollectionGenerator.cs) can be created to generate a valid template collection that can be used with an IoMT Connector or [MedTech service](https://docs.microsoft.com/en-us/azure/healthcare-apis/iot/iot-connector-overview). When the `GenerateTemplateCollection()` is called, a collection of TModel is provided as a parameter and the collection is generated based on the implementation.

### **[TemplateCollectionGenerator](../mapping-generator/Microsoft.Health.Fhir.Ingest.Template.Generator/TemplateCollectionGenerator.cs)**

There are only 3 members that need to be implemented when creating a concrete instance of TemplateCollectionGenerator:

`RequireUniqueTemplateTypeNames` - This property is overridden to provide a bool value that will determine whether or not the generator should throw an exception if 2 TModels provided result in templates that have the same TypeName property, but different content. If true, the generator will throw an InvalidOperationException if the condition occurs. If false, both templates will be added to the collection.

`CollectionType` - Override this property to set the type of collection (type of [TemplateCollectionType](../mapping-generator/Microsoft.Health.Fhir.Ingest.Template.Generator/TemplateCollectionType.cs)). There are 2 possible values: **CollectionContent** - A collection of Device Content Mapping Templates, and **CollectionFhir** - A collection of FHIR Mapping Templates.

`GetTemplate(TModel model, CancellationToken cancellationToken)` - This method is called for each TModel provided in the collection provided in the `GenerateTemplateCollection()` call. When implemented, an instance of [CalculatedContentTemplateGenerator](./Microsoft.Health.Fhir.Ingest.Template.Generator/CalculatedContentTemplateGenerator.cs) or [CodeValueFhirTemplateGenerator](./Microsoft.Health.Fhir.Ingest.Template.Generator/CodeValueFhirTemplateGenerator.cs) can be used to convert TModel into a Template.

## Samples

A good place to view some basic implementations of the abstract classes discussed in this document is by viewing the unit test project.

- **[TestCalculatedContentTemplateGenerator](../mapping-generator/Microsoft.Health.Fhir.Ingest.Template.Generator.Tests/Samples/TestCalculatedContentTemplateGenerator.cs)**
- **[TestCodeValueFhirTemplateGenerator](../mapping-generator/Microsoft.Health.Fhir.Ingest.Template.Generator.Tests/Samples/TestCodeValueFhirTemplateGenerator.cs)**
