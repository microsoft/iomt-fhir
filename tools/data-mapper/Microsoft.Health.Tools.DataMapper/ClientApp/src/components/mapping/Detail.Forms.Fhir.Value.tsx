// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

import * as React from 'react';
import { FormGroup, Label, Input } from 'reactstrap';

import * as Constants from '../Constants';
import { Mapping, FhirValue, FhirSampledData, FhirQuantity } from "../../store/Mapping";

interface FhirValueNameOption {
    displayText: string,
    value: string,
}

interface FhirValueTypeOption {
    displayText: string,
    value: string,
}

const FhirValueTypeOptionsConfig = [
    {
        displayText: 'SampledData',
        value: 'SampledData'
    } as FhirValueTypeOption,
    {
        displayText: 'Quantity',
        value: 'Quantity'
    } as FhirValueTypeOption,
    {
        displayText: 'String',
        value: 'String'
    } as FhirValueTypeOption,
]


const FhirValueName = (props: { data: string; options: FhirValueNameOption[], onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`fhirValueName-${props.modifier}`}>{Constants.Text.LabelFhirValueName}</Label>
            <Input type="select" name={`fhirValueName-${props.modifier}`} id={`fhirValueName-${props.modifier}`}
                value={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            >
                <option></option>
                {props.options.map((option: FhirValueNameOption, index: number) => {
                    return <option value={option.value} key={`option-${index}`}>{option.displayText}</option>;
                })}
            </Input>
        </FormGroup>
    );
};

const FhirValueType = (props: { data: string; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`fhirValueType-${props.modifier}`}>{Constants.Text.LabelFhirValueType}</Label>
            <Input type="select" name={`fhirValueType-${props.modifier}`} id={`fhirValueType-${props.modifier}`}
                value={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            >
                <option></option>
                {
                    FhirValueTypeOptionsConfig.map((option: FhirValueTypeOption, index: number) => {
                        return <option value={option.value} key={`option-${index}`}>{option.displayText}</option>;
                    })
                }
            </Input>
        </FormGroup>
    );
};

