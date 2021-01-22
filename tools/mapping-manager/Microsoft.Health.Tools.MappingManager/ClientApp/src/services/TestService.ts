// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

// -------------
// Testing Services
import { Mapping } from "../store/Mapping";
import * as Utils from "./Utils";

const testNormalization = (mapping: Mapping, deviceSample: string) => {
    return fetch("api/test-normalization", {
        method: "post",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },

        //make sure to serialize your JSON body
        body: JSON.stringify({
            DeviceMapping: Utils.generateDeviceTemplate([mapping]),
            DeviceSample: deviceSample
        })
    });
}

const testFhirTransformation = (mapping: Mapping, normalizedData: string) => {
    return fetch("api/test-transformation", {
        method: "post",
        headers: {
            'Accept': 'application/json',
            'Content-Type': 'application/json'
        },

        //make sure to serialize your JSON body
        body: JSON.stringify({
            FhirMapping: Utils.generateFhirTemplate([mapping]),
            NormalizedData: normalizedData,
            FhirVersion: "R4",
            FhirIdentityResolutionType: mapping.identityResolutionType
        })
    });
}

const TestService = {
    testNormalization: testNormalization,
    testFhirTransformation: testFhirTransformation,
}

export default TestService;

