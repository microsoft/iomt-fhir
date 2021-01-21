// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import { Mapping, FhirValue } from "../store/Mapping";

export const generateDeviceTemplate = (mappings: Mapping[]) => {
    const deviceTemplates = {
        templateType: "CollectionContent",
        template: [] as any
    }
    mappings.forEach(m => {
        const mappingTemplate = {
            templateType: 'JsonPathContent',
            template: {
                typeName: m.typeName,
                typeMatchExpression: m.device?.typeMatchExpression,
                timestampExpression: m.device?.timestampExpression,
                deviceIdExpression: m.device?.deviceIdExpression,
                patientIdExpression: m.device?.patientIdExpression,
                encounterIdExpression: m.device?.encounterIdExpression,
                correlationIdExpression: m.device?.correlationIdExpression,
                values: m.device?.values
            }
        } as any;
        deviceTemplates.template.push(mappingTemplate)
    })

    return JSON.stringify(deviceTemplates, null, 4);
}

const generateFhirValueTemplate = (fhirValue: FhirValue) => {
    if (fhirValue) {
        switch (fhirValue.valueType) {
            case "SampledData":
                return {
                    valueName: fhirValue.valueName,
                    valueType: fhirValue.valueType,
                    ...fhirValue.sampledData
                }
            case "Quantity":
                return {
                    valueName: fhirValue.valueName,
                    valueType: fhirValue.valueType,
                    ...fhirValue.quantity
                }
            case "String":
            default:
                return {
                    valueName: fhirValue.valueName,
                    valueType: fhirValue.valueType,
                };
        }
    }
    return {};
}


export const generateFhirTemplate = (mappings: Mapping[]) => {

    const fhirTemplates = {
        templateType: "CollectionFhir",
        template: [] as any
    }

    mappings.forEach(m => {
        const mappingTemplate = {
            templateType: "CodeValueFhir",
            template: {
                typeName: m.typeName,
                periodInterval: m.fhir?.periodInterval
            }
        } as any;

        if (m.fhir?.value) {
            mappingTemplate.template.value = generateFhirValueTemplate(m.fhir?.value);
        }

        if (m.fhir?.components) {
            mappingTemplate.template.components = [];
            m.fhir.components.forEach(c =>
                mappingTemplate.template.components.push({
                    codes: c.codes,
                    value: generateFhirValueTemplate(c.value)
                })
            );
        }

        if (m.fhir?.codes) {
            mappingTemplate.template.codes = m.fhir?.codes;
        }

        if (m.fhir?.category) {
            mappingTemplate.template.category = m.fhir?.category;
        }

        fhirTemplates.template.push(mappingTemplate)
    })

    return JSON.stringify(fhirTemplates, null, 4);
}

export const sanitize = (text: string) => {
    var element = document.createElement('div');
    element.innerText = text;
    return element.innerHTML;
}