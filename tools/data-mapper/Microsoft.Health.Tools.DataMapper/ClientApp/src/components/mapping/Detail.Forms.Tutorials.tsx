// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { Card, CardTitle, CardBody } from 'reactstrap';

export const DeviceEditFormTutorial = (props: {}) => {
    return (
        <Card>
            <CardBody>
                <CardTitle>
                    <span className="h6">Tutorial</span>
                </CardTitle>
                <p>
                    Device Mapping is used to normalize the data input from your devices.
                </p>
                Note: <br />
                <ul>
                    <li>Field with * is required.</li>
                    <li>Patient ID Expression is required when you choose "Create" as Identity Resolution Type.</li>
                </ul>
                <p>
                    Let's start with one example. Your data input from a heart rate measuring device is as below and its data type name is "HeartRate".
                </p>
                <pre>
                    {
                        JSON.stringify({
                            "payload": {
                                "heartRate": "78",
                            },
                            "metadata": {
                                "measuringTime": "2019-02-01T22:46:01.8750000Z",
                                "deviceId": "dsn-9cvb89",
                                "userId": "uid-12hfd8"
                            }
                        }, null, 4)
                    }
                </pre>
                <p>
                    You will need to fill in below information:
                </p>
                <table className="m-3">
                    <tbody>
                        <tr>
                            <td className="pr-4">Type Match Expression:</td>
                            <td>
                                <pre className="d-inline-flex m-0">$..[?(@payload.heartRate)]</pre>
                            </td>
                        </tr>
                        <tr>
                            <td className="pr-4">Device ID Expression:</td>
                            <td>
                                <pre className="d-inline-flex m-0">$.metadata.deviceId</pre>
                            </td>
                        </tr>
                        <tr>
                            <td className="pr-4">Timestamp Expression:</td>
                            <td>
                                <pre className="d-inline-flex m-0">$.metadata.measuringTime</pre>
                            </td>
                        </tr>
                        <tr>
                            <td className="pr-4">Patient ID Expression:</td>
                            <td>
                                <pre className="d-inline-flex m-0">$.metadata.userId</pre>
                            </td>
                        </tr>
                        <tr>
                            <td className="pr-4" colSpan={2}>Values:</td>
                        </tr>
                        <tr>
                            <td className="pl-4 pr-4">Value Name:</td>
                            <td>
                                <pre className="d-inline-flex m-0">Heart Rate</pre>
                            </td>
                        </tr>
                        <tr>
                            <td className="pl-4 pr-4">Value Expression:</td>
                            <td>
                                <pre className="d-inline-flex m-0">$.payload.heartRate</pre>
                            </td>
                        </tr>
                        <tr>
                            <td className="pl-4 pr-4">Required:</td>
                            <td>
                                <pre className="d-inline-flex m-0">True</pre>
                            </td>
                        </tr>
                    </tbody>
                </table>
                <p>
                    For more information, please visit: <a href="https://github.com/microsoft/iomt-fhir/blob/main/docs/Configuration.md">IoMT Mapping Configuration</a>
                </p>
            </CardBody>
        </Card>
    );
}

