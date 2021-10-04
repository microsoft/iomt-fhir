// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as _ from "lodash";
import { DeviceMapping, DeviceValueSet, FhirCodeableConcept, FhirCoding, FhirComponent, FhirMapping, FhirValue, FhirValueType, Mapping } from "../store/Mapping";

export const generateDeviceMappings = (deviceTemplates: string): Promise<Mapping[]> => {
    return new Promise((resolve, reject) => {
        let mappings: Mapping[] = [];
        const errorPrefix = 'Device Mapping Template error:'

        if (!deviceTemplates || deviceTemplates.trim().length < 1) {
            resolve(mappings);
            return;
        }

        let templates: any;
        try {
            templates = JSON.parse(deviceTemplates);
        }
        catch (err) {
            reject(`${errorPrefix} ${err.message}.`);
            return;
        }

        if (templates.template) {
            for (const template of templates.template) {
                const subTemplate = template.template;
                if (!subTemplate) {
                    continue;
                }

                const typename = subTemplate.typeName;
                if (!typename || typename.trim().length < 1) {
                    reject(`${errorPrefix} The type name cannot be empty.`);
                    return;
                }
                if (_.find(mappings, { typeName: typename })) {
                    reject(`${errorPrefix} More than one template has the typeName "${typename}".
                        Please enter unique typeNames for these templates or remove the templates with duplicate typeNames.`);
                    return;
                }

                let values: DeviceValueSet[] = [];
                if (subTemplate.values) {
                    for (const value of subTemplate.values) {
                        values.push({
                            valueExpression: value.valueExpression,
                            valueName: value.valueName,
                            required: value.required
                        } as DeviceValueSet);
                    }
                }

                const deviceMapping = {
                    typeMatchExpression: subTemplate.typeMatchExpression,
                    timestampExpression: subTemplate.timestampExpression,
                    deviceIdExpression: subTemplate.deviceIdExpression,
                    patientIdExpression: subTemplate.patientIdExpression,
                    encounterIdExpression: subTemplate.encounterIdExpression,
                    correlationIdExpression: subTemplate.correlationIdExpression,
                    values: values
                } as DeviceMapping;

                const mapping = {
                    typeName: typename,
                    device: deviceMapping
                } as Mapping;

                mappings.push(mapping);
            }
        }

        resolve(mappings);
        return;
    });
}

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

export const generateFhirMappings = (fhirTemplates: string): Promise<Mapping[]> => {
    return new Promise((resolve, reject) => {
        let mappings: Mapping[] = [];
        const errorPrefix = 'FHIR Mapping Template error:'

        if (!fhirTemplates || fhirTemplates.trim().length < 1) {
            resolve(mappings);
            return;
        }

        let templates: any;
        try {
            templates = JSON.parse(fhirTemplates);
        }
        catch (err) {
            reject(`${errorPrefix} ${err.message}.`);
            return;
        }

        if (templates.template) {
            for (const template of templates.template) {
                const subTemplate = template.template;
                if (!subTemplate) {
                    continue;
                }

                const typename = subTemplate.typeName;
                if (!typename || typename.trim().length < 1) {
                    reject(`${errorPrefix} The type name cannot be empty.`);
                    return;
                }
                if (_.find(mappings, { typeName: typename })) {
                    reject(`${errorPrefix} More than one template has the typeName "${typename}".
                        Please enter unique typeNames for these templates or remove the templates with duplicate typeNames.`);
                    return;
                }

                let categories: FhirCodeableConcept[] = [];
                if (subTemplate.value?.valueType && subTemplate.value?.valueType == FhirValueType.CodeableConcept) {
                    let codes: FhirCoding[] = [];
                    if (subTemplate.value?.codes) {
                        for (const code of subTemplate.value?.codes) {
                            codes.push({
                                code: code.code,
                                display: code.display,
                                system: code.system
                            } as FhirCoding);
                        }
                    }

                    categories.push({
                        text: subTemplate.value?.text,
                        codes: codes
                    } as FhirCodeableConcept);
                }

                let codes: FhirCoding[] = [];
                if (subTemplate.codes) {
                    for (const code of subTemplate.codes) {
                        codes.push({
                            code: code.code,
                            display: code.display,
                            system: code.system
                        } as FhirCoding);
                    }
                }

                const value = {
                    valueType: subTemplate.value?.valueType,
                    valueName: subTemplate.value?.valueName,
                    sampledData: subTemplate.value?.valueType == FhirValueType.SampledData && {
                        defaultPeriod: subTemplate.value?.defaultPeriod,
                        unit: subTemplate.value?.unit
                    },
                    quantity: subTemplate.value?.valueType == FhirValueType.Quantity && {
                        unit: subTemplate.value?.unit,
                        code: subTemplate.value?.code,
                        system: subTemplate.value?.system
                    }
                } as FhirValue;

                let components: FhirComponent[] = [];
                if (subTemplate.components) {
                    for (const component of subTemplate.components) {
                        let codes: FhirCoding[] = [];
                        if (component.codes) {
                            for (const code of component.codes) {
                                codes.push({
                                    code: code.code,
                                    display: code.display,
                                    system: code.system
                                } as FhirCoding);
                            }
                        }

                        components.push({
                            value: {
                                valueType: component.value?.valueType,
                                valueName: component.value?.valueName,
                                sampledData: component.value?.valueType == FhirValueType.SampledData && {
                                    defaultPeriod: component.value?.defaultPeriod,
                                    unit: component.value?.unit
                                },
                                quantity: component.value?.valueType == FhirValueType.Quantity && {
                                    unit: component.value?.unit,
                                    code: component.value?.code,
                                    system: component.value?.system
                                }
                            },
                            codes: codes
                        } as FhirComponent);
                    }
                }

                let fhirMapping = {
                    periodInterval: subTemplate.periodInterval,
                    category: categories,
                    codes: codes,
                    value: value,
                    components: components
                } as FhirMapping;

                const mapping = {
                    typeName: typename,
                    fhir: fhirMapping
                } as Mapping;

                mappings.push(mapping);
            }
        }

        resolve(mappings);
        return;
    });
}

const generateFhirValueTemplate = (fhirValue: FhirValue) => {
    if (fhirValue) {
        switch (fhirValue.valueType) {
            case FhirValueType.SampledData:
                return {
                    valueName: fhirValue.valueName,
                    valueType: fhirValue.valueType,
                    ...fhirValue.sampledData
                }
            case FhirValueType.Quantity:
                return {
                    valueName: fhirValue.valueName,
                    valueType: fhirValue.valueType,
                    ...fhirValue.quantity
                }
            case FhirValueType.String:
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