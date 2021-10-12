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

const generateFhirCodesMapping = (codes: any): FhirCoding[] => {
    let codings: FhirCoding[] = [];
    if (codes) {
        for (const code of codes) {
            codings.push({
                code: code.code,
                display: code.display,
                system: code.system
            } as FhirCoding);
        }
    }
    return codings;
}

const generateFhirValueMapping = (value: any): FhirValue => {
    if (value) {
        return {
            valueType: value.valueType,
            valueName: value.valueName,
            sampledData: value.valueType == FhirValueType.SampledData && {
                defaultPeriod: value.defaultPeriod,
                unit: value.unit
            },
            quantity: value.valueType == FhirValueType.Quantity && {
                unit: value.unit,
                code: value.code,
                system: value.system
            }
        } as FhirValue;
    }
    return {} as FhirValue;
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
                if (subTemplate.category) {
                    for (const category of subTemplate.category) {
                        categories.push({
                            text: category.text,
                            codes: generateFhirCodesMapping(category.codes)
                        } as FhirCodeableConcept);
                    }
                }

                if (subTemplate.value?.valueType && subTemplate.value?.valueType == FhirValueType.CodeableConcept) {
                    console.log(`FHIR Mapping Template note: CodeableConcept values are not supported in the Data Mapper yet. "${typename}" will be imported without a value.`)
                }

                let components: FhirComponent[] = [];
                if (subTemplate.components) {
                    for (const component of subTemplate.components) {
                        if (component.value?.valueType && component.value?.valueType == FhirValueType.CodeableConcept) {
                            console.log(`FHIR Mapping Template note: CodeableConcept values are not supported in the Data Mapper yet. "${typename}" will be imported without CodeableConcept component values.`)
                        }

                        components.push({
                            value: generateFhirValueMapping(component.value),
                            codes: generateFhirCodesMapping(component.codes)
                        } as FhirComponent);
                    }
                }

                let fhirMapping = {
                    periodInterval: subTemplate.periodInterval,
                    category: categories,
                    codes: generateFhirCodesMapping(subTemplate.codes),
                    value: generateFhirValueMapping(subTemplate.value),
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