export const FhirValueFormTutorial = (props: {}) => {
    return (
        <Card>
            <CardBody>
                <CardTitle>
                    <span className="h6">Tutorial</span>
                </CardTitle>
                <p>
                    Configure the tranfromation of the normalized device value data to FHIR Resource.
                    <a href="https://www.hl7.org/fhir/observation-definitions.html#Observation.value_x_">Observation.Value</a>.
                </p>
                Note: <br />
                <ul>
                    <li>You should use this for single value transformation. For multiple values, please consider using FHIR Components.</li>
                    <li>Value Names are referred to Value Expressions defined on the Device Mapping.</li>
                </ul>
                <p>
                    Observation Value Mapping with the example HeartRate from the Device Mapping:
                </p>
                <table className="m-3">
                    <tbody>
                        <tr>
                            <td className="pr-4">Value Name:</td>
                            <td>
                                <pre className="d-inline-flex m-0">Heart Rate</pre>
                            </td>
                        </tr>
                        <tr>
                            <td className="pr-4">Value Type:</td>
                            <td>
                                <pre className="d-inline-flex m-0">SampledData</pre>
                            </td>
                        </tr>
                        <tr>
                            <td className="pr-4">Default Period in Millisecond:</td>
                            <td>
                                <pre className="d-inline-flex m-0">30000</pre>
                            </td>
                        </tr>
                        <tr>
                            <td className="pr-4">Unit:</td>
                            <td>
                                <pre className="d-inline-flex m-0">count/min</pre>
                            </td>
                        </tr>
                    </tbody>
                </table>
                <p>
                    For more information, please visit: <a href="https://github.com/microsoft/iomt-fhir/blob/main/docs/Configuration.md">IoMT Mapping Configuration</a>
                </p>
            </CardBody>
        </Card>
    );
}

export const FhirComponentsFormTutorial = (props: {}) => {
    return (
        <Card>
            <CardBody>
                <CardTitle>
                    <span className="h6">Tutorial</span>
                </CardTitle>
                <p>
                    Configure the transformation of the normalized device value data to FHIR Resource: <a href="https://www.hl7.org/fhir/observation-definitions.html#Observation.component">Observation.Components</a>
                </p>
                Note: <br />
                <ul>
                    <li>You should use this for transformation of multiple values.</li>
                    <li>Value Names are referred to Value Expressions defined on the Device Mapping.</li>
                    <li>Codings of each component should be specified to the value in it.</li>
                </ul>
                <p>
                    Please check "Heart Rate - Sampled Data" as an example in <a href="https://github.com/microsoft/iomt-fhir/blob/main/docs/Configuration.md">IoMT Mapping Configuration</a>
                </p>
            </CardBody>
        </Card>
    );
}

export const FhirCodesFormTutorial = (props: {}) => {
    return (
        <Card>
            <CardBody>
                <CardTitle>
                    <span className="h6">Tutorial</span>
                </CardTitle>
                <p>
                    Add <a href="https://www.hl7.org/fhir/observation-definitions.html#Observation.code">Observation.code</a> to the final FHIR Observation resource.
                </p>
                <p>
                    For more information, please visit: <a href="https://github.com/microsoft/iomt-fhir/blob/main/docs/Configuration.md">IoMT Mapping Configuration</a>
                </p>
            </CardBody>
        </Card>
    );
}

export const FhirCategoryFormTutorial = (props: {}) => {
    return (
        <Card>
            <CardBody>
                <CardTitle>
                    <span className="h6">Tutorial</span>
                </CardTitle>
                <p>
                    Add <a href="https://www.hl7.org/fhir/observation-definitions.html#Observation.category">Observation.category</a> to the final FHIR Observation resource.
                </p>
                <p>
                    This is optional information.
                </p>
                <p>
                    For more information, please visit: <a href="https://github.com/microsoft/iomt-fhir/blob/main/docs/Configuration.md">IoMT Mapping Configuration</a>
                </p>
            </CardBody>
        </Card>
    );
}

export const FhirGroupingGroupTutorial = (props: {}) => {
    return (
        <Card>
            <CardBody>
                <CardTitle>
                    <span className="h6">Tutorial</span>
                </CardTitle>
                <p>
                    Choose a grouping logic. This will determine how we merge the data values in a designated time window onto a single FHIR Observation resource.
                    You can also skip grouping, so each data input will be transformed to single observation resource.
                </p>
                Available Options: <br />
                <ul>
                    <li>No Grouping</li>
                    <li>Correlation ID Grouping</li>
                    <li>1 Hour</li>
                    <li>1 Day</li>
                </ul>
                <p>
                    For more information, please check "PeriodInterval" in: <a href="https://github.com/microsoft/iomt-fhir/blob/main/docs/Configuration.md">IoMT Mapping Configuration</a>
                </p>
            </CardBody>
        </Card>
    );
}
