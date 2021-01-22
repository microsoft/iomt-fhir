// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

// -----------------
// STATE - This defines the type of data maintained in the Redux store.

export interface DeviceValueSet {
    valueExpression: string;
    valueName: string;
    required: boolean;
}

export interface DeviceMapping {
    dataType: string;
    typeMatchExpression: string;
    deviceIdExpression: string;
    timestampExpression: string;
    patientIdExpression: string;
    correlationIdExpression: string;
    encounterIdExpression: string;
    values: DeviceValueSet[];
}

export interface FhirCoding {
    code: string;
    display: string;
    system: string;
}

export interface FhirValue {
    valueType: string;
    valueName: string;
    sampledData: FhirSampledData;
    quantity: FhirQuantity;
    string: string;
}

export interface FhirSampledData {
    defaultPeriod: number;
    unit: string;
}

export interface FhirQuantity {
    unit: string;
    code: string;
    system: string;
}

export interface FhirComponent {
    value: FhirValue;
    codes: FhirCoding[];
}

export interface FhirCodeableConcept {
    text: string;
    codes: FhirCoding[];
}

export interface FhirMapping {
    // TODO: make this enum;
    periodInterval: number;

    category: FhirCodeableConcept[];
    codes: FhirCoding[];
    value: FhirValue;
    components: FhirComponent[];
}

export interface Mapping {
    id: string;
    typeName: string;
    device: DeviceMapping;
    fhir: FhirMapping;

    // TODO: make this enum.
    identityResolutionType: string;

    // TODO: make this enum.
    fhirVersion: 'R4';
}

export interface MappingListStoreState {
    // Index with mapping Id.
    mappings: Mapping[];
    hasLoaded: boolean;
}

export interface MappingDetailStoreState {
    // Index with mapping Id.
    mapping: Mapping;
    hasLoaded: boolean;
}