const FhirValueDefaultPeriod = (props: { data: number; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`fhirValueDefaultPeriod-${props.modifier}`}>{Constants.Text.LabelFhirValueDefaultPeriod}</Label>
            <Input type="text" name={`fhirValueDefaultPeriod-${props.modifier}`} id={`fhirValueDefaultPeriod-${props.modifier}`}
                value={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
};

const FhirValueUnit = (props: { data: string; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`fhirValueUnit-${props.modifier}`}>{Constants.Text.LabelFhirValueUnit}</Label>
            <Input type="text" name={`fhirValueUnit-${props.modifier}`} id={`fhirValueUnit-${props.modifier}`}
                value={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
};

const FhirValueCode = (props: { data: string; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`fhirValueCode-${props.modifier}`}>{Constants.Text.LabelFhirValueCode}</Label>
            <Input type="text" name={`fhirValueCode-${props.modifier}`} id={`fhirValueCode-${props.modifier}`}
                value={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
};

const FhirValueSystem = (props: { data: string; onUpdate: Function; modifier?: string }) => {
    return (
        <FormGroup>
            <Label for={`fhirValueSystem-${props.modifier}`}>{Constants.Text.LabelFhirValueSystem}</Label>
            <Input type="text" name={`fhirValueSystem-${props.modifier}`} id={`fhirValueSystem-${props.modifier}`}
                value={props.data} onChange={(e) => props.onUpdate(e.target.value)}
            />
        </FormGroup>
    );
};

const FhirValueForm = (props: { data: FhirValue; context: Mapping; onUpdate: Function; modifier?: string }) => {
    const {
        data, context, onUpdate
    } = props;

    const [hasValueName, toggleHasValueName] = React.useState(data.valueName !== undefined);
    const [valueType, setValueType] = React.useState(data.valueType);

    return (
        <React.Fragment>
            <FhirValueName
                data={data.valueName}
                options={
                    context.device?.values !== undefined ?
                        context.device.values.map(v => {
                            return {
                                displayText: v.valueName,
                                value: v.valueName,
                            } as FhirValueNameOption
                        }) : [] as FhirValueNameOption[]
                }
                onUpdate={(updatedValueName: string) => {
                    const updatedFhirValue = {
                        ...props.data,
                        valueName: updatedValueName
                    }
                    toggleHasValueName(updatedValueName !== undefined && updatedValueName.length > 0);
                    onUpdate(updatedFhirValue);
                }}
                modifier={props.modifier}
            />
            {
                hasValueName &&
                    <FhirValueType
                        data={data.valueType}
                        onUpdate={
                            (updatedValueType: string) => {
                                const updatedFhirValue = {
                                    ...props.data,
                                    valueType: updatedValueType
                                }
                                if (updatedValueType === 'SampledData' && updatedFhirValue.sampledData === undefined) {
                                    updatedFhirValue.sampledData = {} as FhirSampledData;
                                }

                                if (updatedValueType === 'Quantity' && updatedFhirValue.quantity === undefined) {
                                    updatedFhirValue.quantity = {} as FhirQuantity;
                                }
                                setValueType(updatedValueType);
                                onUpdate(updatedFhirValue);
                            }
                        }
                        modifier={props.modifier}
                    />
            }
            {
                hasValueName && valueType === 'SampledData' &&
                    <React.Fragment>
                        <FhirValueDefaultPeriod
                            data={data.sampledData?.defaultPeriod}
                            onUpdate={(updatedDefaultPeriod: number) => {
                                const updatedSampledData = {
                                    ...props.data.sampledData,
                                    defaultPeriod: updatedDefaultPeriod
                                }

                                const updatedFhirValue = {
                                    ...props.data,
                                    sampledData: updatedSampledData
                                }
                                onUpdate(updatedFhirValue);
                            }}
                            modifier={props.modifier}
                        />

                        <FhirValueUnit
                            data={data.sampledData?.unit}
                            onUpdate={(updatedUnit: number) => {
                                const updatedSampledData = {
                                    ...props.data.sampledData,
                                    unit: updatedUnit
                                }

                                const updatedFhirValue = {
                                    ...props.data,
                                    sampledData: updatedSampledData
                                }
                                onUpdate(updatedFhirValue);
                            }}
                            modifier={props.modifier}
                        />
                    </React.Fragment>
            }
            {
                hasValueName && valueType === 'Quantity' &&
                    <React.Fragment>
                        <FhirValueCode
                            data={data.quantity?.code}
                            onUpdate={(updatedCode: number) => {
                                const updatedQuantity = {
                                    ...props.data.quantity,
                                    code: updatedCode
                                }
                                const updatedFhirValue = {
                                    ...props.data,
                                    quantity: updatedQuantity
                                }
                                onUpdate(updatedFhirValue);
                            }}
                            modifier={props.modifier}
                        />

                        <FhirValueSystem
                            data={data.quantity?.system}
                            onUpdate={(updatedSystem: number) => {
                                const updatedQuantity = {
                                    ...props.data.quantity,
                                    system: updatedSystem
                                }
                                const updatedFhirValue = {
                                    ...props.data,
                                    quantity: updatedQuantity
                                }
                                onUpdate(updatedFhirValue);
                            }}
                            modifier={props.modifier}
                        />

                        <FhirValueUnit
                            data={data.quantity?.unit}
                            onUpdate={(updatedUnit: number) => {
                                const updatedQuantity = {
                                    ...props.data.quantity,
                                    unit: updatedUnit
                                }
                                const updatedFhirValue = {
                                    ...props.data,
                                    quantity: updatedQuantity
                                }
                                onUpdate(updatedFhirValue);
                            }}
                            modifier={props.modifier}
                        />
                    </React.Fragment>
            }
        </React.Fragment>
    );
};

export default FhirValueForm;
