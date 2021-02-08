// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { FormGroup, Label, Input, Card, CardBody, CardTitle } from 'reactstrap';

import { DeviceValueSet, DeviceMapping } from '../../store/Mapping';
import * as Constants from '../Constants';
import { DeleteIcon } from '../Icons';
import * as Utility from './Utility';

const TypeMatchExpression = (props: { data: string; onUpdate: Function }) => {
    return (
        <FormGroup>
            <Label for="deviceMetaTypeMatch">{Constants.Text.LabelTypeMatchExpression}</Label>
            <Input type="text" name="deviceMetaTypeMatch" id="deviceMetaTypeMatch"
                defaultValue={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
}

const DeviceIdExpression = (props: { data: string; onUpdate: Function }) => {
    return (
        <FormGroup>
            <Label for="deviceMetaDeviceId">{Constants.Text.LabelDeviceIdExpression}</Label>
            <Input type="text" name="deviceMetaDeviceId" id="deviceMetaDeviceId"
                defaultValue={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
}

const TimestampExpression = (props: { data: string; onUpdate: Function }) => {
    return (
        <FormGroup>
            <Label for="deviceMetaTimestamp">{Constants.Text.LabelTimeStampExpression}</Label>
            <Input type="text" name="deviceMetaTimestamp" id="deviceMetaTimestamp"
                defaultValue={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
}

const PatientIdExpression = (props: { data: string; onUpdate: Function }) => {
    return (
        <FormGroup>
            <Label for="deviceMetaPatientId">{Constants.Text.LabelPatientIdExpression}</Label>
            <Input type="text" name="deviceMetaPatientId" id="deviceMetaPatientId"
                defaultValue={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
}

const EncounterIdExpression = (props: { data: string; onUpdate: Function }) => {
    return (
        <FormGroup>
            <Label for="deviceMetaEncounterId">{Constants.Text.LabelEncounterIdExpression}</Label>
            <Input type="text" name="deviceMetaEncounterId" id="deviceMetaEncounterId"
                defaultValue={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
}

const CorrelationIdExpression = (props: { data: string; onUpdate: Function }) => {
    return (
        <FormGroup>
            <Label for="deviceMetaCorrelationId">{Constants.Text.LabelCorrelationIdExpression}</Label>
            <Input type="text" name="deviceMetaCorrelationId" id="deviceMetaCorrelationId"
                defaultValue={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
}

const DeviceValueName = (props: { data: DeviceValueSet; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`deviceValueName-${props.modifier}`}>{Constants.Text.LabelDeviceValueName}</Label>
            <Input type="text" name={`deviceValueName-${props.modifier}`} id={`deviceValueName-${props.modifier}`}
                value={props.data.valueName} onChange={e => props.onUpdate(e.target.value)} />
        </FormGroup>
    );
}

const DeviceValueExpression = (props: { data: DeviceValueSet; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`deviceValueExpression-${props.modifier}`}>{Constants.Text.LabelDeviceValueExpression}</Label>
            <Input type="text" name={`deviceValueExpression-${props.modifier}`} id={`deviceValueExpression-${props.modifier}`}
                value={props.data.valueExpression} onChange={e => props.onUpdate(e.target.value)} />
        </FormGroup>
    );
}

const DeviceValueRequired = (props: { data: DeviceValueSet; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`deviceValueRequired-${props.modifier}`}>{Constants.Text.LabelDeviceValueRequired}</Label>
            <Input type="checkbox" name={`deviceValueRequired-${props.modifier}`} id={`deviceValueRequired-${props.modifier}`} style={{ left: "40%" }}
                checked={props.data.required} onChange={e => props.onUpdate(e.target.checked)} />
        </FormGroup>
    );
}

const DeviceValuesForm = (props: { data: DeviceValueSet[]; onUpdate: Function; }) => {
    // Fix the display of input array.
    const [keyModifier, refreshKeyModifier] = React.useState(Utility.getRandomString());

    const appendDeviceValueSet = () => {
        const updatedDeviceValueSets = [...props.data, {} as DeviceValueSet];
        refreshKeyModifier(Utility.getRandomString());
        props.onUpdate(updatedDeviceValueSets);
    }

    const removeDeviceValueSet = (removeIndex: number) => {
        const updatedDeviceValueSets = [...props.data.filter((_, index) => index !== removeIndex)];
        refreshKeyModifier(Utility.getRandomString());
        props.onUpdate(updatedDeviceValueSets);
    }

    const updateDeviceValueSet = (index: number, name: string, value: any) => {
        const updatedDeviceValueSets = props.data;
        switch (name) {
            case 'required':
                updatedDeviceValueSets[index].required = value;
                break;
            case 'valueExpression':
                updatedDeviceValueSets[index].valueExpression = value;
                break;
            case 'valueName':
                updatedDeviceValueSets[index].valueName = value;
                break;
            default:
        }
        props.onUpdate(updatedDeviceValueSets);
    }

    const renderDeviceValueSet = (valueSet: DeviceValueSet, index: number) => {
        const keySuffix = `-${index}-${keyModifier}`;
        return (
            <Card className="mb-3" key={`deviceValue-${keySuffix}`}>
                <CardTitle className="p-2 border-bottom">
                    Value
                        <button className="btn iomt-cm-btn-link iomt-cm-right p-0" onClick={() => removeDeviceValueSet(index)}>
                        <DeleteIcon />
                    </button>
                </CardTitle>
                <CardBody>
                    <DeviceValueName
                        data={valueSet} onUpdate={(val: string) => updateDeviceValueSet(index, 'valueName', val)}
                        modifier={`deviceValueName-${keySuffix}`} />
                    <DeviceValueExpression
                        data={valueSet} onUpdate={(val: string) => updateDeviceValueSet(index, 'valueExpression', val)}
                        modifier={`deviceValueExpression-${keySuffix}`} />
                    <DeviceValueRequired
                        data={valueSet} onUpdate={(val: string) => updateDeviceValueSet(index, 'required', val)}
                        modifier={`deviceValueRequired-${keySuffix}`} />
                </CardBody>
            </Card>
        )
    }

    return (
        <React.Fragment>
            <div className="position-relative">
                <Label>{Constants.Text.LabelDeviceValues}</Label>
                <button className="btn iomt-cm-btn-link" onClick={appendDeviceValueSet}>
                    + Value
                </button>
            </div>
            {props.data.map(renderDeviceValueSet)}
        </React.Fragment>
    );
}

const DeviceEditForm = (props: { data: DeviceMapping; onUpdate: Function }) => {
    return (
        <React.Fragment>
            <TypeMatchExpression
                data={props.data.typeMatchExpression}
                onUpdate={(updatedTypeMatchExpression: string) => {
                    const updatedDeviceMapping = {
                        ...props.data,
                        typeMatchExpression: updatedTypeMatchExpression
                    } as DeviceMapping;
                    props.onUpdate(updatedDeviceMapping);
                }}
            />
            <DeviceIdExpression
                data={props.data.deviceIdExpression}
                onUpdate={(updatedDeviceIdExpression: string) => {
                    const updatedDeviceMapping = {
                        ...props.data,
                        deviceIdExpression: updatedDeviceIdExpression
                    } as DeviceMapping;
                    props.onUpdate(updatedDeviceMapping);
                }}
            />
            <TimestampExpression
                data={props.data.timestampExpression}
                onUpdate={(updatedTimestampExpression: string) => {
                    const updatedDeviceMapping = {
                        ...props.data,
                        timestampExpression: updatedTimestampExpression
                    } as DeviceMapping;
                    props.onUpdate(updatedDeviceMapping);
                }}
            />
            <PatientIdExpression
                data={props.data.patientIdExpression}
                onUpdate={(updatedPatientIdExpression: string) => {
                    const updatedDeviceMapping = {
                        ...props.data,
                        patientIdExpression: updatedPatientIdExpression
                    } as DeviceMapping;
                    props.onUpdate(updatedDeviceMapping);
                }}
            />
            <EncounterIdExpression
                data={props.data.encounterIdExpression}
                onUpdate={(updatedEncounterIdExpression: string) => {
                    const updatedDeviceMapping = {
                        ...props.data,
                        encounterIdExpression: updatedEncounterIdExpression
                    } as DeviceMapping;
                    props.onUpdate(updatedDeviceMapping);
                }}
            />
            <CorrelationIdExpression
                data={props.data.correlationIdExpression}
                onUpdate={(updatedCorrelationIdExpression: string) => {
                    const updatedDeviceMapping = {
                        ...props.data,
                        correlationIdExpression: updatedCorrelationIdExpression
                    } as DeviceMapping;
                    props.onUpdate(updatedDeviceMapping);
                }}
            />
            <DeviceValuesForm
                data={props.data.values || [] as DeviceValueSet[]}
                onUpdate={(updatedDeviceValueSets: DeviceValueSet[]) => {
                    const updatedDeviceMapping = {
                        ...props.data,
                        values: updatedDeviceValueSets
                    } as DeviceMapping;
                    props.onUpdate(updatedDeviceMapping);
                }}
            />
        </React.Fragment>
    );
}

export default DeviceEditForm;